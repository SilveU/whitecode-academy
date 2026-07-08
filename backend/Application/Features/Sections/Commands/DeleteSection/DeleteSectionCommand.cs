using Application.Common;
using MediatR;

namespace Application.Features.Sections.Commands.DeleteSection
{
    public record DeleteSectionCommand(Guid Id, string CurrentUserId, bool IsInstructor) : IRequest<Result<bool>>;
}
