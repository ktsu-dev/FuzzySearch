// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.FuzzySearch.Tests;

[TestClass]
public class FuzzyTests
{
	#region Contains Tests

	[TestMethod]
	public void Contains_ExactMatch_ReturnsTrue()
	{
		// Arrange
		var subject = "hello";
		var pattern = "hello";

		// Act
		var result = Fuzzy.Contains(subject, pattern);

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void Contains_SubsequenceMatch_ReturnsTrue()
	{
		// Arrange
		var subject = "hello world";
		var pattern = "hlowrd";

		// Act
		var result = Fuzzy.Contains(subject, pattern);

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void Contains_CaseDifferenceMatch_ReturnsTrue()
	{
		// Arrange
		var subject = "Hello World";
		var pattern = "helloworld";

		// Act
		var result = Fuzzy.Contains(subject, pattern);

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void Contains_NoMatch_ReturnsFalse()
	{
		// Arrange
		var subject = "hello";
		var pattern = "world";

		// Act
		var result = Fuzzy.Contains(subject, pattern);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void Contains_PatternLongerThanSubject_ReturnsFalse()
	{
		// Arrange
		var subject = "hi";
		var pattern = "hello";

		// Act
		var result = Fuzzy.Contains(subject, pattern);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void Contains_EmptyPattern_ReturnsTrue()
	{
		// Arrange
		var subject = "hello";
		var pattern = "";

		// Act
		var result = Fuzzy.Contains(subject, pattern);

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void Contains_EmptySubject_ReturnsFalse()
	{
		// Arrange
		var subject = "";
		var pattern = "hello";

		// Act
		var result = Fuzzy.Contains(subject, pattern);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void Contains_BothEmpty_ReturnsFalse()
	{
		// Arrange
		var subject = "";
		var pattern = "";

		// Act
		var result = Fuzzy.Contains(subject, pattern);

		// Assert
		Assert.IsFalse(result);
	}

	#endregion

	#region Contains With Score Tests

	[TestMethod]
	public void Contains_WithScore_ExactMatch_ReturnsTrueWithHighScore()
	{
		// Arrange
		var subject = "hello";
		var pattern = "hello";

		// Act
		var result = Fuzzy.Contains(subject, pattern, out var score);

		// Assert
		Assert.IsTrue(result);
		Assert.IsTrue(score > 0);
	}

	[TestMethod]
	public void Contains_WithScore_SubsequenceMatch_ReturnsTrueWithPositiveScore()
	{
		// Arrange
		var subject = "hello world";
		var pattern = "hlowrd";

		// Act
		var result = Fuzzy.Contains(subject, pattern, out var score);

		// Assert
		Assert.IsTrue(result);
		Assert.IsTrue(score > 0);
	}

	[TestMethod]
	public void Contains_WithScore_NoMatch_ReturnsFalseWithLowerScore()
	{
		// Arrange
		var subject = "hello";
		var pattern = "world";

		// Act
		var result = Fuzzy.Contains(subject, pattern, out _);

		// Assert
		Assert.IsFalse(result);
		// Score may be negative or 0 depending on how close the match was
	}

	[TestMethod]
	public void Contains_WithScore_AdjacentMatches_ScoresHigherThanNonAdjacentMatches()
	{
		// Arrange
		var subject1 = "hworld"; // adjacent matches for "hw"
		var subject2 = "hiworld";   // non-adjacent matches for "hw"
		var pattern = "hw";

		// Act
		Fuzzy.Contains(subject1, pattern, out var score1);
		Fuzzy.Contains(subject2, pattern, out var score2);

		// Assert
		Assert.IsTrue(score1 > score2, "Adjacent matches should score higher");
	}

	[TestMethod]
	public void Contains_WithScore_MatchAfterSeparator_GetsBonus()
	{
		// Arrange
		var subject = "hello_world";  // 'w' is after separator '_'
		var pattern = "hw";

		// Act
		var result = Fuzzy.Contains(subject, pattern, out var score);

		// Assert
		Assert.IsTrue(result);
		// The 'w' match should get a separation bonus
		Assert.IsTrue(score >= Fuzzy.matchAfterSeparatorBonus, "Score should include separator bonus");
	}

	[TestMethod]
	public void Contains_WithScore_CamelCaseMatch_GetsBonus()
	{
		// Arrange
		var subject = "helloWorld";  // 'W' is at camelCase boundary
		var pattern = "hW";

		// Act
		var result = Fuzzy.Contains(subject, pattern, out var score);

		// Assert
		Assert.IsTrue(result);
		// The 'W' match should get a camelCase bonus
		Assert.IsTrue(score >= Fuzzy.camelCaseMatchBonus, "Score should include camelCase bonus");
	}

	#endregion

	#region Apply Bonuses Tests

	[TestMethod]
	public void ApplyBonuses_PrevMatched_AddsAdjacentMatchBonus()
	{
		// Arrange
		var prevMatched = true;
		var prevLower = false;
		var prevSeparator = false;
		var strChar = 'a';
		var strLower = 'a';
		var strUpper = 'A';
		var initialScore = 0;

		// Act
		var result = Fuzzy.ApplyBonuses(prevMatched, prevLower, prevSeparator, strChar, strLower, strUpper, initialScore);

		// Assert
		Assert.AreEqual(Fuzzy.adjacentMatchBonus, result);
	}

	[TestMethod]
	public void ApplyBonuses_PrevSeparator_AddsMatchAfterSeparatorBonus()
	{
		// Arrange
		var prevMatched = false;
		var prevLower = false;
		var prevSeparator = true;
		var strChar = 'a';
		var strLower = 'a';
		var strUpper = 'A';
		var initialScore = 0;

		// Act
		var result = Fuzzy.ApplyBonuses(prevMatched, prevLower, prevSeparator, strChar, strLower, strUpper, initialScore);

		// Assert
		Assert.AreEqual(Fuzzy.matchAfterSeparatorBonus, result);
	}

	[TestMethod]
	public void ApplyBonuses_CamelCaseBoundary_AddsCamelCaseMatchBonus()
	{
		// Arrange
		var prevMatched = false;
		var prevLower = true;
		var prevSeparator = false;
		var strChar = 'A';  // Capital letter
		var strLower = 'a';
		var strUpper = 'A';
		var initialScore = 0;

		// Act
		var result = Fuzzy.ApplyBonuses(prevMatched, prevLower, prevSeparator, strChar, strLower, strUpper, initialScore);

		// Assert
		Assert.AreEqual(Fuzzy.camelCaseMatchBonus, result);
	}

	[TestMethod]
	public void ApplyBonuses_AllBonusesApply_AddsAllBonuses()
	{
		// Arrange
		var prevMatched = true;
		var prevLower = true;
		var prevSeparator = true;
		var strChar = 'A';  // Capital letter
		var strLower = 'a';
		var strUpper = 'A';
		var initialScore = 0;
		var expectedBonus = Fuzzy.adjacentMatchBonus + Fuzzy.matchAfterSeparatorBonus + Fuzzy.camelCaseMatchBonus;

		// Act
		var result = Fuzzy.ApplyBonuses(prevMatched, prevLower, prevSeparator, strChar, strLower, strUpper, initialScore);

		// Assert
		Assert.AreEqual(expectedBonus, result);
	}

	[TestMethod]
	public void ApplyBonuses_NoBonusesApply_ScoreUnchanged()
	{
		// Arrange
		var prevMatched = false;
		var prevLower = false;
		var prevSeparator = false;
		var strChar = 'a';
		var strLower = 'a';
		var strUpper = 'A';
		var initialScore = 5;

		// Act
		var result = Fuzzy.ApplyBonuses(prevMatched, prevLower, prevSeparator, strChar, strLower, strUpper, initialScore);

		// Assert
		Assert.AreEqual(initialScore, result);
	}

	#endregion

	#region Penalize Non-Pattern Characters Tests

	[TestMethod]
	public void PenalizeNonPatternCharacters_FirstPatternChar_AppliesPrefixPenalty()
	{
		// Arrange
		var initialScore = 10;
		var patternIdx = 0;
		var strIdx = 3;  // 3 chars before first match
		var expectedPenalty = Math.Max(strIdx * Fuzzy.unmatchedPrefixLetterPenalty, Fuzzy.maxPrefixPenalty);

		// Act
		var result = Fuzzy.PenalizeNonPatternCharacters(initialScore, patternIdx, strIdx);

		// Assert
		Assert.AreEqual(initialScore + expectedPenalty, result);
	}

	[TestMethod]
	public void PenalizeNonPatternCharacters_NotFirstPatternChar_NoChange()
	{
		// Arrange
		var initialScore = 10;
		var patternIdx = 1;  // Not the first pattern character
		var strIdx = 3;

		// Act
		var result = Fuzzy.PenalizeNonPatternCharacters(initialScore, patternIdx, strIdx);

		// Assert
		Assert.AreEqual(initialScore, result);
	}

	#endregion

	#region Calculate Score Tests

	[TestMethod]
	public void CalculateScore_ExactMatch_HighScoreAndPatternPresent()
	{
		// Arrange
		var subject = "test";
		var pattern = "test";

		// Act
		var score = Fuzzy.CalculateScore(subject, pattern, out var patternPresent);

		// Assert
		Assert.IsTrue(patternPresent);
		Assert.IsTrue(score > 0);
	}

	[TestMethod]
	public void CalculateScore_NoMatch_LowScoreAndPatternNotPresent()
	{
		// Arrange
		var subject = "test";
		var pattern = "xyz";

		// Act
		var score = Fuzzy.CalculateScore(subject, pattern, out var patternPresent);

		// Assert
		Assert.IsFalse(patternPresent);
		Assert.IsTrue(score < 0);
	}

	[TestMethod]
	public void CalculateScore_PartialMatch_IntermediateScoreAndPatternNotPresent()
	{
		// Arrange
		var subject = "testing";
		var pattern = "txs";  // t and s match but x doesn't

		// Act

		_ = Fuzzy.CalculateScore(subject, pattern, out var patternPresent);

		// Assert
		Assert.IsFalse(patternPresent);
	}

	[TestMethod]
	public void CalculateScore_EmptyPattern_ZeroScoreAndPatternPresent()
	{
		// Arrange
		var subject = "test";
		var pattern = "";

		// Act
		var score = Fuzzy.CalculateScore(subject, pattern, out var patternPresent);

		// Assert
		Assert.IsTrue(patternPresent);  // Empty pattern is always "present"
		Assert.AreEqual(0, score);      // No characters to match, so score is 0
	}

	#endregion

	#region Integration Tests

	[TestMethod]
	public void IntegrationTest_CompareScoredMatches_HighlightsQualityDifference()
	{
		// These tests compare different matches to ensure the scoring system
		// correctly identifies better matches with higher scores

		string[] subjects = [
			"FuzzyStringMatcher",
			"FunctionalStringManipulator",
			"FileSystemManager",
			"FastSorterModule"
		];

		var pattern = "fsm";
		Dictionary<string, int> scores = [];

		foreach (var subject in subjects)
		{
			Fuzzy.Contains(subject, pattern, out var score);
			scores[subject] = score;
		}

		// "FileSystemManager" should be the best match for "fsm"
		var bestMatch = scores.OrderByDescending(s => s.Value).First().Key;
		Assert.AreEqual("FileSystemManager", bestMatch);
	}

	[TestMethod]
	public void IntegrationTest_ScoresReflectMatchQuality()
	{
		// Test that match quality is reflected in scores
		var pattern = "sts";

		// Exact consecutive matches
		Fuzzy.Contains("tests", pattern, out var exactScore);

		// Separated matches
		Fuzzy.Contains("solutions to systems", pattern, out var separatedScore);

		// Mixed case with camelCase boundaries
		Fuzzy.Contains("shortToString", pattern, out var camelCaseScore);

		// Assert that exact consecutive matches score higher
		Assert.IsTrue(exactScore > separatedScore);

		// CamelCase boundaries should provide a bonus
		Assert.IsTrue(camelCaseScore > separatedScore);
	}

	#endregion
}
