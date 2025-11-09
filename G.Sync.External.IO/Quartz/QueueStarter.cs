using G.Sync.Service;
using Quartz;
using Quartz.Impl;
using System;
using System.Threading.Tasks;

namespace G.Sync.External.IO.Quartz
{
    public class TaskQueueJob : IJob
    {
        private readonly TaskQueueService _service = new();
        public async Task Execute(IJobExecutionContext context) =>
            await Task.Run(() => _service.ProcessTaskQueues());
    }

    public class QueueStarter
    {
        public async Task StartQueueProcessor()
        {
            var schedulerFactory = new StdSchedulerFactory();
            var scheduler = await schedulerFactory.GetScheduler();

            var jobKey = new JobKey("TaskQueueJob");

            if (!await scheduler.CheckExists(jobKey))
            {
                var job = JobBuilder.Create<TaskQueueJob>().WithIdentity(jobKey).Build();
                var trigger = TriggerBuilder.Create()
                    .WithIdentity("TaskQueueTrigger")
                    .StartNow()
                    .WithSimpleSchedule(x => x.WithIntervalInSeconds(10).RepeatForever())
                    .Build();

                await scheduler.ScheduleJob(job, trigger);
            }

            await scheduler.Start();
            Console.WriteLine("Queue processor started.");
        }
    }
}
