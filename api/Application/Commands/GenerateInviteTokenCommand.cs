using api.Application.Abstractions;

namespace api.Application.Commands;

public record GenerateInviteTokenCommand(
    string TeamId,
    string AdminUserId,
    int ExpiryHours = 168 // 7 days default
) : IRequest<GenerateInviteTokenResult>;

public record GenerateInviteTokenResult(
    string InviteToken,
    DateTime Expiry
);
