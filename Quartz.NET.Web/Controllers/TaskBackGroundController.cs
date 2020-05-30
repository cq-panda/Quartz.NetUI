using Microsoft.AspNetCore.Mvc;
using Quartz.NET.Web.Attr;
using Quartz.NET.Web.Extensions;
using Quartz.NET.Web.Models;
using Quartz.NET.Web.Utility;
using Quartz.Spi;
using System.Threading.Tasks;

namespace Quartz.NET.Web.Controllers
{

    public class TaskBackGroundController : Controller
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IJobFactory _jobFactory;
        public TaskBackGroundController(ISchedulerFactory schedulerFactory, IJobFactory jobFactory)
        {
            this._jobFactory = jobFactory;
            this._schedulerFactory = schedulerFactory;
        }

        public IActionResult Index()
        {
            return View("~/Views/TaskBackGround/Index.cshtml");
        }

        /// <summary>
        /// 获取所有的作业
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> GetJobs()
        {
            return Json(await _schedulerFactory.GetJobs());
        }
        /// <summary>
        /// 获取作业运行日志
        /// </summary>
        /// <param name="taskName"></param>
        /// <param name="groupName"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public IActionResult GetRunLog(string taskName, string groupName, int page = 1)
        {
            return Json(FileQuartz.GetJobRunLog(taskName, groupName, page));
        }
        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="taskOptions"></param>
        /// <returns></returns>
        [TaskAuthor]
        public async Task<IActionResult> Add(TaskOptions taskOptions)
        {
            return Json(await taskOptions.AddJob(_schedulerFactory,jobFactory:_jobFactory));
        }
        [TaskAuthor]
        public async Task<IActionResult> Remove(TaskOptions taskOptions)
        {
            return Json(await _schedulerFactory.Remove(taskOptions));
        }
        [TaskAuthor]
        public async Task<IActionResult> Update(TaskOptions taskOptions)
        {
            return Json(await _schedulerFactory.Update(taskOptions));
        }
        [TaskAuthor]
        public async Task<IActionResult> Pause(TaskOptions taskOptions)
        {
            return Json(await _schedulerFactory.Pause(taskOptions));
        }
        [TaskAuthor]
        public async Task<IActionResult> Start(TaskOptions taskOptions)
        {
            return Json(await _schedulerFactory.Start(taskOptions));
        }
        [TaskAuthor]
        public async Task<IActionResult> Run(TaskOptions taskOptions)
        {
            return Json(await _schedulerFactory.Run(taskOptions));
        }
    }




}