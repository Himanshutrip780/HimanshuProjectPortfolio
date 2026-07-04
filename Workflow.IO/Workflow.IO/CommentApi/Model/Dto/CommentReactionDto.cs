using System;

namespace CommentApi.Model.Dto
{
    public class CommentReactionDto
    {
        public Guid CommentReactionId { get; set; }
        public Guid CommentId { get; set; }
        public Guid UserId { get; set; }
        public string Emoji { get; set; } = string.Empty;
    }
}
