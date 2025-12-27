using api.Application.Abstractions;
using api.Domain.Entities;

namespace api.Application.Commands;

public record RejectMemberCommand(
    string TeamId,
    string MemberId,
    string RejectorUserId
) : IRequest<Team>;
