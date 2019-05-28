using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartz.NET.Web.Extensions
{
    public static class HttpContextExtension
    {

        /// <summary>
        /// 获取Request值
        /// </summary>
        /// <param name="context"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static string Request(this HttpContext context, string parameter)
        {
            if (context == null)
                return null;
            if (context.Request.Method == "POST")
                return context.Request.Form[parameter].ToString();
            else
                return context.Request.Query[parameter].ToString();
        }

        /// <summary>
        /// 是否为ajax请求
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsAjaxRequest(this HttpContext context)
        {
            return context.Request("X-Requested-With") == "XMLHttpRequest"
                || (context.Request.Headers != null
                   && context.Request.Headers["X-Requested-With"] == "XMLHttpRequest");
        }

        /// <summary>
        /// 获取请求的参数
        /// net core 2.0已增加回读方法 context.Request.EnableRewind();
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>

        public static string GetRequestParameters(this HttpContext context)
        {
            if (context.Request.Body == null || !context.Request.Body.CanRead)
                return null;
            if (context.Request.Body.Position > 0)
            {
                context.Request.Body.Position = 0;
            }
            string prarameters = null;
            var bodyStream = context.Request.Body;

            using (var buffer = new MemoryStream())
            {
                //将字节流复制到新的流
                bodyStream.CopyToAsync(buffer);
                buffer.Position = 0L;
                //重新设置body流的读取起始位置(如果不设置后面都读不到值)
                bodyStream.Position = 0L;
                using (var reader = new StreamReader(buffer, Encoding.UTF8))
                {
                    buffer.Seek(0, SeekOrigin.Begin);
                    prarameters = reader.ReadToEnd();
                }
            }
            return prarameters;
        }
    }
}
