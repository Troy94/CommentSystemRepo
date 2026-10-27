using System;
using System.Collections.Generic;
using System.Text;

namespace CommentSystem.Core.Interfaces
{
    public interface IUnitOfWork
    {
        ICommentRepository Comments { get; }
        Task<int> CompleteAsync();
    }

}
