namespace CommentApi.Model.Domain.Entities
{
    public class Comment
    {
        public Guid CommentId { get; private set; }

        public Guid TaskId { get; private set; }

        public Guid AuthorId { get; private set; }

        public Guid? ParentCommentId { get; private set; }

        public string Body { get; private set; } = string.Empty;

        public bool IsDeleted { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }

        public ICollection<CommentReaction> Reactions { get; private set; } = new List<CommentReaction>();

        private Comment()
        {
        }

        public Comment(
            Guid taskId,
            Guid authorId,
            string body,
            Guid? parentCommentId)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                throw new ArgumentException(
                    "Comment body is required");
            }

            CommentId = Guid.NewGuid();

            TaskId = taskId;

            AuthorId = authorId;

            ParentCommentId = parentCommentId;

            Body = body.Trim();

            CreatedAt = DateTime.UtcNow;

            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                throw new ArgumentException(
                    "Comment body is required");
            }

            Body = body.Trim();

            UpdatedAt = DateTime.UtcNow;
        }

        public void SoftDelete()
        {
            IsDeleted = true;

            UpdatedAt = DateTime.UtcNow;
        }
    }
}
