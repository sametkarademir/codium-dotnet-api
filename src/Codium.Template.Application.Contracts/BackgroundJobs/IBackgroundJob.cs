using Hangfire;

namespace Codium.Template.Application.Contracts.BackgroundJobs;

public interface IBackgroundJob<in TParameter> where TParameter : BackgroundJobArgs
{
    Task Execute(TParameter args, IJobCancellationToken cancellationToken);
}