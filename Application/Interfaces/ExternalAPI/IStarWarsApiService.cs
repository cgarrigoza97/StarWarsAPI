using Application.DTOs.Movies;

namespace Application.Interfaces.ExternalAPI;

public interface IStarWarsApiService
{
    Task<IEnumerable<MovieSyncDto>> GetAllFilmsAsync();
}