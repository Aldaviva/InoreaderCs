namespace InoreaderCs.Entities;

internal sealed record SubscriptionListResponse {

    public required IReadOnlyList<Subscription> Subscriptions { get; init; }

}

internal sealed record MinimalFolder(StreamId Id, string Label);