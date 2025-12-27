using api.Domain.Entities;

namespace api.Domain.Ports;

public interface ITeamRepository
{
    Task<IEnumerable<Team>> GetAllAsync();
    Task<Team?> GetByIdAsync(string id);
    Task<IEnumerable<Team>> GetByUserIdAsync(string userId);
    Task<Team?> GetByInviteTokenAsync(string inviteToken);
    Task<Team> CreateAsync(Team team);
    Task<Team> UpdateAsync(Team team);
    Task DeleteAsync(string id);
}
