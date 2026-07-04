namespace FileApi.Model.Domain.Entities
{
    public class FileAttachment
    {
        public Guid FileAttachmentId { get; private set; }

        public Guid TaskId { get; private set; }

        public Guid UploadedById { get; private set; }

        public string OriginalFileName { get; private set; } = string.Empty;

        public string StoredFileName { get; private set; } = string.Empty;

        public string ContentType { get; private set; } = string.Empty;

        public long SizeInBytes { get; private set; }

        public string StoragePath { get; private set; } = string.Empty;

        public bool IsDeleted { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }

        private FileAttachment()
        {
        }

        public FileAttachment(
            Guid taskId,
            Guid uploadedById,
            string originalFileName,
            string storedFileName,
            string contentType,
            long sizeInBytes,
            string storagePath)
        {
            FileAttachmentId = Guid.NewGuid();

            TaskId = taskId;

            UploadedById = uploadedById;

            OriginalFileName = originalFileName;

            StoredFileName = storedFileName;

            ContentType = contentType;

            SizeInBytes = sizeInBytes;

            StoragePath = storagePath;

            CreatedAt = DateTime.UtcNow;

            UpdatedAt = DateTime.UtcNow;
        }

        public void SoftDelete()
        {
            IsDeleted = true;

            UpdatedAt = DateTime.UtcNow;
        }
    }
}
