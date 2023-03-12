using Amazon.CDK;
using AwsFeatureFlagService.Constructs;
using Constructs;

namespace AwsFeatureFlagService
{
    public class AwsFeatureFlagService : Stack
    {
        internal AwsFeatureFlagService(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var storage = new StorageConstruct(this, "storage");

            _ = new ApiConstruct(this, "api", new ApiConstructProps(storage.FlagTable));
        }
    }
}
