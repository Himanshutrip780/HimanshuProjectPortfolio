using System.ComponentModel.DataAnnotations;

namespace CommentApi.Model.Dto
{
    public class CommentReactionRequestDto
    {
        [Required]
        public string Emoji { get; set; } = string.Empty;
    }
}
