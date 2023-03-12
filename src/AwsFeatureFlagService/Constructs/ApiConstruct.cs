using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Lambda;
using Constructs;
using System.Collections.Generic;

namespace AwsFeatureFlagService.Constructs
{
    internal class ApiConstruct : Construct
    {
        public ApiConstruct(Construct scope, string id, ApiConstructProps props) : base(scope, id)
        {
            var api = new RestApi(this, "feature-flag-api", new RestApiProps
            {
                BinaryMediaTypes = new[] { "*/*" },
                MinimumCompressionSize = 0,
            });

            var rootPath = api.Root.AddResource("feature-flag");
            var flagPath = rootPath.AddResource("{flagName}");

            var schemaProperties = new Dictionary<string, IJsonSchema> {
                { "flagName", new JsonSchema { Type = JsonSchemaType.STRING } },
                { "currentVariation", new JsonSchema { Type = JsonSchemaType.BOOLEAN } }
            };

            var jsonSchema = new JsonSchema()
            {
                Type = JsonSchemaType.OBJECT,
                Required = new string[] { "flagName", "currentVariation" },
                Properties = schemaProperties,
            };

            var requestModel = new Model(this, "requestModel", new ModelProps
            {
                RestApi = api,
                ContentType = "application/json",
                Description = "validate request",
                ModelName = "requestModel",
                Schema = jsonSchema
            });

            var createUpdateValidator = new RequestValidator(this, "create-update-validator", new RequestValidatorProps
            {
                RestApi = api,
                RequestValidatorName = "create-update-validator",
                ValidateRequestBody = true,
                ValidateRequestParameters = true
            });

            var getDeleteValidator = new RequestValidator(this, "get-delete-validator", new RequestValidatorProps
            {
                RestApi = api,
                RequestValidatorName = "get-delete-validator",
                ValidateRequestParameters = true
            });

            AddGetFlagEndpoint(flagPath, props.FlagTable, getDeleteValidator);
            AddUpdateFlagEndpoint(flagPath, props.FlagTable, requestModel, createUpdateValidator);
            AddDeleteFlagEndpoint(flagPath, props.FlagTable, getDeleteValidator);
            AddCreateFlagEndpoint(rootPath, props.FlagTable, requestModel, createUpdateValidator);
        }

        private void AddGetFlagEndpoint(Resource category, ITable FlagTable, RequestValidator requestValidator)
        {
            var lambda = CreateLambda("get-flag-lambda", "GetFlagFunction", FlagTable);

            var integration = new LambdaIntegration(lambda);

            category.AddMethod("GET", integration, new MethodOptions
            {
                RequestParameters = new Dictionary<string, bool>
                {
                    { "method.request.path.flagName", true },
                },
                MethodResponses = new MethodResponse[]
                {
                    new MethodResponse
                    {
                        StatusCode = "200",
                        ResponseParameters = new Dictionary<string, bool>
                        {
                            { "method.response.header.Content-Type", true }
                        }
                    }
                },
                RequestValidator = requestValidator
            });
        }

        private void AddUpdateFlagEndpoint(Resource category, ITable FlagTable, Model requestModel, RequestValidator requestValidator)
        {
            var lambda = CreateLambda("update-flag-lambda", "UpdateFlagFunction", FlagTable);

            var integration = new LambdaIntegration(lambda);

            category.AddMethod("PUT", integration, new MethodOptions
            {
                RequestParameters = new Dictionary<string, bool>
                {
                    { "method.request.path.flagName", true },
                },
                MethodResponses = new MethodResponse[]
                {
                    new MethodResponse
                    {
                        StatusCode = "200",
                        ResponseParameters = new Dictionary<string, bool>
                        {
                            { "method.response.header.Content-Type", true }
                        }
                    }
                },
                RequestModels = new Dictionary<string, IModel>
                {
                    { "application/json", requestModel }
                },
                RequestValidator = requestValidator
            });
        }

        private void AddDeleteFlagEndpoint(Resource category, ITable FlagTable, RequestValidator requestValidator)
        {
            var lambda = CreateLambda("delete-flag-lambda", "DeleteFlagFunction", FlagTable);

            var integration = new LambdaIntegration(lambda);

            category.AddMethod("DELETE", integration, new MethodOptions
            {
                RequestParameters = new Dictionary<string, bool>
                {
                    { "method.request.path.flagName", true },
                },
                MethodResponses = new MethodResponse[]
                {
                    new MethodResponse
                    {
                        StatusCode = "200",
                        ResponseParameters = new Dictionary<string, bool>
                        {
                            { "method.response.header.Content-Type", true }
                        }
                    }
                },
                RequestValidator = requestValidator,
            });
        }

        private void AddCreateFlagEndpoint(Resource categories, ITable FlagTable, Model requestModel, RequestValidator requestValidator)
        {
            var lambda = CreateLambda("create-flag-lambda", "CreateFlagFunction", FlagTable);

            var integration = new LambdaIntegration(lambda);

            categories.AddMethod("POST", integration, new MethodOptions
            {
                MethodResponses = new MethodResponse[]
                {
                    new MethodResponse
                    {
                        StatusCode = "200",
                        ResponseParameters = new Dictionary<string, bool>
                        {
                            { "method.response.header.Content-Type", true }
                        }
                    }
                },
                RequestModels = new Dictionary<string, IModel>
                {
                    { "application/json", requestModel }
                },
                RequestValidator = requestValidator
            });
        }

        private Function CreateLambda(string id, string functionClass, ITable FlagTable)
        {
            var lambda = new Function(this, id, new FunctionProps
            {
                Runtime = Runtime.DOTNET_6,
                Architecture = Architecture.ARM_64,
                Code = Code.FromAsset("./src/Lambdas/AwsFeatureFlagService.Lambda.Flag/bin/Release/net6.0/linux-arm64/publish"),
                Handler = $"AwsFeatureFlagService.Lambda.Flag::AwsFeatureFlagService.Lambda.Flag.{functionClass}::FunctionHandler",
                Environment = new Dictionary<string, string>
                {
                    { "FlagTableName", FlagTable.TableName }
                },
                Timeout = Amazon.CDK.Duration.Seconds(10)
            });

            FlagTable.GrantReadWriteData(lambda);
            return lambda;
        }
    }
}
