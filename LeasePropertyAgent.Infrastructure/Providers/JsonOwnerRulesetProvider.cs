using System.Text.Json;
using LeasePropertyAgent.Application.Interfaces;
using LeasePropertyAgent.Application.Models;

namespace LeasePropertyAgent.Infrastructure.Providers;

public class JsonOwnerRulesetProvider : IOwnerRulesetProvider
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions;

    public JsonOwnerRulesetProvider(string filePath)
    {
        _filePath = filePath;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<OwnerRuleset> GetRulesetAsync(
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
        {
            throw new FileNotFoundException(
                "Owner ruleset file was not found.",
                _filePath);
        }

        await using var stream = File.OpenRead(_filePath);

        var ruleset = await JsonSerializer.DeserializeAsync<OwnerRuleset>(
            stream,
            _jsonOptions,
            cancellationToken);

        if (ruleset is null)
        {
            throw new InvalidOperationException(
                "Owner ruleset could not be loaded.");
        }

        return ruleset;
    }
}