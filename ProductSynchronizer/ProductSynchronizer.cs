using ProductSynchronizer.Helpers;
using Quartz;
using Quartz.Impl;
using System.ServiceProcess;
using Log = ProductSynchronizer.Logger.Logger;

namespace ProductSynchronizer
{
    public partial class ProductSynchronizer : ServiceBase
    {
        public ProductSynchronizer()
        {
            InitializeComponent();
        }
        protected override void OnStart(string[] args)
        {
            Log.InitLogger();
            Log.WriteLog("Service started.");
            ScheduleJob();
        }

        protected override void OnStop()
        {
        }

        private static void ScheduleJob()
        {
            Log.WriteLog("Scheduling job.");
            var scheduler = StdSchedulerFactory.GetDefaultScheduler().Result;

            scheduler.Start();

            var job = JobBuilder.Create<SyncJob>().Build();

            var trigger = TriggerBuilder.Create()

                .WithIdentity("SyncJob", "SYNC")

                .WithCronSchedule(ConfigHelper.Config.JobCronConfig)
#if DEBUG
                .StartNow()
#endif
                .Build();

            scheduler.ScheduleJob(job, trigger);
            Log.WriteLog("Job successfully scheduled");
        }
    }
}
