using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs.Authentication;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

public sealed class AccountController : Controller
{
    private readonly IAuthenticationService _authentication;

    public AccountController(IAuthenticationService authentication)
        => _authentication = authentication;

    [HttpGet]
    public IActionResult Register(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterRequest request, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(request);

        var result = await _authentication.RegisterAsync(request);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(request);
        }

        return RedirectToAction("Index", "User");
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRequest request, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(request);

        var result = await _authentication.LoginAsync(request);
        if (result.Succeeded)
        {
            var user = await HttpContext.RequestServices
                .GetRequiredService<UserManager<ApplicationUser>>()
                .FindByEmailAsync(request.Email);

            if (user is not null && await HttpContext.RequestServices
                    .GetRequiredService<UserManager<ApplicationUser>>()
                    .IsInRoleAsync(user, "Admin"))
                return RedirectToAction("Index", "Admin");

            return RedirectToAction("Index", "User");
        }

        if (result.RequiresTwoFactor)
            return RedirectToAction(nameof(TwoFactor));

        if (result.IsLockedOut)
            ModelState.AddModelError(string.Empty, "This account is locked. Try again later.");
        else
            ModelState.AddModelError(string.Empty, "Invalid email or password.");

        return View(request);
    }

    [HttpGet]
    public IActionResult TwoFactor() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.RequestServices
            .GetRequiredService<SignInManager<ApplicationUser>>()
            .SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult ForgotPassword() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        if (!ModelState.IsValid) return View(request);
        await _authentication.SendPasswordResetAsync(request);
        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    [HttpGet]
    public IActionResult ForgotPasswordConfirmation() => View();

    [HttpGet]
    public IActionResult ResetPassword(string email, string token)
        => View(new ResetPasswordRequest { Email = email, Token = token });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        if (!ModelState.IsValid) return View(request);
        var result = await _authentication.ResetPasswordAsync(request);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(request);
        }

        return RedirectToAction(nameof(ResetPasswordConfirmation));
    }

    [HttpGet]
    public IActionResult ResetPasswordConfirmation() => View();

    [HttpGet]
    public IActionResult AccessDenied() => View();
}
