// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.FuzzySearch.Tests;

using System.Reflection;
using System.Text.RegularExpressions;

/// <summary>
/// Guards <c>README.md</c> against documenting an API the library does not ship.
/// </summary>
/// <remarks>
/// The README is the NuGet.org landing page, so a member named there that does not exist is a
/// compile error for the first thing a new consumer copies. These tests read the shipped
/// <c>README.md</c> (copied beside the test assembly by the project file) and check every member
/// and type it names against the library's actual public surface.
/// </remarks>
[TestClass]
public partial class ReadmeTests
{
	/// <summary>Matches a fenced C# sample, capturing its body.</summary>
	[GeneratedRegex("^```csharp\r?\n(.*?)^```", RegexOptions.Multiline | RegexOptions.Singleline)]
	private static partial Regex CSharpSample();

	/// <summary>Matches a member accessed off the <see cref="Fuzzy"/> class, capturing the member name.</summary>
	[GeneratedRegex(@"\bFuzzy\.(\w+)")]
	private static partial Regex FuzzyMemberReference();

	/// <summary>
	/// Matches a library type named without a namespace qualifier, such as <c>FuzzyOptions</c>. The
	/// lookbehind keeps it from firing on the namespace itself in <c>using ktsu.FuzzySearch;</c>.
	/// </summary>
	[GeneratedRegex(@"(?<![.\w])Fuzzy[A-Z]\w*")]
	private static partial Regex LibraryTypeReference();

	/// <summary>Matches a markdown table row, capturing the backticked name in its first cell.</summary>
	[GeneratedRegex(@"^\|\s*`([^`]+)`", RegexOptions.Multiline)]
	private static partial Regex TableRowName();

	[TestMethod]
	public void Readme_CSharpSamples_CallOnlyMembersThatExist()
	{
		// Arrange
		HashSet<string> declaredMembers = PublicMemberNamesOfFuzzy();

		// Act
		string[] documented = [.. CSharpSamples()
			.SelectMany(sample => FuzzyMemberReference().Matches(sample))
			.Select(match => match.Groups[1].Value)
			.Distinct()];

		// Assert
		Assert.IsGreaterThan(0, documented.Length, "No `Fuzzy.` call was found in any README sample, so this test is not checking anything.");
		string[] missing = [.. documented.Where(name => !declaredMembers.Contains(name))];
		Assert.IsEmpty(missing, $"README.md calls Fuzzy members that do not exist: {string.Join(", ", missing)}.");
	}

	[TestMethod]
	public void Readme_CSharpSamples_NameOnlyTypesThatExist()
	{
		// Arrange
		HashSet<string> exportedTypes = PublicTypeNames();

		// Act
		string[] documented = [.. CSharpSamples()
			.SelectMany(sample => LibraryTypeReference().Matches(sample))
			.Select(match => match.Value)
			.Distinct()];

		// Assert
		string[] missing = [.. documented.Where(name => !exportedTypes.Contains(name))];
		Assert.IsEmpty(missing, $"README.md names library types that do not exist: {string.Join(", ", missing)}.");
	}

	[TestMethod]
	public void Readme_ApiReferenceTable_ListsOnlyMembersThatExist()
	{
		// Arrange
		HashSet<string> declaredMembers = PublicMemberNamesOfFuzzy();

		// Act
		string[] listed = [.. TableRowName().Matches(MethodsTable())
			.Select(match => match.Groups[1].Value)
			.Distinct()];

		// Assert
		Assert.IsGreaterThan(0, listed.Length, "The API Reference methods table is empty, so this test is not checking anything.");
		string[] missing = [.. listed.Where(name => !declaredMembers.Contains(name))];
		Assert.IsEmpty(missing, $"The API Reference table lists Fuzzy members that do not exist: {string.Join(", ", missing)}.");
	}

	/// <summary>Reads the <c>README.md</c> copied beside the test assembly.</summary>
	/// <returns>The full text of the README.</returns>
	private static string ReadReadme()
	{
		string path = Path.Combine(AppContext.BaseDirectory, "README.md");
		Assert.IsTrue(File.Exists(path), $"README.md was not copied next to the test assembly; expected it at {path}.");
		return File.ReadAllText(path);
	}

	/// <summary>Gets the body of every fenced C# sample in the README.</summary>
	/// <returns>One string per sample.</returns>
	private static IEnumerable<string> CSharpSamples()
	{
		string readme = ReadReadme();
		MatchCollection samples = CSharpSample().Matches(readme);
		Assert.IsGreaterThan(0, samples.Count, "No ```csharp sample was found in README.md, so these tests are not checking anything.");
		return samples.Select(match => match.Groups[1].Value);
	}

	/// <summary>Gets the API Reference section's methods table.</summary>
	/// <returns>The markdown between the methods heading and the heading that follows it.</returns>
	private static string MethodsTable()
	{
		string readme = ReadReadme();
		int start = readme.IndexOf("#### Methods", StringComparison.Ordinal);
		Assert.IsGreaterThanOrEqualTo(0, start, "README.md no longer has a '#### Methods' heading in its API Reference.");

		int end = readme.IndexOf("\n#### ", start + 1, StringComparison.Ordinal);
		return end < 0 ? readme[start..] : readme[start..end];
	}

	/// <summary>Gets the names of the members <see cref="Fuzzy"/> actually declares publicly.</summary>
	/// <returns>The set of public member names.</returns>
	private static HashSet<string> PublicMemberNamesOfFuzzy() =>
		[.. typeof(Fuzzy)
			.GetMembers(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
			.Select(member => member.Name)];

	/// <summary>Gets the names of the types the library exports.</summary>
	/// <returns>The set of public type names.</returns>
	private static HashSet<string> PublicTypeNames() =>
		[.. typeof(Fuzzy).Assembly.GetExportedTypes().Select(type => type.Name)];
}
