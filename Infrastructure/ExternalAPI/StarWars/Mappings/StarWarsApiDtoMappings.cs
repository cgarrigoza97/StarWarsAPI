using Application.DTOs.Movies;
using Infrastructure.ExternalAPI.StarWars.DTOs;

namespace Infrastructure.ExternalAPI.StarWars.Mappings;

public static class StarWarsApiDtoMappings
{
    public static MovieSyncDto ToMovieSyncDto(this StarWarsApiFilmDto dto)
    {
        var properties = dto.Properties;
        
        return new MovieSyncDto
        {
            Title = properties.Title,
            EpisodeId = properties.EpisodeId,
            OpeningCrawl = properties.OpeningCrawl,
            Director = properties.Director,
            Producer = properties.Producer,
            ReleaseDate = properties.ReleaseDate,
            ExternalId = dto.Id
        };
    }

    public static IEnumerable<MovieSyncDto> ToMovieSyncDtos(this IEnumerable<StarWarsApiFilmDto> dtos)
    {
        return dtos.Select(dto => dto.ToMovieSyncDto());
    }
}
