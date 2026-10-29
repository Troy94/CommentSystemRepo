using CommentSystem.Core.Entities;
using CommentSystem.Core.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace CommentSystem.Presentation.Hubs
{
    public class CommentHub : Hub
    {
        private readonly IUnitOfWork _unitOfWork;

        public CommentHub(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task SendComment(string text, string userName, string email = "")
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            var comment = new Comment
            {
                UserName = string.IsNullOrWhiteSpace(userName) ? "Anonymous" : userName,
                Email = string.IsNullOrWhiteSpace(email) ? "anonymous@example.com" : email,
                Text = text,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Comments.AddAsync(comment);
            await _unitOfWork.CompleteAsync();

            await Clients.All.SendAsync("ReceiveComment", comment);
        }

        public async Task<IEnumerable<Comment>> GetComments()
        {
            return await _unitOfWork.Comments.GetAllAsync();
        }

        public async Task UpdateComment(int commentId, string newText)
        {
            var comment = await _unitOfWork.Comments.GetByIdAsync(commentId);
            if (comment != null)
            {
                comment.Text = newText;
                _unitOfWork.Comments.Update(comment);
                await _unitOfWork.CompleteAsync();

                await Clients.All.SendAsync("CommentUpdated", comment);
            }
        }

        public async Task DeleteComment(int commentId)
        {
            var comment = await _unitOfWork.Comments.GetByIdAsync(commentId);
            if (comment != null)
            {
                _unitOfWork.Comments.Delete(comment);
                await _unitOfWork.CompleteAsync();

                await Clients.All.SendAsync("CommentDeleted", commentId);
            }
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            await Clients.Caller.SendAsync("ConnectionEstablished", new
            {
                message = "Connected to CommentHub",
                connectionId = Context.ConnectionId
            });
        }
    }
}
