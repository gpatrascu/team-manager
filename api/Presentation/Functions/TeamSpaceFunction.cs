using api.Application.Abstractions;
using api.Application.Commands;
using api.Application.Queries;
using api.Domain.Entities;
using api.Domain.Ports;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace api.Presentation.Functions;

public class TeamSpaceFunction
{
    private readonly ILogger<TeamSpaceFunction> _logger;
    private readonly IAuthService _authService;
    private readonly IRequestHandler<CreateTeamCommand, Team> _createTeamHandler;
    private readonly IRequestHandler<GetMyTeamsQuery, IEnumerable<Team>> _getMyTeamsHandler;
    private readonly IRequestHandler<GetTeamByIdQuery, Team?> _getTeamByIdHandler;
    private readonly IRequestHandler<GenerateInviteTokenCommand, GenerateInviteTokenResult> _generateInviteTokenHandler;
    private readonly IRequestHandler<JoinTeamCommand, Team> _joinTeamHandler;
    private readonly IRequestHandler<GetPendingMembersQuery, IEnumerable<TeamMember>> _getPendingMembersHandler;
    private readonly IRequestHandler<ApproveMemberCommand, Team> _approveMemberHandler;
    private readonly IRequestHandler<RejectMemberCommand, Team> _rejectMemberHandler;

    public TeamSpaceFunction(
        ILogger<TeamSpaceFunction> logger,
        IAuthService authService,
        IRequestHandler<CreateTeamCommand, Team> createTeamHandler,
        IRequestHandler<GetMyTeamsQuery, IEnumerable<Team>> getMyTeamsHandler,
        IRequestHandler<GetTeamByIdQuery, Team?> getTeamByIdHandler,
        IRequestHandler<GenerateInviteTokenCommand, GenerateInviteTokenResult> generateInviteTokenHandler,
        IRequestHandler<JoinTeamCommand, Team> joinTeamHandler,
        IRequestHandler<GetPendingMembersQuery, IEnumerable<TeamMember>> getPendingMembersHandler,
        IRequestHandler<ApproveMemberCommand, Team> approveMemberHandler,
        IRequestHandler<RejectMemberCommand, Team> rejectMemberHandler)
    {
        _logger = logger;
        _authService = authService;
        _createTeamHandler = createTeamHandler;
        _getMyTeamsHandler = getMyTeamsHandler;
        _getTeamByIdHandler = getTeamByIdHandler;
        _generateInviteTokenHandler = generateInviteTokenHandler;
        _joinTeamHandler = joinTeamHandler;
        _getPendingMembersHandler = getPendingMembersHandler;
        _approveMemberHandler = approveMemberHandler;
        _rejectMemberHandler = rejectMemberHandler;
    }

