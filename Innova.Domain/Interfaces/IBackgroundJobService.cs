namespace Innova.Domain.Interfaces;

public interface IBackgroundJobService
{
    string Enqueue(Expression<Action> methodCall);
    string Enqueue(Expression<Func<Task>> methodCall);
    string Schedule(Expression<Action> methodCall, TimeSpan delay);
    string Schedule(Expression<Func<Task>> methodCall, TimeSpan delay);
    void AddOrUpdateRecurringJob(string recurringJobId, Expression<Action> methodCall, string cronExpression);
    void AddOrUpdateRecurringJob(string recurringJobId, Expression<Func<Task>> methodCall, string cronExpression);
    void RemoveRecurringJob(string recurringJobId);
    bool Delete(string jobId);
}
