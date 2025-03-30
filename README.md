# ktsu.FuzzySearch

A lightweight .NET library that provides fuzzy string matching capabilities, allowing for approximate string matching with intelligent scoring. It's perfect for implementing search-as-you-type features, command palettes, or any application requiring flexible string matching.

[![License](https://img.shields.io/github/license/ktsu-dev/FuzzySearch.svg?label=License&logo=nuget)](LICENSE.md)

[![NuGet Version](https://img.shields.io/nuget/v/ktsu.FuzzySearch?label=Stable&logo=nuget)](https://nuget.org/packages/ktsu.FuzzySearch)
[![NuGet Version](https://img.shields.io/nuget/vpre/ktsu.FuzzySearch?label=Latest&logo=nuget)](https://nuget.org/packages/ktsu.FuzzySearch)
[![NuGet Downloads](https://img.shields.io/nuget/dt/ktsu.FuzzySearch?label=Downloads&logo=nuget)](https://nuget.org/packages/ktsu.FuzzySearch)

[![GitHub commit activity](https://img.shields.io/github/commit-activity/m/ktsu-dev/FuzzySearch?label=Commits&logo=github)](https://github.com/ktsu-dev/FuzzySearch/commits/main)
[![GitHub contributors](https://img.shields.io/github/contributors/ktsu-dev/FuzzySearch?label=Contributors&logo=github)](https://github.com/ktsu-dev/FuzzySearch/graphs/contributors)
[![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/ktsu-dev/FuzzySearch/dotnet.yml?label=Build&logo=github)](https://github.com/ktsu-dev/FuzzySearch/actions)

## Installation

To install FuzzySearch, you can use the .NET CLI:

```bash
dotnet add package ktsu.FuzzySearch
```

Or you can use the NuGet Package Manager in Visual Studio to search for and install the `ktsu.FuzzySearch` package.

## Usage

### Basic Matching

The simplest way to check if a string contains characters from a pattern in sequence:

```csharp
using ktsu.FuzzySearch;
class Program
{
    static void Main()
    {
        string text = "Hello World"; string pattern = "hlo";
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
        string text = "FuzzyStringMatcher";
        string pattern = "fzm";
        bool isMatch = Fuzzy.Contains(text, pattern, out int score);
    
        // isMatch will be true
        // score will contain a value reflecting match quality, with positive values indicating a better match
        Console.WriteLine($"Match found: {isMatch}, Score: {score}");
    }
}
```

### Implementing Search Features

FuzzySearch is ideal for implementing searchable lists or command palettes:

```csharp
using ktsu.FuzzySearch;
using System.Linq;

class Program
{
    static void Main()
    {
        var items = new[]
        {
            "ApplicationSettings",
            "UserPreferences",
            "SecurityOptions",
            "AccountManagement",
            "SystemConfiguration"
        };
        string searchTerm = "set";
    
        // Find matches and order by score (best matches first)
        var matches = items
            .Select(item => new
            {
                Item = item,
                IsMatch = Fuzzy.Contains(item, searchTerm, out int score),
                Score = score
            })
            .Where(result => result.IsMatch)
            .OrderByDescending(result => result.Score);
        
        foreach (var match in matches)
        {
            Console.WriteLine($"{match.Item}: {match.Score}");
        }

        // Output will prioritize "ApplicationSettings" over other matches
    }
}
```

## Scoring System

The library uses a scoring system that rewards:

- **Consecutive character matches** - Characters that appear adjacent to each other in the pattern and subject receive a bonus
- **Matches after separators** - Characters that appear after `_` or space characters receive a bonus
- **CamelCase boundary matches** - Characters that appear at camelCase boundaries receive a bonus

Meanwhile, penalties are applied for:

- **Unmatched characters** - Characters that are not found in the pattern receive a penalty

## API Reference

### Fuzzy Class

- `bool Contains(string subject, string pattern)`: Determines if the subject string contains all characters from the pattern in sequence.
- `bool Contains(string subject, string pattern, out int score)`: Determines if the subject contains all pattern characters and provides a match quality score.

## Case Sensitivity

All matching operations are case-insensitive, making the library more forgiving and user-friendly for search scenarios.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE.md) file for details.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request for any improvements or bug fixes.

## Acknowledgements

Thanks to the .NET community and ktsu.dev contributors for their support.

This library was adapted from the [implementation](https://gist.github.com/CDillinger/2aa02128f840bdca90340ce08ee71bc2) posted by [Collin Dillinger](https://github.com/CDillinger).