    [Function("GetMyTeams")]
    public async Task<IActionResult> GetMyTeams(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "teams/my-teams")] HttpRequest req)
    {
        _logger.LogInformation("Getting user's teams");

        var userId = _authService.GetUserIdFromRequest(req);
        if (userId == null)
            return new UnauthorizedResult();

        try
        {
            var query = new GetMyTeamsQuery(userId);
            var teams = await _getMyTeamsHandler.Handle(query);

            return new OkObjectResult(teams);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user teams");
            return new StatusCodeResult(500);
        }
    }

    [Function("GetTeamById")]
    public async Task<IActionResult> GetTeamById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "teams/{teamId}")] HttpRequest req,
        string teamId)
    {
        _logger.LogInformation($"Getting team: {teamId}");

        var userId = _authService.GetUserIdFromRequest(req);
        if (userId == null)
            return new UnauthorizedResult();

        try
        {
            var query = new GetTeamByIdQuery(teamId, userId);
            var team = await _getTeamByIdHandler.Handle(query);

            if (team == null)
                return new NotFoundResult();

            return new OkObjectResult(team);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting team {teamId}");
            return new StatusCodeResult(500);
        }
    }

    [Function("CreateTeam")]
    public async Task<IActionResult> CreateTeam(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "teams")] HttpRequest req)
    {
        _logger.LogInformation("Creating new team");

        var userId = _authService.GetUserIdFromRequest(req);
        if (userId == null)
            return new UnauthorizedResult();

        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var dto = JsonSerializer.Deserialize<CreateTeamRequestDto>(requestBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                return new BadRequestObjectResult("Team name is required");

            var command = new CreateTeamCommand(dto.Name, dto.Description, userId);
            var team = await _createTeamHandler.Handle(command);

            return new CreatedResult($"/api/teams/{team.Id}", team);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating team");
            return new StatusCodeResult(500);
        }
    }

    [Function("GenerateInviteToken")]
    public async Task<IActionResult> GenerateInviteToken(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "teams/{teamId}/invite-token")] HttpRequest req,
        string teamId)
    {
        _logger.LogInformation($"Generating invite token for team: {teamId}");

        var userId = _authService.GetUserIdFromRequest(req);
        if (userId == null)
            return new UnauthorizedResult();

        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var dto = string.IsNullOrWhiteSpace(requestBody)
                ? new GenerateInviteTokenRequestDto()
                : JsonSerializer.Deserialize<GenerateInviteTokenRequestDto>(requestBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new GenerateInviteTokenRequestDto();

            var command = new GenerateInviteTokenCommand(teamId, userId, dto.ExpiryHours);
            var result = await _generateInviteTokenHandler.Handle(command);

            return new OkObjectResult(result);
        }
        catch (UnauthorizedAccessException)
        {
            return new StatusCodeResult(403);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, $"Invalid operation generating invite token for team {teamId}");
            return new BadRequestObjectResult(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error generating invite token for team {teamId}");
            return new StatusCodeResult(500);
        }
    }

    [Function("JoinTeam")]
    public async Task<IActionResult> JoinTeam(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "teams/join")] HttpRequest req)
    {
        _logger.LogInformation("User joining team via invite token");

        var userId = _authService.GetUserIdFromRequest(req);
        var principal = _authService.GetClientPrincipal(req);

        if (userId == null || principal == null)
            return new UnauthorizedResult();

        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var dto = JsonSerializer.Deserialize<JoinTeamRequestDto>(requestBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (dto == null || string.IsNullOrWhiteSpace(dto.InviteToken) || string.IsNullOrWhiteSpace(dto.Nickname))
                return new BadRequestObjectResult("InviteToken and Nickname are required");

            // Extract user info from principal
            var userEmail = principal.Claims?.FirstOrDefault(c => c.Typ == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Val ?? "";
            var userName = principal.UserDetails ?? "";

            var command = new JoinTeamCommand(dto.InviteToken, dto.Nickname, userId, userEmail, userName);
            var team = await _joinTeamHandler.Handle(command);

            return new OkObjectResult(team);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation joining team");
            return new BadRequestObjectResult(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining team");
            return new StatusCodeResult(500);
        }
    }

    [Function("GetPendingMembers")]
    public async Task<IActionResult> GetPendingMembers(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "teams/{teamId}/members/pending")] HttpRequest req,
        string teamId)
    {
        _logger.LogInformation($"Getting pending members for team: {teamId}");

        var userId = _authService.GetUserIdFromRequest(req);
        if (userId == null)
            return new UnauthorizedResult();

        try
        {
            var query = new GetPendingMembersQuery(teamId, userId);
            var members = await _getPendingMembersHandler.Handle(query);

            return new OkObjectResult(members);
        }
        catch (UnauthorizedAccessException)
        {
            return new StatusCodeResult(403);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, $"Invalid operation getting pending members for team {teamId}");
            return new BadRequestObjectResult(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting pending members for team {teamId}");
            return new StatusCodeResult(500);
        }
    }

    [Function("ApproveMember")]
    public async Task<IActionResult> ApproveMember(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "teams/{teamId}/members/{memberId}/approve")] HttpRequest req,
        string teamId,
        string memberId)
    {
        _logger.LogInformation($"Approving member {memberId} for team {teamId}");

        var userId = _authService.GetUserIdFromRequest(req);
        if (userId == null)
            return new UnauthorizedResult();

        try
        {
            var command = new ApproveMemberCommand(teamId, memberId, userId);
            var team = await _approveMemberHandler.Handle(command);

            return new OkObjectResult(team);
        }
        catch (UnauthorizedAccessException)
        {
            return new StatusCodeResult(403);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, $"Invalid operation approving member {memberId} for team {teamId}");
            return new BadRequestObjectResult(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error approving member {memberId} for team {teamId}");
            return new StatusCodeResult(500);
        }
    }

    [Function("RejectMember")]
    public async Task<IActionResult> RejectMember(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "teams/{teamId}/members/{memberId}/reject")] HttpRequest req,
        string teamId,
        string memberId)
    {
        _logger.LogInformation($"Rejecting member {memberId} for team {teamId}");

        var userId = _authService.GetUserIdFromRequest(req);
        if (userId == null)
            return new UnauthorizedResult();

        try
        {
            var command = new RejectMemberCommand(teamId, memberId, userId);
            var team = await _rejectMemberHandler.Handle(command);

            return new OkObjectResult(team);
        }
        catch (UnauthorizedAccessException)
        {
            return new StatusCodeResult(403);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, $"Invalid operation rejecting member {memberId} for team {teamId}");
            return new BadRequestObjectResult(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error rejecting member {memberId} for team {teamId}");
            return new StatusCodeResult(500);
        }
    }
}

// DTOs for HTTP deserialization
public record CreateTeamRequestDto(string Name, string? Description);
public record GenerateInviteTokenRequestDto(int ExpiryHours = 168);
public record JoinTeamRequestDto(string InviteToken, string Nickname);
