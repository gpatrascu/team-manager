using api.Application.Abstractions;
using api.Application.Commands;
using api.Domain.Ports;

namespace api.Application.Handlers;

public class GenerateInviteTokenHandler : IRequestHandler<GenerateInviteTokenCommand, GenerateInviteTokenResult>
{
    private readonly ITeamRepository _teamRepository;
    private readonly ITokenGenerator _tokenGenerator;

    public GenerateInviteTokenHandler(ITeamRepository teamRepository, ITokenGenerator tokenGenerator)
    {
        _teamRepository = teamRepository;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<GenerateInviteTokenResult> Handle(GenerateInviteTokenCommand request, CancellationToken cancellationToken = default)
    {
        // Get team
        var team = await _teamRepository.GetByIdAsync(request.TeamId);

        if (team == null)
            throw new InvalidOperationException("Team not found");

        // Check if user is admin
        if (!team.IsUserAdmin(request.AdminUserId))
            throw new UnauthorizedAccessException("Only admins can generate invite tokens");

        // Generate secure token
        var token = _tokenGenerator.GenerateSecureToken();
        var expiry = DateTime.UtcNow.AddHours(request.ExpiryHours);

        // Update team with new token
        team.InviteToken = token;
        team.InviteTokenExpiry = expiry;
        team.UpdatedAt = DateTime.UtcNow;

        await _teamRepository.UpdateAsync(team);

        return new GenerateInviteTokenResult(token, expiry);
    }
}
