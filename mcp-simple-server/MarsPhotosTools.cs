using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace mcp_simple_server;

/// <summary>
/// Provides tools for interacting with NASA's Mars Photos API, including retrieving available Mars rovers
/// and fetching rover photos for a specific date.
/// </summary>
/// <remarks>
/// This class uses an injected <see cref="HttpClient"/> and <see cref="NasaApiConfiguration"/> to perform HTTP requests
/// to the NASA Mars Photos API. The methods are decorated with <see cref="McpServerToolAttribute"/> for server tool integration.
/// </remarks>
[McpServerToolType]
public class MarsPhotosTools(HttpClient httpClient, NasaApiConfiguration config)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly NasaApiConfiguration _config = config;

    [McpServerTool(Name = "get-rovers"),
     Description("Returns a list of all available Mars rovers from NASA's Mars Photos API")]
    public async Task<string> GetRoversAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"mars-photos/api/v1/rovers?api_key={_config.ApiKey}");
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            var roversData = JsonSerializer.Deserialize<JsonElement>(content);
            
            // Format the response for better readability
            var formattedResponse = JsonSerializer.Serialize(roversData, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            return formattedResponse;
        }
        catch (Exception ex)
        {
            return $"Error retrieving rovers: {ex.Message}";
        }
    }

    [McpServerTool(Name = "get-current-date"),
     Description("Returns the current date in YYYY-M-D format for reference when requesting rover photos")]
    public string GetCurrentDate()
    {
        return DateTime.Now.ToString("yyyy-M-d");
    }

    [McpServerTool(Name = "get-rover-photo"),
     Description("Returns available photos for a given date by rover name. Parameters: roverName (string), earthDate (string in YYYY-M-D format)")]
    public async Task<string> GetRoverPhotoAsync(string roverName, string earthDate)
    {
        try
        {
            // Use API key from configuration
            var url = $"mars-photos/api/v1/rovers/{roverName.ToLower()}/photos?earth_date={earthDate}&api_key={_config.ApiKey}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            var photosData = JsonSerializer.Deserialize<JsonElement>(content);
            
            // Format the response for better readability
            var formattedResponse = JsonSerializer.Serialize(photosData, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            return formattedResponse;
        }
        catch (Exception ex)
        {
            return $"Error retrieving rover photos: {ex.Message}";
        }
    }
} 