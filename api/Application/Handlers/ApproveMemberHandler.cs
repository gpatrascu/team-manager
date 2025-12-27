using api.Application.Abstractions;
using api.Application.Commands;
using api.Domain.Entities;
using api.Domain.Ports;

namespace api.Application.Handlers;

public class ApproveMemberHandler : IRequestHandler<ApproveMemberCommand, Team>
{
    private readonly ITeamRepository _teamRepository;

    public ApproveMemberHandler(ITeamRepository teamRepository)
    {
        _teamRepository = teamRepository;
    }

    public async Task<Team> Handle(ApproveMemberCommand request, CancellationToken cancellationToken = default)
    {
        // Get team
        var team = await _teamRepository.GetByIdAsync(request.TeamId);

        if (team == null)
            throw new InvalidOperationException("Team not found");

        // Check if user is admin
        if (!team.IsUserAdmin(request.ApproverUserId))
            throw new UnauthorizedAccessException("Only admins can approve members");

        // Find member
        var member = team.GetMember(request.MemberId);

        if (member == null)
            throw new InvalidOperationException("Member not found");

        // Approve member (domain method)
        member.Approve(request.ApproverUserId);

        team.UpdatedAt = DateTime.UtcNow;

        return await _teamRepository.UpdateAsync(team);
    }
}
