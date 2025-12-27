using api.Application.Abstractions;
using api.Application.Queries;
using api.Domain.Entities;
using api.Domain.Ports;

namespace api.Application.Handlers;

public class GetPendingMembersHandler : IRequestHandler<GetPendingMembersQuery, IEnumerable<TeamMember>>
{
    private readonly ITeamRepository _teamRepository;

    public GetPendingMembersHandler(ITeamRepository teamRepository)
    {
        _teamRepository = teamRepository;
    }

    public async Task<IEnumerable<TeamMember>> Handle(GetPendingMembersQuery request, CancellationToken cancellationToken = default)
    {
        var team = await _teamRepository.GetByIdAsync(request.TeamId);

        if (team == null)
            throw new InvalidOperationException("Team not found");

        // Check if user is admin
        if (!team.IsUserAdmin(request.AdminUserId))
            throw new UnauthorizedAccessException("Only admins can view pending members");

        // Return pending members using domain method
        return team.GetPendingMembers();
    }
}
