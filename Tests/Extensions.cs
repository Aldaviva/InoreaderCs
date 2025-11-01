using System.Text;

namespace Tests;

public static class Extensions {

    public static string? ReadAsString(this HttpContent? requestBody) {
        if (requestBody != null) {
            using MemoryStream memoryStream = new();
            requestBody.CopyTo(memoryStream, null, CancellationToken.None);
            memoryStream.Position = 0;
            using StreamReader reader = new(memoryStream, Encoding.UTF8);
            return reader.ReadToEnd();
        } else {
            return null;
        }
    }

}