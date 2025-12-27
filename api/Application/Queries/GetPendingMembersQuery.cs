using api.Application.Abstractions;
using api.Domain.Entities;

namespace api.Application.Queries;

public record GetPendingMembersQuery(
    string TeamId,
    string AdminUserId
) : IRequest<IEnumerable<TeamMember>>;
