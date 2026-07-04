using System.ComponentModel.DataAnnotations;

namespace CommentApi.Model.Dto
{
    public class UpdateCommentRequestDto
    {
        [Required]
        [MaxLength(4000)]
        public string Body { get; set; } = string.Empty;

        public IEnumerable<Guid> MentionedUserIds { get; set; } =
            new List<Guid>();
    }
}
