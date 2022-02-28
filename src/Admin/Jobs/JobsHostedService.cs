﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Bit.Core.Jobs;
using Bit.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Bit.Admin.Jobs
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
            var timeZone = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
                TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time") :
                TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
            if (_globalSettings.SelfHosted)
            {
                timeZone = TimeZoneInfo.Local;
            }

            var everyTopOfTheHourTrigger = TriggerBuilder.Create()
                .WithIdentity("EveryTopOfTheHourTrigger")
                .StartNow()
                .WithCronSchedule("0 0 * * * ?")
                .Build();
            var everyFiveMinutesTrigger = TriggerBuilder.Create()
                .WithIdentity("EveryFiveMinutesTrigger")
                .StartNow()
                .WithCronSchedule("0 */5 * * * ?")
                .Build();
            var everyFridayAt10pmTrigger = TriggerBuilder.Create()
                .WithIdentity("EveryFridayAt10pmTrigger")
                .StartNow()
                .WithCronSchedule("0 0 22 ? * FRI", x => x.InTimeZone(timeZone))
                .Build();
            var everySaturdayAtMidnightTrigger = TriggerBuilder.Create()
                .WithIdentity("EverySaturdayAtMidnightTrigger")
                .StartNow()
                .WithCronSchedule("0 0 0 ? * SAT", x => x.InTimeZone(timeZone))
                .Build();
            var everySundayAtMidnightTrigger = TriggerBuilder.Create()
                .WithIdentity("EverySundayAtMidnightTrigger")
                .StartNow()
                .WithCronSchedule("0 0 0 ? * SUN", x => x.InTimeZone(timeZone))
                .Build();
            var everyDayAtMidnightUtc = TriggerBuilder.Create()
                .WithIdentity("EveryDayAtMidnightUtc")
                .StartNow()
                .WithCronSchedule("0 0 0 * * ?")
                .Build();

            var jobs = new List<Tuple<IJobDetail, ITrigger>>
            {
                new Tuple<IJobDetail, ITrigger>(CreateDefaultJob(typeof(DeleteSendsJob)), everyFiveMinutesTrigger),
                new Tuple<IJobDetail, ITrigger>(CreateDefaultJob(typeof(DatabaseExpiredGrantsJob)), everyFridayAt10pmTrigger),
                new Tuple<IJobDetail, ITrigger>(CreateDefaultJob(typeof(DatabaseUpdateStatisticsJob)), everySaturdayAtMidnightTrigger),
                new Tuple<IJobDetail, ITrigger>(CreateDefaultJob(typeof(DatabaseRebuildlIndexesJob)), everySundayAtMidnightTrigger),
                new Tuple<IJobDetail, ITrigger>(CreateDefaultJob(typeof(DeleteCiphersJob)), everyDayAtMidnightUtc)
            };

            if (!_globalSettings.SelfHosted)
            {
                jobs.Add(new Tuple<IJobDetail, ITrigger>(CreateDefaultJob(typeof(AliveJob)), everyTopOfTheHourTrigger));
            }

            Jobs = jobs;
            await base.StartAsync(cancellationToken);
        }

        public static void AddJobsServices(IServiceCollection services, bool selfHosted)
        {
            if (!selfHosted)
            {
                services.AddTransient<AliveJob>();
            }
            services.AddTransient<DatabaseUpdateStatisticsJob>();
            services.AddTransient<DatabaseRebuildlIndexesJob>();
            services.AddTransient<DatabaseExpiredGrantsJob>();
            services.AddTransient<DeleteSendsJob>();
            services.AddTransient<DeleteCiphersJob>();
        }
    }
}
