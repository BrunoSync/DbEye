using DbEye.Common.Extensions;
using DbEye.Core.Interceptor;
using DbEye.Demo.Data;
using DbEye.Demo.Entities;
using DbEye.Demo.Seeder;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbEye();
builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.AddInterceptors(serviceProvider.GetRequiredService<DbEyeInterceptor>());
});

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseDbEye();

// Routes

app.MapGet("/api/posts", async (bool? include, AppDbContext context) =>
{
    if (include is true)
    {
        var query = await context.Posts
                        .AsNoTracking()
                        .Include(c => c.Comments)
                        .ToListAsync();

        return query;
    }
    else
    {
        var posts = context.Posts.ToList();
        foreach (var post in posts)
        {
            var comments = await context.Comments
                .Where(c => c.PostId == post.Id)
                .ToListAsync();
        }

        return posts;
    }

});

app.MapGet("/api/posts/{id}", async (Guid id, AppDbContext context) =>
{
    var post = await context.Posts
        .AsNoTracking()
        .FirstOrDefaultAsync(p => p.Id == id);

    return post is null ? Results.NotFound() : Results.Ok(post);
});

app.MapPost("/api/posts", async (Post post, AppDbContext context) =>
{
    context.Posts.Add(post);
    await context.SaveChangesAsync();
    return Results.Created($"/api/posts/{post.Id}", post);
});

app.MapPut("/api/posts/{id}", async (Guid id, Post updated, AppDbContext context) =>
{
    var post = await context.Posts.FindAsync(id);
    if (post is null) return Results.NotFound();

    post.ChangeTitle(updated.Title);
    post.ChangeContent(updated.Content);
    await context.SaveChangesAsync();
    return Results.Ok(post);
});

app.MapDelete("/api/posts/{id}", async (Guid id, AppDbContext context) =>
{
    var post = await context.Posts.FindAsync(id);
    if (post is null) return Results.NotFound();

    context.Posts.Remove(post);
    await context.SaveChangesAsync();
    return Results.NoContent();
});

// Seeder
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbEyerSeeder.Seeder(context, CancellationToken.None);
}

app.Run();