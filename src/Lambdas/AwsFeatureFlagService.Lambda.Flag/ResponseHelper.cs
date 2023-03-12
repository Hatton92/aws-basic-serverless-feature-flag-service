using Amazon.Lambda.APIGatewayEvents;
using System.Net;
using System.Text.Json;

namespace AwsFeatureFlagService.Lambda.Flag;

public static class ResponseHelper
{
    public static APIGatewayProxyResponse Create(string responseBody, HttpStatusCode httpStatusCode, Dictionary<string, string> headers)
    {
        return new APIGatewayProxyResponse
        {
            Body = JsonSerializer.Serialize(responseBody),
            StatusCode = (int)httpStatusCode,
            Headers = headers
        };
    }
}
