using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Movies;

public class UpdateMovieDto
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Episode ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Episode ID must be a positive number")]
    public int EpisodeId { get; set; }

    [Required(ErrorMessage = "Opening crawl is required")]
    [StringLength(1000, ErrorMessage = "Opening crawl cannot exceed 1000 characters")]
    public string OpeningCrawl { get; set; } = string.Empty;

    [Required(ErrorMessage = "Director is required")]
    [StringLength(100, ErrorMessage = "Director name cannot exceed 100 characters")]
    public string Director { get; set; } = string.Empty;

    [Required(ErrorMessage = "Producer is required")]
    [StringLength(200, ErrorMessage = "Producer name cannot exceed 200 characters")]
    public string Producer { get; set; } = string.Empty;

    [Required(ErrorMessage = "Release date is required")]
    public DateTime ReleaseDate { get; set; }

    [Required(ErrorMessage = "External ID is required")]
    [StringLength(50, ErrorMessage = "External ID cannot exceed 50 characters")]
    public string ExternalId { get; set; } = string.Empty;
}
