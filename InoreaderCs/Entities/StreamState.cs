using System.Text.Json.Serialization;

namespace InoreaderCs.Entities;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TagState), "tag")]
[JsonDerivedType(typeof(FolderState), "folder")]
[JsonDerivedType(typeof(ActiveSearchState), "active_search")]
public record StreamState {

    public required StreamId Id { get; init; }

    public required string SortId { get; init; }

}

public abstract record LabelState: StreamState {

    public string Name => Id.Id.Split(['/'], 4)[3];

}

public record FolderState: LabelState;

public record TagState: LabelState {

    [JsonPropertyName("unread_count")]
    public int? UnreadCount { get; init; }

    [JsonPropertyName("unseen_count")]
    public int? UnseenCount { get; init; }

}

internal record ActiveSearchState: TagState;