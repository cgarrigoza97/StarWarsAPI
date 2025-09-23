using Application.DTOs.Movies;
using Application.Interfaces.ExternalAPI;
using Application.Interfaces.Movies;
using Application.Mappings;

namespace Application.Services.Movies;

public class MovieService : IMovieService
{
    private readonly IMovieRepository _movieRepository;
    private readonly IStarWarsApiService _starWarsApiService;

    public MovieService(IMovieRepository movieRepository, IStarWarsApiService starWarsApiService)
    {
        _movieRepository = movieRepository;
        _starWarsApiService = starWarsApiService;
    }

    public async Task<IEnumerable<MovieDto>> GetAllMoviesAsync()
    {
        var movies = await _movieRepository.GetAllAsync();
        return movies.Select(m => m.ToDto());
    }

    public async Task<MovieDto?> GetMovieByIdAsync(Guid id)
    {
        var movie = await _movieRepository.GetByIdAsync(id);

        if (movie == null)
            throw new KeyNotFoundException($"Movie with ID {id} does not exist");
            
        return movie?.ToDto();
    }

    public async Task<MovieDto> CreateMovieAsync(CreateMovieDto createMovieDto)
    {
        var movie = createMovieDto.ToEntity();
        var createdMovie = await _movieRepository.CreateAsync(movie);
        return createdMovie.ToDto();
    }

    public async Task<MovieDto?> UpdateMovieAsync(Guid id, UpdateMovieDto updateMovieDto)
    {
        var movie = await _movieRepository.GetByIdAsync(id);
        if (movie == null)
            throw new KeyNotFoundException($"Movie with ID {id} does not exist");

        movie.UpdateEntity(updateMovieDto);
        var updatedMovie = await _movieRepository.UpdateAsync(movie);
        return updatedMovie.ToDto();
    }

    public async Task<bool> DeleteMovieAsync(Guid id)
    {
        var result = await _movieRepository.DeleteAsync(id);
        if (!result)
            throw new KeyNotFoundException($"Movie with ID {id} does not exist");
        return result;
    }

    public async Task<SyncResultDto> SyncMoviesFromStarWarsApiAsync()
    {
        var starWarsFilms = await _starWarsApiService.GetAllFilmsAsync();
        var syncedMovies = 0;
        var updatedMovies = 0;

        foreach (var film in starWarsFilms)
        {
            var existingMovie = await _movieRepository.GetByExternalIdAsync(film.ExternalId);
            
            if (existingMovie == null)
            {
                var newMovie = film.ToEntity();
                await _movieRepository.CreateAsync(newMovie);
                syncedMovies++;
            }
            else
            {
                existingMovie.Title = film.Title;
                existingMovie.EpisodeId = film.EpisodeId;
                existingMovie.OpeningCrawl = film.OpeningCrawl;
                existingMovie.Director = film.Director;
                existingMovie.Producer = film.Producer;
                existingMovie.ReleaseDate = DateTime.TryParseExact(film.ReleaseDate, "yyyy-MM-dd", 
                    System.Globalization.CultureInfo.InvariantCulture, 
                    System.Globalization.DateTimeStyles.None, out var releaseDate) 
                    ? releaseDate : existingMovie.ReleaseDate;
                existingMovie.UpdatedAt = DateTime.UtcNow;

                await _movieRepository.UpdateAsync(existingMovie);
                updatedMovies++;
            }
        }

        return new SyncResultDto
        {
            SyncedMovies = syncedMovies,
            UpdatedMovies = updatedMovies,
            TotalMovies = syncedMovies + updatedMovies
        };
    }
}
