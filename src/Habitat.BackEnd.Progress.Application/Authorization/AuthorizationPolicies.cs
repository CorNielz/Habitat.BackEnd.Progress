namespace Habitat.BackEnd.Progress.Application.Authorization;

public static class AuthorizationPolicies
{
    public const string AdminOnly = nameof(AdminOnly);
    public const string CommonOrAdmin = nameof(CommonOrAdmin);
}
