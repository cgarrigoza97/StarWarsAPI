using Application.DTOs.Movies;

namespace Application.Interfaces.Movies;

public interface IMovieService
{
    Task<IEnumerable<MovieDto>> GetAllMoviesAsync();
    Task<MovieDto> GetMovieByIdAsync(Guid id);
    Task<MovieDto> CreateMovieAsync(CreateMovieDto createMovieDto);
    Task<MovieDto> UpdateMovieAsync(Guid id, UpdateMovieDto updateMovieDto);
    Task<bool> DeleteMovieAsync(Guid id);
    Task<SyncResultDto> SyncMoviesFromStarWarsApiAsync();
}
