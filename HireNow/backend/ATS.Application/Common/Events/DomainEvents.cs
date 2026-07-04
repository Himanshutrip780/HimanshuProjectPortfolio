using System;
using MediatR;

namespace ATS.Application.Common.Events
{
    public record CandidateMovedEvent(Guid ApplicationId, string OldStage, string NewStage, string Actor) : INotification;

    public record InterviewScheduledEvent(Guid InterviewId, string Actor) : INotification;

    public record OfferSignedEvent(Guid OfferId, string Actor) : INotification;
}
