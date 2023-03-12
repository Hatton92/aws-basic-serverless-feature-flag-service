namespace AwsFeatureFlagService.Lambda.Flag;

public class FlagResponse
{
    public bool Success { get; set; }

    public string? ErrorMessage { get; set; }

    public string? FlagName { get; set; }

    public bool? CurrentVariation { get; set; }
}
