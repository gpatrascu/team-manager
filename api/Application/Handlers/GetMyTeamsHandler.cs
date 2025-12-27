using api.Application.Abstractions;
using api.Application.Queries;
using api.Domain.Entities;
using api.Domain.Ports;

namespace api.Application.Handlers;

public class GetMyTeamsHandler : IRequestHandler<GetMyTeamsQuery, IEnumerable<Team>>
{
    private readonly ITeamRepository _teamRepository;

    public GetMyTeamsHandler(ITeamRepository teamRepository)
    {
        _teamRepository = teamRepository;
    }

    public async Task<IEnumerable<Team>> Handle(GetMyTeamsQuery request, CancellationToken cancellationToken = default)
    {
        return await _teamRepository.GetByUserIdAsync(request.UserId);
    }
}
