using api.Models;
using api.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace api;

public class TeamsFunction
{
    private readonly ILogger<TeamsFunction> _logger;
    private readonly ICosmosDbService _cosmosDbService;

    public TeamsFunction(ILogger<TeamsFunction> logger, ICosmosDbService cosmosDbService)
    {
        _logger = logger;
        _cosmosDbService = cosmosDbService;
    }

    [Function("GetTeams")]
    public async Task<IActionResult> GetTeams([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "teams")] HttpRequest req)
    {
        _logger.LogInformation("Getting all teams");

        try
        {
            var teams = await _cosmosDbService.GetTeamsAsync();
            return new OkObjectResult(teams);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting teams");
            return new StatusCodeResult(500);
        }
    }

    [Function("GetTeam")]
    public async Task<IActionResult> GetTeam([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "teams/{id}")] HttpRequest req, string id)
    {
        _logger.LogInformation($"Getting team with id: {id}");

        try
        {
            var team = await _cosmosDbService.GetTeamAsync(id);
            if (team == null)
            {
                return new NotFoundResult();
            }
            return new OkObjectResult(team);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting team {id}");
            return new StatusCodeResult(500);
        }
    }

    [Function("CreateTeam")]
    public async Task<IActionResult> CreateTeam([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "teams")] HttpRequest req)
    {
        _logger.LogInformation("Creating new team");

        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var team = JsonSerializer.Deserialize<Team>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (team == null)
            {
                return new BadRequestObjectResult("Invalid team data");
            }

            var createdTeam = await _cosmosDbService.CreateTeamAsync(team);
            return new CreatedResult($"/api/teams/{createdTeam.Id}", createdTeam);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating team");
            return new StatusCodeResult(500);
        }
    }

    [Function("UpdateTeam")]
    public async Task<IActionResult> UpdateTeam([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "teams/{id}")] HttpRequest req, string id)
    {
        _logger.LogInformation($"Updating team with id: {id}");

        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var team = JsonSerializer.Deserialize<Team>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (team == null)
            {
                return new BadRequestObjectResult("Invalid team data");
            }

            var updatedTeam = await _cosmosDbService.UpdateTeamAsync(id, team);
            return new OkObjectResult(updatedTeam);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating team {id}");
            return new StatusCodeResult(500);
        }
    }

    [Function("DeleteTeam")]
    public async Task<IActionResult> DeleteTeam([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "teams/{id}")] HttpRequest req, string id)
    {
        _logger.LogInformation($"Deleting team with id: {id}");

        try
        {
            await _cosmosDbService.DeleteTeamAsync(id);
            return new NoContentResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting team {id}");
            return new StatusCodeResult(500);
        }
    }
}
