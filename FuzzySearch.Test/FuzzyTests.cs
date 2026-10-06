// Copyright (c) 2023-2026 ktsu-dev contributors

[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]

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
		Assert.IsTrue(result, "Exact match should return true.");
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
		Assert.IsTrue(result, "Subsequence match should return true.");
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
		Assert.IsTrue(result, "Case-insensitive match should return true.");
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
		Assert.IsFalse(result, "Non-matching pattern should return false.");
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
		Assert.IsFalse(result, "Pattern longer than subject should return false.");
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
		Assert.IsTrue(result, "Empty pattern should match any non-empty subject.");
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
		Assert.IsFalse(result, "Empty subject with non-empty pattern should return false.");
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
		Assert.IsFalse(result, "Both empty strings should return false.");
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
		Assert.IsTrue(result, "Exact match should return true.");
		Assert.IsGreaterThan(0, score, "Exact match should have a positive score.");
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
		Assert.IsTrue(result, "Subsequence match should return true.");
		Assert.IsGreaterThan(0, score, "Subsequence match should have a positive score.");
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
		Assert.IsFalse(result, "Non-matching pattern should return false.");
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
		Assert.IsGreaterThan(score2, score1, "Adjacent matches should score higher");
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
		Assert.IsTrue(result, "Match after separator should return true.");
		// The 'w' match should get a separation bonus
		Assert.IsGreaterThanOrEqualTo(Fuzzy.matchAfterSeparatorBonus, score, "Score should include separator bonus");
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
		Assert.IsTrue(result, "CamelCase match should return true.");
		// The 'W' match should get a camelCase bonus
		Assert.IsGreaterThanOrEqualTo(Fuzzy.camelCaseMatchBonus, score, "Score should include camelCase bonus");
	}

	[TestMethod]
	public void Contains_WithScore_LatePrefixMatch_ScoresLowerThanStartMatch()
	{
		// Arrange
		string pattern = "foo";

		// Act
		bool earlyResult = Fuzzy.Contains("foo", pattern, out int earlyScore);
		bool lateResult = Fuzzy.Contains("xxxxxxxxxxfoo", pattern, out int lateScore);

		// Assert
		Assert.IsTrue(earlyResult, "Start-position match should return true.");
		Assert.IsTrue(lateResult, "Late-position match should return true.");
		Assert.IsGreaterThan(lateScore, earlyScore, "Match at the start should score higher than a late prefix match.");
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
		int expectedPenalty = -3;

		// Act
		int result = Fuzzy.PenalizeNonPatternCharacters(initialScore, patternIdx, strIdx);

		// Assert
		Assert.AreEqual(initialScore + expectedPenalty, result);
	}

	[TestMethod]
	public void PenalizeNonPatternCharacters_FirstPatternChar_CapsPrefixPenalty()
	{
		// Arrange
		int initialScore = 10;
		int patternIdx = 0;
		int strIdx = 10;
		int expectedPenalty = -5;

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
		Assert.IsTrue(patternPresent, "Pattern should be present for exact match.");
		Assert.IsGreaterThan(0, score, "Exact match should have a positive score.");
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
		Assert.IsFalse(patternPresent, "Pattern should not be present when there is no match.");
		Assert.IsLessThan(0, score, "Non-matching pattern should have a negative score.");
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
		Assert.IsFalse(patternPresent, "Pattern should not be fully present for partial match.");
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
		Assert.IsTrue(patternPresent, "Empty pattern is always considered present.");
		Assert.AreEqual(0, score);      // No characters to match, so score is 0
	}

	[TestMethod]
	public void CalculateScore_LongUnmatchedPrefix_PenaltyFlattensAtTheCap()
	{
		// Arrange: the same single-character match, preceded by ever more junk
		string pattern = "y";

		// Act
		int atCap = Fuzzy.CalculateScore("xxxxxy", pattern, out bool atCapPresent);
		int pastCap = Fuzzy.CalculateScore("xxxxxxxxxxxxy", pattern, out bool pastCapPresent);
		int wellPastCap = Fuzzy.CalculateScore(new string('x', 100) + "y", pattern, out bool wellPastCapPresent);

		// Assert: all still match, and the prefix cost stops growing once the cap is reached
		Assert.IsTrue(atCapPresent, "The pattern is present regardless of the prefix length.");
		Assert.IsTrue(pastCapPresent, "The pattern is present regardless of the prefix length.");
		Assert.IsTrue(wellPastCapPresent, "The pattern is present regardless of the prefix length.");

		Assert.AreEqual(atCap, pastCap, "A prefix beyond the cap threshold must not cost any more than one at it.");
		Assert.AreEqual(atCap, wellPastCap, "The prefix penalty must stay flat however long the prefix grows.");
	}

	[TestMethod]
	public void CalculateScore_PrefixPenalty_NeverExceedsTheDocumentedCap()
	{
		// Arrange: a single-character pattern matched at the very end earns no bonuses, so the prefix
		// penalty is the only thing moving the score and the cap is directly observable as a floor
		string pattern = "y";

		// Act & Assert
		for (int prefixLength = 1; prefixLength <= 20; prefixLength++)
		{
			int score = Fuzzy.CalculateScore(new string('x', prefixLength) + "y", pattern, out _);

			Assert.IsGreaterThanOrEqualTo(Fuzzy.maxPrefixPenalty, score,
				$"A prefix of {prefixLength} characters scored {score}, beyond the documented cap of {Fuzzy.maxPrefixPenalty}.");
		}
	}

	[TestMethod]
	public void CalculateScore_LongerPrefix_NeverScoresHigherThanAShorterOne()
	{
		// Arrange
		string pattern = "y";
		int previous = Fuzzy.CalculateScore("y", pattern, out _);

		// Act & Assert: the penalty is monotonic — a longer prefix is never rewarded, only flattened
		for (int prefixLength = 1; prefixLength <= 20; prefixLength++)
		{
			int score = Fuzzy.CalculateScore(new string('x', prefixLength) + "y", pattern, out _);
			Assert.IsLessThanOrEqualTo(previous, score,
				$"A prefix of {prefixLength} characters scored higher than the shorter prefix before it.");
			previous = score;
		}
	}

	[TestMethod]
	public void CalculateScore_NonMatchingSubject_StillReflectsHowMuchWasSkipped()
	{
		// Arrange: the pattern appears in neither subject, so the prefix refund must not apply
		string pattern = "y";

		// Act
		int shortSubject = Fuzzy.CalculateScore("xxx", pattern, out bool shortPresent);
		int longSubject = Fuzzy.CalculateScore("xxxxxxxxxxxx", pattern, out bool longPresent);

		// Assert
		Assert.IsFalse(shortPresent, "The pattern is not present in a subject without it.");
		Assert.IsFalse(longPresent, "The pattern is not present in a subject without it.");
		Assert.IsLessThan(shortSubject, longSubject, "A longer non-matching subject should still score lower.");
	}

	#endregion

	#region Unicode Normalization Tests

	// "café" precomposed (NFC): the accented letter is a single code point U+00E9.
	private const string PrecomposedCafe = "café";

	// "café" decomposed (NFD): a plain 'e' followed by U+0301 COMBINING ACUTE ACCENT.
	private const string DecomposedCafe = "café";

	[TestMethod]
	public void Contains_DecomposedSubject_MatchesPrecomposedPattern()
	{
		// Act
		bool result = Fuzzy.Contains(DecomposedCafe, PrecomposedCafe);

		// Assert
		Assert.IsTrue(result, "A decomposed (NFD) subject should match its canonically equivalent precomposed (NFC) pattern.");
	}

	[TestMethod]
	public void Contains_PrecomposedSubject_MatchesDecomposedPattern()
	{
		// Act
		bool result = Fuzzy.Contains(PrecomposedCafe, DecomposedCafe);

		// Assert
		Assert.IsTrue(result, "A precomposed (NFC) subject should match its canonically equivalent decomposed (NFD) pattern.");
	}

	[TestMethod]
	public void Contains_WithScore_CanonicallyEquivalentForms_ScoreIdentically()
	{
		// Act
		Fuzzy.Contains(PrecomposedCafe, PrecomposedCafe, out int precomposedScore);
		Fuzzy.Contains(DecomposedCafe, PrecomposedCafe, out int decomposedScore);

		// Assert
		Assert.AreEqual(precomposedScore, decomposedScore, "Canonically equivalent text should produce the same score.");
	}

	[TestMethod]
	public void Contains_DecomposedSubject_StillRejectsNonMatchingPattern()
	{
		// Act
		bool result = Fuzzy.Contains(DecomposedCafe, "zzz");

		// Assert
		Assert.IsFalse(result, "Normalization should not turn a non-match into a match.");
	}

	[TestMethod]
	public void Contains_MalformedUnicode_ComparesAsGivenWithoutThrowing()
	{
		// Arrange: a lone high surrogate is not well-formed Unicode and cannot be normalized.
		string subject = "caf\uD83D";

		// Act
		bool result = Fuzzy.Contains(subject, "caf");

		// Assert
		Assert.IsTrue(result, "Text that cannot be normalized should still be matched as it was given.");
	}

	[TestMethod]
	public void Contains_DecomposedSubjectWithLoneSurrogate_StillMatchesPrecomposedPattern()
	{
		// Act
		bool result = Fuzzy.Contains(DecomposedCafe + " \uD800", PrecomposedCafe);

		// Assert
		Assert.IsTrue(result, "An unrelated lone surrogate must not switch off normalization for the rest of the subject.");
	}

	[TestMethod]
	public void Contains_WithScore_DecomposedSubjectWithLoneSurrogate_StillMatchesPrecomposedPattern()
	{
		// Act
		bool result = Fuzzy.Contains(DecomposedCafe + " \uD800", PrecomposedCafe, out _);

		// Assert
		Assert.IsTrue(result, "The scoring overload must agree that the normalized text is present.");
	}

	[TestMethod]
	public void Contains_PatternWithLoneSurrogate_IsStillNormalized()
	{
		// Act
		bool result = Fuzzy.Contains(PrecomposedCafe + " \uDC00", DecomposedCafe + " \uDC00");

		// Assert
		Assert.IsTrue(result, "A lone surrogate in the pattern must not switch off normalization for the rest of it.");
	}

	[TestMethod]
	public void NormalizeForComparison_LoneSurrogate_KeepsItInPlaceAndNormalizesAroundIt()
	{
		// Act
		string result = Fuzzy.NormalizeForComparison(DecomposedCafe + "\uD800" + DecomposedCafe + "\uD83D\uDE01").ToString();

		// Assert
		Assert.AreEqual(PrecomposedCafe + "\uD800" + PrecomposedCafe + "\uD83D\uDE01", result);
	}

	#endregion

	#region Surrogate Pair Tests

	// U+1F601 GRINNING FACE WITH SMILING EYES, a supplementary-plane character stored as the
	// surrogate pair U+D83D U+DE01.
	private const string Emoji = "😁";
	private const string LoneHighSurrogate = "\uD83D";
	private const string LoneLowSurrogate = "\uDE01";

	[TestMethod]
	public void Contains_LoneHighSurrogatePattern_DoesNotMatchHalfOfASurrogatePair()
	{
		// Act
		bool result = Fuzzy.Contains($"x{Emoji}y", LoneHighSurrogate);

		// Assert
		Assert.IsFalse(result, "An unpaired high surrogate must not match the leading half of an unrelated surrogate pair.");
	}

	[TestMethod]
	public void Contains_LoneLowSurrogatePattern_DoesNotMatchHalfOfASurrogatePair()
	{
		// Act
		bool result = Fuzzy.Contains($"x{Emoji}y", LoneLowSurrogate);

		// Assert
		Assert.IsFalse(result, "An unpaired low surrogate must not match the trailing half of an unrelated surrogate pair.");
	}

	[TestMethod]
	public void Contains_WithScore_LoneHighSurrogatePattern_IsNotReportedAsPresent()
	{
		// Act
		bool result = Fuzzy.Contains($"x{Emoji}y", LoneHighSurrogate, out _);

		// Assert
		Assert.IsFalse(result, "The scoring overload must agree that an unpaired surrogate is not present.");
	}

	[TestMethod]
	public void Contains_SurrogatePairPattern_DoesNotMatchALoneSurrogateInTheSubject()
	{
		// Act
		bool result = Fuzzy.Contains($"x{LoneHighSurrogate}y", Emoji);

		// Assert
		Assert.IsFalse(result, "A whole surrogate pair must not match an unpaired surrogate in the subject.");
	}

	[TestMethod]
	public void Contains_SurrogatePairPattern_MatchesTheSameSurrogatePair()
	{
		// Act
		bool result = Fuzzy.Contains($"x{Emoji}y", Emoji);

		// Assert
		Assert.IsTrue(result, "A surrogate pair should still match itself.");
	}

	[TestMethod]
	public void Contains_LoneSurrogatePattern_MatchesTheSameLoneSurrogate()
	{
		// Act
		bool result = Fuzzy.Contains($"x{LoneHighSurrogate}y", LoneHighSurrogate);

		// Assert
		Assert.IsTrue(result, "An unpaired surrogate should still match the same unpaired surrogate.");
	}

	[TestMethod]
	public void Contains_PatternSpanningASurrogatePair_MatchesTheSurroundingCharacters()
	{
		// Act
		bool result = Fuzzy.Contains($"x{Emoji}y", $"x{Emoji}y");

		// Assert
		Assert.IsTrue(result, "A supplementary-plane character adjacent to other matchable characters should match in sequence.");
	}

	[TestMethod]
	public void Contains_SurrogatePairPattern_DoesNotMatchADifferentSurrogatePair()
	{
		// Arrange: U+1F600 GRINNING FACE shares its high surrogate with U+1F601 but differs in the low surrogate.
		string otherEmoji = "😀";

		// Act
		bool result = Fuzzy.Contains($"x{otherEmoji}y", Emoji);

		// Assert
		Assert.IsFalse(result, "Two supplementary-plane characters sharing a high surrogate are still different characters.");
	}

	[TestMethod]
	public void Contains_WithScore_SupplementaryPlanePrefix_PenalizedPerCharacterNotPerCodeUnit()
	{
		// Arrange: both subjects have exactly three unmatched characters before the match, but the emoji prefix
		// occupies six UTF-16 code units rather than three.
		string emojiSubject = $"{Emoji}{Emoji}{Emoji}y";
		string asciiSubject = "abcy";

		// Act
		Fuzzy.Contains(emojiSubject, "y", out int emojiScore);
		Fuzzy.Contains(asciiSubject, "y", out int asciiScore);

		// Assert
		Assert.AreEqual(asciiScore, emojiScore, "The prefix penalty should count skipped characters, not their UTF-16 width.");
	}

	[TestMethod]
	public void Contains_WithScore_SupplementaryPlanePrefix_DoesNotExceedTheUncappedPenalty()
	{
		// Arrange: two skipped codepoints occupy four UTF-16 code units, which is still under the penalty cap,
		// so a code-unit count would show through as a larger penalty rather than being hidden by the cap.
		string emojiSubject = $"{Emoji}{Emoji}y";

		// Act
		Fuzzy.Contains(emojiSubject, "y", out int emojiScore);
		Fuzzy.Contains("aby", "y", out int asciiScore);

		// Assert
		Assert.AreEqual(asciiScore, emojiScore, "Two skipped supplementary-plane characters should cost the same as two skipped ASCII characters.");
	}

	// Supplementary-plane letters with a case mapping, each stored as a surrogate pair.
	private const string DeseretCapitalLongI = "\U00010400";
	private const string DeseretSmallLongI = "\U00010428";
	private const string DeseretCapitalLongE = "\U00010401";
	private const string AdlamCapitalAlif = "\U0001E900";
	private const string AdlamSmallAlif = "\U0001E922";
	private const string OsageCapitalAOsage = "\U000104B0\U000104B1";
	private const string OsageSmallAOsage = "\U000104D8\U000104D9";

	[TestMethod]
	[DataRow(DeseretCapitalLongI, DeseretSmallLongI, DisplayName = "Deseret, uppercase subject")]
	[DataRow(DeseretSmallLongI, DeseretCapitalLongI, DisplayName = "Deseret, lowercase subject")]
	[DataRow(AdlamCapitalAlif, AdlamSmallAlif, DisplayName = "Adlam, uppercase subject")]
	[DataRow(AdlamSmallAlif, AdlamCapitalAlif, DisplayName = "Adlam, lowercase subject")]
	[DataRow(OsageCapitalAOsage, OsageSmallAOsage, DisplayName = "Osage, uppercase subject")]
	public void Contains_SupplementaryPlaneLetterInOtherCase_ReturnsTrue(string subject, string pattern)
	{
		// Act
		bool result = Fuzzy.Contains(subject, pattern);

		// Assert
		Assert.IsTrue(result, "Case is always ignored, including for letters outside the Basic Multilingual Plane.");
	}

	[TestMethod]
	[DataRow(DeseretCapitalLongI, DeseretSmallLongI, DisplayName = "Deseret")]
	[DataRow(AdlamCapitalAlif, AdlamSmallAlif, DisplayName = "Adlam")]
	[DataRow(OsageCapitalAOsage, OsageSmallAOsage, DisplayName = "Osage")]
	public void Contains_WithScore_SupplementaryPlaneLetterInOtherCase_ScoresLikeTheExactCaseMatch(string subject, string pattern)
	{
		// Act
		bool exactCase = Fuzzy.Contains($"x{subject}y", subject, out int exactCaseScore);
		bool otherCase = Fuzzy.Contains($"x{subject}y", pattern, out int otherCaseScore);

		// Assert
		Assert.IsTrue(exactCase, "The exact-case pattern should match.");
		Assert.IsTrue(otherCase, "The other-case pattern should match.");
		Assert.AreEqual(exactCaseScore, otherCaseScore, "Ignoring case must not change the score.");
	}

	[TestMethod]
	public void Contains_SupplementaryPlaneLetter_DoesNotMatchADifferentLetterInOtherCase()
	{
		// Act
		bool result = Fuzzy.Contains(DeseretCapitalLongE, DeseretSmallLongI);

		// Assert
		Assert.IsFalse(result, "Ignoring case must not make two different supplementary-plane letters equal.");
	}

	#endregion

	#region Case Folding Tests

	[TestMethod]
	[DataRow("ΟΔΟΣ", "οδος")]
	[DataRow("bcaAσ", "ς")]
	[DataRow("bcaAσ", "Σ")]
	[DataRow("Μ", "µ")]
	[DataRow("οδος", "ΟΔΟΣ")]
	[DataRow("µ", "μ")]
	public void Contains_LettersWithMoreThanOneLowercaseForm_MatchIgnoringCase(string subject, string pattern)
	{
		// Act
		bool result = Fuzzy.Contains(subject, pattern);
		bool scoredResult = Fuzzy.Contains(subject, pattern, out _);

		// Assert
		Assert.IsTrue(result, $"\"{pattern}\" should match \"{subject}\" ignoring case.");
		Assert.IsTrue(scoredResult, $"The scoring overload should agree that \"{pattern}\" matches \"{subject}\".");
	}

	[TestMethod]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Lower-casing the pattern is one of the two case changes under test.")]
	public void Contains_ChangingThePatternsCase_NeverChangesTheResultOrScore()
	{
		// Arrange: an alphabet holding both sigma forms, the micro sign and the Greek mu alongside plain letters.
		const string alphabet = "abAσςΣµΜμ ";
		Random random = new(88);

		for (int i = 0; i < 20000; i++)
		{
			string subject = RandomString(random, alphabet, random.Next(0, 8));
			string pattern = RandomString(random, alphabet, random.Next(1, 4));

			// Act
			bool result = Fuzzy.Contains(subject, pattern, out int score);
			bool upperResult = Fuzzy.Contains(subject, pattern.ToUpperInvariant(), out int upperScore);
			bool lowerResult = Fuzzy.Contains(subject, pattern.ToLowerInvariant(), out int lowerScore);

			// Assert
			Assert.AreEqual(result, upperResult, $"Upper-casing pattern \"{pattern}\" changed the match against \"{subject}\".");
			Assert.AreEqual(result, lowerResult, $"Lower-casing pattern \"{pattern}\" changed the match against \"{subject}\".");
			Assert.AreEqual(score, upperScore, $"Upper-casing pattern \"{pattern}\" changed the score against \"{subject}\".");
			Assert.AreEqual(score, lowerScore, $"Lower-casing pattern \"{pattern}\" changed the score against \"{subject}\".");
		}
	}

	private static string RandomString(Random random, string alphabet, int length)
	{
		char[] chars = new char[length];
		for (int i = 0; i < length; i++)
		{
			chars[i] = alphabet[random.Next(alphabet.Length)];
		}

		return new string(chars);
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
		Assert.IsGreaterThan(separatedScore, exactScore, "Exact consecutive matches should score higher than separated matches.");

		// CamelCase boundaries should provide a bonus
		Assert.IsGreaterThan(separatedScore, camelCaseScore, "CamelCase matches should score higher than separated matches.");
	}

	#endregion

	#region Repeated Letter Tests

	private static int ScoreOf(string subject, string pattern)
	{
		Fuzzy.Contains(subject, pattern, out int score);
		return score;
	}

	[TestMethod]
	public void Score_RepeatedFirstLetter_ScoresBelowExactMatch()
	{
		Assert.IsGreaterThan(ScoreOf("aab", "ab"), ScoreOf("ab", "ab"), "A repeated letter should cost something.");
	}

	[TestMethod]
	public void Score_ManyRepeatedLetters_ScoreBelowOneTrailingLetter()
	{
		Assert.IsGreaterThan(ScoreOf("aaaaaaaaaab", "ab"), ScoreOf("abx", "ab"), "Nine repeated letters should cost more than one trailing letter.");
	}

	[TestMethod]
	public void Score_RepeatedLettersAfterSeparatorMatch_AreCharged()
	{
		Assert.IsGreaterThan(ScoreOf("a_bbbbbbbbbb", "ab"), ScoreOf("a_b", "ab"), "Repeated letters after a separator match should cost something.");
	}

	[TestMethod]
	[DataRow("item", "itemMap")]
	[DataRow("list", "listTools")]
	[DataRow("test", "testTest")]
	[DataRow("ab", "ab_b")]
	public void Score_ExactMatch_OutranksSameTextWithSuffix(string exact, string withSuffix)
	{
		Assert.IsGreaterThan(ScoreOf(withSuffix, exact), ScoreOf(exact, exact), $"'{exact}' should outrank '{withSuffix}' for pattern '{exact}'.");
	}

	#endregion

	#region Separator Tests

	[TestMethod]
	public void Score_PathSegmentMatch_OutranksMidWordMatch()
	{
		Assert.IsGreaterThan(ScoreOf("domain.cs", "main"), ScoreOf("src/main.cs", "main"), "A match starting a path segment should outrank one buried in a word.");
	}

	[TestMethod]
	public void Score_HyphenatedWordMatch_OutranksJoinedWordMatch()
	{
		Assert.IsGreaterThan(ScoreOf("getlist", "list"), ScoreOf("get-list", "list"), "A match after a hyphen should outrank one in the middle of a word.");
	}

	[TestMethod]
	[DataRow("src-main.cs")]
	[DataRow("src.main.cs")]
	[DataRow("src/main.cs")]
	[DataRow("src\\main.cs")]
	[DataRow("src\tmain.cs")]
	[DataRow("src main.cs")]
	public void Score_EverySeparator_EarnsTheSameBonusAsUnderscore(string subject)
	{
		Assert.AreEqual(ScoreOf("src_main.cs", "main"), ScoreOf(subject, "main"), $"'{subject}' should score like 'src_main.cs'.");
	}

	[TestMethod]
	[DataRow('_')]
	[DataRow('-')]
	[DataRow('.')]
	[DataRow('/')]
	[DataRow('\\')]
	[DataRow(' ')]
	[DataRow('\t')]
	[DataRow('\n')]
	public void IsSeparator_SeparatorCharacters_ReturnTrue(char value)
	{
		Assert.IsTrue(Fuzzy.IsSeparator(value));
	}

	[TestMethod]
	[DataRow('a')]
	[DataRow('Z')]
	[DataRow('0')]
	[DataRow(':')]
	public void IsSeparator_OtherCharacters_ReturnFalse(char value)
	{
		Assert.IsFalse(Fuzzy.IsSeparator(value));
	}

	#endregion
}
