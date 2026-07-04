using System;

namespace CommentApi.Model.Domain.Entities
{
    public class CommentReaction
    {
        public Guid CommentReactionId { get; private set; }
        public Guid CommentId { get; private set; }
        public Guid UserId { get; private set; }
        public string Emoji { get; private set; } = string.Empty;

        private CommentReaction()
        {
        }

        public CommentReaction(Guid commentId, Guid userId, string emoji)
        {
            if (string.IsNullOrWhiteSpace(emoji))
            {
                throw new ArgumentException("Emoji is required");
            }

            CommentReactionId = Guid.NewGuid();
            CommentId = commentId;
            UserId = userId;
            Emoji = emoji.Trim();
        }
    }
}
