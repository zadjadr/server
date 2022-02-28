﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bit.Core.Jobs;
using Bit.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Bit.Notifications.Jobs
{
    public class JobsHostedService : BaseJobsHostedService
    {
        public JobsHostedService(
            GlobalSettings globalSettings,
            IServiceProvider serviceProvider,
            ILogger<JobsHostedService> logger,
            ILogger<JobListener> listenerLogger)
            : base(globalSettings, serviceProvider, logger, listenerLogger) { }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            var everyFiveMinutesTrigger = TriggerBuilder.Create()
                .WithIdentity("EveryFiveMinutesTrigger")
                .StartNow()
                .WithCronSchedule("0 */30 * * * ?")
                .Build();

            Jobs = new List<Tuple<IJobDetail, ITrigger>>
            {
                new Tuple<IJobDetail, ITrigger>(CreateDefaultJob(typeof(LogConnectionCounterJob)), everyFiveMinutesTrigger)
            };

            await base.StartAsync(cancellationToken);
        }

        public static void AddJobsServices(IServiceCollection services)
        {
            services.AddTransient<LogConnectionCounterJob>();
        }
    }
}
