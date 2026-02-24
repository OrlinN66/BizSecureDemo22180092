using System.Security.Claims;
using BizSecureDemo22180092.Data;
using BizSecureDemo22180092.Models;
using BizSecureDemo22180092.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace BizSecureDemo22180092.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _db;
    private readonly PasswordHasher<AppUser> _hasher;

    public AccountController(AppDbContext db, PasswordHasher<AppUser> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterVm());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var email = vm.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == email))
        {
            ModelState.AddModelError("", "Този email вече е регистриран.");
            return View(vm);
        }

        var user = new AppUser { Email = email };
        user.PasswordHash = _hasher.HashPassword(user, vm.Password);
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult Login() => View(new LoginVm());

    // Ex 4: Rate limiting on login endpoint (5 requests per minute per IP)
    [EnableRateLimiting("login")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var email = vm.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        // Do not reveal whether user exists
        if (user == null)
        {
            ModelState.AddModelError("", "Грешен email или парола.");
            return View(vm);
        }

        // Ex 4: Check account lockout
        if (user.LockoutUntilUtc != null && user.LockoutUntilUtc > DateTime.UtcNow)
        {
            ModelState.AddModelError("", $"Акаунтът е временно заключен. Опитайте отново след {user.LockoutUntilUtc.Value.ToLocalTime():HH:mm:ss}.");
            return View(vm);
        }

        // Wrong password
        if (_hasher.VerifyHashedPassword(user, user.PasswordHash, vm.Password) == PasswordVerificationResult.Failed)
        {
            // Ex 4: Increment failed logins; lock after 5 attempts for 5 minutes
            user.FailedLogins = (user.FailedLogins ?? 0) + 1;
            if (user.FailedLogins >= 5)
            {
                user.LockoutUntilUtc = DateTime.UtcNow.AddMinutes(5);
                user.FailedLogins = 0;
            }
            await _db.SaveChangesAsync();

            ModelState.AddModelError("", "Грешен email или парола.");
            return View(vm);
        }

        // Success - reset counters
        user.FailedLogins = 0;
        user.LockoutUntilUtc = null;
        await _db.SaveChangesAsync();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Email)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }
}
