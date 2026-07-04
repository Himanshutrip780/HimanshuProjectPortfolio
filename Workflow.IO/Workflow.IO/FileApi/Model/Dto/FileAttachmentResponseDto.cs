namespace FileApi.Model.Dto
{
    public class FileAttachmentResponseDto
    {
        public Guid FileAttachmentId { get; set; }

        public Guid TaskId { get; set; }

        public Guid UploadedById { get; set; }

        public string OriginalFileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public long SizeInBytes { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
