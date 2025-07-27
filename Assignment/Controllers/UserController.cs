using Assignment.Models;
using Assignment.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Assignment.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }
        [Route("login")]
        public IActionResult Login()
        {
            return View();
        }
        [Route("register")]
        public IActionResult Register()
        {
            return View();
        }
        [HttpGet]
        public IActionResult GoogleLogin(string returnUrl = "/")
        {
            var redirectUrl = Url.Action("GoogleCallback", "User", new { returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, "Google");
        }

        [HttpGet]
        public async Task<IActionResult> GoogleCallback(string returnUrl = "/")
        {
            var authenticateResult = await HttpContext.AuthenticateAsync("ExternalCookie");
            if (!authenticateResult.Succeeded)
            {
                return RedirectToAction("Login");
            }

            await HttpContext.SignOutAsync("ExternalCookie");

            var claims = authenticateResult.Principal.Claims;
            var googleId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var username = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId || (!string.IsNullOrEmpty(email) && u.Email == email));

            if (user != null)
            {
                if (string.IsNullOrEmpty(user.GoogleId) && !string.IsNullOrEmpty(googleId))
                {
                    user.GoogleId = googleId;
                    user.UpdatedAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                }

                await CookieAuthHelper.SignInUserAsync(
                    HttpContext,
                    user.Id,
                    user.Name,
                    user.Role,
                    user.Email,
                    user.Phone,
                    user.DateOfBirth
                );
                return RedirectToAction("Index", "Home");
            }

            TempData["GoogleId"] = googleId;
            TempData["Name"] = username;
            TempData["Email"] = email;
            return RedirectToAction("Register");
        }
        [HttpGet]
        public IActionResult FacebookLogin(string returnUrl = "/")
        {
            var redirectUrl = Url.Action("FacebookCallback", "User", new { returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, "Facebook");
        }

        [HttpGet]
        public async Task<IActionResult> FacebookCallback(string returnUrl = "/")
        {
            var authenticateResult = await HttpContext.AuthenticateAsync("ExternalCookie");
            if (!authenticateResult.Succeeded)
            {
                return RedirectToAction("Login");
            }

            await HttpContext.SignOutAsync("ExternalCookie");

            var claims = authenticateResult.Principal.Claims;
            var facebookId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var username = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.FacebookId == facebookId || (!string.IsNullOrEmpty(email) && u.Email == email));

            if (user != null)
            {
                if (string.IsNullOrEmpty(user.FacebookId) && !string.IsNullOrEmpty(facebookId))
                {
                    user.FacebookId = facebookId;
                    user.UpdatedAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                }

                await CookieAuthHelper.SignInUserAsync(
                    HttpContext,
                    user.Id,
                    user.Name,
                    user.Role,
                    user.Email,
                    user.Phone,
                    user.DateOfBirth
                );
                return RedirectToAction("Index", "Home");
            }

            TempData["FacebookId"] = facebookId;
            TempData["Name"] = username;
            TempData["Email"] = email;
            return RedirectToAction("Register");
        }
        [HttpGet]
        public IActionResult GitHubLogin(string returnUrl = "/")
        {
            var redirectUrl = Url.Action("GitHubCallback", "User", new { returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, "GitHub");
        }

        [HttpGet]
        public async Task<IActionResult> GitHubCallback(string returnUrl = "/")
        {
            var authenticateResult = await HttpContext.AuthenticateAsync("ExternalCookie");
            if (!authenticateResult.Succeeded)
            {
                return RedirectToAction("Login");
            }

            await HttpContext.SignOutAsync("ExternalCookie");

            var claims = authenticateResult.Principal.Claims;
            var gitHubId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var username = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.GitHubId == gitHubId ||
                (!string.IsNullOrEmpty(email) && u.Email == email));

            if (user != null)
            {
                if (string.IsNullOrEmpty(user.GitHubId) && !string.IsNullOrEmpty(gitHubId))
                {
                    user.GitHubId = gitHubId;
                    user.UpdatedAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                }

                await CookieAuthHelper.SignInUserAsync(
                    HttpContext,
                    user.Id,
                    user.Name,
                    user.Role,
                    user.Email,
                    user.Phone,
                    user.DateOfBirth
                );
                return RedirectToAction("Index", "Home");
            }

            TempData["GitHubId"] = gitHubId;
            TempData["Name"] = username;
            TempData["Email"] = email;
            return RedirectToAction("Register");
        }
        [HttpGet]
        public IActionResult DiscordLogin(string returnUrl = "/")
        {
            var redirectUrl = Url.Action("DiscordCallback", "User", new { returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, "Discord");
        }

        [HttpGet]
        public async Task<IActionResult> DiscordCallback(string returnUrl = "/")
        {
            var authenticateResult = await HttpContext.AuthenticateAsync("ExternalCookie");
            if (!authenticateResult.Succeeded)
            {
                return RedirectToAction("Login");
            }

            await HttpContext.SignOutAsync("ExternalCookie");

            var claims = authenticateResult.Principal.Claims;
            var discordId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var username = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.DiscordId == discordId ||
                (!string.IsNullOrEmpty(email) && u.Email == email));

            if (user != null)
            {
                if (string.IsNullOrEmpty(user.DiscordId) && !string.IsNullOrEmpty(discordId))
                {
                    user.DiscordId = discordId;
                    user.UpdatedAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                }

                await CookieAuthHelper.SignInUserAsync(
                    HttpContext,
                    user.Id,
                    user.Name,
                    user.Role,
                    user.Email,
                    user.Phone,
                    user.DateOfBirth
                );
                return RedirectToAction("Index", "Home");
            }

            TempData["DiscordId"] = discordId;
            TempData["Name"] = username;
            TempData["Email"] = email;
            return RedirectToAction("Register");
        }
    }
}
