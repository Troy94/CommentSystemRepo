using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using CommentSystem.Core.Entities;
using CommentSystem.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CommentSystem.Data.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly CommentDbContext _context;

        public CommentRepository(CommentDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Comment>> GetAllAsync() => await _context.Comments.ToListAsync();
        public async Task<Comment?> GetByIdAsync(int id) => await _context.Comments.FindAsync(id);
        public async Task AddAsync(Comment comment) => await _context.Comments.AddAsync(comment);
        public void Update(Comment comment) => _context.Comments.Update(comment);
        public void Delete(Comment comment) => _context.Comments.Remove(comment);
    }
}
