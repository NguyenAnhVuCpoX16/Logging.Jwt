namespace Logging.Jwt.Web.Middleware;

public class SwaggerAuthMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path;

        if (path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase))
        {
            if (!(context.User.Identity?.IsAuthenticated ?? false))
            {
                var returnUrl = Uri.EscapeDataString(context.Request.Path + context.Request.QueryString);
                context.Response.Redirect($"/login?returnUrl={returnUrl}");
                return;
            }
        }

        await next(context);
    }
}
