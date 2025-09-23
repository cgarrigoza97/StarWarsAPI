using Application.DTOs.Movies;
using Application.Interfaces.ExternalAPI;
using Application.Interfaces.Movies;
using Application.Services.Movies;
using Domain.Entities;
using Moq;

namespace Application.Tests.Services;

[TestFixture]
public class MovieServiceTests
{
    private Mock<IMovieRepository> _mockMovieRepository;
    private Mock<IStarWarsApiService> _mockStarWarsApiService;
    private MovieService _movieService;

    [SetUp]
    public void Setup()
    {
        _mockMovieRepository = new Mock<IMovieRepository>();
        _mockStarWarsApiService = new Mock<IStarWarsApiService>();
        _movieService = new MovieService(_mockMovieRepository.Object, _mockStarWarsApiService.Object);
    }

    [Test]
    public async Task GetAllMoviesAsync_ReturnsAllMovies()
    {
        var movies = new List<Movie>
        {
            new Movie { Id = Guid.NewGuid(), Title = "A New Hope", EpisodeId = 4, Director = "George Lucas" },
            new Movie { Id = Guid.NewGuid(), Title = "The Empire Strikes Back", EpisodeId = 5, Director = "Irvin Kershner" }
        };

        _mockMovieRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(movies);

        var result = await _movieService.GetAllMoviesAsync();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(result.First().Title, Is.EqualTo("A New Hope"));
        Assert.That(result.Skip(1).First().Title, Is.EqualTo("The Empire Strikes Back"));
    }

    [Test]
    public async Task GetAllMoviesAsync_NoMovies_ReturnsEmptyCollection()
    {
        _mockMovieRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Movie>());

