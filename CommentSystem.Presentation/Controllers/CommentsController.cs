namespace CommentSystem.Presentation.Controllers
{
    using System.ComponentModel.DataAnnotations;
    using System.Text.Encodings.Web;
    using CommentSystem.Core.Entities;
    using CommentSystem.Core.Interfaces;
    using CommentSystem.Presentation.Services;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private const int DefaultPageSize = 25;

        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly ICommentNotificationService _commentNotificationService;
        private readonly ICaptchaVerificationService _captchaVerificationService;
        private readonly ILogger<CommentsController> _logger;

        public CommentsController(IUnitOfWork unitOfWork, ICacheService cacheService, ICommentNotificationService commentNotificationService, ICaptchaVerificationService captchaVerificationService, ILogger<CommentsController> logger)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _commentNotificationService = commentNotificationService;
            _captchaVerificationService = captchaVerificationService;
            _logger = logger;
        }

        [HttpGet]
        [HttpGet("main")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string sortBy = "CreatedAt",
            [FromQuery] string sortDirection = "desc",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = DefaultPageSize)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = DefaultPageSize;

            var normalizedSortBy = NormalizeSort(sortBy);
            var normalizedDirection = NormalizeSortDirection(sortDirection);

            var comments = await GetSanitizedCommentsAsync();
            var topLevelComments = comments
                .Where(comment => comment.ParentId is null)
                .OrderByComment(normalizedSortBy, normalizedDirection)
                .ToList();

            var totalCount = topLevelComments.Count;
            var totalPages = totalCount == 0 ? 1 : (int)Math.Ceiling(totalCount / (double)pageSize);
            if (page > totalPages) page = totalPages;

            var pagedComments = topLevelComments
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            foreach (var comment in pagedComments)
            {
                comment.Replies = BuildReplies(comments, comment.Id);
            }

            return Ok(new
            {
                page,
                pageSize,
                totalCount,
                totalPages,
                sortBy = normalizedSortBy,
                sortDirection = normalizedDirection,
                items = pagedComments
            });
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
                if (comment == null)
                {
                    return NotFound();
                }

                comment = SanitizeComment(comment);
                await _cacheService.SetAsync(cacheKey, comment, TimeSpan.FromMinutes(5));
                return Ok(comment);
            }

            _logger.LogInformation("Comment {CommentId} served from Redis cache.", id);
            return Ok(comment);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Comment comment)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (!await _captchaVerificationService.VerifyAsync(comment.CaptchaToken))
            {
                return BadRequest("Captcha verification failed.");
            }

            if (comment.ParentId.HasValue)
            {
                var parentExists = await _unitOfWork.Comments.GetByIdAsync(comment.ParentId.Value);
                if (parentExists is null)
                {
                    return BadRequest("Parent comment does not exist.");
                }
            }

            var createdComment = new Comment
            {
                UserName = SanitizeText(comment.UserName),
                Email = SanitizeText(comment.Email),
                Text = SanitizeText(comment.Text),
                ParentId = comment.ParentId,
                CreatedAt = DateTime.UtcNow
            };

            if (!IsValidComment(createdComment))
            {
                return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
                {
                    [nameof(Comment)] = new[] { "Comment fields are invalid." }
                }));
            }

            await _unitOfWork.Comments.AddAsync(createdComment);
            await _unitOfWork.CompleteAsync();

            await _cacheService.RemoveAsync(CacheKeys.AllComments);
            await _commentNotificationService.NotifyCommentCreatedAsync(createdComment);
            return CreatedAtAction(nameof(GetById), new { id = createdComment.Id }, createdComment);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Comment comment)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var existing = await _unitOfWork.Comments.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            existing.UserName = SanitizeText(comment.UserName);
            existing.Email = SanitizeText(comment.Email);
            existing.Text = SanitizeText(comment.Text);
            existing.ParentId = comment.ParentId;

            if (!IsValidComment(existing))
            {
                return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
                {
                    [nameof(Comment)] = new[] { "Comment fields are invalid." }
                }));
            }

            _unitOfWork.Comments.Update(existing);
            await _unitOfWork.CompleteAsync();

            await _cacheService.RemoveAsync(CacheKeys.AllComments);
            await _cacheService.RemoveAsync(CacheKeys.CommentById(id));
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var allComments = (await _unitOfWork.Comments.GetAllAsync()).ToList();
            var idsToDelete = GetCommentBranchIds(allComments, id);

            foreach (var commentId in idsToDelete)
            {
                var commentToDelete = allComments.FirstOrDefault(comment => comment.Id == commentId);
                if (commentToDelete is null)
                {
                    continue;
                }

                _unitOfWork.Comments.Delete(commentToDelete);
            }

            await _unitOfWork.CompleteAsync();

            await _cacheService.RemoveAsync(CacheKeys.AllComments);
            await _cacheService.RemoveAsync(CacheKeys.CommentById(id));
            return NoContent();
        }

        private async Task<List<Comment>> GetSanitizedCommentsAsync()
        {
            var comments = await _cacheService.GetAsync<List<Comment>>(CacheKeys.AllComments);
            if (comments is not null)
            {
                _logger.LogInformation("Comments list served from Redis cache.");
                return comments.Select(SanitizeComment).ToList();
            }

            _logger.LogInformation("Comments list not found in Redis cache. Loading from database.");
            comments = (await _unitOfWork.Comments.GetAllAsync()).ToList();
            await _cacheService.SetAsync(CacheKeys.AllComments, comments, TimeSpan.FromMinutes(5));
            return comments.Select(SanitizeComment).ToList();
        }

        private static List<int> GetCommentBranchIds(IEnumerable<Comment> comments, int rootId)
        {
            var result = new List<int>();
            var queue = new Queue<int>();
            queue.Enqueue(rootId);

            while (queue.Count > 0)
            {
                var currentId = queue.Dequeue();
                result.Add(currentId);

                foreach (var child in comments.Where(comment => comment.ParentId == currentId))
                {
                    queue.Enqueue(child.Id);
                }
            }

            return result;
        }

        private static List<Comment> BuildReplies(IEnumerable<Comment> comments, int parentId)
        {
            return comments
                .Where(comment => comment.ParentId == parentId)
                .OrderByDescending(comment => comment.CreatedAt)
                .Select(comment =>
                {
                    var reply = SanitizeComment(comment);
                    reply.Replies = BuildReplies(comments, comment.Id);
                    return reply;
                })
                .ToList();
        }

        private static string NormalizeSort(string sortBy)
        {
            return string.IsNullOrWhiteSpace(sortBy) ? "CreatedAt" : sortBy.Trim();
        }

        private static string NormalizeSortDirection(string sortDirection)
        {
            return string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase) ? "asc" : "desc";
        }

        private static bool IsValidComment(Comment comment)
        {
            if (string.IsNullOrWhiteSpace(comment.UserName) || comment.UserName.Length > 128)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(comment.Email) || comment.Email.Length > 320)
            {
                return false;
            }

            var emailValidator = new EmailAddressAttribute();
            if (!emailValidator.IsValid(comment.Email))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(comment.Text) || comment.Text.Length > 4000)
            {
                return false;
            }

            return true;
        }

        private static string SanitizeText(string? value)
        {
            return HtmlEncoder.Default.Encode(value?.Trim() ?? string.Empty);
        }

        private static Comment SanitizeComment(Comment comment)
        {
            return new Comment
            {
                Id = comment.Id,
                UserName = SanitizeText(comment.UserName),
                Email = SanitizeText(comment.Email),
                Text = SanitizeText(comment.Text),
                ParentId = comment.ParentId,
                CreatedAt = comment.CreatedAt,
                Replies = new List<Comment>()
            };
        }
    }

    internal static class CommentQueryableExtensions
    {
        public static IEnumerable<Comment> OrderByComment(this IEnumerable<Comment> source, string sortBy, string sortDirection)
        {
            return sortBy switch
            {
                "UserName" => sortDirection == "asc"
                    ? source.OrderBy(comment => comment.UserName)
                    : source.OrderByDescending(comment => comment.UserName),
                "Email" => sortDirection == "asc"
                    ? source.OrderBy(comment => comment.Email)
                    : source.OrderByDescending(comment => comment.Email),
                _ => sortDirection == "asc"
                    ? source.OrderBy(comment => comment.CreatedAt)
                    : source.OrderByDescending(comment => comment.CreatedAt)
            };
        }
    }
}
