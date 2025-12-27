using api.Models;
using Microsoft.Azure.Cosmos;

namespace api.Services;

public class CosmosDbService : ICosmosDbService
{
    private readonly Container _container;

    public CosmosDbService(CosmosClient cosmosClient, string databaseName, string containerName)
    {
        _container = cosmosClient.GetContainer(databaseName, containerName);
    }

    public async Task<IEnumerable<Team>> GetTeamsAsync()
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

    public async Task<Team?> GetTeamAsync(string id)
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

    public async Task<Team> CreateTeamAsync(Team team)
    {
        team.Id = Guid.NewGuid().ToString();
        team.CreatedAt = DateTime.UtcNow;
        team.UpdatedAt = DateTime.UtcNow;
        var response = await _container.CreateItemAsync(team, new PartitionKey(team.Id));
        return response.Resource;
    }

    public async Task<Team> UpdateTeamAsync(string id, Team team)
    {
        team.Id = id;
        team.UpdatedAt = DateTime.UtcNow;
        var response = await _container.UpsertItemAsync(team, new PartitionKey(id));
        return response.Resource;
    }

    public async Task DeleteTeamAsync(string id)
    {
        await _container.DeleteItemAsync<Team>(id, new PartitionKey(id));
    }
}
