using System.Text.Json.Serialization;

namespace Infrastructure.ExternalAPI.StarWars.DTOs;

public class StarWarsApiResponseDto
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("result")]
    public List<StarWarsApiFilmDto> Result { get; set; } = new();

    [JsonPropertyName("apiVersion")]
    public string ApiVersion { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;
}
