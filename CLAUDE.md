# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

- `dotnet build` - Build the solution
- `dotnet test` - Run all tests
- `dotnet test --filter "FullyQualifiedName~TestName"` - Run a specific test

## Architecture

This is a lightweight .NET fuzzy string matching library (`ktsu.FuzzySearch`) published as a NuGet package.

### Project Structure

- **FuzzySearch/** - Main library (multi-targets netstandard2.0, netstandard2.1, net5.0-net10.0)
- **FuzzySearch.Test/** - MSTest test project (targets net10.0 only)

### Core Implementation

The library consists of a single static class `Fuzzy` in `FuzzySearch/Fuzzy.cs` that provides fuzzy string matching with intelligent scoring:

- `Contains(ReadOnlySpan<char>, ReadOnlySpan<char>)` - Check if subject contains pattern characters in sequence
- `Contains(..., out int outScore)` - Same but also returns a quality score

The scoring algorithm rewards:
- Adjacent/consecutive character matches (`adjacentMatchBonus`)
- Matches after separators like `_` or space (`matchAfterSeparatorBonus`)
- Matches at camelCase boundaries (`camelCaseMatchBonus`)

And penalizes:
- Unmatched characters (`unmatchedLetterPenalty`)

### SDK

Uses `ktsu.Sdk` for project configuration (defined in `global.json`). This SDK provides common build settings, multi-targeting configuration, and NuGet packaging defaults.
