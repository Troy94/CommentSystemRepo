using CommentSystem.Core.Interfaces;
using CommentSystem.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommentSystem.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CommentDbContext _context;
        public ICommentRepository Comments { get; }

        public UnitOfWork(CommentDbContext context)
        {
            _context = context;
            Comments = new CommentRepository(_context);
        }

        public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();
    }
}
