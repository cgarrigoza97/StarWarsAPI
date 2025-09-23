namespace Application.Interfaces.BackgroundServices;

public interface IMovieSyncBackgroundService
{
    void ScheduleRecurringSync();
    string EnqueueImmediateSync();
}
