#nullable disable
using System.Text.Json.Serialization;

namespace AwsFeatureFlagService.Lambda.Flag;

public class FlagRequest
{
    [JsonPropertyName("flagName")]
    public string FlagName { get; set; }

    [JsonPropertyName("currentVariation")]
    public bool Value { get; set; }
}
