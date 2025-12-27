using api.Application.Abstractions;
using api.Application.Queries;
using api.Domain.Entities;
using api.Domain.Ports;

namespace api.Application.Handlers;

public class GetTeamByIdHandler : IRequestHandler<GetTeamByIdQuery, Team?>
{
    private readonly ITeamRepository _teamRepository;

    public GetTeamByIdHandler(ITeamRepository teamRepository)
    {
        _teamRepository = teamRepository;
    }

    public async Task<Team?> Handle(GetTeamByIdQuery request, CancellationToken cancellationToken = default)
    {
        var team = await _teamRepository.GetByIdAsync(request.TeamId);

        // Verify user has access (is admin or active member)
        if (team != null)
        {
            bool isAdmin = team.IsUserAdmin(request.UserId);
            bool isActiveMember = team.Members.Any(m => m.UserId == request.UserId && m.IsActive());
            bool isPendingMember = team.Members.Any(m => m.UserId == request.UserId && m.IsPending());

            if (!isAdmin && !isActiveMember && !isPendingMember)
                return null; // User has no access to this team
        }

        return team;
    }
}
