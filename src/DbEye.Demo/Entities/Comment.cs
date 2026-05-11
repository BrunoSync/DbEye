using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DbEye.Demo.Entities
{
    public class Comment
    {
        // Constructor
        public Comment(Guid postId, string content)
        {
            Id = Guid.NewGuid();
            PostId = postId;
            Content = content;
            CreatedAt = DateTime.UtcNow;
        }

        // Properties
        public Guid Id { get; init; }
        public Guid PostId { get; init; }
        public string Content { get; private set; } = string.Empty;
        public DateTime CreatedAt { get; init; }

        // Methods 
        public void ChangeContent(string content)
        => Content = content;
    }
}