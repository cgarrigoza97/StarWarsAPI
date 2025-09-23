using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using API.Test.Integration;
using Application.DTOs.Movies;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace API.Test.Controllers;

[TestFixture]
public class MoviesControllerTests : IDisposable
{
    private CustomWebApplicationMovieFactory _factory;
    private HttpClient _client;
    
    private const string SecretKey = "ThisIsATestSecretKeyThatIsLongEnoughForHmacSha256Algorithm";
    private const string Issuer = "TestIssuer";
    private const string Audience = "TestAudience";

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new CustomWebApplicationMovieFactory();
        _client = _factory.CreateClient();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    [SetUp]
    public void SetUp()
    {
        _factory.MovieRepositoryMock.Reset();
        _factory.MovieServiceMock.Reset();
        _factory.MovieSyncBackgroundServiceMock.Reset();
        
        _client.DefaultRequestHeaders.Authorization = null;
    }

    private string GenerateJwtToken(string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Email, "test@example.com"),
            new Claim("firstName", "Test"),
            new Claim("lastName", "User")
        };

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private MovieDto CreateSampleMovieDto()
    {
        return new MovieDto
        {
            Id = Guid.NewGuid(),
            Title = "Test Movie",
            EpisodeId = 1,
            OpeningCrawl = "Test opening crawl",
            Director = "Test Director",
            Producer = "Test Producer",
            ReleaseDate = DateTime.Now,
            ExternalId = "test-1",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };
    }

    private CreateMovieDto CreateSampleCreateMovieDto()
    {
        return new CreateMovieDto
        {
            Title = "New Test Movie",
            EpisodeId = 10,
            OpeningCrawl = "New test opening crawl",
            Director = "New Test Director",
            Producer = "New Test Producer",
            ReleaseDate = DateTime.Now,
            ExternalId = "new-test-10"
        };
    }

    private UpdateMovieDto CreateSampleUpdateMovieDto()
    {
        return new UpdateMovieDto
        {
            Title = "Updated Test Movie",
            EpisodeId = 1,
            OpeningCrawl = "Updated test opening crawl",
            Director = "Updated Test Director",
            Producer = "Updated Test Producer",
            ReleaseDate = DateTime.Now,
            ExternalId = "updated-test-1"
        };
    }

    [Test]
    public async Task GetAllMovies_ShouldReturn401_WhenNoToken()
    {
        var response = await _client.GetAsync("/api/Movies");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task GetAllMovies_ShouldReturn200_WhenUserIsUser()
    {
        var token = GenerateJwtToken("User");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var movies = new List<MovieDto> { CreateSampleMovieDto() };
        _factory.MovieServiceMock.Setup(s => s.GetAllMoviesAsync())
            .ReturnsAsync(movies);

        var response = await _client.GetAsync("/api/Movies");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetAllMovies_ShouldReturn200_WhenUserIsAdmin()
    {
        var token = GenerateJwtToken("Admin");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var movies = new List<MovieDto> { CreateSampleMovieDto() };
        _factory.MovieServiceMock.Setup(s => s.GetAllMoviesAsync())
            .ReturnsAsync(movies);

        var response = await _client.GetAsync("/api/Movies");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetMovieById_ShouldReturn401_WhenNoToken()
    {
        var movieId = Guid.NewGuid();

        var response = await _client.GetAsync($"/api/Movies/{movieId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task GetMovieById_ShouldReturn200_WhenUserIsUser()
    {
        var token = GenerateJwtToken("User");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var movieId = Guid.NewGuid();
        var movie = CreateSampleMovieDto();
        movie.Id = movieId;
        
        _factory.MovieServiceMock.Setup(s => s.GetMovieByIdAsync(movieId))
            .ReturnsAsync(movie);

        var response = await _client.GetAsync($"/api/Movies/{movieId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetMovieById_ShouldReturn200_WhenUserIsAdmin()
    {
        var token = GenerateJwtToken("Admin");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var movieId = Guid.NewGuid();
        var movie = CreateSampleMovieDto();
        movie.Id = movieId;
        
        _factory.MovieServiceMock.Setup(s => s.GetMovieByIdAsync(movieId))
            .ReturnsAsync(movie);

        var response = await _client.GetAsync($"/api/Movies/{movieId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task CreateMovie_ShouldReturn401_WhenNoToken()
    {
        var createMovieDto = CreateSampleCreateMovieDto();
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(createMovieDto), 
            Encoding.UTF8, 
            "application/json");

        var response = await _client.PostAsync("/api/Movies", jsonContent);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task CreateMovie_ShoulReturn403_WhenUserIsUser()
    {
        var token = GenerateJwtToken("User");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var createMovieDto = CreateSampleCreateMovieDto();
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(createMovieDto), 
            Encoding.UTF8, 
            "application/json");

        var response = await _client.PostAsync("/api/Movies", jsonContent);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task CreateMovie_ShouldReturn201_WhenUserIsAdmin()
    {
        var token = GenerateJwtToken("Admin");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var createMovieDto = CreateSampleCreateMovieDto();
        var createdMovie = CreateSampleMovieDto();
        createdMovie.Title = createMovieDto.Title;
        
        _factory.MovieServiceMock.Setup(s => s.CreateMovieAsync(It.IsAny<CreateMovieDto>()))
            .ReturnsAsync(createdMovie);
        
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(createMovieDto), 
            Encoding.UTF8, 
            "application/json");

        var response = await _client.PostAsync("/api/Movies", jsonContent);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    }

    [Test]
    public async Task UpdateMovie_ShouldReturn401_WhenNoToken()
    {
        var movieId = Guid.NewGuid();
        var updateMovieDto = CreateSampleUpdateMovieDto();
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(updateMovieDto), 
            Encoding.UTF8, 
            "application/json");

        var response = await _client.PutAsync($"/api/Movies/{movieId}", jsonContent);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task UpdateMovie_ShouldReturn403_WhenUserIsUser()
    {
        var token = GenerateJwtToken("User");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var movieId = Guid.NewGuid();
        var updateMovieDto = CreateSampleUpdateMovieDto();
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(updateMovieDto), 
            Encoding.UTF8, 
            "application/json");

        var response = await _client.PutAsync($"/api/Movies/{movieId}", jsonContent);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task UpdateMovie_ShouldReturn200_WhenUserIsAdmin()
    {
        var token = GenerateJwtToken("Admin");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var movieId = Guid.NewGuid();
        var updateMovieDto = CreateSampleUpdateMovieDto();
        var updatedMovie = CreateSampleMovieDto();
        updatedMovie.Id = movieId;
        updatedMovie.Title = updateMovieDto.Title;
        
        _factory.MovieServiceMock.Setup(s => s.UpdateMovieAsync(movieId, It.IsAny<UpdateMovieDto>()))
            .ReturnsAsync(updatedMovie);
        
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(updateMovieDto), 
            Encoding.UTF8, 
            "application/json");

        var response = await _client.PutAsync($"/api/Movies/{movieId}", jsonContent);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task DeleteMovie_ShouldReturn401_WhenNoToken()
    {
        var movieId = Guid.NewGuid();

        var response = await _client.DeleteAsync($"/api/Movies/{movieId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task DeleteMovie_ShouldReturn403_WhenUserIsUser()
    {
        var token = GenerateJwtToken("User");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var movieId = Guid.NewGuid();

        var response = await _client.DeleteAsync($"/api/Movies/{movieId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task DeleteMovie_ShouldReturn204_WhenUserIsAdmin()
    {
        var token = GenerateJwtToken("Admin");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var movieId = Guid.NewGuid();
        
        _factory.MovieServiceMock.Setup(s => s.DeleteMovieAsync(movieId))
            .ReturnsAsync(true);

        var response = await _client.DeleteAsync($"/api/Movies/{movieId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task TriggerMovieSync_ShouldReturn401_WhenNoToken()
    {
        var response = await _client.PostAsync("/api/Movies/sync", new StringContent(""));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task TriggerMovieSync_ShouldReturn403_WhenUserIsUser()
    {
        var token = GenerateJwtToken("User");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsync("/api/Movies/sync", new StringContent(""));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task TriggerMovieSync_ShouldReturn200_WhenUserIsAdmin()
    {
        var token = GenerateJwtToken("Admin");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var jobId = "job-12345";
        _factory.MovieSyncBackgroundServiceMock.Setup(s => s.EnqueueImmediateSync())
            .Returns(jobId);

        var response = await _client.PostAsync("/api/Movies/sync", new StringContent(""));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }


    [Test]
    public async Task GetAllMovies_ShouldReturn401_WhenInvalidToken()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token");

        var response = await _client.GetAsync("/api/Movies");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task CreateMovie_ShouldReturn401_WhenExpiredToken()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var expiredToken = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(-10), // Expired 10 minutes ago
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(expiredToken);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenString);
        
        var createMovieDto = CreateSampleCreateMovieDto();
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(createMovieDto), 
            Encoding.UTF8, 
            "application/json");

        var response = await _client.PostAsync("/api/Movies", jsonContent);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    public void Dispose()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }
}
