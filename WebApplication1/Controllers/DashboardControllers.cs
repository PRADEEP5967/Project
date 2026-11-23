using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

[Authorize]
public sealed class UserController : Controller
{
    public IActionResult Index() => View();
}

[Authorize(Roles = "Admin")]
public sealed class AdminController : Controller
{
    public IActionResult Index() => View();
}
