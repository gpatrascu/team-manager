using api.Services;
using api.Domain.Ports;
using api.Domain.Entities;
using api.Infrastructure.Adapters.Persistence;
using api.Infrastructure.Adapters.Security;
using api.Application.Abstractions;
using api.Application.Commands;
using api.Application.Queries;
using api.Application.Handlers;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Disable Application Insights for local development
// Uncomment these lines and add APPLICATIONINSIGHTS_CONNECTION_STRING to local.settings.json for production
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

// Register Cosmos DB with options to prevent blocking startup
builder.Services.AddSingleton<CosmosClient>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var connectionString = configuration["CosmosDbConnectionString"];

    var clientOptions = new CosmosClientOptions
    {
        // Use Gateway mode to avoid TCP connection overhead during startup
        ConnectionMode = Microsoft.Azure.Cosmos.ConnectionMode.Gateway,
        RequestTimeout = TimeSpan.FromSeconds(10)
    };

    return new CosmosClient(connectionString, clientOptions);
});

// Legacy service (keeping for backwards compatibility during migration)
builder.Services.AddSingleton<ICosmosDbService>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var cosmosClient = serviceProvider.GetRequiredService<CosmosClient>();
    var databaseName = configuration["CosmosDbDatabaseName"];
    var containerName = configuration["CosmosDbContainerName"];
    return new CosmosDbService(cosmosClient, databaseName!, containerName!);
});

// ===== Hexagonal Architecture - Ports and Adapters =====

// Domain Ports (Interfaces) - Implemented by Infrastructure Adapters
builder.Services.AddSingleton<ITeamRepository>(sp =>
{
    var cosmosClient = sp.GetRequiredService<CosmosClient>();
    var config = sp.GetRequiredService<IConfiguration>();
    return new CosmosDbTeamRepository(
        cosmosClient,
        config["CosmosDbDatabaseName"]!,
        config["CosmosDbContainerName"]!
    );
});

builder.Services.AddSingleton<ITokenGenerator, CryptoTokenGenerator>();
builder.Services.AddSingleton<IAuthService, AzureSwaAuthService>();

// Application Handlers - Commands
builder.Services.AddScoped<IRequestHandler<CreateTeamCommand, Team>, CreateTeamHandler>();
builder.Services.AddScoped<IRequestHandler<JoinTeamCommand, Team>, JoinTeamHandler>();
builder.Services.AddScoped<IRequestHandler<GenerateInviteTokenCommand, GenerateInviteTokenResult>, GenerateInviteTokenHandler>();
builder.Services.AddScoped<IRequestHandler<ApproveMemberCommand, Team>, ApproveMemberHandler>();
builder.Services.AddScoped<IRequestHandler<RejectMemberCommand, Team>, RejectMemberHandler>();

// Application Handlers - Queries
builder.Services.AddScoped<IRequestHandler<GetMyTeamsQuery, IEnumerable<Team>>, GetMyTeamsHandler>();
builder.Services.AddScoped<IRequestHandler<GetTeamByIdQuery, Team?>, GetTeamByIdHandler>();
builder.Services.AddScoped<IRequestHandler<GetPendingMembersQuery, IEnumerable<TeamMember>>, GetPendingMembersHandler>();

builder.Build().Run();
