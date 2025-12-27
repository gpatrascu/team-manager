using api.Domain.Entities;
using api.Domain.Ports;
using Microsoft.Azure.Cosmos;

namespace api.Infrastructure.Adapters.Persistence;

public class CosmosDbTeamRepository : ITeamRepository
{
    private readonly Container _container;

    public CosmosDbTeamRepository(CosmosClient cosmosClient, string databaseName, string containerName)
    {
        _container = cosmosClient.GetContainer(databaseName, containerName);
    }

    public async Task<IEnumerable<Team>> GetAllAsync()
    {
        var query = _container.GetItemQueryIterator<Team>(new QueryDefinition("SELECT * FROM c"));
        var results = new List<Team>();

        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response.ToList());
        }

        return results;
    }

    public async Task<Team?> GetByIdAsync(string id)
    {
        try
        {
            var response = await _container.ReadItemAsync<Team>(id, new PartitionKey(id));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IEnumerable<Team>> GetByUserIdAsync(string userId)
    {
        var queryText = @"SELECT * FROM c WHERE
                          ARRAY_CONTAINS(c.admins, @userId) OR
                          EXISTS(SELECT VALUE m FROM m IN c.members
                                 WHERE m.userId = @userId AND m.status = 'active')";

        var queryDefinition = new QueryDefinition(queryText)
            .WithParameter("@userId", userId);

        var query = _container.GetItemQueryIterator<Team>(queryDefinition);
        var results = new List<Team>();

        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response.ToList());
        }

        return results;
    }

    public async Task<Team?> GetByInviteTokenAsync(string inviteToken)
    {
        var queryText = @"SELECT * FROM c WHERE c.inviteToken = @token";

        var queryDefinition = new QueryDefinition(queryText)
            .WithParameter("@token", inviteToken);

        var query = _container.GetItemQueryIterator<Team>(queryDefinition);

        if (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            return response.FirstOrDefault();
        }

        return null;
    }

    public async Task<Team> CreateAsync(Team team)
    {
        team.Id = Guid.NewGuid().ToString();
        team.CreatedAt = DateTime.UtcNow;
        team.UpdatedAt = DateTime.UtcNow;

        var response = await _container.CreateItemAsync(team, new PartitionKey(team.Id));
        return response.Resource;
    }

    public async Task<Team> UpdateAsync(Team team)
    {
        team.UpdatedAt = DateTime.UtcNow;
        var response = await _container.UpsertItemAsync(team, new PartitionKey(team.Id));
        return response.Resource;
    }

    public async Task DeleteAsync(string id)
    {
        await _container.DeleteItemAsync<Team>(id, new PartitionKey(id));
    }
}
