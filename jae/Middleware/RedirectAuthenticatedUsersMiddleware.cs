public class RedirectAuthenticatedUsersMiddleware
{
    private readonly RequestDelegate _next;

    public RedirectAuthenticatedUsersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        // Check if the user is authenticated
        if (context.User.Identity.IsAuthenticated)
        {
            // User is logged in, redirect them only if they are not already on the desired page
            if (!context.Request.Path.StartsWithSegments("/Responses/Index"))
            {
                context.Response.Redirect("/Responses/Index"); // Replace with your desired URL
                return;
            }
        }

        // If not authenticated or already on the desired page, continue with the request pipeline
        await _next(context);
    }
}
