using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ATS.Application.Common.Interfaces;
using ATS.Domain.Entities;
using ATS.Shared.Models;

namespace ATS.Application.Features.CandidateNotes
{
    public record CreateCandidateNoteCommand : IRequest<Result<Guid>>
    {
        public Guid CandidateId { get; init; }
        public Guid? ApplicationId { get; init; }
        public string Text { get; init; }
        public string AuthorName { get; init; }
    }

    public class CreateCandidateNoteCommandHandler : IRequestHandler<CreateCandidateNoteCommand, Result<Guid>>
    {
        private readonly IApplicationDbContext _context;

        public CreateCandidateNoteCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<Guid>> Handle(CreateCandidateNoteCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
            {
                return Result<Guid>.Failure("Note text cannot be empty.");
            }

            var candidate = await _context.Candidates.FirstOrDefaultAsync(x => x.Id == request.CandidateId, cancellationToken);
            if (candidate == null)
            {
                return Result<Guid>.Failure("Candidate not found.");
            }

            if (request.ApplicationId.HasValue)
            {
                var app = await _context.Applications.FirstOrDefaultAsync(x => x.Id == request.ApplicationId.Value, cancellationToken);
                if (app == null)
                {
                    return Result<Guid>.Failure("Application not found.");
                }
            }

            var note = new CandidateNote
            {
                CandidateId = request.CandidateId,
                ApplicationId = request.ApplicationId,
                Text = request.Text,
                AuthorName = request.AuthorName
            };

            await _context.CandidateNotes.AddAsync(note, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(note.Id);
        }
    }

    public record DeleteCandidateNoteCommand(Guid Id) : IRequest<Result>;

    public class DeleteCandidateNoteCommandHandler : IRequestHandler<DeleteCandidateNoteCommand, Result>
    {
        private readonly IApplicationDbContext _context;

        public DeleteCandidateNoteCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(DeleteCandidateNoteCommand request, CancellationToken cancellationToken)
        {
            var note = await _context.CandidateNotes.Include(n => n.Candidate).FirstOrDefaultAsync(n => n.Id == request.Id, cancellationToken);
            if (note == null || note.Candidate == null)
            {
                return Result.Failure("Note not found.");
            }

            _context.CandidateNotes.Remove(note);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
