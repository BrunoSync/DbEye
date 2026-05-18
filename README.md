# DbEye

[![NuGet](https://img.shields.io/nuget/v/DbEye.svg)](https://www.nuget.org/packages/DbEye)
[![Downloads](https://img.shields.io/nuget/dt/DbEye.svg)](https://www.nuget.org/packages/DbEye)

**DbEye** is a lightweight middleware for ASP.NET Core that detects **N+1 query problems** and **slow queries** in Entity Framework Core applications — right in your development logs, per HTTP request.

> ⚠️ DbEye throws on startup if the current environment is not in the `AllowedEnvironments` list. By default, only `Development` is allowed.

## Installation

```bash
dotnet add package DbEye
```

## Setup

Add DbEye to your `Program.cs`:

```csharp
builder.Services.AddDbEye();
builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    options.AddInterceptors(serviceProvider.GetRequiredService<DbEyeInterceptor>());
});

app.UseDbEye();
```

## Configuration

By default, queries taking longer than **500ms** are flagged as slow, and **2 or more identical queries** per request are flagged as N+1. You can customize all thresholds globally or per endpoint:

```csharp
builder.Services.AddDbEye(options =>
{
    options.SlowQueryThresholdMs = 350;
    options.EndpointThresholds = new Dictionary<string, int>
    {
        { "/api/reports", 2000 },
        { "/api/comments", 200 }
    };
    options.NPlus1Threshold = 3;
    options.EndpointNPlus1Thresholds = new Dictionary<string, int>
    {
        { "/api/posts", 5 }
    };
    options.AllowedEnvironments = ["Development", "Staging"];
    options.ExcludeEndpoints("api/posts", "api/comments");
    options.ExcludeScalar();
    options.ExcludeSwagger();
});
```

Endpoints not listed in `EndpointThresholds` or `EndpointNPlus1Thresholds` fall back to the global threshold.

## How it works

DbEye hooks into EF Core via an interceptor and analyzes query patterns on each request. When a problem is detected, a warning is emitted through ASP.NET Core's standard logging pipeline — it shows up directly in your terminal or VS Code output panel. When everything looks healthy, it logs a confirmation instead.

- **N+1 detection** — identifies when multiple identical queries are fired in a loop that could be resolved with a single join or `Include()`
- **Slow query detection** — flags queries that exceed the configured threshold
- **Environment-aware** — throws on startup if the current environment is not in `AllowedEnvironments`, zero overhead in production

No external dashboard, no extra dependencies to run — just clear warnings where you already look.

## Example output

**N+1 detected:**
```
warn: DbEye[0]
      --------------------------------------------------
      ⚠️  N+1 detected at GET /api/posts
      Query repeated 5x - SELECT * FROM "Comments" WHERE "PostId" = ...
      --------------------------------------------------
```

**Slow query detected:**
```
warn: DbEye[0]
      --------------------------------------------------
      ⚠️  Slow query detected at GET /api/comments
      Duration: 732ms - SELECT * FROM "Comments"
      --------------------------------------------------
```

**No issues:**
```
info: DbEye[0]
      --------------------------------------------------
      ✅  GET /api/posts - no issues detected
      --------------------------------------------------
```

## Try it out

The repository includes a demo project with a pre-configured Postgres database. Just clone and run:

```bash
git clone https://github.com/BrunoSync/DbEye
cd DbEye
docker compose up --build
```

The API will be available at `http://localhost:5000`. Watch the warnings in real time:

```bash
docker compose logs -f api
```

### Triggering N+1

`GET /api/posts` without the `include` parameter fires one query per post to load comments:

```bash
curl http://localhost:5000/api/posts
```

To fix it, pass `?include=true` — DbEye will stay silent:

```bash
curl http://localhost:5000/api/posts?include=true
```

### Triggering a slow query

`GET /api/comments` with `?delay=true` simulates a slow query:

```bash
curl http://localhost:5000/api/comments?delay=true
```

### Other endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/posts` | List posts (`?include=true` to avoid N+1) |
| `GET` | `/api/posts/{id}` | Get post by id |
| `POST` | `/api/posts` | Create post |
| `PUT` | `/api/posts/{id}` | Update post |
| `DELETE` | `/api/posts/{id}` | Delete post |
| `GET` | `/api/comments` | List comments (`?delay=true` to simulate slow query) |
| `GET` | `/api/comments/{id}` | Get comment by id |
| `POST` | `/api/comments` | Create comment |
| `PUT` | `/api/comments/{id}` | Update comment |
| `DELETE` | `/api/comments/{id}` | Delete comment |

## Supported databases

| Database | Provider |
|----------|----------|
| PostgreSQL | Npgsql.EntityFrameworkCore.PostgreSQL |
| SQL Server | Microsoft.EntityFrameworkCore.SqlServer |
| SQLite | Microsoft.EntityFrameworkCore.Sqlite |

## Supported frameworks

| Target  | EF Core |
|---------|---------|
| .NET 8  | 8.x     |
| .NET 9  | 9.x     |
| .NET 10 | 10.x    |

## Support

If DbEye saved you from a N+1 in production, consider buying me a coffee ☕

[![Buy Me A Coffee](https://img.shields.io/badge/Buy%20Me%20A%20Coffee-support-yellow?logo=buy-me-a-coffee)](https://buymeacoffee.com/brunosync)

## License

MIT