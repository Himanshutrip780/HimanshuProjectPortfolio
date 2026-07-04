namespace CommentApi.Model.Dto
{
    public class CommentResponseDto
    {
        public Guid CommentId { get; set; }

        public Guid TaskId { get; set; }

        public Guid AuthorId { get; set; }

        public Guid? ParentCommentId { get; set; }

        public string Body { get; set; } = string.Empty;

        public IEnumerable<Guid> MentionedUserIds { get; set; } =
            new List<Guid>();

        public bool IsDeleted { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public IEnumerable<CommentReactionDto> Reactions { get; set; } = new List<CommentReactionDto>();
    }
}
