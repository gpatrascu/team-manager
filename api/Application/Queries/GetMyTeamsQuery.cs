using api.Application.Abstractions;
using api.Domain.Entities;

namespace api.Application.Queries;

public record GetMyTeamsQuery(
    string UserId
) : IRequest<IEnumerable<Team>>;
