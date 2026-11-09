using System.Text.Json;
using Codium.Template.Application.Contracts.BackgroundJobs;
using Codium.Template.Domain.Shared.Extensions;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace Codium.Template.Application.BackgroundJobs;

public class HangfireJobExecutor(ILogger<HangfireJobExecutor> logger) : IBackgroundJobExecutor
{
    public void Enqueue<TJob, TParameter>(TParameter args)
        where TJob : IBackgroundJob<TParameter> where TParameter : BackgroundJobArgs
    {
        var logDetails = new Dictionary<string, object>()
        {
            { "JobName", typeof(TJob).Name },
            { "JobId", args.JobId },
            { "ScheduledAt", args.ScheduledAt },
            { "Parameters", JsonSerializer.Serialize(args) }
        };
        
        logger
            .WithProperties()
            .AddRange(logDetails)
            .LogInformation("Enqueue job");

        try
        {
            BackgroundJob.Enqueue<TJob>(job => job.Execute(args, JobCancellationToken.Null));
        }
        catch (Exception e)
        {
            logger
                .WithProperties()
                .AddRange(logDetails)
                .LogError("Failed to enqueue job", e);
            
            throw;
        }
    }

    public void Schedule<TJob, TParameter>(TParameter args, TimeSpan delay)
        where TJob : IBackgroundJob<TParameter> where TParameter : BackgroundJobArgs
    {
        var logDetails = new Dictionary<string, object>()
        {
            { "JobName", typeof(TJob).Name },
            { "JobId", args.JobId },
            { "ScheduledAt", args.ScheduledAt },
            { "DelayInMinutes", delay.TotalMinutes },
            { "Parameters", JsonSerializer.Serialize(args) }
        };
        
        logger
            .WithProperties()
            .AddRange(logDetails)
            .LogInformation("Schedule job");

        try
        {
            BackgroundJob.Schedule<TJob>(job => job.Execute(args, JobCancellationToken.Null), delay);
        }
        catch (Exception e)
        {
            logger
                .WithProperties()
                .AddRange(logDetails)
                .LogError("Failed to schedule job", e);
            
            throw;
        }
    }
}