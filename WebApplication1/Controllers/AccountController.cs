using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs.Authentication;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

public sealed class AccountController : Controller
{
    private readonly IAuthenticationService _authentication;
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly UserManager<ApplicationUser> _users;

    public AccountController(IAuthenticationService authentication, SignInManager<ApplicationUser> signIn, UserManager<ApplicationUser> users)
    { _authentication = authentication; _signIn = signIn; _users = users; }

    [HttpGet]
    public IActionResult Register(string? returnUrl = null) { ViewData["ReturnUrl"] = returnUrl; return View(); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterRequest request, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(request);
        var result = await _authentication.RegisterAsync(request);
        if (!result.Succeeded) { foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description); return View(request); }
        return RedirectToAction("Index", "User");
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null) { ViewData["ReturnUrl"] = returnUrl; return View(); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRequest request, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(request);
        var result = await _authentication.LoginAsync(request);
        if (result.Succeeded)
        {
            var user = await _users.FindByEmailAsync(request.Email);
            if (user is not null && await _users.IsInRoleAsync(user, "Admin")) return RedirectToAction("Index", "Admin");
            return RedirectToAction("Index", "User");
        }
        if (result.RequiresTwoFactor)
        {
            var user = await _users.FindByEmailAsync(request.Email);
            if (user is null) return RedirectToAction(nameof(Login));
            var providers = await _users.GetValidTwoFactorProvidersAsync(user);
            var provider = providers.Contains("Email") ? "Email" : providers.FirstOrDefault();
            if (provider is null) { ModelState.AddModelError(string.Empty, "No two-factor provider is configured."); return View(request); }
            await _signIn.SendTwoFactorCodeAsync(provider);
            return View("TwoFactor", new TwoFactorRequest { Provider = provider, RememberMe = request.RememberMe });
        }
        if (result.IsLockedOut) ModelState.AddModelError(string.Empty, "This account is locked. Try again later.");
        else ModelState.AddModelError(string.Empty, "Invalid email or password.");
        return View(request);
    }

    [HttpGet]
    public IActionResult TwoFactor() => View(new TwoFactorRequest());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> TwoFactor(TwoFactorRequest request)
    {
        if (!ModelState.IsValid) return View(request);
        var result = await _signIn.TwoFactorSignInAsync(request.Provider, request.Code, request.RememberMe, request.RememberMachine);
        if (result.Succeeded)
        {
            var user = await _signIn.GetTwoFactorAuthenticationUserAsync();
            if (user is not null && await _users.IsInRoleAsync(user, "Admin")) return RedirectToAction("Index", "Admin");
            return RedirectToAction("Index", "User");
        }
        if (result.IsLockedOut) ModelState.AddModelError(string.Empty, "This account is locked. Try again later.");
        else ModelState.AddModelError(string.Empty, "Invalid verification code.");
        return View(request);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout() { await _signIn.SignOutAsync(); return RedirectToAction(nameof(Login)); }

    [HttpGet]
    public IActionResult ForgotPassword() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        if (!ModelState.IsValid) return View(request);
        await _authentication.SendPasswordResetAsync(request);
        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    [HttpGet] public IActionResult ForgotPasswordConfirmation() => View();
    [HttpGet]
    public IActionResult ResetPassword(string email, string token) => View(new ResetPasswordRequest { Email = email, Token = token });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        if (!ModelState.IsValid) return View(request);
        var result = await _authentication.ResetPasswordAsync(request);
        if (!result.Succeeded) { foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description); return View(request); }
        return RedirectToAction(nameof(ResetPasswordConfirmation));
    }

    [HttpGet] public IActionResult ResetPasswordConfirmation() => View();
    [HttpGet] public IActionResult AccessDenied() => View();
}
