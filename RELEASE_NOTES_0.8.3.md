# FsGrpc.Tools 0.8.3 Release Notes

## Summary
This release fixes critical MSBuild task loading issues in version 0.8.2 that prevented the package from working correctly.

## Issue #1: MSBuild Task Loading Failure - FIXED

### Problem
The MSBuild task assembly `Protobuf.MSBuild.dll` in version 0.8.2 was compiled for .NET 8.0 (`net8.0` TFM) and included a `Protobuf.MSBuild.runtimeconfig.json` file. This caused MSBuild to fail loading the assembly with the following error:

```
MSB4062: The "Grpc.Tools.ProtoToolsPlatform" task could not be loaded from the assembly 
C:\Users\[user]\.nuget\packages\fsgrpc.tools\0.8.2\build\_protobuf\net8.0\Protobuf.MSBuild.dll. 
Could not load file or assembly 'System.Runtime, Version=8.0.0.0, Culture=neutral, 
PublicKeyToken=b03f5f7f11d50a3a' or one of its dependencies.
```

### Root Cause
- MSBuild tasks should target `netstandard2.0` for maximum compatibility with different MSBuild hosts
- The .NET 8.0 runtime dependencies could not be resolved by MSBuild
- Version 0.8.0 (working version) did NOT have a `.runtimeconfig.json` file

### Solution Implemented
1. **Changed target framework** from `net8.0` to `netstandard2.0` in `FsGrpc.Tools.csproj`
2. **Removed unnecessary dependencies** that were not compatible with netstandard2.0:
   - Removed `fsgrpc` (not used by MSBuild tasks)
   - Removed `Focal.Core` (not used by MSBuild tasks)
   - Removed `Grpc.AspNetCore.Server.ClientFactory` (not used by MSBuild tasks)
3. **Set `GenerateDependencyFile` to `false`** to prevent creation of `.runtimeconfig.json`
4. **Updated targets file** to reference `netstandard2.0` directory instead of `net8.0`
5. **Removed `Protobuf.MSBuild.runtimeconfig.json`** file that was causing compatibility issues

### Files Modified
- `FsGrpc.Tools/FsGrpc.Tools.csproj`
  - Changed `<TargetFramework>` from `net8.0` to `netstandard2.0`
  - Added `<LangVersion>latest</LangVersion>` for C# language support
  - Changed `<GenerateDependencyFile>` from `true` to `false`
  - Removed incompatible package references
  - Updated packaging paths from `net8.0` to `netstandard2.0`
  
- `FsGrpc.Tools/build/_protobuf/Google.Protobuf.Tools.targets`
  - Updated `_Protobuf_MsBuildAssembly` path from `net8.0` to `netstandard2.0`

- `FsGrpc.Tools/build/_protobuf/net8.0/Protobuf.MSBuild.runtimeconfig.json`
  - **DELETED** - No longer needed with netstandard2.0 target

## Issue #2: Generated Code References Missing Types

### Status
This issue is related to the `FsGrpc.ProtocGenFsGrpc` code generator (version 0.8.1) and `Google.Protobuf` version compatibility. The MSBuild task changes in this release do not directly address this issue, but they ensure the build system can properly invoke the code generator.

### Recommendation
If you encounter errors like:
```
error FS0039: The type 'Annotation' is not defined in 'Google.Protobuf'.
```

This indicates a mismatch between:
1. The version of `Google.Protobuf` referenced in your project
2. The version of `FsGrpc.ProtocGenFsGrpc` used to generate code
3. The protobuf descriptor files being used

**Solution**: Ensure all protobuf-related packages are using compatible versions.

## Testing
- All existing unit tests pass (10/10 tests successful)
- Package builds successfully in Release configuration
- Package structure verified to contain correct assemblies in `build/_protobuf/netstandard2.0/`

## Breaking Changes
None. This is a bug fix release that restores functionality broken in 0.8.2.

## Compatibility
- MSBuild: All versions that support netstandard2.0 assemblies
- .NET Framework: 4.7.2+
- .NET Core: 2.0+
- .NET: 5.0+

## Migration from 0.8.2
Simply update your package reference from 0.8.2 to 0.8.3. No code changes required.

```xml
<PackageReference Include="FsGrpc.Tools" Version="0.8.3" />
```
