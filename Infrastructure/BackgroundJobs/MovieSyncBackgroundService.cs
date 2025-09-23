using Application.DTOs.Movies;
using Application.Interfaces.BackgroundServices;
using Application.Interfaces.Movies;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundJobs;

public class MovieSyncBackgroundService : IMovieSyncBackgroundService
{
    private readonly IMovieService _movieService;
    private readonly ILogger<MovieSyncBackgroundService> _logger;

    public MovieSyncBackgroundService(IMovieService movieService, ILogger<MovieSyncBackgroundService> logger)
    {
        _movieService = movieService;
        _logger = logger;
    }

    public async Task<SyncResultDto> SyncMoviesAsync()
    {
        try
        {
            _logger.LogInformation("Starting Star Wars movies synchronization...");
            
            var result = await _movieService.SyncMoviesFromStarWarsApiAsync();
            
            _logger.LogInformation(
                "Movie synchronization completed. Synced: {SyncedMovies}, Updated: {UpdatedMovies}, Total: {TotalMovies}",
                result.SyncedMovies, result.UpdatedMovies, result.TotalMovies);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during movie synchronization");
            throw;
        }
    }

    public void ScheduleRecurringSync()
    {
        RecurringJob.AddOrUpdate(
            "sync-star-wars-movies",
            () => SyncMoviesAsync(),
            Cron.Daily(2));
        
        _logger.LogInformation("Recurring movie sync job scheduled to run daily at 2 AM");
    }

    public string EnqueueImmediateSync()
    {
        var jobId = BackgroundJob.Enqueue(() => SyncMoviesAsync());
        _logger.LogInformation("Immediate movie sync job enqueued with ID: {JobId}", jobId);
        return jobId;
    }
}
