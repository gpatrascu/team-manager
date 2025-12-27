using api.Application.Abstractions;
using api.Domain.Entities;

namespace api.Application.Commands;

public record ApproveMemberCommand(
    string TeamId,
    string MemberId,
    string ApproverUserId
) : IRequest<Team>;
