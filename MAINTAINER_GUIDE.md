# Quick Reference: MSBuild Task Package Best Practices

## For FsGrpc.Tools Maintainers

### Golden Rules

1. **Target Framework**: Always use `netstandard2.0` for MSBuild tasks
2. **Language Version**: Always use `7.3` (or lower) when targeting netstandard2.0  
3. **No Dependency File**: Always set `<GenerateDependencyFile>false</GenerateDependencyFile>`
4. **Test on Multiple Hosts**: Test with Visual Studio AND `dotnet build`

### Project File Settings

```xml
<PropertyGroup>
  <!-- REQUIRED: MSBuild tasks must target netstandard2.0 -->
  <TargetFramework>netstandard2.0</TargetFramework>
  
  <!-- REQUIRED: C# 7.3 is max for netstandard2.0 compatibility -->
  <LangVersion>7.3</LangVersion>
  
  <!-- REQUIRED: No deps file needed for netstandard2.0 -->
  <GenerateDependencyFile>false</GenerateDependencyFile>
  
  <!-- OPTIONAL: But recommended for build tasks -->
  <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
</PropertyGroup>
```

### Package Structure

```
FsGrpc.Tools.x.x.x.nupkg
??? build/
?   ??? FsGrpc.Tools.props
?   ??? FsGrpc.Tools.targets
?   ??? _protobuf/
?       ??? Google.Protobuf.Tools.targets
?       ??? netstandard2.0/          ? MSBuild tasks here
?           ??? Protobuf.MSBuild.dll
?           ??? MedallionTopologicalSort.dll
??? tools/                            ? Cross-platform binaries
    ??? windows_x64/protoc.exe
    ??? linux_x64/protoc
    ??? macosx_x64/protoc
```

### Verification Checklist

Before releasing:

- [ ] `dotnet build -c Release` succeeds
- [ ] Output is in `bin/Release/netstandard2.0/`
- [ ] `Protobuf.MSBuild.dll` exists
- [ ] No `.deps.json` file
- [ ] No `.runtimeconfig.json` file
- [ ] GitHub Actions workflows don't use unnecessary matrix strategies
- [ ] Step names accurately reflect what's being done
- [ ] Test in a consuming F# project
- [ ] Test with Visual Studio build
- [ ] Test with `dotnet build` CLI

### Common Mistakes to Avoid

? **DON'T**: Use `<LangVersion>latest</LangVersion>` with netstandard2.0  
? **DO**: Use `<LangVersion>7.3</LangVersion>`

? **DON'T**: Target `net6.0`, `net8.0`, etc. for MSBuild tasks  
? **DO**: Target `netstandard2.0` always

? **DON'T**: Include runtime dependencies like `System.Runtime`  
? **DO**: Only reference `Microsoft.Build.Framework` and `Microsoft.Build.Utilities.Core`

? **DON'T**: Set `<GenerateDependencyFile>true</GenerateDependencyFile>`  
? **DO**: Set it to `false` or omit (defaults to false for libraries)

### Debugging MSBuild Loading Issues

If you see `MSB4062: Could not load...` errors:

1. **Check the DLL with ildasm**
   ```powershell
   ildasm /text Protobuf.MSBuild.dll | Select-String "System.Runtime"
   ```
   Look for version numbers - should only see netstandard versions.

2. **Check MSBuild binding logs**
   ```
   set MSBUILDLOGVERBOSITY=diagnostic
   msbuild project.csproj /v:diag > build.log 2>&1
   ```
   Search for "Could not load" in the log.

3. **Verify target framework**
   ```powershell
   # Should show ".NETStandard,Version=v2.0"
   [System.Reflection.Assembly]::LoadFrom("Protobuf.MSBuild.dll").GetCustomAttributes($false) | Where-Object {$_.GetType().Name -eq "TargetFrameworkAttribute"}
   ```

### C# 7.3 Feature Reference

What you CAN use:

- Expression-bodied members: `property => value;`
- Out variables: `if (TryParse(s, out var x))`
- Pattern matching: `if (obj is string s)`
- Local functions: `void LocalFunc() { }`
- Tuples: `(string, int) tuple = ("a", 1);`
- `readonly` structs
- Generic constraints: `where T : unmanaged`

What you CANNOT use:

- Nullable reference types: `string?`
- Switch expressions: `x switch { ... }`
- Default interface members
- Using declarations: `using var ...`
- Static local functions
- Indices and ranges: `array[^1]`, `array[1..3]`
- Async streams: `IAsyncEnumerable<T>`
- Record types: `record Person(...)`

### Package Reference Configuration

For MSBuild tasks, consumers should reference like this:

```xml
<PackageReference Include="FsGrpc.Tools" Version="0.8.4">
  <!-- Optional: Explicitly mark as build-time only -->
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
</PackageReference>
```

### When to Bump Version

- **Patch (0.8.x)**: Bug fixes, no API changes
- **Minor (0.x.0)**: New features, backward compatible
- **Major (x.0.0)**: Breaking changes

MSBuild task loading fix = **Patch** (restores broken functionality)

### Testing Matrix

Test combinations:

| MSBuild Host | Consumer TFM | Should Work |
|--------------|--------------|-------------|
| VS 2022 | net8.0 | ? |
| VS 2022 | net6.0 | ? |
| VS 2022 | netstandard2.0 | ? |
| dotnet CLI 8.0 | net8.0 | ? |
| dotnet CLI 8.0 | net6.0 | ? |
| VS 2019 | net48 | ? |

All should work with netstandard2.0 MSBuild tasks!

### Questions to Ask Before Release

1. Does it build with .NET 8 SDK? ? Must be **YES**
2. Does it target netstandard2.0? ? Must be **YES**
3. Does it use LangVersion 7.3 or lower? ? Must be **YES**
4. Can Visual Studio 2019 load it? ? Must be **YES**
5. Are there any .NET 8 runtime references? ? Must be **NO**

If any answer is wrong, **DON'T RELEASE**.

### Resources

- [MSBuild Task Reference](https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild-task-reference)
- [.NET Standard Compatibility](https://learn.microsoft.com/en-us/dotnet/standard/net-standard)
- [C# Language Versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/configure-language-version)
- [NuGet Package Authoring](https://learn.microsoft.com/en-us/nuget/create-packages/creating-a-package)

## Emergency Rollback Plan

If 0.8.4 still has issues:

1. Identify what's wrong (check GitHub issues, test reports)
2. Yank the package from NuGet (if published publicly)
3. Direct users back to 0.8.0 or 0.8.1 (last known working versions)
4. Create hotfix branch from working version
5. Apply minimal fix only
6. Test extensively
7. Release as 0.8.5

**Remember**: Users are stuck if the package doesn't load. Reliability > features.
