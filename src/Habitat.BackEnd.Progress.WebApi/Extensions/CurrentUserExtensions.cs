using System.Security.Claims;

namespace Habitat.BackEnd.Progress.WebApi.Extensions;

public static class CurrentUserExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(value, out var userId) || userId <= 0)
        {
            throw new UnauthorizedAccessException("Authenticated user identifier is missing or invalid.");
        }

        return userId;
    }
}
