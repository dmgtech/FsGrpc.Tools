dotnet restore Grpc.sln --packages=packages

dotnet pack --configuration Release Grpc.Tools --output artifacts

rm artifacts/FsGrpc.Tools.1.0.0.zip
cp artifacts/FsGrpc.Tools.1.0.0.nupkg artifacts/FsGrpc.Tools.1.0.0.zip