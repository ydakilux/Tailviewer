# Tailviewer - Agent Development Guide

This document provides essential guidelines for AI coding agents working on the Tailviewer codebase.

## Project Overview

Tailviewer is a free and open-source log file viewer for Windows, built with C# and WPF (.NET Framework 4.7.1).
The application supports live filtering, multiline log entries, merging multiple files, and a plugin system for custom log formats.

## Build & Test Commands

### Building the Solution
```bash
# Build entire solution
msbuild Tailviewer.sln /p:Configuration=Release

# Build specific project
msbuild src/Tailviewer.Core/Tailviewer.Core.csproj /p:Configuration=Debug

# Build with dotnet CLI (if SDK supports .NET Framework)
dotnet build Tailviewer.sln
```

### Running Tests
```bash
# Run all tests in a specific test project using dotnet
dotnet test src/Tailviewer.Core.Tests/Tailviewer.Core.Tests.csproj

# Run tests using NUnit Console Runner (if installed)
nunit3-console bin/Tailviewer.Core.Tests.dll

# Run specific test fixture
dotnet test --filter "FullyQualifiedName~SubstringFilterTest"

# Run single test method
dotnet test --filter "FullyQualifiedName~SubstringFilterTest.TestMatch1"
```

### Output Directory
- All projects output to: `bin/` (repository root)
- Debug builds: `bin/` 
- Release builds: `bin/`

## Project Structure

```
src/
├── Tailviewer/                    # Main WPF application
├── Tailviewer.Tests/              # Main app tests
├── Tailviewer.Core/               # Core log processing library
├── Tailviewer.Core.Tests/         # Core library tests
├── Tailviewer.Api/                # Public plugin API
├── Tailviewer.Api.Tests/          # API tests
├── Tailviewer.Archiver/           # Log archiving functionality
├── Tailviewer.Acceptance.Tests/   # End-to-end tests
├── Tailviewer.System.Tests/       # System integration tests
├── Tailviewer.PluginRepository/   # Plugin repository server
└── Plugins/                       # Built-in plugins
```

## Code Style Guidelines

### Naming Conventions

**Classes & Interfaces:**
- PascalCase for all types: `TextLogSourceFactory`, `ILogSource`
- Interface names start with `I`: `ILogSource`, `ITaskScheduler`
- Test fixtures end with `Test`: `SubstringFilterTest`
- ViewModels end with `ViewModel`: `MainWindowViewModel`

**Fields & Properties:**
- Private fields use camelCase with underscore prefix: `_taskScheduler`, `_filesystem`
- Public properties use PascalCase: `RawContent`, `IsText`
- Constants use PascalCase: `Log`

**Methods:**
- PascalCase: `OpenRead()`, `PassesFilter()`, `EndsWithNewline()`
- Test methods start with `Test`: `TestMatch1()`, `TestPassesFilter()`

### Using Statements
```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;      // Test projects only
using NUnit.Framework;       // Test projects only
using Tailviewer.Api;
using Tailviewer.Core;
```
- System namespaces first (alphabetical)
- Third-party libraries next
- Project namespaces last
- No unused using statements

### Formatting & Layout

**Braces:**
- Opening braces on new line (Allman style)
- Always use braces, even for single-line blocks

**Indentation:**
- Tabs for indentation (NOT spaces)
- Follow existing indentation in files

**Regions:**
```csharp
#region Implementation of IFileLogSourceFactory
// Implementation here
#endregion
```
- Use regions for interface implementations
- Keep regions minimal and meaningful

**XML Documentation:**
```csharp
/// <summary>
/// Brief description of the method/class.
/// </summary>
/// <param name="fileName">Parameter description</param>
/// <returns>Return value description</returns>
public ILogSource OpenRead(string fileName)
{
    // ...
}
```
- All public members should have XML documentation
- Keep summaries concise and descriptive
- Use `<inheritdoc />` for interface implementations when appropriate

### Type Annotations

**Explicit Types Preferred:**
```csharp
// Preferred
ITaskScheduler taskScheduler = new TaskScheduler();
List<LogLineMatch> matches = new List<LogLineMatch>();

// Acceptable for obvious types
var filter = new SubstringFilter("a", true);
var length = that.Length;
```

**Nullability:**
- Check for null before use (pre-nullable reference types era)
- Use null checks at method boundaries
```csharp
if (that == null)
    return false;
```

### Testing Guidelines

**Test Framework:**
- NUnit 3.12 for all tests
- FluentAssertions 5.6 for assertions

**Test Structure:**
```csharp
[TestFixture]
public sealed class SubstringFilterTest
{
    [Test]
    public void TestMatch1()
    {
        // Arrange
        var filter = new SubstringFilter("Foobar", true);
        var matches = new List<LogLineMatch>();
        
        // Act
        new Action(() => filter.Match(entry, matches)).Should().NotThrow();
        
        // Assert
        matches.Should().BeEmpty("because null content should not match");
    }
}
```

**Assertion Style:**
- Use FluentAssertions syntax: `actual.Should().Be(expected)`
- Always provide a reason: `Should().Be(expected, "because X")`
- From CONTRIBUTING.md: Assertions should declare the reason **why** in text form

**Test Requirements (from CONTRIBUTING.md):**
- Bugfixes require tests that reproduce the bug (unless infeasible)
- New classes/methods require tests
- Tests must build on AppVeyor before PRs are accepted

### Error Handling

**Logging:**
```csharp
private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

Log.WarnFormat("Log file {0} has been determined to be a binary file ({1})", fileName, format);
Log.ErrorFormat("Failed to process {0}", fileName);
```
- Use log4net for logging
- Static readonly Log field per class
- Use format methods with parameters (not string interpolation in logs)

**Exceptions:**
- Check preconditions early
- Return null or default values when appropriate (older .NET style)
- Use specific exception types

### Assembly Signing
- All assemblies are strong-named
- Key file: `sig/key.snk`
- This is configured in all .csproj files

## Pull Request Guidelines (from CONTRIBUTING.md)

- Keep PRs **small** - avoid hundreds of file changes
- One concern per PR - don't mix features with refactoring
- PRs must build on AppVeyor
- Don't break existing features
- Big features should be split into multiple PRs
- Bugfixes require tests which reproduce the bug
- New classes/methods require tests

## CI/CD

- **CI Platform:** AppVeyor
- All tests must pass before merge
- Build status: https://ci.appveyor.com/project/Kittyfisto/sharptail

## Common Patterns

**Dependency Injection:**
```csharp
public TextLogSourceFactory(IFilesystem filesystem, ITaskScheduler taskScheduler)
{
    _filesystem = filesystem;
    _taskScheduler = taskScheduler;
}
```
- Constructor injection throughout
- Store dependencies in readonly fields

**Extension Methods:**
```csharp
public static class StringExtensions
{
    public static bool EndsWithNewline(this string that)
    {
        if (that == null)
            return false;
        // ...
    }
}
```
- Use extension methods for utility functions
- Place in static classes ending with `Extensions`

## Additional Notes

- **Target Framework:** .NET Framework 4.7.1
- **UI Framework:** WPF with MVVM pattern
- **Minimum Windows Version:** Windows 7
- **IDE:** Visual Studio 2015 or higher (ReSharper optional but recommended)
- ReSharper settings in `.sln.DotSettings` files
