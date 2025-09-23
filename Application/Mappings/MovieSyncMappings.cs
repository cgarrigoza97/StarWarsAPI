using System.Globalization;
using Application.DTOs.Movies;
using Domain.Entities;

namespace Application.Mappings;

public static class MovieSyncMappings
{
    public static Movie ToEntity(this MovieSyncDto dto)
    {
        return new Movie
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            EpisodeId = dto.EpisodeId,
            OpeningCrawl = dto.OpeningCrawl,
            Director = dto.Director,
            Producer = dto.Producer,
            ReleaseDate = DateTime.TryParseExact(dto.ReleaseDate, "yyyy-MM-dd", 
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var releaseDate) 
                ? releaseDate : DateTime.MinValue,
            ExternalId = dto.ExternalId,
            CreatedAt = DateTime.UtcNow
        };
    }
}
