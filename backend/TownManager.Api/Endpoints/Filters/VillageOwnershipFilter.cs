using System.Security.Claims;
using TownManager.Application.Interfaces;

namespace TownManager.Api.Endpoints.Filters;

public class VillageOwnershipFilter : IEndpointFilter
{
    // Order-scoped routes (cancel build/train) use {orderId}, not {villageId};
    // ownership is checked in their command handler instead of here.
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Results.Problem(statusCode: 401, title: "Unauthorized");

        if (TryGetVillageId(context, out var villageId))
        {
            var playerRepo = context.HttpContext.RequestServices.GetRequiredService<IPlayerRepository>();
            var ownerUserId = await playerRepo.GetUserIdByVillageIdAsync(villageId, context.HttpContext.RequestAborted);
            if (ownerUserId is null)
                return Results.Problem(statusCode: 404, title: "Village not found.");
            if (ownerUserId != userId)
                return Results.Problem(statusCode: 403, title: "You do not own this village.");
        }

        return await next(context);
    }

    private static bool TryGetVillageId(EndpointFilterInvocationContext context, out Guid villageId)
    {
        var values = context.HttpContext.Request.RouteValues;
        foreach (var key in new[] { "villageId", "id" })
        {
            if (values.TryGetValue(key, out var val) && val is string s && Guid.TryParse(s, out villageId))
                return true;
        }
        villageId = Guid.Empty;
        return false;
    }
}
