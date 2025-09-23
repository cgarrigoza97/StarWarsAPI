using Application.DTOs.Movies;
using Domain.Entities;

namespace Application.Mappings;

public static class MovieMappings
{
    public static Movie ToEntity(this CreateMovieDto dto)
    {
        return new Movie
        {
            Title = dto.Title,
            EpisodeId = dto.EpisodeId,
            OpeningCrawl = dto.OpeningCrawl,
            Director = dto.Director,
            Producer = dto.Producer,
            ReleaseDate = dto.ReleaseDate,
            ExternalId = dto.ExternalId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static MovieDto ToDto(this Movie movie)
    {
        return new MovieDto
        {
            Id = movie.Id,
            Title = movie.Title,
            EpisodeId = movie.EpisodeId,
            OpeningCrawl = movie.OpeningCrawl,
            Director = movie.Director,
            Producer = movie.Producer,
            ReleaseDate = movie.ReleaseDate,
            ExternalId = movie.ExternalId,
            CreatedAt = movie.CreatedAt,
            UpdatedAt = movie.UpdatedAt
        };
    }

    public static void UpdateEntity(this Movie movie, UpdateMovieDto dto)
    {
        movie.Title = dto.Title;
        movie.EpisodeId = dto.EpisodeId;
        movie.OpeningCrawl = dto.OpeningCrawl;
        movie.Director = dto.Director;
        movie.Producer = dto.Producer;
        movie.ReleaseDate = dto.ReleaseDate;
        movie.ExternalId = dto.ExternalId;
        movie.UpdatedAt = DateTime.UtcNow;
    }
}
