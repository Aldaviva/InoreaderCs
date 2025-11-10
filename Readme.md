📰 InoreaderCs
===

[![NuGet package](https://img.shields.io/nuget/v/InoreaderCs?label=package&logo=nuget&color=informational)](https://www.nuget.org/packages/InoreaderCs) [![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/Aldaviva/InoreaderCs/dotnetpackage.yml?branch=master&logo=github)](https://github.com/Aldaviva/InoreaderCs/actions/workflows/dotnetpackage.yml) [![Testspace](https://img.shields.io/testspace/tests/Aldaviva/Aldaviva:InoreaderCs/master?passed_label=passing&failed_label=failing&logo=data%3Aimage%2Fsvg%2Bxml%3Bbase64%2CPHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCA4NTkgODYxIj48cGF0aCBkPSJtNTk4IDUxMy05NCA5NCAyOCAyNyA5NC05NC0yOC0yN3pNMzA2IDIyNmwtOTQgOTQgMjggMjggOTQtOTQtMjgtMjh6bS00NiAyODctMjcgMjcgOTQgOTQgMjctMjctOTQtOTR6bTI5My0yODctMjcgMjggOTQgOTQgMjctMjgtOTQtOTR6TTQzMiA4NjFjNDEuMzMgMCA3Ni44My0xNC42NyAxMDYuNS00NFM1ODMgNzUyIDU4MyA3MTBjMC00MS4zMy0xNC44My03Ni44My00NC41LTEwNi41UzQ3My4zMyA1NTkgNDMyIDU1OWMtNDIgMC03Ny42NyAxNC44My0xMDcgNDQuNXMtNDQgNjUuMTctNDQgMTA2LjVjMCA0MiAxNC42NyA3Ny42NyA0NCAxMDdzNjUgNDQgMTA3IDQ0em0wLTU1OWM0MS4zMyAwIDc2LjgzLTE0LjgzIDEwNi41LTQ0LjVTNTgzIDE5Mi4zMyA1ODMgMTUxYzAtNDItMTQuODMtNzcuNjctNDQuNS0xMDdTNDczLjMzIDAgNDMyIDBjLTQyIDAtNzcuNjcgMTQuNjctMTA3IDQ0cy00NCA2NS00NCAxMDdjMCA0MS4zMyAxNC42NyA3Ni44MyA0NCAxMDYuNVMzOTAgMzAyIDQzMiAzMDJ6bTI3NiAyODJjNDIgMCA3Ny42Ny0xNC44MyAxMDctNDQuNXM0NC02NS4xNyA0NC0xMDYuNWMwLTQyLTE0LjY3LTc3LjY3LTQ0LTEwN3MtNjUtNDQtMTA3LTQ0Yy00MS4zMyAwLTc2LjY3IDE0LjY3LTEwNiA0NHMtNDQgNjUtNDQgMTA3YzAgNDEuMzMgMTQuNjcgNzYuODMgNDQgMTA2LjVTNjY2LjY3IDU4NCA3MDggNTg0em0tNTU3IDBjNDIgMCA3Ny42Ny0xNC44MyAxMDctNDQuNXM0NC02NS4xNyA0NC0xMDYuNWMwLTQyLTE0LjY3LTc3LjY3LTQ0LTEwN3MtNjUtNDQtMTA3LTQ0Yy00MS4zMyAwLTc2LjgzIDE0LjY3LTEwNi41IDQ0UzAgMzkxIDAgNDMzYzAgNDEuMzMgMTQuODMgNzYuODMgNDQuNSAxMDYuNVMxMDkuNjcgNTg0IDE1MSA1ODR6IiBmaWxsPSIjZmZmIi8%2BPC9zdmc%2B)](https://aldaviva.testspace.com/spaces/326067) [![Coveralls](https://img.shields.io/coveralls/github/Aldaviva/InoreaderCs?logo=coveralls)](https://coveralls.io/github/Aldaviva/InoreaderCs?branch=master)

*.NET client for the [Inoreader HTTP API](https://www.inoreader.com/developers/)*

<!-- MarkdownTOC autolink="true" bracket="round" autoanchor="false" levels="1,2,3" bullets="-" -->

- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Features](#features)
- [Configuration](#configuration)
    - [Authentication](#authentication)
    - [API Client](#api-client)
- [Usage](#usage)
    - [List articles](#list-articles)
    - [Check if there are new articles](#check-if-there-are-new-articles)
    - [Mark an article read or unread](#mark-an-article-read-or-unread)
    - [Star or unstar an article](#star-or-unstar-an-article)
    - [Tag or untag an article](#tag-or-untag-an-article)
    - [Subscribe to a feed](#subscribe-to-a-feed)
    - [Show the number of unread articles](#show-the-number-of-unread-articles)
    - [List subscriptions, folders, or tags](#list-subscriptions-folders-or-tags)
    - [Rename subscriptions, folders, or tags](#rename-subscriptions-folders-or-tags)
    - [Organize subscriptions into folders](#organize-subscriptions-into-folders)
    - [Delete subscriptions, folders, or tags](#delete-subscriptions-folders-or-tags)
    - [Mark all articles as read](#mark-all-articles-as-read)
    - [Get self user](#get-self-user)

<!-- /MarkdownTOC -->

## Prerequisites
- Any .NET runtime that is compatible with [.NET Standard 2.0](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-0)
    - .NET 5 or later
    - .NET Core 2.0 or later
    - .NET Framework 4.6.2 or later
- [Inoreader account](https://www.inoreader.com/signup)

## Installation

```ps1
dotnet add package InoreaderCs
```

## Features

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

## Configuration

### Authentication

#### Persisting auth tokens
Implement the `IAuthTokenPersister` interface so that auth tokens can be saved and loaded.

```cs
public class MyAuthTokenPersister: IAuthTokenPersister {

    public async Task<PersistedAuthTokens?> LoadAuthTokens() {
        // load auth tokens
    }

    public async Task SaveAuthTokens(PersistedAuthTokens authToken) {
        // save auth tokens
    }

}
```

#### Authentication client
If your app authenticates to Inoreader using OAuth2, subclass the `OAuth2Client` abstract class so that users can see and grant consent for OAuth2 app access to their account.

```cs
public class MyOauth2Client(Oauth2Parameters oauthParameters, IAuthTokenPersister authTokenPersister, IHttpClient? httpClient, ILoggerFactory? loggerFactory)
    : Oauth2Client(oauthParameters, authTokenPersister, httpClient, loggerFactory) {

    protected override Uri AuthorizationReceiverCallbackUrl => /* URL of your web server's OAuth2 callback resource */;

    protected override async Task<ConsentResult> ShowConsentPageToUser(Uri consentUri, Uri codeReceiverUri, Task authorizationSuccess) {
        // show consent page to user
    }

}
```

Otherwise, construct a `PasswordAuthClient` instance.

### API Client
Create a new instance of `InoreaderClient`. It uses one instance of an `IAuthClient`, so if you have multiple auth strategies, construct additional `InoreaderClient` instances.

```cs
Oauth2Parameters oauthParameters = new(appId: 123, appKey: "abc");

using IInoreaderClient inoreader = new InoreaderClient(new InoreaderOptions {
    AuthClient = new MyOauth2Client(oauthParameters, new MyAuthTokenPersister())
});
```

## Usage

### List articles
```cs
DetailedArticles articleList = await inoreader.Newsfeed.ListArticlesDetailed();
foreach (Article article in articleList.Articles) {
    Console.WriteLine(article.Title);
}
```

Useful options:

- To only fetch articles from a specific folder, tag, or subscription/feed, replace `Newsfeed` (which refers to all articles in the entire account) with `Folders`, `Tags`, or `Subscriptions`, respectively.
- To change the page size, pass the `maxArticles` parameter.
- To only return articles crawled after a certain time, pass the `minTime` parameter.
- To remove or require articles to have a specific state like read or starred, pass the `subtract` or `intersect` parameters, respectively.
- To fetch subsequent pages from a multi-page response, pass the previous page or its `PaginationToken` as the `pagination` parameter.
> [!CAUTION]
> Even when the `pagination` parameter is sent properly, the Inoreader API servers sometimes ignore it and incorrectly return the first page again instead of the desired page. This makes it look like many of the articles are duplicates. After fetching multiple pages of articles, always filter by the unique `Article.ShortId`  to remove the erroneous duplicates, for example, using [`IEnumerable<T>.Distinct`](https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.distinct).

### Check if there are new articles
```cs
DateTimeOffset mostRecentArticleTime = default;
while (true) {
    BriefArticles mostRecentArticle = await inoreader.Newsfeed.ListArticlesBrief(maxArticles: 1, minTime: mostRecentArticleTime);

    if (mostRecentArticle.Articles[0].CrawlTime is var articleTime && articleTime != mostRecentArticleTime) {
        mostRecentArticleTime = articleTime;
        Console.WriteLine("New article received");
    }
    await Task.Delay(TimeSpan.FromMinutes(10));
}
```

### Mark an article read or unread
```cs
await inoreader.Articles.MarkArticles(ArticleState.Read, [article]);
```
```cs
await inoreader.Articles.UnmarkArticles(ArticleState.Read, [article]);
```

### Star or unstar an article
```cs
await inoreader.Articles.MarkArticles(ArticleState.Starred, [article]);
```
```cs
await inoreader.Articles.UnmarkArticles(ArticleState.Starred, [article]);
```

### Tag or untag an article
```cs
await inoreader.Articles.TagArticles("my tag", [article]);
```
```cs
await inoreader.Articles.UntagArticles("my tag", [article]);
```

### Subscribe to a feed
```cs
Uri feedUrl = new("https://arstechnica.com/science/feed/");
SubscriptionCreationResult subscription = await inoreader.Subscriptions.Subscribe(feedUrl, CancellationToken.None);

// alternative that immediately sets name or folder
await inoreader.Subscriptions.Subscribe(feed, title: "Science", folder: "Technology");
```

### Show the number of unread articles
```cs
NewsfeedUnreadCounts unreadResponse = await inoreader.Newsfeed.GetUnreadCounts();
int unreadCount = unreadResponse.AllArticles.UnreadCount;
string unreadLabel = $"{unreadCount:N0}{(unreadCount == unreadResponse.MaxDisplayableCount ? "+" : "")}";
```

### List subscriptions, folders, or tags
```cs
IEnumerable<Subscription> subscriptions = await inoreader.Subscriptions.List();
IEnumerable<FolderState> folders = await inoreader.Folders.List();
IEnumerable<TagState> tags = await inoreader.Tags.List();
```

### Rename subscriptions, folders, or tags
```cs
Uri subscription = new("https://arstechnica.com/science/feed/");
await inoreader.Subscriptions.Rename(subscription, newName: "Science!");
await inoreader.Folders.Rename("Technology", newName: "New Technology");
await inoreader.Tags.Rename("My tag", newName: "My new tag");
```

### Organize subscriptions into folders
```cs
Uri subscription = new("https://arstechnica.com/science/feed/");
await inoreader.Subscriptions.AddToFolder(subscription, "Technology");
await inoreader.Subscriptions.RemoveFromFolder(subscription, "Technology");
```

### Delete subscriptions, folders, or tags
```cs
await inoreader.Subscriptions.Unsubscribe(new Uri("https://arstechnica.com/science/feed/"));
await inoreader.Folders.Delete("Technology");
await inoreader.Tags.Delete("My tag");
```

### Mark all articles as read
```cs
Article latestArticle;
await inoreader.Newsfeed.MarkAllArticlesAsRead(latestArticle.CrawlTime);
await inoreader.Folders.MarkAllArticlesAsRead("Technology", latestArticle.CrawlTime);
await inoreader.Tags.MarkAllArticlesAsRead("My tag", latestArticle.CrawlTime);
await inoreader.Subscriptions.MarkAllArticlesAsRead(new Uri("https://arstechnica.com/science/feed/"), latestArticle.CrawlTime);
```

### Get self user
```cs
User self = await inoreader.Users.GetSelf();
```
