using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using System.Net;
using System.Text.Json;

namespace AwsFeatureFlagService.Lambda.Flag;

public class DeleteFlagFunction
{
    private readonly DynamoDBContext _context;
    private readonly string _tableName;
    private readonly Dictionary<string, string> _headers = new()
    {
        { "Content-Type", "application/json" },
        { "Access-Control-Allow-Origin", "*" },
    };

    public DeleteFlagFunction()
    {
        _context = new DynamoDBContext(new AmazonDynamoDBClient());
        var tableName = Environment.GetEnvironmentVariable("FlagTableName");
        if (tableName is null)
        {
            throw new Exception("Missing Table Name Variable");
        }

        _tableName = tableName;
    }

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest input)
    {
        var flagName = input.PathParameters["flagName"];

        var operationConfig = new DynamoDBOperationConfig() { OverrideTableName = _tableName };
        var existingFlag = await _context.LoadAsync<Flag>(flagName, operationConfig);

        if (existingFlag == null)
        {
            return CreateResponse(false, "Error Deleting Flag: Feature Flag Does Not Exist", HttpStatusCode.BadRequest);
        }

        try
        {
            await _context.DeleteAsync(existingFlag, operationConfig);
            return CreateResponse(true, null, HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error Deleting Flag: {0}", ex.Message);
            return CreateResponse(false, "Error Deleting Flag", HttpStatusCode.InternalServerError);
        }
    }

    private APIGatewayProxyResponse CreateResponse(bool success, string? errorMessage, HttpStatusCode httpStatusCode)
    {
        var flagResponse = new FlagResponse()
        {
            Success = success,
            ErrorMessage = errorMessage
        };

        return ResponseHelper.Create(JsonSerializer.Serialize(flagResponse), httpStatusCode, _headers);
    }
}