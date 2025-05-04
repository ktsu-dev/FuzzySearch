# ktsu.FuzzySearch

> A lightweight .NET library that provides fuzzy string matching capabilities, allowing for approximate string matching with intelligent scoring.

[![License](https://img.shields.io/github/license/ktsu-dev/FuzzySearch.svg?label=License&logo=nuget)](LICENSE.md)
[![NuGet Version](https://img.shields.io/nuget/v/ktsu.FuzzySearch?label=Stable&logo=nuget)](https://nuget.org/packages/ktsu.FuzzySearch)
[![NuGet Version](https://img.shields.io/nuget/vpre/ktsu.FuzzySearch?label=Latest&logo=nuget)](https://nuget.org/packages/ktsu.FuzzySearch)
[![NuGet Downloads](https://img.shields.io/nuget/dt/ktsu.FuzzySearch?label=Downloads&logo=nuget)](https://nuget.org/packages/ktsu.FuzzySearch)
[![GitHub commit activity](https://img.shields.io/github/commit-activity/m/ktsu-dev/FuzzySearch?label=Commits&logo=github)](https://github.com/ktsu-dev/FuzzySearch/commits/main)
[![GitHub contributors](https://img.shields.io/github/contributors/ktsu-dev/FuzzySearch?label=Contributors&logo=github)](https://github.com/ktsu-dev/FuzzySearch/graphs/contributors)
[![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/ktsu-dev/FuzzySearch/dotnet.yml?label=Build&logo=github)](https://github.com/ktsu-dev/FuzzySearch/actions)

## Introduction

FuzzySearch is a .NET library that provides fuzzy string matching capabilities with intelligent scoring. It's perfect for implementing search-as-you-type features, command palettes, or any application requiring flexible string matching. This library offers both basic contains-style matching and more sophisticated algorithms that can rank multiple potential matches by relevance.

## Features

- **Fuzzy String Matching**: Match strings even when they contain typos or missing characters
- **Intelligent Scoring**: Rank matches by quality with a smart scoring algorithm
- **Case Insensitivity**: Optional case-insensitive matching
- **Filtering Collections**: Filter lists of strings and rank results
- **Customizable Parameters**: Adjust matching behavior to suit different needs
- **Lightweight**: Minimal dependencies, focused on performance
- **Well-tested**: Comprehensive test suite ensuring reliability

## Installation

### Package Manager Console

```powershell
Install-Package ktsu.FuzzySearch
```

### .NET CLI

```bash
dotnet add package ktsu.FuzzySearch
```

### Package Reference

```xml
<PackageReference Include="ktsu.FuzzySearch" Version="x.y.z" />
```

## Usage Examples

### Basic Matching

The simplest way to check if a string contains characters from a pattern in sequence:

```csharp
using ktsu.FuzzySearch;

class Program
{
    static void Main()
    {
        string text = "Hello World";
        string pattern = "hlo";
        
        bool isMatch = Fuzzy.Contains(text, pattern); // Returns true
    }
}
```

### Matching with Scoring

To get both a match result and a score that indicates the quality of the match:

```csharp
using ktsu.FuzzySearch;

class Program
{
    static void Main()
    {
        string text = "Hello World";
        string pattern = "hlo";
        
        var result = Fuzzy.Match(text, pattern);
        
        Console.WriteLine($"Is match: {result.IsMatch}");          // True
        Console.WriteLine($"Score: {result.Score}");               // A value between 0-1
        Console.WriteLine($"Character indices: {result.Indices}"); // Indices of matched characters
    }
}
```

### Filtering a Collection

Filter a list of strings and sort them by match quality:

```csharp
using ktsu.FuzzySearch;

class Program
{
    static void Main()
    {
        var items = new List<string>
        {
            "AppDataStorage",
            "Application Settings",
            "Data Store",
            "File System",
            "Storage Provider"
        };
        
        string pattern = "appstor";
        
        // Filter and rank by match quality
        var results = Fuzzy.Filter(items, pattern);
        
        foreach (var result in results)
        {
            Console.WriteLine($"{result.Item} (Score: {result.Score})");
        }
        
        // Output might be:
        // AppDataStorage (Score: 0.89)
        // Application Settings (Score: 0.65)
        // Storage Provider (Score: 0.52)
    }
}
```

### Advanced Options

Customize the matching behavior with options:

```csharp
using ktsu.FuzzySearch;

class Program
{
    static void Main()
    {
        var options = new FuzzyOptions
        {
            CaseSensitive = true,              // Default is false
            ScoreThreshold = 0.4,              // Minimum score to consider a match
            BonusConsecutiveChars = 1.5,       // Bonus for consecutive matched characters
            BonusStartOfWord = 2.0,            // Bonus for matches at word boundaries
            PenaltyUnmatched = 0.1,            // Penalty for unmatched characters
            MaxPatternLength = 64              // Maximum pattern length to consider
        };
        
        string text = "FileSystemWatcher";
        string pattern = "FSW";
        
        var result = Fuzzy.Match(text, pattern, options);
        Console.WriteLine($"Score with custom options: {result.Score}");
    }
}
```

### Object Collections

Filter and match against object collections by providing a selector function:

```csharp
using ktsu.FuzzySearch;

class Program
{
    class FileItem
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
    }
    
    static void Main()
    {
        var files = new List<FileItem>
        {
            new FileItem { Name = "Document.pdf", Path = "/documents/", Size = 1024 },
            new FileItem { Name = "Presentation.pptx", Path = "/presentations/", Size = 2048 },
            new FileItem { Name = "Spreadsheet.xlsx", Path = "/spreadsheets/", Size = 512 }
        };
        
        string pattern = "doc";
        
        // Filter objects using a selector function
        var results = Fuzzy.Filter(files, pattern, item => item.Name);
        
        foreach (var result in results)
        {
            Console.WriteLine($"{result.Item.Name} (Score: {result.Score})");
        }
    }
}
```

## API Reference

### `Fuzzy` Static Class

The main class providing fuzzy matching functionality.

#### Methods

| Name | Parameters | Return Type | Description |
|------|------------|-------------|-------------|
| `Contains` | `string text, string pattern, bool caseSensitive = false` | `bool` | Checks if the text contains the pattern in sequence |
| `Match` | `string text, string pattern, FuzzyOptions options = null` | `FuzzyResult` | Matches text against pattern with scoring |
| `Filter` | `IEnumerable<string> items, string pattern, FuzzyOptions options = null` | `IEnumerable<FuzzyItem<string>>` | Filters and ranks a collection of strings |
| `Filter<T>` | `IEnumerable<T> items, string pattern, Func<T, string> selector, FuzzyOptions options = null` | `IEnumerable<FuzzyItem<T>>` | Filters and ranks a collection of objects using a selector function |

### `FuzzyResult` Class

Represents the result of a fuzzy match operation.

#### Properties

| Name | Type | Description |
|------|------|-------------|
| `IsMatch` | `bool` | Indicates if the pattern matches the text |
| `Score` | `double` | A value between 0 and 1 indicating match quality (1 is perfect) |
| `Indices` | `int[]` | The indices in the text where pattern characters were matched |

### `FuzzyOptions` Class

Configuration options for fuzzy matching.

#### Properties

| Name | Type | Default | Description |
|------|------|---------|-------------|
| `CaseSensitive` | `bool` | `false` | Whether matching should be case-sensitive |
| `ScoreThreshold` | `double` | `0.3` | Minimum score required to consider a match valid |
| `BonusConsecutiveChars` | `double` | `1.0` | Score bonus for consecutive matched characters |
| `BonusStartOfWord` | `double` | `1.5` | Score bonus for matches at word boundaries |
| `PenaltyUnmatched` | `double` | `0.1` | Score reduction for unmatched characters |

## Contributing

Contributions are welcome! Here's how you can help:

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

Please make sure to update tests as appropriate and adhere to the existing coding style.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.
