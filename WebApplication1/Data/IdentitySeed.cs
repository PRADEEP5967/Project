using Microsoft.AspNetCore.Identity;
using WebApplication1.Models;

namespace WebApplication1.Data;

public static class IdentitySeed
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration, ILogger logger)
    {
        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "Admin", "User" })
            if (!await roles.RoleExistsAsync(role)) await roles.CreateAsync(new IdentityRole(role));

        var email = configuration["SeedAdmin:Email"];
        var password = configuration["SeedAdmin:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return;
        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await users.FindByEmailAsync(email);
        if (admin is null)
        {
            admin = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, FirstName = "System", LastName = "Administrator" };
            var result = await users.CreateAsync(admin, password);
            if (!result.Succeeded) { logger.LogError("Could not seed admin: {Errors}", string.Join(", ", result.Errors.Select(x => x.Description))); return; }
        }
        if (!await users.IsInRoleAsync(admin, "Admin")) await users.AddToRoleAsync(admin, "Admin");
    }
}
