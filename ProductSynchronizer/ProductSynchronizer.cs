using ProductSynchronizer.Helpers;
using ProductSynchronizer.Logger;
using Quartz;
using Quartz.Impl;
using System;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace ProductSynchronizer
{
    public partial class ProductSynchronizer : ServiceBase
    {
        static IScheduler _scheduler;
        public ProductSynchronizer()
        {
            InitializeComponent();
        }
        protected override void OnStart(string[] args)
        {
            Log.WriteLog("Service started.");
            ScheduleJob().GetAwaiter().GetResult();
        }

        protected override void OnStop()
        {
        }

        private static async Task ScheduleJob()
        {
            Log.WriteLog("Scheduling job.");
            _scheduler = await StdSchedulerFactory.GetDefaultScheduler();

            var job = JobBuilder.Create<SyncJob>()
                .WithIdentity("myJob", "SYNC")
                .Build();

            var trigger = TriggerBuilder.Create()

                .WithIdentity("SyncJob", "SYNC")

                .StartNow()

                .WithCronSchedule(ConfigHelper.Config.JobCronConfig)

                .Build();

            await _scheduler.ScheduleJob(job, trigger);

            await _scheduler.TriggerJob(job.Key);

            await _scheduler.Start();

            Log.WriteLog("Job successfully scheduled");
        }
    }
}
