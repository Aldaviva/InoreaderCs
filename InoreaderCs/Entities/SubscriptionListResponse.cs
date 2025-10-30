namespace InoreaderCs.Entities;

internal record SubscriptionListResponse {

    public required IReadOnlyList<Subscription> Subscriptions { get; init; }

}

public record MinimalFolder(StreamId Id, string Label);