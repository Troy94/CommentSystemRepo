namespace CommentSystem.Presentation.Controllers
{
    using CommentSystem.Core;
    using CommentSystem.Core.Entities;
    using CommentSystem.Core.Interfaces;
    using CommentSystem.Presentation.Services;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly ICommentNotificationService _commentNotificationService;
        private readonly ILogger<CommentsController> _logger;

        public CommentsController(IUnitOfWork unitOfWork, ICacheService cacheService, ICommentNotificationService commentNotificationService, ILogger<CommentsController> logger)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _commentNotificationService = commentNotificationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var comments = await _cacheService.GetAsync<List<Comment>>(CacheKeys.AllComments);
            if (comments is not null)
            {
                _logger.LogInformation("Comments list served from Redis cache.");
                return Ok(comments);
            }

            _logger.LogInformation("Comments list not found in Redis cache. Loading from database.");
            comments = (await _unitOfWork.Comments.GetAllAsync()).ToList();
            await _cacheService.SetAsync(CacheKeys.AllComments, comments, TimeSpan.FromMinutes(5));

            return Ok(comments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var cacheKey = CacheKeys.CommentById(id);
            var comment = await _cacheService.GetAsync<Comment>(cacheKey);

            if (comment is null)
            {
                _logger.LogInformation("Comment {CommentId} not found in Redis cache. Loading from database.", id);
                comment = await _unitOfWork.Comments.GetByIdAsync(id);
                if (comment == null) return NotFound();

                await _cacheService.SetAsync(cacheKey, comment, TimeSpan.FromMinutes(5));
                return Ok(comment);
            }

            _logger.LogInformation("Comment {CommentId} served from Redis cache.", id);
            return Ok(comment);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Comment comment)
        {
            await _unitOfWork.Comments.AddAsync(comment);
            await _unitOfWork.CompleteAsync();

            await _cacheService.RemoveAsync(CacheKeys.AllComments);
            await _commentNotificationService.NotifyCommentCreatedAsync(comment);
            return CreatedAtAction(nameof(GetById), new { id = comment.Id }, comment);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Comment comment)
        {
            var existing = await _unitOfWork.Comments.GetByIdAsync(id);
            if (existing == null) return NotFound();

            existing.Text = comment.Text;
            _unitOfWork.Comments.Update(existing);
            await _unitOfWork.CompleteAsync();

            await _cacheService.RemoveAsync(CacheKeys.AllComments);
            await _cacheService.RemoveAsync(CacheKeys.CommentById(id));
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _unitOfWork.Comments.GetByIdAsync(id);
            if (existing == null) return NotFound();

            _unitOfWork.Comments.Delete(existing);
            await _unitOfWork.CompleteAsync();

            await _cacheService.RemoveAsync(CacheKeys.AllComments);
            await _cacheService.RemoveAsync(CacheKeys.CommentById(id));
            return NoContent();
        }
    }

}
