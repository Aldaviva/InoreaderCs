using System.Collections.Immutable;
using System.Diagnostics;

namespace InoreaderCs.Entities;

internal sealed class LabelNameCache: IDisposable {

    private readonly InoreaderClient _client;
    private readonly TimeSpan        _cacheDuration;
    private readonly Stopwatch       _freshness   = new();
    private readonly SemaphoreSlim   _lock        = new(1);
    private readonly HashSet<string> _folderNames = [];
    private readonly HashSet<string> _tagNames    = [];

    private event EventHandler<IEnumerable<StreamState>> TagAndFolderStatesListed;

    public LabelNameCache(InoreaderClient client, TimeSpan cacheDuration) {
        _client        = client;
        _cacheDuration = cacheDuration >= TimeSpan.FromSeconds(1) ? cacheDuration : TimeSpan.FromHours(1);

        client.Requests.TagAndFolderStatesListed += OnTagAndFolderStatesListed;
        TagAndFolderStatesListed                 += OnTagAndFolderStatesListedUnsynchronized;
    }

    /// <exception cref="OperationCanceledException"><paramref name="ct"/> is canceled</exception>
    public async Task<Labels> GetLabelNames(bool force = false, CancellationToken ct = default) {
        try {
            await _lock.WaitAsync(ct).ConfigureAwait(false);

            if (force || !_freshness.IsRunning || _freshness.Elapsed > _cacheDuration) {
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
            if (!ct.IsCancellationRequested) {
                _lock.Release();
            }
        }
    }

    private void OnTagAndFolderStatesListed(object? sender, IEnumerable<StreamState> labelStates) {
        _lock.Wait();
        try {
            TagAndFolderStatesListed(this, labelStates);
        } finally {
            _lock.Release();
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
        _lock.Wait();
        try {
            HashSet<string> labels = isFolder ? _folderNames : _tagNames;
            if (remove) {
                labels.Remove(labelName);
            } else {
                labels.Add(labelName);
            }
        } finally {
            _lock.Release();
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