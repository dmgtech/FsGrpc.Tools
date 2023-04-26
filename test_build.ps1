dotnet restore FsGrpc.Tools

dotnet pack --configuration Release FsGrpc.Tools --output artifacts -p:FSGRPCTOOLS_VERSION=0.5.2

# rm artifacts/FsGrpc.Tools.1.0.0.zip
# cp artifacts/FsGrpc.Tools.1.0.0.nupkg artifacts/FsGrpc.Tools.1.0.0.zip