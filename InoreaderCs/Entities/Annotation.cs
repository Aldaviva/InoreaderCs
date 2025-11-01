using System.Text.Json.Serialization;

namespace InoreaderCs.Entities;

/// <summary>
/// A custom user annotation or note added to an article.
/// </summary>
public record Annotation {

    /// <summary>
    /// Unique ID of this annotation.
    /// </summary>
    public required long Id { get; init; }

    /// <summary>
    /// If the annotation highlights a range of text, this is the start index of the range, otherwise <c>0</c>. Requires Pro account.
    /// </summary>
    public required int Start { get; init; }

    /// <summary>
    /// If the annotation highlights a range of text, this is the end index of the range, otherwise <c>0</c>. Requires Pro account.
    /// </summary>
    public required int End { get; init; }

    /// <summary>
    /// When the user created the annotation.
    /// </summary>
    [JsonPropertyName("added_on")] // Unlike the rest of the API, this class switched its property names from lower camel case to lower snake case for some reason
    public required DateTimeOffset AddedOn { get; init; }

    /// <summary>
    /// If the annotation highlights a range of text, the text that was highlighted, otherwise <see cref="string.Empty"/>. Requires Pro account.
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// The custom text of the note that the user supplied.
    /// </summary>
    public required string Note { get; init; }

    /// <summary>
    /// The numeric ID of the user who added the note.
    /// </summary>
    [JsonPropertyName("user_id")]
    public required long UserId { get; init; }

    /// <summary>
    /// The full name (not username or email address) of the user who added the note.
    /// </summary>
    [JsonPropertyName("user_name")]
    public required string UserFullName { get; init; }

    /// <summary>
    /// URL of a JPEG image of the user's profile picture or avatar.
    /// </summary>
    [JsonPropertyName("user_profile_picture")]
    public required Uri UserProfilePicture { get; init; }

}