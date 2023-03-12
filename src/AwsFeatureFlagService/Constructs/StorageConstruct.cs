using Amazon.CDK.AWS.DynamoDB;
using Constructs;

namespace AwsFeatureFlagService.Constructs
{
    internal class StorageConstruct : Construct
    {
        public ITable FlagTable { get; private set; }

        public StorageConstruct(Construct scope, string id) : base(scope, id)
        {
            var tableProps = new TableProps
            {
                PartitionKey = new Attribute { Name = "name", Type = AttributeType.STRING },
                BillingMode = BillingMode.PAY_PER_REQUEST,
                RemovalPolicy = Amazon.CDK.RemovalPolicy.DESTROY
            };

            FlagTable = new Table(this, "flags", tableProps);
        }
    }
}
