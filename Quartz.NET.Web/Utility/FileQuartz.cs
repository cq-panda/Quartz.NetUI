using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using Quartz.NET.Web.Constant;
using Quartz.NET.Web.Enum;
using Quartz.NET.Web.Extensions;
using Quartz.NET.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Quartz.NET.Web.Utility
{
    public class FileQuartz
    {
        private static string _rootPath { get; set; }

        private static string _logPath { get; set; }
        /// <summary>
        /// 创建作业所在根目录及日志文件夹 
        /// </summary>
        /// <returns></returns>
        public static string CreateQuartzRootPath(IWebHostEnvironment env)
        {
            if (!string.IsNullOrEmpty(_rootPath))
                return _rootPath;
            _rootPath = $"{Directory.GetParent(env.ContentRootPath).FullName}\\{QuartzFileInfo.QuartzSettingsFolder}\\";
            _rootPath = _rootPath.ReplacePath();
            if (!Directory.Exists(_rootPath))
            {
                Directory.CreateDirectory(_rootPath);
            }
            _logPath = _rootPath + QuartzFileInfo.Logs + "\\";
            _logPath = _logPath.ReplacePath();
            //生成日志文件夹
            if (!Directory.Exists(_logPath))
            {
                Directory.CreateDirectory(_logPath);
            }
            return _rootPath;
        }

        /// <summary>
        /// 初始化作业日志文件,以txt作为文件
        /// </summary>
        /// <param name="groupJobName"></param>
        public static void InitGroupJobFileLog(string groupJobName)
        {
            string jobFile = _logPath + groupJobName;
            jobFile = jobFile.ReplacePath();
            if (!File.Exists(jobFile))
            {
                File.Create(jobFile);
            }
        }

        public static List<TaskLog> GetJobRunLog(string taskName, string groupName, int page, int pageSize = 100)
        {
            string path = $"{_logPath}{groupName}\\{taskName}.txt";
            List<TaskLog> list = new List<TaskLog>();

            path = path.ReplacePath();
            if (!File.Exists(path))
                return list;
            var logs = FileHelper.ReadPageLine(path, page, pageSize, true);
            foreach (string item in logs)
            {
                string[] arr = item?.Split('_');
                if (item == "" || arr == null || arr.Length == 0)
                    continue;
                if (arr.Length != 3)
                {
                    list.Add(new TaskLog() { Msg = item });
                    continue;
                }
                list.Add(new TaskLog() { BeginDate = arr[0], EndDate = arr[1], Msg = arr[2] });
            }
            return list.OrderByDescending(x => x.BeginDate).ToList();
        }

        public static void WriteJobConfig(List<TaskOptions> taskList)
        {
            string jobs = JsonConvert.SerializeObject(taskList);
            //写入配置文件
            FileHelper.WriteFile(_rootPath, QuartzFileInfo.JobConfigFileName, jobs);
        }

        public static void WriteStartLog(string content)
        {
            content = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "," + content;
            if (!content.EndsWith("\r\n"))
            {
                content += "\r\n";
            }
            FileHelper.WriteFile(FileQuartz.LogPath, "start.txt", content, true);
        }
        public static void WriteJobAction(JobAction jobAction, ITrigger trigger, string taskName, string groupName)
        {
            WriteJobAction(jobAction, taskName, groupName, trigger == null ? "未找到作业" : "OK");
        }
        public static void WriteJobAction(JobAction jobAction, string taskName, string groupName, string content = null)
        {
            content = $"{jobAction.ToString()} --  {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}  --分组：{groupName},作业：{taskName},消息:{content ?? "OK"}\r\n";
            FileHelper.WriteFile(FileQuartz.LogPath, "action.txt", content, true);
        }

        public static void WriteAccess(string content = null)
        {
            content = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}_{content}\r\n";
            FileHelper.WriteFile(FileQuartz.LogPath, "access.txt", content, true);
        }

        public static string GetAccessLog(int pageSize=1)
        {
            string path = FileQuartz.LogPath + "access.txt";
            path = path.ReplacePath();
            if (!File.Exists(path))
                return "没有找到目录";
            return string.Join("<br/>", FileHelper.ReadPageLine(path, pageSize, 5000, true).ToList());
        }
        public static string RootPath
        {
            get { return _rootPath; }
        }

        public static string LogPath
        {
            get { return _logPath; }
        }
    }
}
