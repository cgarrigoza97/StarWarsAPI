namespace Application.DTOs.Movies;

public class MovieSyncDto
{
    public string Title { get; set; } = string.Empty;
    public int EpisodeId { get; set; }
    public string OpeningCrawl { get; set; } = string.Empty;
    public string Director { get; set; } = string.Empty;
    public string Producer { get; set; } = string.Empty;
    public string ReleaseDate { get; set; } = string.Empty;
    public string ExternalId { get; set; } = string.Empty;
}