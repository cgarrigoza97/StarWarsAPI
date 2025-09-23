using Application.DTOs.Common;
using Application.DTOs.Movies;
using Application.Interfaces.BackgroundServices;
using Application.Interfaces.Movies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace StarWarsWebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[SwaggerTag("Star Wars movies management endpoints")]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;
    private readonly IMovieSyncBackgroundService _movieSyncBackgroundService;

    public MoviesController(IMovieService movieService, IMovieSyncBackgroundService movieSyncBackgroundService)
    {
        _movieService = movieService;
        _movieSyncBackgroundService = movieSyncBackgroundService;
    }

    /// <summary>
    /// Get all Star Wars movies
    /// </summary>
    /// <returns>List of all Star Wars movies</returns>
    /// <response code="200">Successfully retrieved all movies</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    [HttpGet]
    [Authorize(Policy = "UserPolicy")]
    [SwaggerOperation(
        Summary = "Get all movies",
        Description = "Retrieves a list of all Star Wars movies. Available to all users"
    )]
    [SwaggerResponse(200, "Successfully retrieved all movies", typeof(IEnumerable<MovieDto>))]
    [SwaggerResponse(401, "Unauthorized - JWT token required", typeof(ErrorResponseDto))]
    public async Task<ActionResult<IEnumerable<MovieDto>>> GetAllMovies()
    {
        var movies = await _movieService.GetAllMoviesAsync();
        return Ok(movies);
    }

    /// <summary>
    /// Get a specific movie by ID
    /// </summary>
    /// <param name="id">The id of the movie</param>
    /// <returns>Movie details</returns>
    /// <response code="200">Successfully retrieved movie details</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="404">Movie not found</response>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "UserPolicy")]
    [SwaggerOperation(
        Summary = "Get movie by ID",
        Description = "Retrieves the information about a specific movie by ID. Available to all users"
    )]
    [SwaggerResponse(200, "Successfully retrieved movie details", typeof(MovieDto))]
    [SwaggerResponse(401, "Unauthorized - JWT token required", typeof(ErrorResponseDto))]
    [SwaggerResponse(404, "Movie not found", typeof(ErrorResponseDto))]
    public async Task<ActionResult<MovieDto>> GetMovieById(Guid id)
    {
        var movie = await _movieService.GetMovieByIdAsync(id);
        return Ok(movie);
    }

    /// <summary>
    /// Create a new Star Wars movie
    /// </summary>
    /// <param name="createMovieDto">Movie creation details</param>
    /// <returns>Created movie details</returns>
    /// <response code="201">Movie successfully created</response>
    /// <response code="400">Invalid input data or validation errors</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="403">Forbidden - Admin role required</response>
    [HttpPost]
    [Authorize(Policy = "AdminPolicy")]
    [SwaggerOperation(
        Summary = "Create a new movie",
        Description = "Creates a new Star Wars movie with the provided details. Only available to admin"
    )]
    [SwaggerResponse(201, "Movie successfully created", typeof(MovieDto))]
    [SwaggerResponse(400, "Invalid input data or validation errors", typeof(ErrorResponseDto))]
    [SwaggerResponse(401, "Unauthorized - JWT token required", typeof(ErrorResponseDto))]
    [SwaggerResponse(403, "Forbidden - Admin role required", typeof(ErrorResponseDto))]
    public async Task<ActionResult<MovieDto>> CreateMovie([FromBody] CreateMovieDto createMovieDto)
    {
        var movie = await _movieService.CreateMovieAsync(createMovieDto);
        return CreatedAtAction(nameof(GetMovieById), new { id = movie.Id }, movie);
    }

    /// <summary>
    /// Update an existing Star Wars movie
    /// </summary>
    /// <param name="id">The id of the movie to update</param>
    /// <param name="updateMovieDto">Updated movie details</param>
    /// <returns>Updated movie details</returns>
    /// <response code="200">Movie successfully updated</response>
    /// <response code="400">Invalid input data or validation errors</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="403">Forbidden - Admin role required</response>
    /// <response code="404">Movie not found</response>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminPolicy")]
    [SwaggerOperation(
        Summary = "Update an existing movie",
        Description = "Updates an existing Star Wars movie with the provided details. Only available to admin"
    )]
    [SwaggerResponse(200, "Movie successfully updated", typeof(MovieDto))]
    [SwaggerResponse(400, "Invalid input data or validation errors", typeof(ErrorResponseDto))]
    [SwaggerResponse(401, "Unauthorized - JWT token required", typeof(ErrorResponseDto))]
    [SwaggerResponse(403, "Forbidden - Admin role required", typeof(ErrorResponseDto))]
    [SwaggerResponse(404, "Movie not found", typeof(ErrorResponseDto))]
    public async Task<ActionResult<MovieDto>> UpdateMovie(Guid id, [FromBody] UpdateMovieDto updateMovieDto)
    {
        var movie = await _movieService.UpdateMovieAsync(id, updateMovieDto);
        return Ok(movie);
    }

    /// <summary>
    /// Delete a Star Wars movie
    /// </summary>
    /// <param name="id">The id of the movie to delete</param>
    /// <returns>No content on successful deletion</returns>
    /// <response code="204">Movie successfully deleted</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="403">Forbidden - Admin role required</response>
    /// <response code="404">Movie not found</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminPolicy")]
    [SwaggerOperation(
        Summary = "Delete a movie",
        Description = "Deletes a Star Wars movie from the system. Only available to admin"
    )]
    [SwaggerResponse(204, "Movie successfully deleted")]
    [SwaggerResponse(401, "Unauthorized - JWT token required", typeof(ErrorResponseDto))]
    [SwaggerResponse(403, "Forbidden - Admin role required", typeof(ErrorResponseDto))]
    [SwaggerResponse(404, "Movie not found", typeof(ErrorResponseDto))]
    public async Task<ActionResult> DeleteMovie(Guid id)
    {
        await _movieService.DeleteMovieAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Trigger background synchronization of movies from Star Wars API
    /// </summary>
    /// <returns>Sync job details</returns>
    /// <response code="200">Synchronization job successfully queued</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="403">Forbidden - Admin role required</response>
    [HttpPost("sync")]
    [Authorize(Policy = "AdminPolicy")]
    [SwaggerOperation(
        Summary = "Trigger movie synchronization",
        Description = "Triggers background synchronization of movies from the external Star Wars API. The synchronization runs asynchronously and can be monitored via the Hangfire dashboard. Only available to admin."
    )]
    [SwaggerResponse(200, "Synchronization job successfully queued", typeof(object))]
    [SwaggerResponse(401, "Unauthorized - JWT token required", typeof(ErrorResponseDto))]
    [SwaggerResponse(403, "Forbidden - Admin role required", typeof(ErrorResponseDto))]
    public ActionResult TriggerMovieSync()
    {
        var jobId = _movieSyncBackgroundService.EnqueueImmediateSync();
        return Ok(new { 
            message = "Movie synchronization has been queued for background processing.", 
            jobId = jobId,
            dashboardUrl = "/hangfire"
        });
    }
}