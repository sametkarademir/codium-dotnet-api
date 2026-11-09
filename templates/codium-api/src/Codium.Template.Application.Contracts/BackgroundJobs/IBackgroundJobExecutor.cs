namespace Codium.Template.Application.Contracts.BackgroundJobs;

public interface IBackgroundJobExecutor
{
    void Enqueue<TJob, TParameter>(TParameter args) where TJob : IBackgroundJob<TParameter> where TParameter : BackgroundJobArgs;

    void Schedule<TJob, TParameter>(TParameter args, TimeSpan delay) where TJob : IBackgroundJob<TParameter> where TParameter : BackgroundJobArgs;
}