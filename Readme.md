📰 InoreaderCs
===

*.NET client for Inoreader HTTP API*

Like [IsaacSchemm/InoreaderFs](https://github.com/IsaacSchemm/InoreaderFs), but not fucked up.

- `ot` query parameter is sent in the correct microsecond format, not seconds
- Has a built-in OAuth client with refresh logic
- Parse, handle, and use well-known Stream IDs
- Observe rate-limiting statistics
- Use modern `HttpClient` instead of ancient, disgusting `HttpWebRequest`
- Set custom `User-Agent` or any other HTTP request headers
- Easily get article's read and starred state, short ID, description, and original feed name and URL
- Updated in the last 6 years