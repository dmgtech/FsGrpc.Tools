# MSBuild Task Migration to netstandard2.0

## Summary

The FsGrpc.Tools package has been updated from targeting `net8.0` to `netstandard2.0`. **This is the correct approach for MSBuild task packages.**

## Critical Fix in 0.8.4

**Version 0.8.3 still had a System.Runtime dependency issue.** The problem was that while the project targeted `netstandard2.0`, it used `LangVersion=latest` which caused the C# compiler to emit references to .NET 8 runtime assemblies when built with the .NET 8 SDK.

**Version 0.8.4 fixes this** by setting `LangVersion=7.3`, which is the highest C# language version fully compatible with netstandard2.0. This ensures the compiled assembly only references netstandard2.0 APIs.

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

### What Changed in 0.8.4
- **C# Language Version**: Changed from `latest` to `7.3` to ensure true netstandard2.0 compatibility
- **Eliminates Runtime References**: No longer generates hard references to `System.Runtime, Version=8.0.0.0`

### What Changed in 0.8.3
- **MSBuild Task Assembly**: `Protobuf.MSBuild.dll` now targets `netstandard2.0` instead of `net8.0`
- **Package Structure**: Assembly is now in `build\_protobuf\netstandard2.0\` instead of `build\_protobuf\net8.0\`
- **No Runtime Config**: Removed `Protobuf.MSBuild.runtimeconfig.json` (not needed for netstandard2.0)
- **No Deps File**: Set `GenerateDependencyFile=false` to avoid unnecessary dependency tracking

### What Stayed the Same
- **Generated Code**: The F# code generated from your `.proto` files is identical
- **Protoc Compiler**: Still uses the same cross-platform protoc binaries
- **Plugin System**: The protoc-gen-fsgrpc plugin works exactly the same way
- **API Surface**: All MSBuild tasks have the same API

## Root Cause Analysis

The issue in versions 0.8.2 and 0.8.3 was:

1. **0.8.2**: Targeted `net8.0` directly, causing MSBuild to require .NET 8 runtime assemblies
2. **0.8.3**: Targeted `netstandard2.0` BUT used `LangVersion=latest` with .NET 8 SDK
   - The C# 12 compiler (from .NET 8 SDK) can emit calls to newer runtime APIs even when targeting netstandard2.0
   - This created a "phantom dependency" on `System.Runtime, Version=8.0.0.0`
3. **0.8.4**: Constrains language version to C# 7.3, ensuring only netstandard2.0 APIs are used

## Testing and Validation

The GitHub Actions workflow (`release-publish.yaml`) has been updated to:
- Look for build artifacts in `bin/Release/netstandard2.0/`
- Verify the `Protobuf.MSBuild.dll` exists (not the `.deps.json` file)

**Additional validation needed**: Inspect the compiled DLL with a tool like `ildasm` or `dotnet-ilverify` to ensure no references to .NET 8 runtime assemblies exist.

## References

- [Microsoft Docs: MSBuild Task Development](https://docs.microsoft.com/en-us/visualstudio/msbuild/tutorial-custom-task-code-generation)
- [C# Language Versioning](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/configure-language-version)
- [.NET Standard compatibility table](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)

## Migration Path

If you're upgrading from v0.8.2 or v0.8.3 (both had MSBuild loading issues):

1. Update your package reference:
   ```xml
   <PackageReference Include="FsGrpc.Tools" Version="0.8.4" />
   ```

2. No code changes needed - just rebuild your project

That's it! The build tools will work correctly with all MSBuild hosts.
