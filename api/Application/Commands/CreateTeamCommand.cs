using api.Application.Abstractions;
using api.Domain.Entities;

namespace api.Application.Commands;

public record CreateTeamCommand(
    string Name,
    string? Description,
    string CreatorUserId
) : IRequest<Team>;
