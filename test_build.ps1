dotnet restore Grpc.sln --packages=packages

dotnet pack --configuration Release Grpc.Tools --output artifacts
