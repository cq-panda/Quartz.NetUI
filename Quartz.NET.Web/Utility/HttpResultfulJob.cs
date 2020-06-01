using Microsoft.Extensions.Configuration;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;
using Quartz.NET.Web.Extensions;
using Quartz.NET.Web.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Quartz.NET.Web.Utility
{
    public class HttpResultfulJob : IJob
    {
        readonly IHttpClientFactory httpClientFactory;
        /// <summary>
        /// 2020.05.31增加构造方法
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="httpClientFactory"></param>
        public HttpResultfulJob(IServiceProvider serviceProvider, IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
            //serviceProvider.GetService()
        }
        public async Task Execute(IJobExecutionContext context)
        {
            DateTime dateTime = DateTime.Now;
            TaskOptions taskOptions = context.GetTaskOptions();
            string httpMessage = "";
            AbstractTrigger trigger = (context as JobExecutionContextImpl).Trigger as AbstractTrigger;
            if (taskOptions == null)
            {
                FileHelper.WriteFile(FileQuartz.LogPath + trigger.Group, $"{trigger.Name}.txt", "未到找作业或可能被移除", true);
                return;
            }
            Console.WriteLine($"作业[{taskOptions.TaskName}]开始:{ DateTime.Now.ToString("yyyy-MM-dd HH:mm:sss")}");
            if (string.IsNullOrEmpty(taskOptions.ApiUrl) || taskOptions.ApiUrl == "/")
            {
                FileHelper.WriteFile(FileQuartz.LogPath + trigger.Group, $"{trigger.Name}.txt", $"{ DateTime.Now.ToString("yyyy-MM-dd HH:mm:sss")}未配置url,", true);
                return;
            }

            try
            {
                Dictionary<string, string> header = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(taskOptions.AuthKey)
                    && !string.IsNullOrEmpty(taskOptions.AuthValue))
                {
                    header.Add(taskOptions.AuthKey.Trim(), taskOptions.AuthValue.Trim());
                }

                httpMessage = await httpClientFactory.HttpSendAsync(
                    taskOptions.RequestType?.ToLower() == "get" ? HttpMethod.Get : HttpMethod.Post,
                    taskOptions.ApiUrl,
                    header);
            }
            catch (Exception ex)
            {
                httpMessage = ex.Message;
            }

            try
            {
                string logContent = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}_{dateTime.ToString("yyyy-MM-dd HH:mm:ss")}_{(string.IsNullOrEmpty(httpMessage) ? "OK" : httpMessage)}\r\n";
                FileHelper.WriteFile(FileQuartz.LogPath + taskOptions.GroupName + "\\", $"{taskOptions.TaskName}.txt", logContent, true);
            }
            catch (Exception)
            {
            }
            Console.WriteLine(trigger.FullName + " " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:sss") + " " + httpMessage);
            return;
        }
    }
}
