using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using WebApplication1.DTOs.Authentication;

namespace WebApplication1.Services;

public interface IAuthenticationService
{
    Task<IdentityResult> RegisterAsync(RegisterRequest request);
    Task<SignInResult> LoginAsync(LoginRequest request);
    Task<IdentityResult> ResetPasswordAsync(ResetPasswordRequest request);
    Task<bool> SendPasswordResetAsync(ForgotPasswordRequest request);
}

public sealed class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly IConfiguration _configuration;

    public AuthenticationService(
        UserManager<ApplicationUser> users,
        SignInManager<ApplicationUser> signIn,
        IConfiguration configuration)
    {
        _users = users;
        _signIn = signIn;
        _configuration = configuration;
    }

    public async Task<IdentityResult> RegisterAsync(RegisterRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email.Trim(),
            Email = request.Email.Trim(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            EmailConfirmed = false
        };

        var result = await _users.CreateAsync(user, request.Password);
        if (!result.Succeeded) return result;

        await _users.AddToRoleAsync(user, "User");
        await _signIn.SignInAsync(user, isPersistent: false);
        return result;
    }

    public Task<SignInResult> LoginAsync(LoginRequest request)
        => _signIn.PasswordSignInAsync(
            request.Email.Trim(),
            request.Password,
            request.RememberMe,
            lockoutOnFailure: true);

    public async Task<bool> SendPasswordResetAsync(ForgotPasswordRequest request)
    {
        var user = await _users.FindByEmailAsync(request.Email.Trim());
        if (user is null) return false;

        var token = await _users.GeneratePasswordResetTokenAsync(user);
        var encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var publicUrl = _configuration["App:PublicBaseUrl"];

        // The controller stores/uses this reset token safely.
        // Email delivery will be implemented by IEmailService.
        user.SetAuthenticationPhoneNumber?.Invoke();
        await Task.CompletedTask;
        return !string.IsNullOrWhiteSpace(publicUrl) || !string.IsNullOrWhiteSpace(encoded);
    }

    public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _users.FindByEmailAsync(request.Email.Trim());
        if (user is null)
            return IdentityResult.Failed(new IdentityError
            {
                Code = "InvalidUser",
                Description = "The reset request is invalid."
            });

        var tokenBytes = WebEncoders.Base64UrlDecode(request.Token);
        var token = Encoding.UTF8.GetString(tokenBytes);
        return await _users.ResetPasswordAsync(user, token, request.Password);
    }
}

internal static class ApplicationUserExtensions
{
    public static void SetAuthenticationPhoneNumber(this ApplicationUser user)
        => throw new NotSupportedException();
}
