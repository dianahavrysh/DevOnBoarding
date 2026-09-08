using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Filters;

/// <summary>
/// Action filter that requires an authenticated user, as set by
/// JwtAuthenticationMiddleware. Returns 401 Unauthorized if the
/// current request's HttpContext.User is not authenticated.
/// Use instead of [Authorize] to avoid registering an authentication scheme.
/// </summary>
public class RequireAuthAttribute : ActionFilterAttribute {
    public override void OnActionExecuting(ActionExecutingContext context) {
        if (context.HttpContext.User.Identity?.IsAuthenticated != true) {
            context.Result = new UnauthorizedObjectResult(new { error = "Authentication required." });
            return;
        }

        base.OnActionExecuting(context);
    }
}
