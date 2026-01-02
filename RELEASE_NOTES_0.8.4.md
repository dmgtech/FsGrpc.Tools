# FsGrpc.Tools 0.8.4 Release Notes

## Summary
This release fixes the remaining MSBuild task loading issue in version 0.8.3 by constraining the C# language version to ensure true netstandard2.0 compatibility.

## Critical Fix: C# Language Version Constraint

### Problem in 0.8.3
Version 0.8.3 correctly targeted `netstandard2.0` but still failed with the same `System.Runtime, Version=8.0.0.0` loading error because it used `LangVersion=latest` (C# 12 with .NET 8 SDK).

**Root cause**: The C# 12 compiler can emit IL that references .NET 8 runtime APIs even when targeting netstandard2.0, creating a "phantom dependency" on newer runtime assemblies that MSBuild hosts cannot resolve.

### Solution Implemented
Changed the C# language version from `latest` to `7.3`, which is the highest version fully compatible with netstandard2.0:

```xml
<!-- Before (0.8.3) -->
<LangVersion>latest</LangVersion>

<!-- After (0.8.4) -->
<LangVersion>7.3</LangVersion>
```

### Why C# 7.3?
- **netstandard2.0 compatibility**: C# 7.3 only uses language features that compile to IL compatible with netstandard2.0
- **No runtime dependencies**: Compiled assembly only references types available in netstandard2.0
- **MSBuild host compatibility**: Works with all MSBuild hosts (.NET Framework, .NET Core, .NET 5+)

### Files Modified
- `FsGrpc.Tools/FsGrpc.Tools.csproj`
  - Changed `<LangVersion>` from `latest` to `7.3`
  
- `.github/workflows/release-publish.yaml`
  - Updated artifact verification paths from `net8.0` to `netstandard2.0`
  - Changed verification from `.deps.json` to `.dll` file

## What This Means for You

**If you experienced MSBuild errors with 0.8.2 or 0.8.3**, upgrade to 0.8.4:

```xml
<PackageReference Include="FsGrpc.Tools" Version="0.8.4" />
```

**No code changes required** - just update the package reference and rebuild.

## Technical Details

### C# 7.3 Feature Set (What We're Using)
- ? Expression-bodied members (`property => value;`)
- ? Out variables (`if (int.TryParse(..., out var result))`)
- ? Pattern matching (basic `is` expressions)
- ? Local functions
- ? Tuple types
- ? `readonly` structs

### C# 8+ Features We DON'T Use
- ? Nullable reference types
- ? Switch expressions
- ? Default interface members
- ? Using declarations
- ? Static local functions

The codebase naturally fits within C# 7.3 constraints, so this change has **zero behavioral impact**.

## Compatibility

This package is a **build-time only dependency**:
- ? Works with .NET Framework 4.7.2+ projects
- ? Works with .NET Core 2.0+ projects
- ? Works with .NET 5/6/7/8/9+ projects
- ? Works with Visual Studio 2017+
- ? Works with all MSBuild hosts

Your application's target framework is completely independent of this package.

## Testing

### Build Verification
```bash
dotnet build -c Release FsGrpc.Tools/FsGrpc.Tools.csproj
# Should produce bin/Release/netstandard2.0/Protobuf.MSBuild.dll
```

### Integration Test
1. Create an F# project targeting any framework
2. Add `<PackageReference Include="FsGrpc.Tools" Version="0.8.4" />`
3. Add a `.proto` file with `<Protobuf Include="example.proto" />`
4. Build - should succeed without MSBuild task loading errors

## Version History

| Version | Status | Issue |
|---------|--------|-------|
| 0.8.0 | ? Working | - |
| 0.8.1 | ? Working | - |
| 0.8.2 | ? Broken | Targeted `net8.0` directly |
| 0.8.3 | ? Broken | Targeted `netstandard2.0` but used `LangVersion=latest` |
| 0.8.4 | ? Fixed | Uses `netstandard2.0` with `LangVersion=7.3` |

## Breaking Changes
None. This is a bug fix release that fully restores functionality.

## Migration from 0.8.2 or 0.8.3
Simply update your package reference - no other changes needed:

```xml
<PackageReference Include="FsGrpc.Tools" Version="0.8.4" />
```

## Additional Documentation

See `ISSUE_ROOT_CAUSE_AND_FIX.md` for a detailed technical analysis of the root cause and fix.

See `NETSTANDARD_MIGRATION.md` for information about why MSBuild tasks should target netstandard2.0.
