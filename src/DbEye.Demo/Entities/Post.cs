using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DbEye.Demo.Entities
{
    public class Post
    {
        // Constructor
        public Post(string title, string content)
        {
            Id = Guid.NewGuid();
            Title = title;
            Content = content;
            CreatedAt = DateTime.UtcNow;
        }

        // Properties
        public Guid Id { get; init; }
        public string Title { get; private set; } = string.Empty;
        public string Content { get; private set; } = string.Empty;
        public DateTime CreatedAt { get; init; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        // Methods
        public void ChangeTitle(string title)
        => Title = title;

        public void ChangeContent(string content)
        => Content = content;
    }
}