# DbEye

[![NuGet](https://img.shields.io/nuget/v/DbEye.svg)](https://www.nuget.org/packages/DbEye)
[![Downloads](https://img.shields.io/nuget/dt/DbEye.svg)](https://www.nuget.org/packages/DbEye)

**DbEye** is a lightweight middleware for ASP.NET Core that detects **N+1 query problems** and **slow queries** in Entity Framework Core applications — right in your development logs, per HTTP request.

> ⚠️ DbEye is intended for **development use only**. Disable or remove it in production environments.

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
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.AddInterceptors(serviceProvider.GetRequiredService<DbEyeInterceptor>());
});

app.UseDbEye();
```

## Configuration

By default, queries taking longer than **500ms** are flagged as slow. You can customize this threshold:

```csharp
builder.Services.AddDbEye(options =>
{
    options.SlowQueryThresholdMs = 200;
});
```

## How it works

DbEye hooks into EF Core via an interceptor and tracks every query fired during a request. Each request gets its own isolated context — queries from different requests never mix. When a problem is detected, a warning is emitted through ASP.NET Core's standard logging pipeline, showing up directly in your terminal or VS Code output panel.

- **N+1 detection** — identifies when the same query is fired multiple times in a single request, a pattern that could be resolved with a single join or `Include()`
- **Slow query detection** — flags queries that exceed `SlowQueryThresholdMs`

No external dashboard, no extra dependencies to run — just clear warnings where you already look.

## Example output

**N+1 detected:**
```
warn: DbEye[0]
      ⚠️  N+1 detected at GET /api/posts
      Query repeated 100x - SELECT * FROM "Comments" WHERE "PostId" = ...
```

**Slow query detected:**
```
warn: DbEye[0]
      ⚠️  Slow query detected at GET /api/comments
      Duration: 732ms - SELECT * FROM "Comments"
```

## Try it out

The repository includes a demo project with a pre-configured Postgres database. Just clone and run:

```bash
git clone https://github.com/BrunoSync/DbEye
cd DbEye
docker compose up
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
