using api.Application.Abstractions;
using api.Application.Commands;
using api.Domain.Entities;
using api.Domain.Ports;

namespace api.Application.Handlers;

public class RejectMemberHandler : IRequestHandler<RejectMemberCommand, Team>
{
    private readonly ITeamRepository _teamRepository;

    public RejectMemberHandler(ITeamRepository teamRepository)
    {
        _teamRepository = teamRepository;
    }

    public async Task<Team> Handle(RejectMemberCommand request, CancellationToken cancellationToken = default)
    {
        // Get team
        var team = await _teamRepository.GetByIdAsync(request.TeamId);

        if (team == null)
            throw new InvalidOperationException("Team not found");

        // Check if user is admin
        if (!team.IsUserAdmin(request.RejectorUserId))
            throw new UnauthorizedAccessException("Only admins can reject members");

        // Find member
        var member = team.GetMember(request.MemberId);

        if (member == null)
            throw new InvalidOperationException("Member not found");

        // Reject member (domain method)
        member.Reject();

        team.UpdatedAt = DateTime.UtcNow;

        return await _teamRepository.UpdateAsync(team);
    }
}
