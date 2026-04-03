
using Ninject;
using Quartz;
using Quartz.Spi;
namespace G.Sync.External.IO.Quartz
{
    public class NinjectJobFactory(IKernel kernel) : IJobFactory
    {
        private readonly IKernel _kernel = kernel;

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return (IJob)_kernel.Get(bundle.JobDetail.JobType);
        }
        public void ReturnJob(IJob job)
        {
        }
    }
}
