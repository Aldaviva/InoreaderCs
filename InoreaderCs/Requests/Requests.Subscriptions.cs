using InoreaderCs.Entities;
using Unfucked.HTTP.Exceptions;

namespace InoreaderCs.Requests;

internal partial class Requests {

    /// <inheritdoc />
    async Task<IEnumerable<Subscription>> IInoreaderClient.ISubscriptionMethods.List(CancellationToken cancellationToken) {
        try {
            return (await ApiBase
                    .Path("subscription/list")
                    .Get<SubscriptionListResponse>(cancellationToken)
                    .ConfigureAwait(false))
                .Subscriptions;
        } catch (HttpException e) {
            throw TransformError(e, "Failed to list subscriptions");
        }
    }

    /// <inheritdoc />
    Task IInoreaderClient.ISubscriptionMethods.Rename(Uri feedLocation, string newTitle, CancellationToken cancellationToken) =>
        ModifySubscription(StreamId.ForFeed(feedLocation), SubscriptionEditAction.Edit, newTitle, null, null, cancellationToken);

    /// <inheritdoc />
    async Task IInoreaderClient.ISubscriptionMethods.AddToFolder(Uri feedLocation, string folder, CancellationToken cancellationToken) {
        await ModifySubscription(StreamId.ForFeed(feedLocation), SubscriptionEditAction.Edit, null, folder, null, cancellationToken).ConfigureAwait(false);
        client.LabelNameCache.Edit(folder, true, false);
    }

    /// <inheritdoc />
    Task IInoreaderClient.ISubscriptionMethods.RemoveFromFolder(Uri feedLocation, string folder, CancellationToken cancellationToken) =>
        ModifySubscription(StreamId.ForFeed(feedLocation), SubscriptionEditAction.Edit, null, null, folder, cancellationToken);

    /// <inheritdoc />
    async Task<SubscriptionCreationResult> IInoreaderClient.ISubscriptionMethods.Subscribe(Uri feedLocation, CancellationToken cancellationToken) {
        try {
            return await ApiBase
                .Path("subscription/quickadd")
                .Post<SubscriptionCreationResult>(new FormUrlEncodedContent(new Dictionary<string, string> {
                    ["quickadd"] = StreamId.ForFeed(feedLocation)
                }), cancellationToken)
                .ConfigureAwait(false);
        } catch (HttpException e) {
            throw TransformError(e, "Failed to subscribe to feed");
        }
    }

    /// <inheritdoc />
    async Task IInoreaderClient.ISubscriptionMethods.Subscribe(Uri feedLocation, string? title, string? folder, CancellationToken cancellationToken) {
        await ModifySubscription(StreamId.ForFeed(feedLocation), SubscriptionEditAction.Follow, title, folder, null, cancellationToken).ConfigureAwait(false);
        if (folder is not null) {
            client.LabelNameCache.Edit(folder, true, false);
        }
    }

    /// <inheritdoc />
    Task IInoreaderClient.ISubscriptionMethods.Unsubscribe(Uri feedLocation, CancellationToken cancellationToken) =>
        ModifySubscription(StreamId.ForFeed(feedLocation), SubscriptionEditAction.Unfollow, null, null, null, cancellationToken);

}