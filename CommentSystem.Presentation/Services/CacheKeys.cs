namespace CommentSystem.Presentation.Services
{
    public static class CacheKeys
    {
        public const string AllComments = "comments:all";

        public static string CommentById(int id) => $"comments:{id}";
    }
}
