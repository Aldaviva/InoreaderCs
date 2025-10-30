using System.Text.Json.Serialization;

namespace InoreaderCs.Entities;

public record SubscriptionCreationResult {

    [JsonInclude]
    private int NumResults { get; init; }

    public bool IsSuccesfullySubscribed => NumResults != 0;

    public required string StreamName { get; init; }

    public required string StreamId { get; init; }

}