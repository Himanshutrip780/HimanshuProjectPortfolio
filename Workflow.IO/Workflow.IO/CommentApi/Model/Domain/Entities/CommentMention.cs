namespace CommentApi.Model.Domain.Entities
{
    public class CommentMention
    {
        public Guid CommentMentionId { get; private set; }

        public Guid CommentId { get; private set; }

        public Guid MentionedUserId { get; private set; }

        public DateTime CreatedAt { get; private set; }

        private CommentMention()
        {
        }

        public CommentMention(
            Guid commentId,
            Guid mentionedUserId)
        {
            CommentMentionId = Guid.NewGuid();

            CommentId = commentId;

            MentionedUserId = mentionedUserId;

            CreatedAt = DateTime.UtcNow;
        }
    }
}
