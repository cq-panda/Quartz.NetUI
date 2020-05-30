using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Quartz.Impl.Triggers;
using Quartz.NET.Web.Constant;
using Quartz.NET.Web.Enum;
using Quartz.NET.Web.Models;
using Quartz.NET.Web.Utility;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quartz.NET.Web.Extensions
{
    public static class QuartzNETExtension
    {
        private static List<TaskOptions> _taskList = new List<TaskOptions>();

        /// <summary>
        /// 初始化作业
        /// </summary>
        /// <param name="applicationBuilder"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseQuartz(this IApplicationBuilder applicationBuilder, IWebHostEnvironment env)
        {
            IServiceProvider services = applicationBuilder.ApplicationServices;

            ISchedulerFactory _schedulerFactory = services.GetService<ISchedulerFactory>();

            string path = FileQuartz.CreateQuartzRootPath(env);
            string jobConfig = FileHelper.ReadFile(path + QuartzFileInfo.JobConfigFileName);
            if (string.IsNullOrEmpty(jobConfig))
            {
                FileHelper.WriteFile(FileQuartz.LogPath, "start.txt", $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},没有默认配置任务\r\n", true);
                return applicationBuilder;
            }

            int errorCount = 0;
            string errorMsg = "";
            TaskOptions options = null;
            try
            {
                _taskList = JsonConvert.DeserializeObject<List<TaskOptions>>(jobConfig);
                _taskList.ForEach(x =>
                {
                    options = x;
                    var result = x.AddJob(_schedulerFactory, true, jobFactory: services.GetService<IJobFactory>()).GetAwaiter().GetResult();
                });
            }
            catch (Exception ex)
            {
                errorCount = +1;
                errorMsg += $"作业:{options?.TaskName},异常：{ex.Message}";
            }
            string content = $"成功:{   _taskList.Count - errorCount}个,失败{errorCount}个,异常：{errorMsg}\r\n";
            FileQuartz.WriteStartLog(content);


            return applicationBuilder;
        }

        /// <summary>
        /// 获取所有的作业
        /// </summary>
        /// <param name="schedulerFactory"></param>
        /// <returns></returns>
        public static async Task<List<TaskOptions>> GetJobs(this ISchedulerFactory schedulerFactory)
        {
            List<TaskOptions> list = new List<TaskOptions>();
            try
            {
                IScheduler _scheduler = await schedulerFactory.GetScheduler();
                var groups = await _scheduler.GetJobGroupNames();
                foreach (var groupName in groups)
                {
                    foreach (var jobKey in await _scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(groupName)))
                    {
                        TaskOptions taskOptions = _taskList.Where(x => x.GroupName == jobKey.Group && x.TaskName == jobKey.Name)
                            .FirstOrDefault();
                        if (taskOptions == null)
                            continue;

                        var triggers = await _scheduler.GetTriggersOfJob(jobKey);
                        foreach (ITrigger trigger in triggers)
                        {
                            DateTimeOffset? dateTimeOffset = trigger.GetPreviousFireTimeUtc();
                            if (dateTimeOffset != null)
                            {
                                taskOptions.LastRunTime = Convert.ToDateTime(dateTimeOffset.ToString());
                            }
                            else
                            {
                                var runlog = FileQuartz.GetJobRunLog(taskOptions.TaskName, taskOptions.GroupName, 1, 2);
                                if (runlog.Count > 0)
                                {
                                    DateTime.TryParse(runlog[0].BeginDate, out DateTime lastRunTime);
                                    taskOptions.LastRunTime = lastRunTime;
                                }
                            }
                        }
                        list.Add(taskOptions);
                    }
                }
            }
            catch (Exception ex)
            {
                FileQuartz.WriteStartLog("获取作业异常：" + ex.Message + ex.StackTrace);
            }
            return list;
        }

        /// <summary>
        /// 添加作业
        /// </summary>
        /// <param name="taskOptions"></param>
        /// <param name="schedulerFactory"></param>
        /// <param name="init">是否初始化,否=需要重新生成配置文件，是=不重新生成配置文件</param>
        /// <returns></returns>
        public static async Task<object> AddJob(this TaskOptions taskOptions, ISchedulerFactory schedulerFactory, bool init = false, IJobFactory jobFactory = null)
        {
            try
            {
                (bool, string) validExpression = taskOptions.Interval.IsValidExpression();
                if (!validExpression.Item1)
                    return new { status = false, msg = validExpression.Item2 };

                (bool, object) result = taskOptions.Exists(init);
                if (!result.Item1)
                    return result.Item2;
                if (!init)
                {
                    _taskList.Add(taskOptions);
                    FileQuartz.WriteJobConfig(_taskList);
                }

                IJobDetail job = JobBuilder.Create<HttpResultfulJob>()
               .WithIdentity(taskOptions.TaskName, taskOptions.GroupName)
              .Build();
                ITrigger trigger = TriggerBuilder.Create()
                   .WithIdentity(taskOptions.TaskName, taskOptions.GroupName)
                   .StartNow()
                   .WithDescription(taskOptions.Describe)
                   .WithCronSchedule(taskOptions.Interval)
                   .Build();

                IScheduler scheduler = await schedulerFactory.GetScheduler();

                if (jobFactory == null)
                {
                    try
                    {
                        jobFactory = HttpContext.Current.RequestServices.GetService<IJobFactory>();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"创建任务[{taskOptions.TaskName}]异常,{ex.Message}");
                    }
                }

                if (jobFactory != null)
                {
                    scheduler.JobFactory = jobFactory;
                }

                await scheduler.ScheduleJob(job, trigger);
                if (taskOptions.Status == (int)TriggerState.Normal)
                {
                    await scheduler.Start();
                }
                else
                {
                    await schedulerFactory.Pause(taskOptions);
                    FileQuartz.WriteStartLog($"作业:{taskOptions.TaskName},分组:{taskOptions.GroupName},新建时未启动原因,状态为:{taskOptions.Status}");
                }
                if (!init)
                    FileQuartz.WriteJobAction(JobAction.新增, taskOptions.TaskName, taskOptions.GroupName);
            }
            catch (Exception ex)
            {
                return new { status = false, msg = ex.Message };
            }
            return new { status = true };
        }

        /// <summary>
        /// 移除作业
        /// </summary>
        /// <param name="schedulerFactory"></param>
        /// <param name="taskName"></param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public static Task<object> Remove(this ISchedulerFactory schedulerFactory, TaskOptions taskOptions)
        {
            return schedulerFactory.TriggerAction(taskOptions.TaskName, taskOptions.GroupName, JobAction.删除, taskOptions);
        }

        /// <summary>
        /// 更新作业
        /// </summary>
        /// <param name="schedulerFactory"></param>
        /// <param name="taskOptions"></param>
        /// <returns></returns>
        public static Task<object> Update(this ISchedulerFactory schedulerFactory, TaskOptions taskOptions)
        {
            return schedulerFactory.TriggerAction(taskOptions.TaskName, taskOptions.GroupName, JobAction.修改, taskOptions);
        }

        /// <summary>
        /// 暂停作业
        /// </summary>
        /// <param name="schedulerFactory"></param>
        /// <param name="taskOptions"></param>
        /// <returns></returns>
        public static Task<object> Pause(this ISchedulerFactory schedulerFactory, TaskOptions taskOptions)
        {
            return schedulerFactory.TriggerAction(taskOptions.TaskName, taskOptions.GroupName, JobAction.暂停, taskOptions);
        }

        /// <summary>
        /// 启动作业
        /// </summary>
        /// <param name="schedulerFactory"></param>
        /// <param name="taskOptions"></param>
        /// <returns></returns>
        public static Task<object> Start(this ISchedulerFactory schedulerFactory, TaskOptions taskOptions)
        {
            return schedulerFactory.TriggerAction(taskOptions.TaskName, taskOptions.GroupName, JobAction.开启, taskOptions);
        }

        /// <summary>
        /// 立即执行一次作业
        /// </summary>
        /// <param name="schedulerFactory"></param>
        /// <param name="taskOptions"></param>
        /// <returns></returns>
        public static Task<object> Run(this ISchedulerFactory schedulerFactory, TaskOptions taskOptions)
        {
            return schedulerFactory.TriggerAction(taskOptions.TaskName, taskOptions.GroupName, JobAction.立即执行, taskOptions);
        }

        public static object ModifyTaskEntity(this TaskOptions taskOptions, ISchedulerFactory schedulerFactory, JobAction action)
        {
            TaskOptions options = null;
            object result = null;
            switch (action)
            {
                case JobAction.删除:
                    for (int i = 0; i < _taskList.Count; i++)
                    {
                        options = _taskList[i];
                        if (options.TaskName == taskOptions.TaskName && options.GroupName == taskOptions.GroupName)
                        {
                            _taskList.RemoveAt(i);
                        }
                    }
                    break;
                case JobAction.修改:
                    options = _taskList.Where(x => x.TaskName == taskOptions.TaskName && x.GroupName == taskOptions.GroupName).FirstOrDefault();
                    //移除以前的配置
                    if (options != null)
                    {
                        _taskList.Remove(options);
                    }

                    //生成任务并添加新配置
                    result = taskOptions.AddJob(schedulerFactory, false).GetAwaiter().GetResult();
                    break;
                case JobAction.暂停:
                case JobAction.开启:
                case JobAction.停止:
                case JobAction.立即执行:
                    options = _taskList.Where(x => x.TaskName == taskOptions.TaskName && x.GroupName == taskOptions.GroupName).FirstOrDefault();
                    if (action == JobAction.暂停)
                    {
                        options.Status = (int)TriggerState.Paused;
                    }
                    else if (action == JobAction.停止)
                    {
                        options.Status = (int)action;
                    }
                    else
                    {
                        options.Status = (int)TriggerState.Normal;
                    }
                    break;
            }
            //生成配置文件
            FileQuartz.WriteJobConfig(_taskList);
            FileQuartz.WriteJobAction(action, taskOptions.TaskName, taskOptions.GroupName, "操作对象：" + JsonConvert.SerializeObject(taskOptions));
            return result;
        }

        /// <summary>
        /// 触发新增、删除、修改、暂停、启用、立即执行事件
        /// </summary>
        /// <param name="schedulerFactory"></param>
        /// <param name="taskName"></param>
        /// <param name="groupName"></param>
        /// <param name="action"></param>
        /// <param name="taskOptions"></param>
        /// <returns></returns>
        public static async Task<object> TriggerAction(this ISchedulerFactory schedulerFactory, string taskName, string groupName, JobAction action, TaskOptions taskOptions = null)
        {
            string errorMsg = "";
            try
            {
                IScheduler scheduler = await schedulerFactory.GetScheduler();
                List<JobKey> jobKeys = scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(groupName)).Result.ToList();
                if (jobKeys == null || jobKeys.Count() == 0)
                {
                    errorMsg = $"未找到分组[{groupName}]";
                    return new { status = false, msg = errorMsg };
                }
                JobKey jobKey = jobKeys.Where(s => scheduler.GetTriggersOfJob(s).Result.Any(x => (x as CronTriggerImpl).Name == taskName)).FirstOrDefault();
                if (jobKey == null)
                {
                    errorMsg = $"未找到触发器[{taskName}]";
                    return new { status = false, msg = errorMsg };
                }
                var triggers = await scheduler.GetTriggersOfJob(jobKey);
                ITrigger trigger = triggers?.Where(x => (x as CronTriggerImpl).Name == taskName).FirstOrDefault();

                if (trigger == null)
                {
                    errorMsg = $"未找到触发器[{taskName}]";
                    return new { status = false, msg = errorMsg };
                }
                object result = null;
                switch (action)
                {
                    case JobAction.删除:
                    case JobAction.修改:
                        await scheduler.PauseTrigger(trigger.Key);
                        await scheduler.UnscheduleJob(trigger.Key);// 移除触发器
                        await scheduler.DeleteJob(trigger.JobKey);
                        result = taskOptions.ModifyTaskEntity(schedulerFactory, action);
                        break;
                    case JobAction.暂停:
                    case JobAction.停止:
                    case JobAction.开启:
                        result = taskOptions.ModifyTaskEntity(schedulerFactory, action);
                        if (action == JobAction.暂停)
                        {
                            await scheduler.PauseTrigger(trigger.Key);
                        }
                        else if (action == JobAction.开启)
                        {
                            await scheduler.ResumeTrigger(trigger.Key);
                            //   await scheduler.RescheduleJob(trigger.Key, trigger);
                        }
                        else
                        {
                            await scheduler.Shutdown();
                        }
                        break;
                    case JobAction.立即执行:
                        await scheduler.TriggerJob(jobKey);
                        break;
                }
                return result ?? new { status = true, msg = $"作业{action.ToString()}成功" };
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                return new { status = false, msg = ex.Message };
            }
            finally
            {
                FileQuartz.WriteJobAction(action, taskName, groupName, errorMsg);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>通过作业上下文获取作业对应的配置参数
        /// <returns></returns>
        public static TaskOptions GetTaskOptions(this IJobExecutionContext context)
        {
            AbstractTrigger trigger = (context as JobExecutionContextImpl).Trigger as AbstractTrigger;
            TaskOptions taskOptions = _taskList.Where(x => x.TaskName == trigger.Name && x.GroupName == trigger.Group).FirstOrDefault();
            return taskOptions ?? _taskList.Where(x => x.TaskName == trigger.JobName && x.GroupName == trigger.JobGroup).FirstOrDefault();
        }

        /// <summary>
        /// 作业是否存在
        /// </summary>
        /// <param name="taskOptions"></param>
        /// <param name="init">初始化的不需要判断</param>
        /// <returns></returns>
        public static (bool, object) Exists(this TaskOptions taskOptions, bool init)
        {
            if (!init && _taskList.Any(x => x.TaskName == taskOptions.TaskName && x.GroupName == taskOptions.GroupName))
            {
                return (false,
                    new
                    {
                        status = false,
                        msg = $"作业:{taskOptions.TaskName},分组：{taskOptions.GroupName}已经存在"
                    });
            }
            return (true, null);
        }

        public static (bool, string) IsValidExpression(this string cronExpression)
        {
            try
            {
                CronTriggerImpl trigger = new CronTriggerImpl();
                trigger.CronExpressionString = cronExpression;
                DateTimeOffset? date = trigger.ComputeFirstFireTimeUtc(null);
                return (date != null, date == null ? $"请确认表达式{cronExpression}是否正确!" : "");
            }
            catch (Exception e)
            {
                return (false, $"请确认表达式{cronExpression}是否正确!{e.Message}");
            }
        }
    }

}
