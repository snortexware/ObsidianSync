using G.Sync.Entities.Interfaces;
using G.Sync.Service;
using Ninject;
using Quartz;
using Quartz.Impl;
using System;
using System.Threading.Tasks;

namespace G.Sync.External.IO.Quartz
{
    public class TaskQueueJob : IJob
    {
        [Inject] public ITaskQueueService Service { get; set; }

        public async Task Execute(IJobExecutionContext context)
        {
            await Service.ProcessTaskQueues();
        }
    }

    public class QueueStarter(IKernel kernel) : IQueueStarter
    {
        private readonly IKernel _kernel = kernel;

        public async Task StartQueueProcessor()
        {
            var schedulerFactory = new StdSchedulerFactory();
            var scheduler = await schedulerFactory.GetScheduler();

            scheduler.JobFactory = new NinjectJobFactory(_kernel);

            var jobKey = new JobKey("TaskQueueJob");

            if (!await scheduler.CheckExists(jobKey))
            {
                var job = JobBuilder.Create<TaskQueueJob>()
                    .WithIdentity(jobKey)
                    .Build();

                var trigger = TriggerBuilder.Create()
                    .WithIdentity("TaskQueueTrigger")
                    .StartNow()
                    .WithSimpleSchedule(x => x.WithIntervalInSeconds(3).RepeatForever())
                    .Build();

                await scheduler.ScheduleJob(job, trigger);
            }

            await scheduler.Start();
        }
    }
}
