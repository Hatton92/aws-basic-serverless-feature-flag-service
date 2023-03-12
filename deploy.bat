dotnet publish src\Lambdas\AwsFeatureFlagService.Lambda.Flag\ -c Release -r linux-arm64 --self-contained
cdk deploy --profile=personal