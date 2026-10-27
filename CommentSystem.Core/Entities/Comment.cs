using System;
using System.Collections.Generic;
using System.Text;

namespace CommentSystem.Core.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
