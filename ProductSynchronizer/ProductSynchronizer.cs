using System;
using ProductSynchronizer.Helpers;
using ProductSynchronizer.Parsers;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using Quartz;
using Quartz.Impl;

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
            ScheduleJob();
        }

        protected override void OnStop()
        {
        }

        private static void ScheduleJob()
        {
            var scheduler = StdSchedulerFactory.GetDefaultScheduler().Result;

            scheduler.Start();

            var job = JobBuilder.Create<SyncJob>().Build();

            var trigger = TriggerBuilder.Create()

                .WithIdentity("SyncJob", "SYNC")

                .WithCronSchedule(ConfigHelper.Config.JobCronConfig)

                .StartNow()

                .Build();

            scheduler.ScheduleJob(job, trigger);
        }
    }
}
