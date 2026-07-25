using System.Linq.Expressions;
using Application.Interfaces.BackgroundJobs;
using Hangfire;
using Hangfire.Common;


namespace Infrastructure.BackgroundJobs
{
    public class BackgroundJobService : IApplicationBackgroundJobClient
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IRecurringJobManager _recurringJobManager;
        public BackgroundJobService(IBackgroundJobClient backgroundJobClient, IRecurringJobManager recurringJobManager)
        {
            _backgroundJobClient = backgroundJobClient;
            _recurringJobManager = recurringJobManager;
        }
        public void AddOrUpdateRecurring<T>(Expression<Func<T, Task>> job, string jobIdentifier, string cronSchedule)
        {
            _recurringJobManager.AddOrUpdate(jobIdentifier, Job.FromExpression(job), cronSchedule);
        }

        public string Enqueue<T>(Expression<Func<T, Task>> job)
        {
            return _backgroundJobClient.Enqueue(job);
        }

        public string Schedule<T>(Expression<Func<T, Task>> job, DateTimeOffset date)
        {
            return _backgroundJobClient.Schedule(job, date);
        }
    }
}