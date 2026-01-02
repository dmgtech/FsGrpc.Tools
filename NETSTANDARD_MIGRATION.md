# MSBuild Task Migration to netstandard2.0

## Summary

The FsGrpc.Tools package has been updated from targeting `net8.0` to `netstandard2.0`. **This is the correct approach for MSBuild task packages.**

## Why netstandard2.0?

### MSBuild Task Best Practice
Microsoft's guidance for MSBuild task assemblies is to target `netstandard2.0` because:

1. **Maximum Compatibility**: Works with all MSBuild hosts (Visual Studio, dotnet CLI, msbuild.exe)
2. **Not a Runtime Dependency**: MSBuild tasks are loaded by the build system, not by your application
3. **Avoids Runtime Conflicts**: Prevents issues like the one in v0.8.2 where .NET 8.0 runtime dependencies couldn't be resolved

### What This Means for Consumers

**Your application's target framework is INDEPENDENT of this package's target framework.**

- If your app targets .NET 6 ? ? Still works
- If your app targets .NET 8 ? ? Still works
- If your app targets .NET Framework 4.7.2 ? ? Still works
- If your app targets .NET Standard 2.0 ? ? Still works

The build tools run in the MSBuild host process, not in your application's process.

## Technical Details

### What Changed
- **MSBuild Task Assembly**: `Protobuf.MSBuild.dll` now targets `netstandard2.0` instead of `net8.0`
- **Package Structure**: Assembly is now in `build\_protobuf\netstandard2.0\` instead of `build\_protobuf\net8.0\`
- **No Runtime Config**: Removed `Protobuf.MSBuild.runtimeconfig.json` (not needed for netstandard2.0)
- **No Deps File**: Set `GenerateDependencyFile=false` to avoid unnecessary dependency tracking

### What Stayed the Same
- **Generated Code**: The F# code generated from your `.proto` files is identical
- **Protoc Compiler**: Still uses the same cross-platform protoc binaries
- **Plugin System**: The protoc-gen-fsgrpc plugin works exactly the same way
- **API Surface**: All MSBuild tasks have the same API

## Testing and Validation

The GitHub Actions workflow (`release-publish.yaml`) has been updated to:
- Look for build artifacts in `bin/Release/netstandard2.0/`
- Verify the `Protobuf.MSBuild.dll` exists (not the `.deps.json` file)

## References

- [Microsoft Docs: MSBuild Task Development](https://docs.microsoft.com/en-us/visualstudio/msbuild/tutorial-custom-task-code-generation)
- [MSBuild SDK Resolver Target Framework Requirements](https://github.com/microsoft/MSBuildSdks/wiki)
- [NuGet Package Targeting for MSBuild](https://docs.microsoft.com/en-us/nuget/create-packages/supporting-multiple-target-frameworks)

## Migration Path

If you're upgrading from v0.8.2 (which was broken) or v0.8.0/v0.8.1:

1. Update your package reference:
   ```xml
   <PackageReference Include="FsGrpc.Tools" Version="0.8.3" />
   ```

2. No code changes needed - just rebuild your project

That's it! The build tools will work exactly as before, but with better MSBuild compatibility.
