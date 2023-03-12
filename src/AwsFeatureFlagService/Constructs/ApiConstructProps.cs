using Amazon.CDK.AWS.DynamoDB;

namespace AwsFeatureFlagService.Constructs
{
    internal struct ApiConstructProps
    {
        internal ITable FlagTable { get; private set; }

        internal ApiConstructProps(ITable flagTable)
        {
            FlagTable = flagTable;
        }
    }
}
