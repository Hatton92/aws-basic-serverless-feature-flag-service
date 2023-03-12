using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

namespace AwsFeatureFlagService.Lambda.Flag;

public class DynamoDBNativeBooleanConverter : IPropertyConverter
{
    public DynamoDBEntry ToEntry(object value) => new DynamoDBBool(bool.TryParse(value?.ToString(), out var val) && val);

    public object FromEntry(DynamoDBEntry entry) => entry.AsDynamoDBBool().Value;
}