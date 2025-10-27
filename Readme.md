📰 InoreaderCs
===

[![NuGet](https://img.shields.io/nuget/v/InoreaderCs?logo=nuget&color=informational)](https://www.nuget.org/packages/InoreaderCs)

*.NET client for the [Inoreader HTTP API](https://www.inoreader.com/developers/)*

Like [IsaacSchemm/InoreaderFs](https://github.com/IsaacSchemm/InoreaderFs), but not fucked up:

- The `ot` query parameter to `stream/items/ids` is sent in the correct microsecond format, not seconds, so it isn't ignored
- Has a built-in OAuth2 client with refresh logic and a password-based auth client, both of which make auth requests automatically
- Parse, handle, and use Stream IDs for well-known system states, folders, and tags
- Observe rate-limiting statistics
- Uses modern, interchangeable, customizable `HttpClient` instead of ancient, disgusting `HttpWebRequest`
- Set custom `User-Agent` or any other HTTP request headers
- Easily get article's read and starred state, short ID, description, and original feed name and URL
- Objects are configurable because they are not static classes, so you don't need to pass authentication to literally every request, you can just set it up once, for example in an IoC context, and not have to pass it around your entire codebase.
- Interfaces allow mocking and interchangeability, instead of everything being sealed static classes.
- Full documentation of methods and entities
- Exceptions have information about what went wrong
- Updated in the last 6 years