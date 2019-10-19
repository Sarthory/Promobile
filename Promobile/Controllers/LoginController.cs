using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Promobile.Data;
using Promobile.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Promobile.Extensions;
using Promobile.Models.Utils;
using Promobile.ViewModels;

namespace Promobile.Controllers
{
    public class LoginController : Controller
    {
        private readonly PromobileContext _db;

        public LoginController(PromobileContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var viewModel = new LoginViewModel
            {
                HasErrors = false,
                ErrorMessage = ""
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Login(IFormCollection form)
        {
            var userName = form["username"];
            var systemPassword = form["password"];
            var existentUser = LoginUser(userName, systemPassword);

            if (existentUser == null)
            {
                var viewModel = new LoginViewModel
                {
                    HasErrors = true,
                    ErrorMessage = "Usuário ou senha inválidos."
                };

                return View("Index", viewModel);
            }

            if (existentUser.UserType == UserType.ClientUser)
            {
                var viewModel = new LoginViewModel
                {
                    HasErrors = true,
                    ErrorMessage = "Usuário cliente não tem\npermissões no administrativo."
                };

                return View("Index", viewModel);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, existentUser.Name),
                new Claim("RegionDivision", existentUser.RegionDivision.ToString()),
                new Claim("UserType", existentUser.UserType.ToString())
            };

            var userIdentity = new ClaimsIdentity(claims, "login");

            var principal = new ClaimsPrincipal(userIdentity);

            await HttpContext.SignInAsync(
                principal,
                new AuthenticationProperties
                {
                    AllowRefresh = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(25),
                    RedirectUri = $"{Url.Action("Index", "Home")}"
                });

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index");
        }

        private User LoginUser(string username, string password)
        {
            var passwordHash = PromobileExtensions.ToHash(password);

            var loggedUser = _db.Users.FirstOrDefault(u => u.Login == username && u.Password == passwordHash);
            return loggedUser;
        }
    }
}