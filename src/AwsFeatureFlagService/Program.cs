using Amazon.CDK;

namespace AwsFeatureFlagService
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            _ = new AwsFeatureFlagService(app, "AwsFeatureFlagService");
            app.Synth();
        }
    }
}
