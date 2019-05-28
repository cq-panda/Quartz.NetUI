using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Quartz.NET.Web.Utility;
using System.Linq;
using System.Text;

namespace Quartz.NET.Web.Filters
{
    public class TaskAuthorizeFilter : IAuthorizationFilter
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IMemoryCache _memoryCache;
        public TaskAuthorizeFilter(IHttpContextAccessor accessor, IMemoryCache memoryCache)
        {
            this._accessor = accessor;
            this._memoryCache = memoryCache;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            WriteLog();
            if (context.Filters.Any(item => item is IAllowAnonymousFilter))
                return;
        }

        private void WriteLog()
        {
            try
            {
                var request = _accessor.HttpContext.Request;
                string url = new StringBuilder()
                     .Append(request.Scheme)
                     .Append("://")
                     .Append(request.Host)
                     .Append(request.PathBase)
                     .Append(request.Path)
                     .Append(request.QueryString)
                     .ToString();
                FileQuartz.WriteAccess(_accessor.HttpContext.Connection.RemoteIpAddress.ToString() + "_" + url);
            }
            finally { }
        }
    }
}
