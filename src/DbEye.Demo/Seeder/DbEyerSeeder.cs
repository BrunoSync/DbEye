using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DbEye.Demo.Data;
using DbEye.Demo.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.VisualBasic;

namespace DbEye.Demo.Seeder
{
    public static class DbEyerSeeder
    {
        public static async Task Seeder(AppDbContext context, CancellationToken ct)
        {
            if (await context.Posts.AnyAsync(ct))
                return;

            var count = 0;

            while (count < 100)
            {
                var newPost = new Post(
                    new string('a', 80),
                    new string('b', 200)
                );

                var commentsCount = 0;
                while (commentsCount < 5)
                {
                    var newComment = new Comment(
                        newPost.Id,
                        new string('a', 100)
                    );
                    
                    await context.Comments.AddAsync(newComment, ct);
                    commentsCount += 1;
                }

                await context.Posts.AddAsync(newPost, ct);
                count += 1;
            }
            await context.SaveChangesAsync(ct);
        }
    }
}