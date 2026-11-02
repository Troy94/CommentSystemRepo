using CommentSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CommentSystem.Data
{
    public class CommentDbContext : DbContext
    {
        public CommentDbContext(DbContextOptions<CommentDbContext> options) : base(options)
        {
        }

        public DbSet<Comment> Comments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.Property(comment => comment.UserName).HasMaxLength(128).IsRequired();
                entity.Property(comment => comment.Email).HasMaxLength(320).IsRequired();
                entity.Property(comment => comment.Text).HasMaxLength(4000).IsRequired();
                entity.HasIndex(comment => comment.ParentId);
                entity.HasIndex(comment => comment.CreatedAt);
                entity.HasIndex(comment => comment.UserName);
                entity.HasIndex(comment => comment.Email);
            });

            modelBuilder.Entity<Comment>().HasData(
                new Comment
                {
                    Id = 1,
                    UserName = "Alice",
                    Email = "alice@example.com",
                    Text = "This is the first test comment",
                    CreatedAt = new DateTime(2026, 10, 1, 9, 0, 0, DateTimeKind.Utc)
                },
                new Comment
                {
                    Id = 2,
                    UserName = "Bob",
                    Email = "bob@example.com",
                    Text = "Huh, there is the second test comment",
                    CreatedAt = new DateTime(2026, 10, 2, 10, 30, 0, DateTimeKind.Utc)
                },
                new Comment
                {
                    Id = 3,
                    UserName = "Charlie",
                    Email = "charlie@example.com",
                    Text = "What a miserable day, the third test comment!",
                    CreatedAt = new DateTime(2026, 10, 3, 12, 15, 0, DateTimeKind.Utc)
                });
        }
    }
}
