/*using InoreaderCs.Entities;
using Unfucked.HTTP.Exceptions;
using UnionTypes;

namespace InoreaderCs;

// @formatter:wrap_chained_method_calls chop_always

internal class OldRequests {

    /// <inheritdoc />
    public async Task<FullArticles> ListFullArticles(StreamId stream, int maxArticles = 20, DateTimeOffset? minTime = null, StreamId? subtract = null, StreamId? intersect = null,
                                                     PaginationToken? pagination = null, bool ascendingOrder = false, bool includeFolders = true, bool includeAnnotations = false,
                                                     CancellationToken cancellationToken = default) {
        try {
            return await apiTarget
                .Path("stream/contents/{streamId}")
                .ResolveTemplate("streamId", stream)
                .QueryParam("n", maxArticles.Clip(1, 200))
                .QueryParam("ot", minTime?.ToUnixTimeMicroseconds())
                .QueryParam("r", ascendingOrder ? "o" : null)
                .QueryParam("xt", subtract)
                .QueryParam("it", intersect)
                .QueryParam("c", pagination)
                .QueryParam("includeAllDirectStreamIds", includeFolders)
                .QueryParam("annotations", Convert.ToInt32(includeAnnotations)) // docs are wrong, "true" is ignored
                .Get<FullArticles>(cancellationToken)
                .ConfigureAwait(false);
        } catch (HttpException e) {
            throw TransformError(e, $"Failed to list articles in feed {stream}");
        }
    }

    /// <inheritdoc />
    public async Task<MinimalArticles> ListMinimalArticles(StreamId stream, int maxArticles = 20, DateTimeOffset? minTime = null, StreamId? subtract = null, StreamId? intersect = null,
                                                           PaginationToken? pagination = null, bool ascendingOrder = false, bool includeFolders = true, CancellationToken cancellationToken = default) {
        try {
            return await apiTarget
                .Path("stream/items/ids")
                .QueryParam("n", maxArticles.Clip(1, 1000))
                .QueryParam("ot", minTime?.ToUnixTimeMicroseconds())
                .QueryParam("r", ascendingOrder ? "o" : null)
                .QueryParam("xt", subtract)
                .QueryParam("it", intersect)
                .QueryParam("c", pagination)
                .QueryParam("s", stream)
                .QueryParam("includeAllDirectStreamIds", includeFolders)
                .Get<MinimalArticles>(cancellationToken)
                .ConfigureAwait(false);
        } catch (HttpException e) {
            throw TransformError(e, $"Failed to list article IDs in feed {stream}");
        }
    }

    /// <inheritdoc />
    public async Task MarkArticles(StreamId label, bool removeLabel = false, CancellationToken cancellationToken = default, params IEnumerable<Union<Article, string>> articlesOrIds) {
        try {
            IReadOnlyList<KeyValuePair<string, string>> articleIdFormParams = articlesOrIds
                .Select(articleOrId => articleOrId.Switch(article => article.ShortId, id => id))
                .Select(id => new KeyValuePair<string, string>("i", id))
                .ToList();

            if (articleIdFormParams.Count != 0) {
                HttpContent body = new FormUrlEncodedContent(articleIdFormParams.Prepend(new KeyValuePair<string, string>(removeLabel ? "r" : "a", label.ToString())));
                (await apiTarget
                        .Path("edit-tag")
                        .Post(body, cancellationToken)
                        .ConfigureAwait(false))
                    .Dispose();
            }
        } catch (HttpException e) {
            throw TransformError(e, $"Failed to {(removeLabel ? "untag" : "tag")} articles with tag {label}");
        }
    }

    /// <inheritdoc />
    public async Task<User> GetSelfUser(CancellationToken cancellationToken = default) {
        try {
            return await apiTarget
                .Path("user-info")
                .Get<User>(cancellationToken)
                .ConfigureAwait(false);
        } catch (HttpException e) {
            throw TransformError(e, "Failed to get self user info");
        }
    }

    /// <inheritdoc />
    public async Task<SubscriptionCreationResult> SubscribeToFeed(Uri feedLocation, CancellationToken cancellationToken = default) {
        try {
            return await apiTarget
                .Path("subscription/quickadd")
                .Post<SubscriptionCreationResult>(new FormUrlEncodedContent(new Dictionary<string, string> {
                    ["quickadd"] = StreamId.Feed(feedLocation)
                }), cancellationToken)
                .ConfigureAwait(false);
        } catch (HttpException e) {
            throw TransformError(e, "Failed to subscribe to feed");
        }
    }

    /// <inheritdoc />
    public async Task SubscribeToFeed(StreamIdentifier feedLocation, string? title = null, string? folder = null, CancellationToken cancellationToken = default) {
        try {
            (await apiTarget
                    .Path("subscription/edit")
                    .Post(new FormUrlEncodedContent(new Dictionary<string, string?> {
                        ["ac"] = "follow",
                        ["s"]  = feedLocation.Switch(streamId => streamId, uri => StreamId.Feed(uri), streamId => streamId.Id),
                        ["t"]  = title,
                        ["a"]  = folder
                    }.Compact()), cancellationToken)
                    .ConfigureAwait(false))
                .Dispose();
        } catch (HttpException e) {
            throw TransformError(e, "Failed to subscribe to feed");
        }
    }

    /// <inheritdoc />
    public async Task ModifyFeed(StreamIdentifier feedLocation, string? newTitle = null, string? newFolder = null, string? removeFromFolder = null,
                                 CancellationToken cancellationToken = default) {
        try {
            (await apiTarget
                    .Path("subscription/edit")
                    .Post(new FormUrlEncodedContent(new Dictionary<string, string?> {
                        ["ac"] = "edit",
                        ["s"]  = feedLocation.Switch(streamId => streamId, uri => StreamId.Feed(uri), streamId => streamId.Id),
                        ["t"]  = newTitle,
                        ["a"]  = newFolder,
                        ["r"]  = removeFromFolder
                    }.Compact()), cancellationToken)
                    .ConfigureAwait(false))
                .Dispose();
        } catch (HttpException e) {
            throw TransformError(e, "Failed to modify feed");
        }
    }

    /// <inheritdoc />
    public async Task UnsubscribeFromFeed(StreamIdentifier feedLocation, CancellationToken cancellationToken = default) {
        try {
            (await apiTarget
                    .Path("subscription/edit")
                    .Post(new FormUrlEncodedContent(new Dictionary<string, string> {
                        ["ac"] = "unfollow",
                        ["s"]  = feedLocation.Switch(streamId => streamId, uri => StreamId.Feed(uri), streamId => streamId)
                    }), cancellationToken)
                    .ConfigureAwait(false))
                .Dispose();
        } catch (HttpException e) {
            throw TransformError(e, "Failed to unsubscribe from feed");
        }
    }

    /#1#// <inheritdoc />
    public async Task<IDictionary<StreamId, FolderState>> GetFolderStates(CancellationToken cancellationToken = default) {
        try {
            JsonObject response = await ApiTarget
                .Path("preferences/stream/list")
                .Get<JsonObject>(cancellationToken)
                .ConfigureAwait(false);

            return response.ToDictionary(pair => StreamId.Parse(pair.Key), pair => {
                bool?  isExpanded           = null;
                string subscriptionOrdering = string.Empty;

                foreach (JsonNode idValueObj in pair.Value!.AsArray()!) {
                    JsonNode? value = idValueObj!["value"];
                    switch (idValueObj["id"]!.GetValue<string>()) {
                        case "subscription-ordering":
                            subscriptionOrdering = value!.GetValue<string>();
                            break;
                        case "is-expanded":
                            isExpanded = value!.GetValue<string>() == "true";
                            break;
                    }
                }

                return new FolderState(subscriptionOrdering, isExpanded);
            });
        } catch (HttpException e) {
            throw TransformError(e, "Failed to get stream settings");
        }
    }#1#

    /// <inheritdoc />
    public async Task<IEnumerable<Subscription>> ListSubscriptions(CancellationToken cancellationToken = default) {
        try {
            return (await apiTarget
                    .Path("subscription/list")
                    .Get<SubscriptionListResponse>(cancellationToken)
                    .ConfigureAwait(false))
                .Subscriptions;
        } catch (HttpException e) {
            throw TransformError(e, "Failed to list subscriptions");
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<FolderState>> ListFolders(CancellationToken cancellationToken = default) =>
        (await ListTagOrFolderState(cancellationToken)
            .ConfigureAwait(false))
        .OfType<FolderState>();

    /// <inheritdoc />
    public async Task<IEnumerable<TagState>> ListTags(CancellationToken cancellationToken = default) =>
        (await ListTagOrFolderState(cancellationToken)
            .ConfigureAwait(false))
        .OfType<TagState>();

    /// <exception cref="InoreaderException"></exception>
    private async Task<IEnumerable<StreamState>> ListTagOrFolderState(CancellationToken cancellationToken) {
        try {
            return (await apiTarget
                    .Path("tag/list")
                    .QueryParam("types", 1)
                    .QueryParam("counts", 1)
                    .Get<TagList>(cancellationToken)
                    .ConfigureAwait(false))
                .Tags;
        } catch (HttpException e) {
            throw TransformError(e, "Failed to list tag and folder states");
        }
    }

    /// <inheritdoc />
    public async Task<LabelUnreadCounts> GetUnreadCounts(CancellationToken cancellationToken = default) {
        try {
            UnreadCountResponses response = await apiTarget.Path("unread-count")
                .Get<UnreadCountResponses>(cancellationToken)
                .ConfigureAwait(false);

            return new LabelUnreadCounts(response.UnreadCounts.ToDictionary(res => res.Id, res => new StreamUnreadState(res.Count, res.NewestArticleTime)), response.Max);
        } catch (HttpException e) {
            throw TransformError(e, "Failed to get unread counts");
        }
    }

    /// <inheritdoc />
    public async Task RenameFolderOrTag(Union<string, StreamId> folderOrTag, string newName, CancellationToken cancellationToken = default) {
        StreamId streamId = folderOrTag.Switch(StreamId.Label, Identity);
        try {
            (await apiTarget
                    .Path("rename-tag")
                    .Post(new FormUrlEncodedContent(new Dictionary<string, string> {
                        ["s"]    = streamId,
                        ["dest"] = newName
                    }), cancellationToken)
                    .ConfigureAwait(false))
                .Dispose();
        } catch (HttpException e) {
            throw TransformError(e, $"Failed to rename folder or tag {streamId} to {newName}");
        }
    }

    /// <inheritdoc />
    public async Task DeleteFolderOrTag(Union<string, StreamId> folderOrTag, CancellationToken cancellationToken = default) {
        StreamId streamId = folderOrTag.Switch(StreamId.Label, Identity);
        try {
            (await apiTarget
                    .Path("disable-tag")
                    .Post(new FormUrlEncodedContent(new Dictionary<string, string> {
                        ["s"] = streamId
                    }), cancellationToken)
                    .ConfigureAwait(false))
                .Dispose();
        } catch (HttpException e) {
            throw TransformError(e, $"Failed to delete folder or tag {streamId}");
        }
    }

    /// <inheritdoc />
    public async Task MarkAllArticlesRead(Union<string, StreamId>? folderOrTag, DateTimeOffset maxSeenArticleTime, CancellationToken cancellationToken = default) {
        StreamId streamId = folderOrTag?.Switch(StreamId.Label, Identity) ?? StreamId.ReadingList;
        try {
            (await apiTarget
                    .Path("mark-all-as-read")
                    .Post(new FormUrlEncodedContent(new Dictionary<string, string> {
                        ["s"]  = streamId,
                        ["ts"] = Convert.ToString(maxSeenArticleTime.ToUnixTimeMicroseconds())
                    }), cancellationToken)
                    .ConfigureAwait(false))
                .Dispose();
        } catch (HttpException e) {
            throw TransformError(e, $"Failed to mark all articles as read in {streamId}");
        }
    }

}*/

