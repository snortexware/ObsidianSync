using G.Sync.Repository;
using G.Sync.Service;
using G.Sync.Service.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace G.Sync.External.IO.Quartz
{
    public class TaskQueueJob : IJob
    {
        private readonly TaskQueueService _taskQueueService;

        public TaskQueueJob(TaskQueueService taskQueueService)
        {
            _taskQueueService = taskQueueService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await Task.Delay(5000); 
            await Task.Run(() => _taskQueueService.ProcessTaskQueues());
        }
    }

    public class QueueStarter
    {
        public async Task StartQueueProcessor()
        {
            var services = new ServiceCollection();

            services.AddDbContext<GSyncContext>();
            services.AddScoped<ITaskQueueRepository, TaskQueueRepository>();
            services.AddScoped<IFolderFileService, FolderFileService>();
            services.AddScoped<TaskQueueService>();

            services.AddSingleton<IJobFactory, SingletonJobFactory>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            services.AddScoped<TaskQueueJob>();

            var serviceProvider = services.BuildServiceProvider();
            var schedulerFactory = serviceProvider.GetRequiredService<ISchedulerFactory>();
            var scheduler = await schedulerFactory.GetScheduler();
            scheduler.JobFactory = serviceProvider.GetRequiredService<IJobFactory>();

            var jobKey = new JobKey("TaskQueueJob");

            if (!await scheduler.CheckExists(jobKey))
            {
                var job = JobBuilder.Create<TaskQueueJob>()
                    .WithIdentity(jobKey)
                    .Build();

                var trigger = TriggerBuilder.Create()
                    .WithIdentity("TaskQueueTrigger")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(10) 
                        .RepeatForever())
                    .Build();

                await scheduler.ScheduleJob(job, trigger);
            }

            await scheduler.Start();
            Console.WriteLine("Queue processor started.");
        }
    }

    public class SingletonJobFactory : IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public SingletonJobFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return _serviceProvider.GetRequiredService(bundle.JobDetail.JobType) as IJob;
        }

        public void ReturnJob(IJob job) { }
    }
}
