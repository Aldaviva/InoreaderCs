📰 InoreaderCs
===

[![NuGet package](https://img.shields.io/nuget/v/InoreaderCs?label=package&logo=nuget&color=informational)](https://www.nuget.org/packages/InoreaderCs) [![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/Aldaviva/InoreaderCs/dotnetpackage.yml?branch=master&logo=github)](https://github.com/Aldaviva/InoreaderCs/actions/workflows/dotnetpackage.yml) [![Testspace](https://img.shields.io/testspace/tests/Aldaviva/Aldaviva:InoreaderCs/master?passed_label=passing&failed_label=failing&logo=data%3Aimage%2Fsvg%2Bxml%3Bbase64%2CPHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCA4NTkgODYxIj48cGF0aCBkPSJtNTk4IDUxMy05NCA5NCAyOCAyNyA5NC05NC0yOC0yN3pNMzA2IDIyNmwtOTQgOTQgMjggMjggOTQtOTQtMjgtMjh6bS00NiAyODctMjcgMjcgOTQgOTQgMjctMjctOTQtOTR6bTI5My0yODctMjcgMjggOTQgOTQgMjctMjgtOTQtOTR6TTQzMiA4NjFjNDEuMzMgMCA3Ni44My0xNC42NyAxMDYuNS00NFM1ODMgNzUyIDU4MyA3MTBjMC00MS4zMy0xNC44My03Ni44My00NC41LTEwNi41UzQ3My4zMyA1NTkgNDMyIDU1OWMtNDIgMC03Ny42NyAxNC44My0xMDcgNDQuNXMtNDQgNjUuMTctNDQgMTA2LjVjMCA0MiAxNC42NyA3Ny42NyA0NCAxMDdzNjUgNDQgMTA3IDQ0em0wLTU1OWM0MS4zMyAwIDc2LjgzLTE0LjgzIDEwNi41LTQ0LjVTNTgzIDE5Mi4zMyA1ODMgMTUxYzAtNDItMTQuODMtNzcuNjctNDQuNS0xMDdTNDczLjMzIDAgNDMyIDBjLTQyIDAtNzcuNjcgMTQuNjctMTA3IDQ0cy00NCA2NS00NCAxMDdjMCA0MS4zMyAxNC42NyA3Ni44MyA0NCAxMDYuNVMzOTAgMzAyIDQzMiAzMDJ6bTI3NiAyODJjNDIgMCA3Ny42Ny0xNC44MyAxMDctNDQuNXM0NC02NS4xNyA0NC0xMDYuNWMwLTQyLTE0LjY3LTc3LjY3LTQ0LTEwN3MtNjUtNDQtMTA3LTQ0Yy00MS4zMyAwLTc2LjY3IDE0LjY3LTEwNiA0NHMtNDQgNjUtNDQgMTA3YzAgNDEuMzMgMTQuNjcgNzYuODMgNDQgMTA2LjVTNjY2LjY3IDU4NCA3MDggNTg0em0tNTU3IDBjNDIgMCA3Ny42Ny0xNC44MyAxMDctNDQuNXM0NC02NS4xNyA0NC0xMDYuNWMwLTQyLTE0LjY3LTc3LjY3LTQ0LTEwN3MtNjUtNDQtMTA3LTQ0Yy00MS4zMyAwLTc2LjgzIDE0LjY3LTEwNi41IDQ0UzAgMzkxIDAgNDMzYzAgNDEuMzMgMTQuODMgNzYuODMgNDQuNSAxMDYuNVMxMDkuNjcgNTg0IDE1MSA1ODR6IiBmaWxsPSIjZmZmIi8%2BPC9zdmc%2B)](https://aldaviva.testspace.com/spaces/326067) [![Coveralls](https://img.shields.io/coveralls/github/Aldaviva/InoreaderCs?logo=coveralls)](https://coveralls.io/github/Aldaviva/InoreaderCs?branch=master)

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
- Automated tests.
- Includes correct, strongly-typed arguments in API methods like `subscription/edit`, which are incorrect in the official documentation and unehlpful, weakly-typed strings in InoreaderFs.