        var result = await _movieService.GetAllMoviesAsync();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(0));
    }

    [Test]
    public async Task GetMovieByIdAsync_ExistingMovie_ReturnsMovie()
    {
        var movieId = Guid.NewGuid();
        var movie = new Movie
        {
            Id = movieId,
            Title = "A New Hope",
            EpisodeId = 4,
            Director = "George Lucas",
            Producer = "Gary Kurtz, Rick McCallum",
            OpeningCrawl = "It is a period of civil war...",
            ReleaseDate = new DateTime(1977, 5, 25),
            ExternalId = "1"
        };

        _mockMovieRepository.Setup(x => x.GetByIdAsync(movieId)).ReturnsAsync(movie);

        var result = await _movieService.GetMovieByIdAsync(movieId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(movieId));
        Assert.That(result.Title, Is.EqualTo("A New Hope"));
        Assert.That(result.EpisodeId, Is.EqualTo(4));
        Assert.That(result.Director, Is.EqualTo("George Lucas"));
    }

    [Test]
    public async Task GetMovieByIdAsync_NonExistentMovie_ThrowsKeyNotFoundException()
    {
        var movieId = Guid.NewGuid();
        _mockMovieRepository.Setup(x => x.GetByIdAsync(movieId)).ReturnsAsync((Movie?)null);

        Assert.ThrowsAsync<KeyNotFoundException>(() => _movieService.GetMovieByIdAsync(movieId));
    }

    [Test]
    public async Task CreateMovieAsync_ValidInput_ReturnsCreatedMovie()
    {
        var createMovieDto = new CreateMovieDto
        {
            Title = "A New Hope",
            EpisodeId = 4,
            Director = "George Lucas",
            Producer = "Gary Kurtz, Rick McCallum",
            OpeningCrawl = "It is a period of civil war...",
            ReleaseDate = new DateTime(1977, 5, 25),
            ExternalId = "1"
        };

        var createdMovie = new Movie
        {
            Id = Guid.NewGuid(),
            Title = createMovieDto.Title,
            EpisodeId = createMovieDto.EpisodeId,
            Director = createMovieDto.Director,
            Producer = createMovieDto.Producer,
            OpeningCrawl = createMovieDto.OpeningCrawl,
            ReleaseDate = createMovieDto.ReleaseDate,
            ExternalId = createMovieDto.ExternalId
        };

        _mockMovieRepository.Setup(x => x.CreateAsync(It.IsAny<Movie>())).ReturnsAsync(createdMovie);

        var result = await _movieService.CreateMovieAsync(createMovieDto);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo(createMovieDto.Title));
        Assert.That(result.EpisodeId, Is.EqualTo(createMovieDto.EpisodeId));
        Assert.That(result.Director, Is.EqualTo(createMovieDto.Director));
    }

    [Test]
    public async Task UpdateMovieAsync_ExistingMovie_ReturnsUpdatedMovie()
    {
        var movieId = Guid.NewGuid();
        var updateMovieDto = new UpdateMovieDto
        {
            Title = "A New Hope - Updated",
            EpisodeId = 4,
            Director = "George Lucas",
            Producer = "Gary Kurtz, Rick McCallum",
            OpeningCrawl = "Updated opening crawl...",
            ReleaseDate = new DateTime(1977, 5, 25),
            ExternalId = "1"
        };

        var existingMovie = new Movie
        {
            Id = movieId,
            Title = "A New Hope",
            EpisodeId = 4,
            Director = "George Lucas"
        };

        var updatedMovie = new Movie
        {
            Id = movieId,
            Title = updateMovieDto.Title,
            EpisodeId = updateMovieDto.EpisodeId,
            Director = updateMovieDto.Director,
            Producer = updateMovieDto.Producer,
            OpeningCrawl = updateMovieDto.OpeningCrawl,
            ReleaseDate = updateMovieDto.ReleaseDate,
            ExternalId = updateMovieDto.ExternalId
        };

        _mockMovieRepository.Setup(x => x.GetByIdAsync(movieId)).ReturnsAsync(existingMovie);
        _mockMovieRepository.Setup(x => x.UpdateAsync(It.IsAny<Movie>())).ReturnsAsync(updatedMovie);

        var result = await _movieService.UpdateMovieAsync(movieId, updateMovieDto);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo(updateMovieDto.Title));
        Assert.That(result.OpeningCrawl, Is.EqualTo(updateMovieDto.OpeningCrawl));
    }

    [Test]
    public async Task UpdateMovieAsync_NonExistentMovie_ThrowsKeyNotFoundException()
    {
        var movieId = Guid.NewGuid();
        var updateMovieDto = new UpdateMovieDto
        {
            Title = "A New Hope - Updated",
            EpisodeId = 4,
            Director = "George Lucas",
            Producer = "Gary Kurtz, Rick McCallum",
            OpeningCrawl = "Updated opening crawl...",
            ReleaseDate = new DateTime(1977, 5, 25),
            ExternalId = "1"
        };

        _mockMovieRepository.Setup(x => x.GetByIdAsync(movieId)).ReturnsAsync((Movie?)null);

        Assert.ThrowsAsync<KeyNotFoundException>(() => _movieService.UpdateMovieAsync(movieId, updateMovieDto));
    }

    [Test]
    public async Task DeleteMovieAsync_ExistingMovie_ReturnsTrue()
    {
        var movieId = Guid.NewGuid();
        _mockMovieRepository.Setup(x => x.DeleteAsync(movieId)).ReturnsAsync(true);

        var result = await _movieService.DeleteMovieAsync(movieId);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task DeleteMovieAsync_NonExistentMovie_ThrowsKeyNotFoundException()
    {
        var movieId = Guid.NewGuid();
        _mockMovieRepository.Setup(x => x.DeleteAsync(movieId)).ReturnsAsync(false);

        Assert.ThrowsAsync<KeyNotFoundException>(() => _movieService.DeleteMovieAsync(movieId));
    }

    [Test]
    public async Task SyncMoviesFromStarWarsApiAsync_NewMovies_CreatesThem()
    {
        var starWarsFilms = new List<MovieSyncDto>
        {
            new MovieSyncDto
            {
                Title = "A New Hope",
                EpisodeId = 4,
                Director = "George Lucas",
                Producer = "Gary Kurtz, Rick McCallum",
                OpeningCrawl = "It is a period of civil war...",
                ReleaseDate = "1977-05-25",
                ExternalId = "1"
            },
            new MovieSyncDto
            {
                Title = "The Empire Strikes Back",
                EpisodeId = 5,
                Director = "Irvin Kershner",
                Producer = "Gary Kurtz, Rick McCallum",
                OpeningCrawl = "It is a dark time for the Rebellion...",
                ReleaseDate = "1980-05-17",
                ExternalId = "2"
            }
        };

        _mockStarWarsApiService.Setup(x => x.GetAllFilmsAsync()).ReturnsAsync(starWarsFilms);
        _mockMovieRepository.Setup(x => x.GetByExternalIdAsync("1")).ReturnsAsync((Movie?)null);
        _mockMovieRepository.Setup(x => x.GetByExternalIdAsync("2")).ReturnsAsync((Movie?)null);
        _mockMovieRepository.Setup(x => x.CreateAsync(It.IsAny<Movie>())).ReturnsAsync((Movie m) => m);

        var result = await _movieService.SyncMoviesFromStarWarsApiAsync();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.SyncedMovies, Is.EqualTo(2));
        Assert.That(result.UpdatedMovies, Is.EqualTo(0));
        Assert.That(result.TotalMovies, Is.EqualTo(2));
    }

    [Test]
    public async Task SyncMoviesFromStarWarsApiAsync_ExistingMovies_UpdatesThem()
    {
        var starWarsFilm = new MovieSyncDto
        {
            Title = "A New Hope - Updated",
            EpisodeId = 4,
            Director = "George Lucas",
            Producer = "Gary Kurtz, Rick McCallum",
            OpeningCrawl = "Updated opening crawl...",
            ReleaseDate = "1977-05-25",
            ExternalId = "1"
        };

        var existingMovie = new Movie
        {
            Id = Guid.NewGuid(),
            Title = "A New Hope",
            EpisodeId = 4,
            Director = "George Lucas",
            ExternalId = "1",
            ReleaseDate = new DateTime(1977, 5, 25)
        };

        _mockStarWarsApiService.Setup(x => x.GetAllFilmsAsync()).ReturnsAsync(new List<MovieSyncDto> { starWarsFilm });
        _mockMovieRepository.Setup(x => x.GetByExternalIdAsync("1")).ReturnsAsync(existingMovie);
        _mockMovieRepository.Setup(x => x.UpdateAsync(It.IsAny<Movie>())).ReturnsAsync((Movie m) => m);

        var result = await _movieService.SyncMoviesFromStarWarsApiAsync();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.SyncedMovies, Is.EqualTo(0));
        Assert.That(result.UpdatedMovies, Is.EqualTo(1));
        Assert.That(result.TotalMovies, Is.EqualTo(1));
    }
}
