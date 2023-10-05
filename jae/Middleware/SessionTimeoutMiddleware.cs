using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;


public class SessionTimeoutMiddleware
{
    private readonly RequestDelegate _next;

    public SessionTimeoutMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        // Check if the user is authenticated and if the session has timed out
        if (context.User.Identity.IsAuthenticated)
        {
            var lastActivity = context.Session.GetString("LastActivity");
            if (string.IsNullOrEmpty(lastActivity) || DateTime.Now.Subtract(DateTime.Parse(lastActivity)) > TimeSpan.FromMinutes(1))
            {
                // Session has timed out (1 minute of inactivity)
                // Log the user out and redirect to the login page
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                context.Response.Redirect("/Account/Login"); // Adjust the login page URL as needed
                return;
            }
        }

        await _next(context);
    }
}

