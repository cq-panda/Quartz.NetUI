using System;
using System.Threading.Tasks;

namespace Quartz.NET
{
    public class HelloJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
             Console.Out.WriteLineAsync("Greetings from HelloJob!"+DateTime.Now.ToString("yyyy-MM-dd HH:mm:sss"));
            return Task.CompletedTask;
        }
    }
}
