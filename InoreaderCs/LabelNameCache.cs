using InoreaderCs.Entities;
using System.Diagnostics;

namespace InoreaderCs;

internal class LabelNameCache {

    private readonly InoreaderClient _client;
    private readonly TimeSpan        _cacheDuration;
    private readonly Stopwatch       _freshness    = new();
    private readonly SemaphoreSlim   _fetchingLock = new(1);
    private readonly ISet<string>    _folderNames  = new HashSet<string>();
    private readonly ISet<string>    _tagNames     = new HashSet<string>();

    private event EventHandler<IEnumerable<StreamState>> TagAndFolderStatesListed;

    public LabelNameCache(InoreaderClient client, TimeSpan cacheDuration) {
        _client        = client;
        _cacheDuration = cacheDuration;

        client.Requests.TagAndFolderStatesListed += OnTagAndFolderStatesListed;
        TagAndFolderStatesListed                 += OnTagAndFolderStatesListedUnsynchronized;
    }

    public async Task<Labels> GetLabelNames(CancellationToken ct = default) {
        if (IsStale()) {
            try {
                await _fetchingLock.WaitAsync(ct).ConfigureAwait(false);
                if (IsStale()) {
                    _client.Requests.TagAndFolderStatesListed -= OnTagAndFolderStatesListed;
                    try {
                        TagAndFolderStatesListed(this, await FetchStreamStates(ct).ConfigureAwait(false));
                    } finally {
                        _client.Requests.TagAndFolderStatesListed += OnTagAndFolderStatesListed;
                    }
                }
            } catch (OperationCanceledException) { } finally {
                if (!ct.IsCancellationRequested) {
                    _fetchingLock.Release();
                }
            }
        }

        return new Labels(_folderNames, _tagNames);

        bool IsStale() => !_freshness.IsRunning || _freshness.Elapsed > _cacheDuration;
    }

    private void OnTagAndFolderStatesListed(object? sender, IEnumerable<StreamState> labelStates) {
        _fetchingLock.Wait();
        try {
            TagAndFolderStatesListed(this, labelStates);
        } finally {
            _fetchingLock.Release();
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
        _fetchingLock.Wait();
        try {
            ISet<string> labels = isFolder ? _folderNames : _tagNames;
            if (remove) {
                labels.Remove(labelName);
            } else {
                labels.Add(labelName);
            }
        } finally {
            _fetchingLock.Release();
        }
    }

    private async Task<IEnumerable<StreamState>> FetchStreamStates(CancellationToken ct = default) {
        try {
            return await _client.Requests.ListTagAndFolderStates(ct).ConfigureAwait(false);
        } catch (InoreaderException) {
            return [];
        }
    }

    public readonly record struct Labels(ISet<string> Folders, ISet<string> Tags);

}