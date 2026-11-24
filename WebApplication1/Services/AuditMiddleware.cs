using System.Security.Claims;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Services;

public sealed class AuditMiddleware
{
    private readonly RequestDelegate _next;

    public AuditMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext db)
    {
        await _next(context);
        if (context.Request.Method == HttpMethods.Post && context.User.Identity?.IsAuthenticated == true)
        {
            var action = $"{context.Request.Method} {context.Request.Path}";
            db.AuditLogs.Add(new AuditLog
            {
                Action = action,
                UserEmail = context.User.FindFirstValue(ClaimTypes.Email) ?? context.User.Identity.Name,
                IpAddress = context.Connection.RemoteIpAddress?.ToString()
            });
            await db.SaveChangesAsync();
        }
    }
}
