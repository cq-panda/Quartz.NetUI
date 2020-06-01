using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Quartz.NET.Web.Utility
{
    public static class HttpManager
    {

        public static string GetUserIP(IHttpContextAccessor httpContextAccessor)
        {
            var Request = httpContextAccessor.HttpContext.Request;
            string realIP = null;
            string forwarded = null;
            string remoteIpAddress = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            if (Request.Headers.ContainsKey("X-Real-IP"))
            {
                realIP = Request.Headers["X-Real-IP"].ToString();
                if (realIP != remoteIpAddress)
                {
                    remoteIpAddress = realIP;
                }
            }
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                forwarded = Request.Headers["X-Forwarded-For"].ToString();
                if (forwarded != remoteIpAddress)
                {
                    remoteIpAddress = forwarded;
                }
            }
            return remoteIpAddress;
        }

       

        public static async Task<string> HttpSendAsync(this IHttpClientFactory httpClientFactory, HttpMethod method, string url, Dictionary<string, string> headers = null)
        {

            var client = httpClientFactory.CreateClient();
            var content = new StringContent("");
            // content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            var request = new HttpRequestMessage(method, url)
            {
                Content = content
            };
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }
            try
            {
                HttpResponseMessage httpResponseMessage = await client.SendAsync(request);

                var result = await httpResponseMessage.Content
                    .ReadAsStringAsync();
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ex.Message;
            }
        }
    }
}
