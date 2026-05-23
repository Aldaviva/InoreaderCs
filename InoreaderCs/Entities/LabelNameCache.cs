using System.Collections.Immutable;
using System.Diagnostics;

namespace InoreaderCs.Entities;

internal sealed class LabelNameCache: IDisposable {

    private readonly InoreaderClient      _client;
    private readonly TimeSpan             _cacheDuration;
    private readonly Stopwatch            _freshness   = new();
    private readonly ReaderWriterLockSlim _lock        = new();
    private readonly HashSet<string>      _folderNames = [];
    private readonly HashSet<string>      _tagNames    = [];

    private event EventHandler<IEnumerable<StreamState>> TagAndFolderStatesListed;

    public LabelNameCache(InoreaderClient client, TimeSpan cacheDuration) {
        _client        = client;
        _cacheDuration = cacheDuration >= TimeSpan.FromSeconds(1) ? cacheDuration : TimeSpan.FromHours(1);

        client.Requests.TagAndFolderStatesListed += OnTagAndFolderStatesListed;
        TagAndFolderStatesListed                 += OnTagAndFolderStatesListedUnsynchronized;
    }

    public async Task<Labels> GetLabelNames(bool force = false, CancellationToken ct = default) {
        if (force || IsStale()) {
            _lock.EnterWriteLock();
            try {
                if (force || IsStale()) {
                    _client.Requests.TagAndFolderStatesListed -= OnTagAndFolderStatesListed;
                    try {
                        TagAndFolderStatesListed(this, await FetchStreamStates(ct).ConfigureAwait(false));
                    } catch (InoreaderException) {
                        // ignore
                    } finally {
                        _client.Requests.TagAndFolderStatesListed += OnTagAndFolderStatesListed;
                    }
                }
                return new Labels(_folderNames, _tagNames);
            } finally {
                _lock.ExitWriteLock();
            }
        }

        _lock.EnterReadLock();
        try {
            return new Labels(_folderNames, _tagNames);
        } finally {
            _lock.ExitReadLock();
        }

        bool IsStale() => !_freshness.IsRunning || _freshness.Elapsed > _cacheDuration;
    }

    private void OnTagAndFolderStatesListed(object? sender, IEnumerable<StreamState> labelStates) {
        _lock.EnterWriteLock();
        try {
            TagAndFolderStatesListed(this, labelStates);
        } finally {
            _lock.ExitWriteLock();
        }
    }

    private void OnTagAndFolderStatesListedUnsynchronized(object? sender, IEnumerable<StreamState> labelStates) {
        _folderNames.Clear();
        _tagNames.Clear();

        foreach (StreamState labelState in labelStates) {
            switch (labelState) {
                case FolderState f:
                    _folderNames.Add(f.Name);
                    break;
                case TagState t:
                    _tagNames.Add(t.Name);
                    break;
            }
        }

        _freshness.Restart();
    }

    public void Edit(string labelName, bool isFolder, bool remove) {
        _lock.EnterWriteLock();
        try {
            HashSet<string> labels = isFolder ? _folderNames : _tagNames;
            if (remove) {
                labels.Remove(labelName);
            } else {
                labels.Add(labelName);
            }
        } finally {
            _lock.ExitWriteLock();
        }
    }

    /// <exception cref="InoreaderException"></exception>
    private async Task<IEnumerable<StreamState>> FetchStreamStates(CancellationToken ct = default) =>
        await _client.Requests.ListTagAndFolderStates(ct).ConfigureAwait(false);

    internal readonly record struct Labels(ISet<string> Folders, ISet<string> Tags) {

        public ISet<string> Folders { get; } = Folders.ToImmutableHashSet();
        public ISet<string> Tags { get; } = Tags.ToImmutableHashSet();

    }

    public void Dispose() {
        _lock.Dispose();
    }

}