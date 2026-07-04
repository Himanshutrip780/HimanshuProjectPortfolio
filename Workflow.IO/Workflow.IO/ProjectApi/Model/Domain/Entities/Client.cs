using System;

namespace ProjectApi.Model.Domain.Entities
{
    public class Client
    {
        public Guid ClientId { get; set; }
        public string Name { get; private set; } = string.Empty;
        public string Industry { get; private set; } = string.Empty;
        public string ContactPerson { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string Keywords { get; private set; } = string.Empty; // Comma-separated search terms for auto-classification
        public Guid OrganizationId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private Client()
        {
        }

        public Client(string name, string industry, string contactPerson, string email, string keywords, Guid organizationId)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Client name is required");
            }

            ClientId = Guid.NewGuid();
            Name = name.Trim();
            Industry = string.IsNullOrWhiteSpace(industry) ? "Technology" : industry.Trim();
            ContactPerson = string.IsNullOrWhiteSpace(contactPerson) ? "Contact Person" : contactPerson.Trim();
            Email = string.IsNullOrWhiteSpace(email) ? "info@company.com" : email.Trim();
            Keywords = string.IsNullOrWhiteSpace(keywords) ? name.ToLower().Trim() : keywords.ToLower().Trim();
            OrganizationId = organizationId;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string name, string industry, string contactPerson, string email, string keywords)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Client name is required");
            }

            Name = name.Trim();
            Industry = industry.Trim();
            ContactPerson = contactPerson.Trim();
            Email = email.Trim();
            Keywords = keywords.ToLower().Trim();
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
