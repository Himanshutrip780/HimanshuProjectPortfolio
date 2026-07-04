using System;

namespace ATS.Application.Features.CandidateNotes
{
    public class CandidateNoteDto
    {
        public Guid Id { get; set; }
        public Guid CandidateId { get; set; }
        public Guid? ApplicationId { get; set; }
        public string Text { get; set; }
        public string AuthorName { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
