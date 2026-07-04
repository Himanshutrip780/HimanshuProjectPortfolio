namespace UserApi.Model.Domain.Common
{
    public abstract class BaseAuditableEntity
    {
        public DateTime CreatedAt { get; protected set; }
        public DateTime UpdatedAt { get; protected set; }
        public bool IsDeleted { get; protected set; }
        protected BaseAuditableEntity()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            IsDeleted = false;
        }
        public void MarkAsUpdated()
        {
            UpdatedAt = DateTime.UtcNow;
        }
        public void SoftDelete()
        {
            IsDeleted = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}