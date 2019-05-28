using Quartz.Impl;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace Quartz.NET
{
    class Program
    {
        private static void Main(string[] args)
        {
            // trigger async evaluation
            RunProgram().GetAwaiter().GetResult();
            Console.WriteLine("Press any key to close the application");
            Console.ReadKey();
        }

        private static async Task RunProgram()
        {
            try
            {
                // Grab the Scheduler instance from the Factory
                NameValueCollection props = new NameValueCollection
                {
                    { "quartz.serializer.type", "binary" }
                };
                StdSchedulerFactory factory = new StdSchedulerFactory(props);
                IScheduler scheduler = await factory.GetScheduler();

                // define the job and tie it to our HelloJob class
                IJobDetail job = JobBuilder.Create<HelloJob>()
                    .WithIdentity("job1", "group1")
                    .Build();

                ITrigger trigger = trigger = TriggerBuilder.Create()
                .WithIdentity("trigger1", "group1")
                .WithCronSchedule("0/4 * * * * ? ")
                .Build();

                //// Trigger the job to run now, and then repeat every 10 seconds
                //ITrigger trigger = TriggerBuilder.Create()
                //    .WithIdentity("trigger1", "group1")
                //    .StartNow()
                //    .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(02,30))
                //    .Build();
         //       ITrigger trigger = trigger = TriggerBuilder.Create()
         //.WithIdentity("trigger1", "group1")
         //.WithSimpleSchedule(x => x
         //     .WithIntervalInSeconds(3)//每5秒执行一次
         //     .RepeatForever())
         //// .EndAt(DateBuilder.DateOf(22, 0 0))//晚上22点结束
         //.Build();
                // Tell quartz to schedule the job using our trigger
                await scheduler.ScheduleJob(job, trigger);

                await scheduler.Start();
                // some sleep to show what's happening
               // await Task.Delay(TimeSpan.FromSeconds(60));

                // and last shut down the scheduler when you are ready to close your program
             //   await scheduler.Shutdown();
            }
            catch (SchedulerException se)
            {
                await Console.Error.WriteLineAsync(se.ToString());
            }
        }
    }
}
