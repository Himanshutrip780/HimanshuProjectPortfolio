using System.ComponentModel.DataAnnotations;

namespace CommentApi.Model.Dto
{
    public class CreateCommentRequestDto
    {
        [Required]
        [MaxLength(4000)]
        public string Body { get; set; } = string.Empty;

        public Guid? ParentCommentId { get; set; }

        public IEnumerable<Guid> MentionedUserIds { get; set; } =
            new List<Guid>();
    }
}
