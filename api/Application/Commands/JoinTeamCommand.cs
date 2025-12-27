using api.Application.Abstractions;
using api.Domain.Entities;

namespace api.Application.Commands;

public record JoinTeamCommand(
    string InviteToken,
    string Nickname,
    string UserId,
    string UserEmail,
    string UserName
) : IRequest<Team>;
