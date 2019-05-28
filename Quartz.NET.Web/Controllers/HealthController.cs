using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Quartz.NET.Web.Controllers
{
    public class HealthController : Controller
    {
        /// <summary>
        /// 定时调用此接口让站点一直保持运行状态
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost,HttpGet]
        public IActionResult KeepAlive()
        {
            return Json(new { status = true });
        }

        [AllowAnonymous]
        [HttpPost, HttpGet]
        public IActionResult Index()
        {
            return Json(new { status = true });
        }
    }
}