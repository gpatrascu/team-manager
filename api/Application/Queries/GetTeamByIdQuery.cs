using api.Application.Abstractions;
using api.Domain.Entities;

namespace api.Application.Queries;

public record GetTeamByIdQuery(
    string TeamId,
    string UserId
) : IRequest<Team?>;
