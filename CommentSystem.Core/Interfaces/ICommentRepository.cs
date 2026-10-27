using CommentSystem.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommentSystem.Core.Interfaces
{
    public interface ICommentRepository
    {
        Task<IEnumerable<Comment>> GetAllAsync();
        Task<Comment?> GetByIdAsync(int id);
        Task AddAsync(Comment comment);
        void Update(Comment comment);
        void Delete(Comment comment);
    }

}
