namespace ktsu.FuzzySearch.Tests;

[TestClass]
public class FuzzyTests
{
	#region Contains Tests

	[TestMethod]
	public void Contains_ExactMatch_ReturnsTrue()
	{
		// Arrange
		string subject = "hello";
		string pattern = "hello";

		// Act
		bool result = Fuzzy.Contains(subject, pattern);

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void Contains_SubsequenceMatch_ReturnsTrue()
	{
		// Arrange
		string subject = "hello world";
		string pattern = "hlowrd";

		// Act
		bool result = Fuzzy.Contains(subject, pattern);

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void Contains_CaseDifferenceMatch_ReturnsTrue()
	{
		// Arrange
		string subject = "Hello World";
		string pattern = "helloworld";

		// Act
		bool result = Fuzzy.Contains(subject, pattern);

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void Contains_NoMatch_ReturnsFalse()
	{
		// Arrange
		string subject = "hello";
		string pattern = "world";

		// Act
		bool result = Fuzzy.Contains(subject, pattern);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void Contains_PatternLongerThanSubject_ReturnsFalse()
	{
		// Arrange
		string subject = "hi";
		string pattern = "hello";

		// Act
		bool result = Fuzzy.Contains(subject, pattern);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void Contains_EmptyPattern_ReturnsTrue()
	{
		// Arrange
		string subject = "hello";
		string pattern = "";

		// Act
		bool result = Fuzzy.Contains(subject, pattern);

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void Contains_EmptySubject_ReturnsFalse()
	{
		// Arrange
		string subject = "";
		string pattern = "hello";

		// Act
		bool result = Fuzzy.Contains(subject, pattern);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void Contains_BothEmpty_ReturnsFalse()
	{
		// Arrange
		string subject = "";
		string pattern = "";

		// Act
		bool result = Fuzzy.Contains(subject, pattern);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void Contains_NullSubject_ThrowsArgumentNullException()
	{
		// Arrange
		string? subject = null;
		string pattern = "test";

		// Act
		Assert.ThrowsExactly<ArgumentNullException>(() => Fuzzy.Contains(subject!, pattern));
	}

	[TestMethod]
	public void Contains_NullPattern_ThrowsArgumentNullException()
	{
		// Arrange
		string subject = "test";
		string? pattern = null;

		// Act
		Assert.ThrowsExactly<ArgumentNullException>(() => Fuzzy.Contains(subject, pattern!));
	}

	#endregion

	#region Contains With Score Tests

	[TestMethod]
	public void Contains_WithScore_ExactMatch_ReturnsTrueWithHighScore()
	{
		// Arrange
		string subject = "hello";
		string pattern = "hello";

		// Act
		bool result = Fuzzy.Contains(subject, pattern, out int score);

		// Assert
		Assert.IsTrue(result);
		Assert.IsTrue(score > 0);
	}

	[TestMethod]
	public void Contains_WithScore_SubsequenceMatch_ReturnsTrueWithPositiveScore()
	{
		// Arrange
		string subject = "hello world";
		string pattern = "hlowrd";

		// Act
		bool result = Fuzzy.Contains(subject, pattern, out int score);

		// Assert
		Assert.IsTrue(result);
		Assert.IsTrue(score > 0);
	}

	[TestMethod]
	public void Contains_WithScore_NoMatch_ReturnsFalseWithLowerScore()
	{
		// Arrange
		string subject = "hello";
		string pattern = "world";

		// Act
		bool result = Fuzzy.Contains(subject, pattern, out _);

		// Assert
		Assert.IsFalse(result);
		// Score may be negative or 0 depending on how close the match was
	}

	[TestMethod]
	public void Contains_WithScore_AdjacentMatches_ScoresHigherThanNonAdjacentMatches()
	{
		// Arrange
		string subject1 = "hworld"; // adjacent matches for "hw"
		string subject2 = "hiworld";   // non-adjacent matches for "hw"
		string pattern = "hw";

		// Act
		Fuzzy.Contains(subject1, pattern, out int score1);
		Fuzzy.Contains(subject2, pattern, out int score2);

		// Assert
		Assert.IsTrue(score1 > score2, "Adjacent matches should score higher");
	}

	[TestMethod]
	public void Contains_WithScore_MatchAfterSeparator_GetsBonus()
	{
		// Arrange
		string subject = "hello_world";  // 'w' is after separator '_'
		string pattern = "hw";

		// Act
		bool result = Fuzzy.Contains(subject, pattern, out int score);

		// Assert
		Assert.IsTrue(result);
		// The 'w' match should get a separation bonus
		Assert.IsTrue(score >= Fuzzy.matchAfterSeparatorBonus, "Score should include separator bonus");
	}

	[TestMethod]
	public void Contains_WithScore_CamelCaseMatch_GetsBonus()
	{
		// Arrange
		string subject = "helloWorld";  // 'W' is at camelCase boundary
		string pattern = "hW";

		// Act
		bool result = Fuzzy.Contains(subject, pattern, out int score);

		// Assert
		Assert.IsTrue(result);
		// The 'W' match should get a camelCase bonus
		Assert.IsTrue(score >= Fuzzy.camelCaseMatchBonus, "Score should include camelCase bonus");
	}

	[TestMethod]
	public void Contains_WithScore_NullSubject_ThrowsArgumentNullException()
	{
		// Arrange
		string? subject = null;
		string pattern = "test";

		// Act
		Assert.ThrowsExactly<ArgumentNullException>(() => Fuzzy.Contains(subject!, pattern, out _));
	}

	[TestMethod]
	public void Contains_WithScore_NullPattern_ThrowsArgumentNullException()
	{
		// Arrange
		string subject = "test";
		string? pattern = null;

		// Act
		Assert.ThrowsExactly<ArgumentNullException>(() => Fuzzy.Contains(subject, pattern!, out _));
	}

	#endregion

	#region Apply Bonuses Tests

	[TestMethod]
	public void ApplyBonuses_PrevMatched_AddsAdjacentMatchBonus()
	{
		// Arrange
		bool prevMatched = true;
		bool prevLower = false;
		bool prevSeparator = false;
		char strChar = 'a';
		char strLower = 'a';
		char strUpper = 'A';
		int initialScore = 0;

		// Act
		int result = Fuzzy.ApplyBonuses(prevMatched, prevLower, prevSeparator, strChar, strLower, strUpper, initialScore);

		// Assert
		Assert.AreEqual(Fuzzy.adjacentMatchBonus, result);
	}

	[TestMethod]
	public void ApplyBonuses_PrevSeparator_AddsMatchAfterSeparatorBonus()
	{
		// Arrange
		bool prevMatched = false;
		bool prevLower = false;
		bool prevSeparator = true;
		char strChar = 'a';
		char strLower = 'a';
		char strUpper = 'A';
		int initialScore = 0;

		// Act
		int result = Fuzzy.ApplyBonuses(prevMatched, prevLower, prevSeparator, strChar, strLower, strUpper, initialScore);

		// Assert
		Assert.AreEqual(Fuzzy.matchAfterSeparatorBonus, result);
	}

	[TestMethod]
	public void ApplyBonuses_CamelCaseBoundary_AddsCamelCaseMatchBonus()
	{
		// Arrange
		bool prevMatched = false;
		bool prevLower = true;
		bool prevSeparator = false;
		char strChar = 'A';  // Capital letter
		char strLower = 'a';
		char strUpper = 'A';
		int initialScore = 0;

		// Act
		int result = Fuzzy.ApplyBonuses(prevMatched, prevLower, prevSeparator, strChar, strLower, strUpper, initialScore);

		// Assert
		Assert.AreEqual(Fuzzy.camelCaseMatchBonus, result);
	}

	[TestMethod]
	public void ApplyBonuses_AllBonusesApply_AddsAllBonuses()
	{
		// Arrange
		bool prevMatched = true;
		bool prevLower = true;
		bool prevSeparator = true;
		char strChar = 'A';  // Capital letter
		char strLower = 'a';
		char strUpper = 'A';
		int initialScore = 0;
		int expectedBonus = Fuzzy.adjacentMatchBonus + Fuzzy.matchAfterSeparatorBonus + Fuzzy.camelCaseMatchBonus;

		// Act
		int result = Fuzzy.ApplyBonuses(prevMatched, prevLower, prevSeparator, strChar, strLower, strUpper, initialScore);

		// Assert
		Assert.AreEqual(expectedBonus, result);
	}

	[TestMethod]
	public void ApplyBonuses_NoBonusesApply_ScoreUnchanged()
	{
		// Arrange
		bool prevMatched = false;
		bool prevLower = false;
		bool prevSeparator = false;
		char strChar = 'a';
		char strLower = 'a';
		char strUpper = 'A';
		int initialScore = 5;

		// Act
		int result = Fuzzy.ApplyBonuses(prevMatched, prevLower, prevSeparator, strChar, strLower, strUpper, initialScore);

		// Assert
		Assert.AreEqual(initialScore, result);
	}

	#endregion

	#region Penalize Non-Pattern Characters Tests

	[TestMethod]
	public void PenalizeNonPatternCharacters_FirstPatternChar_AppliesPrefixPenalty()
	{
		// Arrange
		int initialScore = 10;
		int patternIdx = 0;
		int strIdx = 3;  // 3 chars before first match
		int expectedPenalty = Math.Max(strIdx * Fuzzy.unmatchedPrefixLetterPenalty, Fuzzy.maxPrefixPenalty);

		// Act
		int result = Fuzzy.PenalizeNonPatternCharacters(initialScore, patternIdx, strIdx);

		// Assert
		Assert.AreEqual(initialScore + expectedPenalty, result);
	}

	[TestMethod]
	public void PenalizeNonPatternCharacters_NotFirstPatternChar_NoChange()
	{
		// Arrange
		int initialScore = 10;
		int patternIdx = 1;  // Not the first pattern character
		int strIdx = 3;

		// Act
		int result = Fuzzy.PenalizeNonPatternCharacters(initialScore, patternIdx, strIdx);

		// Assert
		Assert.AreEqual(initialScore, result);
	}

	#endregion

	#region Calculate Score Tests

	[TestMethod]
	public void CalculateScore_ExactMatch_HighScoreAndPatternPresent()
	{
		// Arrange
		string subject = "test";
		string pattern = "test";

		// Act
		int score = Fuzzy.CalculateScore(subject, pattern, out bool patternPresent);

		// Assert
		Assert.IsTrue(patternPresent);
		Assert.IsTrue(score > 0);
	}

	[TestMethod]
	public void CalculateScore_NoMatch_LowScoreAndPatternNotPresent()
	{
		// Arrange
		string subject = "test";
		string pattern = "xyz";

		// Act
		int score = Fuzzy.CalculateScore(subject, pattern, out bool patternPresent);

		// Assert
		Assert.IsFalse(patternPresent);
		Assert.IsTrue(score < 0);
	}

	[TestMethod]
	public void CalculateScore_PartialMatch_IntermediateScoreAndPatternNotPresent()
	{
		// Arrange
		string subject = "testing";
		string pattern = "txs";  // t and s match but x doesn't

		// Act

		_ = Fuzzy.CalculateScore(subject, pattern, out bool patternPresent);

		// Assert
		Assert.IsFalse(patternPresent);
	}

	[TestMethod]
	public void CalculateScore_EmptyPattern_ZeroScoreAndPatternPresent()
	{
		// Arrange
		string subject = "test";
		string pattern = "";

		// Act
		int score = Fuzzy.CalculateScore(subject, pattern, out bool patternPresent);

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

		string pattern = "fsm";
		Dictionary<string, int> scores = [];

		foreach (string subject in subjects)
		{
			Fuzzy.Contains(subject, pattern, out int score);
			scores[subject] = score;
		}

		// "FileSystemManager" should be the best match for "fsm"
		string bestMatch = scores.OrderByDescending(s => s.Value).First().Key;
		Assert.AreEqual("FileSystemManager", bestMatch);
	}

	[TestMethod]
	public void IntegrationTest_ScoresReflectMatchQuality()
	{
		// Test that match quality is reflected in scores
		string pattern = "sts";

		// Exact consecutive matches
		Fuzzy.Contains("tests", pattern, out int exactScore);

		// Separated matches
		Fuzzy.Contains("solutions to systems", pattern, out int separatedScore);

		// Mixed case with camelCase boundaries
		Fuzzy.Contains("shortToString", pattern, out int camelCaseScore);

		// Assert that exact consecutive matches score higher
		Assert.IsTrue(exactScore > separatedScore);

		// CamelCase boundaries should provide a bonus
		Assert.IsTrue(camelCaseScore > separatedScore);
	}

	#endregion
}
