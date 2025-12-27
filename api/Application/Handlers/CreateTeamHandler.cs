using api.Application.Abstractions;
using api.Application.Commands;
using api.Domain.Entities;
using api.Domain.Ports;

namespace api.Application.Handlers;

public class CreateTeamHandler : IRequestHandler<CreateTeamCommand, Team>
{
    private readonly ITeamRepository _teamRepository;

    public CreateTeamHandler(ITeamRepository teamRepository)
    {
        _teamRepository = teamRepository;
    }

    public async Task<Team> Handle(CreateTeamCommand request, CancellationToken cancellationToken = default)
    {
        var team = new Team
        {
            Name = request.Name,
            Description = request.Description,
            Admins = new List<string> { request.CreatorUserId }
        };

        return await _teamRepository.CreateAsync(team);
    }
}
