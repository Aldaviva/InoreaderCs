namespace InoreaderCs.Entities;

internal record SubscriptionListResponse {

    public required IReadOnlyList<Subscription> Subscriptions { get; init; }

}

internal record MinimalFolder(StreamId Id, string Label);