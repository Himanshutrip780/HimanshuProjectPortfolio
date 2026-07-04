using TaskApi.Model.Domain.Enums;

namespace TaskApi.Model.Domain.Entities
{
    public class BoardColumn
    {
        public Guid BoardColumnId { get; private set; }

        public Guid BoardId { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public Enums.TaskStatus Status { get; private set; }

        public int SortOrder { get; private set; }

        private BoardColumn()
        {
        }

        public BoardColumn(
            Guid boardId,
            string name,
            Enums.TaskStatus status,
            int sortOrder)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "Board column name is required");
            }

            BoardColumnId = Guid.NewGuid();

            BoardId = boardId;

            Name = name.Trim();

            Status = status;

            SortOrder = sortOrder;
        }
    }
}
