using api.Application.Abstractions;
using api.Application.Commands;
using api.Domain.Entities;
using api.Domain.Ports;

namespace api.Application.Handlers;

public class JoinTeamHandler : IRequestHandler<JoinTeamCommand, Team>
{
    private readonly ITeamRepository _teamRepository;

    public JoinTeamHandler(ITeamRepository teamRepository)
    {
        _teamRepository = teamRepository;
    }

    public async Task<Team> Handle(JoinTeamCommand request, CancellationToken cancellationToken = default)
    {
        // Find team by invite token
        var team = await _teamRepository.GetByInviteTokenAsync(request.InviteToken);

        if (team == null)
            throw new InvalidOperationException("Invalid or expired invite token");

        // Validate token is still valid
        if (!team.IsTokenValid())
            throw new InvalidOperationException("Invite token has expired");

        // Check if user is already a member
        if (team.HasMember(request.UserId))
            throw new InvalidOperationException("User is already a member of this team");

        // Add user as pending member
        var newMember = new TeamMember
        {
            UserId = request.UserId,
            Name = request.UserName,
            Email = request.UserEmail,
            Nickname = request.Nickname,
            Status = "pending",
            Role = "Member"
        };

        team.Members.Add(newMember);
        team.UpdatedAt = DateTime.UtcNow;

        return await _teamRepository.UpdateAsync(team);
    }
}
