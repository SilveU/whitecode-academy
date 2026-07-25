using System.Linq.Expressions;

namespace Application.Interfaces.BackgroundJobs
{
    public interface IApplicationBackgroundJobClient
    {
        string Enqueue<T>(Expression<Func<T, Task>> job);
        string Schedule<T>(Expression<Func<T, Task>> job, DateTimeOffset date);
        void AddOrUpdateRecurring<T>(Expression<Func<T, Task>> job, string jobIdentifier, string cronSchedule);
    }
}