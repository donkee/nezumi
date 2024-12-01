using System.Text.Json.Serialization;

namespace Nezumi.Models;

public class Window
{
    [JsonPropertyName("title")]
    public string Title { get; set; }
    [JsonPropertyName("exe")]
    public string Exe { get; set; }
    [JsonPropertyName("class")]
    public string Class { get; set; }
}