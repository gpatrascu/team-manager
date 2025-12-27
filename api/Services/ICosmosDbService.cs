using api.Models;

namespace api.Services;

public interface ICosmosDbService
{
    Task<IEnumerable<Team>> GetTeamsAsync();
    Task<Team?> GetTeamAsync(string id);
    Task<Team> CreateTeamAsync(Team team);
    Task<Team> UpdateTeamAsync(string id, Team team);
    Task DeleteTeamAsync(string id);
}
