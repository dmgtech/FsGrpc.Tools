# FsGrpc.Tools MSBuild Loading Issue - Root Cause and Fix

## Issue Summary

**Versions Affected**: 0.8.2, 0.8.3  
**Status**: Fixed in 0.8.4  
**Severity**: Critical - Package completely non-functional

## Problem Description

MSBuild fails to load the `Protobuf.MSBuild.dll` assembly with the error:

```
MSB4062: The "Grpc.Tools.ProtoToolsPlatform" task could not be loaded from the assembly 
C:\Users\[user]\.nuget\packages\fsgrpc.tools\0.8.3\build\_protobuf\netstandard2.0\Protobuf.MSBuild.dll. 
Could not load file or assembly 'System.Runtime, Version=8.0.0.0, Culture=neutral, 
PublicKeyToken=b03f5f7f11d50a3a' or one of its dependencies.
```

## Root Cause Analysis

### Version 0.8.2
- **Problem**: Directly targeted `net8.0` framework
- **Impact**: MSBuild hosts (especially .NET Framework-based Visual Studio) cannot load .NET 8 assemblies
- **Why it failed**: Included `Protobuf.MSBuild.runtimeconfig.json` requiring .NET 8 runtime

### Version 0.8.3  
- **Problem**: Targeted `netstandard2.0` BUT used `LangVersion=latest` (C# 12)
- **Impact**: C# 12 compiler emitted references to .NET 8 runtime assemblies despite netstandard2.0 target
- **Why it failed**: "Phantom dependency" on `System.Runtime, Version=8.0.0.0` embedded in the compiled IL

The critical insight: **Target framework and language version are independent**. Using C# 12 features with .NET 8 SDK causes the compiler to emit references to newer runtime APIs, even when targeting older frameworks.

## The Fix (Version 0.8.4)

### Changes Made

1. **Constrain C# Language Version**
   ```xml
   <!-- Changed from -->
   <LangVersion>latest</LangVersion>
   
   <!-- To -->
   <LangVersion>7.3</LangVersion>
   ```
   
   **Rationale**: C# 7.3 is the highest language version fully supported by netstandard2.0 without requiring newer runtime APIs.

2. **Maintain netstandard2.0 Target**
   ```xml
   <TargetFramework>netstandard2.0</TargetFramework>
   ```
   
   **Rationale**: MSBuild tasks MUST target netstandard2.0 for maximum compatibility across MSBuild hosts.

3. **Keep GenerateDependencyFile=false**
   ```xml
   <GenerateDependencyFile>false</GenerateDependencyFile>
   ```
   
   **Rationale**: netstandard2.0 assemblies don't need `.deps.json` files.

### Why This Works

- **C# 7.3**: Only uses language features that compile to IL compatible with netstandard2.0
- **netstandard2.0**: Provides stable ABI that all MSBuild hosts can load
- **No runtime dependencies**: The compiled DLL only references types available in netstandard2.0

### Code Compatibility

The codebase already uses only C# 7.3-compatible features:
- ? Expression-bodied members
- ? Out variables  
- ? Pattern matching (basic)
- ? Local functions
- ? No C# 8+ features (nullable reference types, switch expressions, etc.)

## Verification Steps

To verify the fix works:

1. **Build the project**
   ```
   dotnet build -c Release FsGrpc.Tools/FsGrpc.Tools.csproj
   ```

2. **Inspect the assembly** (optional but recommended)
   ```powershell
   # Check for .NET 8 runtime references
   ildasm FsGrpc.Tools/bin/Release/netstandard2.0/Protobuf.MSBuild.dll /text | Select-String "System.Runtime.*Version=8"
   # Should return nothing if fix is correct
   ```

3. **Test in consuming project**
   - Create an F# project
   - Add package reference to FsGrpc.Tools 0.8.4
   - Add a `.proto` file
   - Build - should succeed without MSBuild task loading errors

## Prevention

To prevent this issue in future:

1. **Always use appropriate LangVersion for target framework**
   - netstandard2.0 ? C# 7.3 max
   - netstandard2.1 ? C# 8.0 max
   - net5.0+ ? Use `latest` if desired

2. **Test on multiple MSBuild hosts**
   - Visual Studio 2019/2022 (both have different MSBuild versions)
   - `dotnet build` CLI
   - .NET Framework projects vs .NET Core projects

3. **Inspect compiled assemblies**
   - Use `ildasm` or IL inspection tools
   - Check for unexpected runtime assembly references

## Related Documentation

- [C# Language Versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/configure-language-version)
- [.NET Standard versions](https://learn.microsoft.com/en-us/dotnet/standard/net-standard)
- [MSBuild Task Writing](https://learn.microsoft.com/en-us/visualstudio/msbuild/task-writing)

## Timeline

- **v0.8.0**: Working (likely targeted net6.0 with appropriate LangVersion)
- **v0.8.1**: Working
- **v0.8.2**: Broken - switched to net8.0 target
- **v0.8.3**: Still broken - netstandard2.0 target but LangVersion=latest
- **v0.8.4**: Fixed - netstandard2.0 with LangVersion=7.3
