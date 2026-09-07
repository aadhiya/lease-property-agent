using System.Text.Json.Serialization;

namespace LeasePropertyAgent.Application.Models;

public class OwnerRuleset
{
    [JsonPropertyName("ruleset_name")]
    public string RulesetName { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;

    [JsonPropertyName("rules")]
    public List<OwnerRule> Rules { get; set; } = new();
}