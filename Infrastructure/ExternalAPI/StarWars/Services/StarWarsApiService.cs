using System.Text.Json;
using Application.DTOs.Movies;
using Application.Interfaces.ExternalAPI;
using Infrastructure.ExternalAPI.StarWars.DTOs;
using Infrastructure.ExternalAPI.StarWars.Mappings;

namespace Infrastructure.ExternalAPI.StarWars.Services;

public class StarWarsApiService : IStarWarsApiService
{
    private readonly HttpClient _httpClient;
    private const string StarWarsApiBaseUrl = "https://www.swapi.tech/api/";

    public StarWarsApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(StarWarsApiBaseUrl);
    }

    public async Task<IEnumerable<MovieSyncDto>> GetAllFilmsAsync()
    {
        var response = await _httpClient.GetAsync("films");
        response.EnsureSuccessStatusCode();

        var jsonContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<StarWarsApiResponseDto>(jsonContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (apiResponse != null && apiResponse.Result.Any())
        {
            return apiResponse.Result.ToMovieSyncDtos();
        }

        return new List<MovieSyncDto>();
    }
}
