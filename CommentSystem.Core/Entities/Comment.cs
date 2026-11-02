using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommentSystem.Core.Entities
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        [StringLength(128)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [StringLength(320)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(4000)]
        public string Text { get; set; } = string.Empty;

        public int? ParentId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public List<Comment> Replies { get; set; } = new();

        [NotMapped]
        public string? CaptchaToken { get; set; }
    }

}
