# ktsu.FuzzySearch

> A lightweight .NET library that provides fuzzy string matching capabilities, allowing for approximate string matching with intelligent scoring.

[![License](https://img.shields.io/github/license/ktsu-dev/FuzzySearch.svg?label=License&logo=nuget)](LICENSE.md)
[![NuGet Version](https://img.shields.io/nuget/v/ktsu.FuzzySearch?label=Stable&logo=nuget)](https://nuget.org/packages/ktsu.FuzzySearch)
[![NuGet Version](https://img.shields.io/nuget/vpre/ktsu.FuzzySearch?label=Latest&logo=nuget)](https://nuget.org/packages/ktsu.FuzzySearch)
[![NuGet Downloads](https://img.shields.io/nuget/dt/ktsu.FuzzySearch?label=Downloads&logo=nuget)](https://nuget.org/packages/ktsu.FuzzySearch)
[![GitHub commit activity](https://img.shields.io/github/commit-activity/m/ktsu-dev/FuzzySearch?label=Commits&logo=github)](https://github.com/ktsu-dev/FuzzySearch/commits/main)
[![GitHub contributors](https://img.shields.io/github/contributors/ktsu-dev/FuzzySearch?label=Contributors&logo=github)](https://github.com/ktsu-dev/FuzzySearch/graphs/contributors)
[![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/ktsu-dev/FuzzySearch/dotnet.yml?branch=main&label=Build&logo=github)](https://github.com/ktsu-dev/FuzzySearch/actions)

## Introduction

FuzzySearch is a .NET library that provides fuzzy string matching capabilities with intelligent scoring. It's perfect for implementing search-as-you-type features, command palettes, or any application requiring flexible string matching.

The whole library is one static class, `Fuzzy`, with two `Contains` overloads: one that answers whether a subject matches a pattern, and one that also hands back a score you can rank by.

## Features

- **Subsequence Matching**: Match a subject against a pattern whose characters appear in order but not necessarily together
- **Intelligent Scoring**: Rank matches by quality with a scoring algorithm that rewards adjacent matches, matches after separators, and matches at camelCase boundaries
- **Case Insensitive**: Matching always ignores case; there is no case-sensitive mode
- **Unicode Aware**: Input is normalized to NFC and compared one codepoint at a time, so surrogate pairs match as a whole
- **Span Based**: Takes `ReadOnlySpan<char>`, and allocates nothing for ASCII input
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

The second overload also reports a score, so you can tell a good match from a barely-there one:

```csharp
using ktsu.FuzzySearch;

class Program
{
    static void Main()
    {
        string text = "Hello World";
        string pattern = "hlo";

        bool isMatch = Fuzzy.Contains(text, pattern, out int score);

        Console.WriteLine($"Is match: {isMatch}"); // True
        Console.WriteLine($"Score: {score}");      // Higher is better
    }
}
```

The score is an unbounded `int`, not a normalized ratio. It is only meaningful when comparing candidates against the *same* pattern, and it is reported whether or not the whole pattern was found — a near miss still scores. Use the return value to decide whether it matched at all, and the score to order the matches.

### Ranking a Collection

There is no built-in filter method. Ranking a collection is a `Contains` call per candidate:

```csharp
using ktsu.FuzzySearch;

class Program
{
    static void Main()
    {
        string[] items =
        [
            "AppDataStorage",
            "Application Settings",
            "Data Store",
            "File System",
            "Storage Provider",
        ];

        string pattern = "stor";

        List<(string Item, int Score)> matches = [];
        foreach (string item in items)
        {
            if (Fuzzy.Contains(item, pattern, out int score))
            {
                matches.Add((item, score));
            }
        }

        foreach ((string item, int score) in matches.OrderByDescending(match => match.Score))
        {
            Console.WriteLine($"{item} (Score: {score})");
        }

        // Storage Provider (Score: 15)
        // Data Store (Score: 14)
        // AppDataStorage (Score: 10)
    }
}
```

### Matching Against Objects

The same shape works for objects — project each one to the text you want matched:

```csharp
using ktsu.FuzzySearch;

class Program
{
    sealed class FileItem
    {
        public required string Name { get; init; }
        public required string Path { get; init; }
    }

    static void Main()
    {
        FileItem[] files =
        [
            new() { Name = "Document.pdf", Path = "/documents/" },
            new() { Name = "Presentation.pptx", Path = "/presentations/" },
            new() { Name = "Spreadsheet.xlsx", Path = "/spreadsheets/" },
        ];

        string pattern = "doc";

        foreach (FileItem file in files)
        {
            if (Fuzzy.Contains(file.Name, pattern, out int score))
            {
                Console.WriteLine($"{file.Name} (Score: {score})");
            }
        }
    }
}
```

## API Reference

### `Fuzzy` Static Class

`Fuzzy` is the library's entire public surface. It is a static class with two methods, both overloads of `Contains`.

#### Methods

| Name | Parameters | Return Type | Description |
|------|------------|-------------|-------------|
| `Contains` | `ReadOnlySpan<char> subject, ReadOnlySpan<char> pattern` | `bool` | Whether `subject` contains every character of `pattern`, in order |
| `Contains` | `ReadOnlySpan<char> subject, ReadOnlySpan<char> pattern, out int outScore` | `bool` | The same answer, plus a match-quality score |

Both overloads take `ReadOnlySpan<char>`, so a `string` argument is passed straight through by the compiler's implicit conversion — there is no separate `string` overload to look for.

#### Behaviour

- **Case is always ignored.** There is no option to make matching case-sensitive.
- **An empty pattern matches any non-empty subject**, and scores `0`. An empty subject never matches.
- **Scores are unbounded `int`s**, comparable only within a single pattern. `outScore` is set even when the method returns `false`.
- **Input is normalized to NFC** before comparison, so precomposed and decomposed text match. This relies on the runtime's globalization data: under `InvariantGlobalization`, `string.Normalize` is a no-op and the two forms will not match.
- **Comparison advances one codepoint at a time**, so a surrogate pair matches only as a whole and a lone surrogate cannot match half of an unrelated character.

#### Scoring

| Rule | Effect |
|------|--------|
| Match adjacent to the previous match | `+5` |
| Match after a `_` or space separator | `+10` |
| Match at a camelCase boundary | `+10` |
| Each unmatched character | `-1` |
| Unmatched characters before the first match | `-1` each, capped at `-5` |

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
