using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Test.Repositories;

[TestFixture]
public class MovieRepositoryTests
{
    private ApplicationDbContext _context;
    private MovieRepository _movieRepository;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _movieRepository = new MovieRepository(_context);

        SeedTestData();
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    private void SeedTestData()
    {
        var movies = new List<Movie>
        {
            new Movie
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Title = "A New Hope",
                EpisodeId = 4,
                Director = "George Lucas",
                Producer = "Gary Kurtz, Rick McCallum",
                OpeningCrawl = "It is a period of civil war...",
                ReleaseDate = new DateTime(1977, 5, 25),
                ExternalId = "1",
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new Movie
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Title = "The Empire Strikes Back",
                EpisodeId = 5,
                Director = "Irvin Kershner",
                Producer = "Gary Kurtz, Rick McCallum",
                OpeningCrawl = "It is a dark time for the Rebellion...",
                ReleaseDate = new DateTime(1980, 5, 17),
                ExternalId = "2",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new Movie
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Title = "Return of the Jedi",
                EpisodeId = 6,
                Director = "Richard Marquand",
                Producer = "Howard G. Kazanjian, George Lucas, Rick McCallum",
                OpeningCrawl = "Luke Skywalker has returned to his home planet...",
                ReleaseDate = new DateTime(1983, 5, 25),
                ExternalId = "3",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            }
        };

        _context.Movies.AddRange(movies);
        _context.SaveChanges();
    }

    [Test]
    public async Task GetAllAsync_ReturnsAllMovies()
    {
        var result = await _movieRepository.GetAllAsync();
        var moviesList = result.ToList();

        Assert.That(moviesList, Is.Not.Null);
        Assert.That(moviesList.Count, Is.EqualTo(3));
    }

    [Test]
    public async Task GetAllAsync_EmptyDatabase_ReturnsEmptyCollection()
    {
        _context.Movies.RemoveRange(_context.Movies);
        await _context.SaveChangesAsync();

        var result = await _movieRepository.GetAllAsync();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(0));
    }

    [Test]
    public async Task GetByIdAsync_ExistingMovie_ReturnsMovie()
    {
        var movieId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var result = await _movieRepository.GetByIdAsync(movieId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(movieId));
        Assert.That(result.Title, Is.EqualTo("A New Hope"));
        Assert.That(result.EpisodeId, Is.EqualTo(4));
        Assert.That(result.Director, Is.EqualTo("George Lucas"));
        Assert.That(result.ExternalId, Is.EqualTo("1"));
    }

    [Test]
    public async Task GetByIdAsync_NonExistentMovie_ReturnsNull()
    {
        var nonExistentId = Guid.NewGuid();

        var result = await _movieRepository.GetByIdAsync(nonExistentId);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetByExternalIdAsync_ExistingExternalId_ReturnsMovie()
    {
        var externalId = "2";

        var result = await _movieRepository.GetByExternalIdAsync(externalId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ExternalId, Is.EqualTo(externalId));
        Assert.That(result.Title, Is.EqualTo("The Empire Strikes Back"));
        Assert.That(result.EpisodeId, Is.EqualTo(5));
        Assert.That(result.Director, Is.EqualTo("Irvin Kershner"));
    }

    [Test]
    public async Task GetByExternalIdAsync_NonExistentExternalId_ReturnsNull()
    {
        var nonExistentExternalId = "999";

        var result = await _movieRepository.GetByExternalIdAsync(nonExistentExternalId);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CreateAsync_ValidMovie_CreatesAndReturnsMovie()
    {
        var newMovie = new Movie
        {
            Title = "The Phantom Menace",
            EpisodeId = 1,
            Director = "George Lucas",
            Producer = "Rick McCallum",
            OpeningCrawl = "Turmoil has engulfed the Galactic Republic...",
            ReleaseDate = new DateTime(1999, 5, 19),
            ExternalId = "4"
        };

        var result = await _movieRepository.CreateAsync(newMovie);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(result.Title, Is.EqualTo("The Phantom Menace"));
        Assert.That(result.EpisodeId, Is.EqualTo(1));
        Assert.That(result.ExternalId, Is.EqualTo("4"));

        var savedMovie = await _context.Movies.FindAsync(result.Id);
        Assert.That(savedMovie, Is.Not.Null);
        Assert.That(savedMovie.Title, Is.EqualTo("The Phantom Menace"));
    }

    [Test]
    public async Task UpdateAsync_ExistingMovie_UpdatesAndReturnsMovie()
    {
        var movieId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var movieToUpdate = await _context.Movies.FindAsync(movieId);
        Assert.That(movieToUpdate, Is.Not.Null);

        movieToUpdate.Title = "A New Hope - Updated";
        movieToUpdate.Director = "George Lucas - Updated";
        movieToUpdate.OpeningCrawl = "Updated opening crawl...";
        movieToUpdate.UpdatedAt = DateTime.UtcNow;

        var result = await _movieRepository.UpdateAsync(movieToUpdate);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo("A New Hope - Updated"));
        Assert.That(result.Director, Is.EqualTo("George Lucas - Updated"));
        Assert.That(result.OpeningCrawl, Is.EqualTo("Updated opening crawl..."));
        Assert.That(result.UpdatedAt, Is.Not.Null);

        var updatedMovie = await _context.Movies.FindAsync(movieId);
        Assert.That(updatedMovie, Is.Not.Null);
        Assert.That(updatedMovie.Title, Is.EqualTo("A New Hope - Updated"));
        Assert.That(updatedMovie.Director, Is.EqualTo("George Lucas - Updated"));
    }

    [Test]
    public async Task DeleteAsync_ExistingMovie_DeletesMovieAndReturnsTrue()
    {
        var movieId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        
        var existingMovie = await _context.Movies.FindAsync(movieId);
        Assert.That(existingMovie, Is.Not.Null);

        var result = await _movieRepository.DeleteAsync(movieId);

        Assert.That(result, Is.True);

        var deletedMovie = await _context.Movies.FindAsync(movieId);
        Assert.That(deletedMovie, Is.Null);

        var remainingMovies = await _context.Movies.ToListAsync();
        Assert.That(remainingMovies.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task DeleteAsync_NonExistentMovie_ReturnsFalse()
    {
        var nonExistentId = Guid.NewGuid();

        var result = await _movieRepository.DeleteAsync(nonExistentId);

        Assert.That(result, Is.False);

        var allMovies = await _context.Movies.ToListAsync();
        Assert.That(allMovies.Count, Is.EqualTo(3));
    }
}
