using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using System.Net;
using System.Text.Json;

namespace AwsFeatureFlagService.Lambda.Flag;

public class CreateFlagFunction
{
    private readonly DynamoDBContext _context;
    private readonly string _tableName;
    private readonly Dictionary<string, string> _headers = new()
    {
        { "Content-Type", "application/json" },
        { "Access-Control-Allow-Origin", "*" },
    };

    public CreateFlagFunction()
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
        var flagRequest = JsonSerializer.Deserialize<FlagRequest>(input.Body)!;
        Console.WriteLine("input.Body: {0}", flagRequest);

        var operationConfig = new DynamoDBOperationConfig() { OverrideTableName = _tableName };

        var existingFlag = await _context.LoadAsync<Flag>(flagRequest.FlagName, operationConfig);

        if (existingFlag != null)
        {
            return CreateResponse(false, "Error Creating Flag: Feature Flag Already Exsits", HttpStatusCode.BadRequest);
        }

        try
        {
            var flag = new Flag()
            {
                Name = flagRequest.FlagName,
                Value = flagRequest.Value
            };

            await _context.SaveAsync(flag, operationConfig);
            return CreateResponse(true, null, HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error Creating Flag: {0}", ex.Message);
            return CreateResponse(false, "Error Creating Flag", HttpStatusCode.InternalServerError);
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