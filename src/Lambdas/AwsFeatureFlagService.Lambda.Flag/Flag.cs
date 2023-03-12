#nullable disable
using Amazon.DynamoDBv2.DataModel;

namespace AwsFeatureFlagService.Lambda.Flag;

public class Flag
{
    [DynamoDBHashKey("name")] //Partition key
    public string Name
    {
        get; set;
    }

    [DynamoDBProperty("value", typeof(DynamoDBNativeBooleanConverter))]
    public bool Value
    {
        get; set;
    }
}

