📰 InoreaderCs
===

[![NuGet](https://img.shields.io/nuget/v/InoreaderCs?logo=nuget&color=informational)](https://www.nuget.org/packages/InoreaderCs)

*.NET client for the [Inoreader HTTP API](https://www.inoreader.com/developers/)*

Like [IsaacSchemm/InoreaderFs](https://github.com/IsaacSchemm/InoreaderFs), but not fucked up:

- Correctly send the `ot` query parameter to `stream/items/ids` in microsecond format, not seconds, so it isn't ignored.
- Has a built-in OAuth2 client with smart refresh logic as well as a password-based auth client, both of which make auth requests automatically and support pluggable persistence strategies.
- Observe rate-limiting statistics.
- Uses modern, interchangeable, customizable `HttpClient` instead of ancient, disgusting `HttpWebRequest`.
- Allows you to set custom HTTP request headers, such as `User-Agent`.
- Easily gets article's read and starred state, short ID, description, and original feed name and URL.
- Instances are configurable because they are not static classes, so you don't need to supply authentication to literally every request, you can just set it up once, for example in an IoC context, and not have to pass it around your entire codebase.
- Interfaces allow mocking and interchangeability, instead of everything being sealed static classes.
- Hierarchical interface structure makes it easier to find the API method you want and understand what it applies to.
- Facade pattern hides the complexity of the Inoreader API's very overloaded methods with lots of conditionally valid parameters.
- Avoids the insane concept of stream IDs and all their complexity of parsing, translating, handling, and using them, because developers today shouldn't have to deal with weird Google decisions from 2001 that Inoreader bent over backwards to be compatible with for no real benefit because there is no Google Reader client that anyone pointed at Inoreader as a drop-in replacement backend.
- Full documentation of methods and entities.
- Exceptions have information about what went wrong.
- Updated in the last 6 years by someone who uses Inoreader and this library heavily every day.