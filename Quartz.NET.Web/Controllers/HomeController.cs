using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Quartz.NET.Web.Extensions;
using System;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Quartz.NET.Web.Controllers
{
    public class HomeController : Controller
    {
        private IConfiguration _configuration { get; set; }
        private IMemoryCache _memoryCache { get; set; }
        public HomeController(IConfiguration configuration, IMemoryCache memoryCache)
        {
            this._configuration = configuration;
            this._memoryCache = memoryCache;
        }
        [AllowAnonymous]
        public IActionResult Index()
        {
            if (!string.IsNullOrEmpty(HttpContext.Request("ReturnUrl")))
            {
                return new ContentResult
                {
                    ContentType = "text/html",
                    StatusCode = (int)HttpStatusCode.Unauthorized
                };
            }
            string msg = _memoryCache.Get("msg")?.ToString();
            if (msg != null)
            {
                ViewBag.msg = msg;
                _memoryCache.Remove("msg");
            }
            return View();
        }

        /// <summary>
        /// 登陆
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public async Task<IActionResult> ValidateAuthor(string token)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _memoryCache.Remove("msg");
            string _token = _configuration["token"];
            string superToken = _configuration["superToken"];
            if (!string.IsNullOrEmpty(token) && (token == _token || token == superToken))
            {
                _memoryCache.Set("isSuperToken", token == superToken);
                ClaimsIdentity claimIdentity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                claimIdentity.AddClaim(new Claim("token", token));
                //claimIdentity.AddClaim(new Claim("superToken", _configuration["superToken"]?.ToString()));
                await HttpContext.SignInAsync(new ClaimsPrincipal(claimIdentity), new AuthenticationProperties()
                {
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
                });
            }
            else
            {
                _memoryCache.Set("msg", string.IsNullOrEmpty(token) ? "请填写token" : "token不正确");
            }
            return new RedirectResult("/");
        }

        /// <summary>
        /// 退出
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return new RedirectResult("/");
        }

        public IActionResult Guide()
        {
            return View("~/Views/Home/Guide.cshtml");
        }
        public IActionResult Help()
        {
            return View("~/Views/Home/Help.cshtml");
        }
    }
}
