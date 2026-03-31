using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

// NOTE: THIS FUNCTION REQUIRES MUCH MORE:
// - UPDATES
// - CHECKS
// - REVISIONS
// *** THEREFORE: THIS SCRIPT SHOULD BE CHECKED AND UPDATED
// *** FOR CONSTANTLY FOR ANY POSSIBLE LOOPHOLES IN PARSING 
// *** PLAYER RESPONSE

public class PlayerOfferParser
{
    static readonly string LimitPattern =
    @"\b(max|maximum|at most|atleast|at least|only|just|best i can do|highest i can go|most i can do|limit)\b";

    static readonly string ProposalPattern =
    @"\b(let'?s go|how about|what about|make it|go with|i'?ll do)\b";

    static readonly string NegativePattern =
    @"\b(too much|too high|overpriced|scam|ripoff|crazy|insane)\b";

    static readonly string ContrastPattern =
    @"\b(but|instead|rather|still)\b";

    public enum OfferConfidence
    {
        None,
        Low,
        Medium,
        High
    }

    enum NumberContext
    {
        OfferCandidate,
        Rejection,
        ReferencePrice,
        Unclear
    }

    public class OfferResult
    {
        public float? OfferValue;
        public OfferConfidence Confidence;
        public string DebugReason;
    }

    // ================================================= \\\
    // Considered as "Entry Point" to parse offer result \\\
    // ================================================= \\\
    public static OfferResult Parse(string input)
    {
        var numbers = ExtractAllNumbers(input);

        var ranges = ExtractRanges(input);
        numbers.AddRange(ranges);

        if (numbers.Count == 0)
        {
            return new OfferResult
            {
                OfferValue = null,
                Confidence = OfferConfidence.None,
                DebugReason = "No numbers found"
            };
        }

        var candidates = new List<(float value, int score, NumberContext context)>();

        foreach (Match match in Regex.Matches(input, @"\b\d+(\.\d+)?\b"))
        {
            float value = float.Parse(match.Value);

            var (score, context) = ClassifyNumber(input, match);
            candidates.Add((value, score, context));
        }

        float selected = ResolveFinalOffer(input, candidates);

        var best = candidates.OrderByDescending(x => x.score).First();

        return new OfferResult
        {
            OfferValue = selected,
            Confidence = ScoreConfidence(best.score),
            DebugReason = "Offer detected"
        };
    }


    static List<float> ExtractAllNumbers(string input)
    {
        var matches = Regex.Matches(input, @"\b\d+(\.\d+)?\b");

        return matches
            .Select(m => float.Parse(m.Value))
            .ToList();
    }


    // ================================================= \\\
    // New scoring-based classification (more reliable) \\\
    // ================================================= \\\
    static (int score, NumberContext context) ClassifyNumber(string input, Match match)
    {
        string lowered = input.ToLower();

        int index = match.Index;

        int start = Math.Max(0, index - 40);
        int length = Math.Min(80, lowered.Length - start);
        string window = lowered.Substring(start, length);

        bool hasLimit = Regex.IsMatch(window, LimitPattern);
        bool hasRejection = Regex.IsMatch(window, @"\b(not|never|won't|wouldn't)\b");
        bool hasReference = Regex.IsMatch(window, @"\b(price|worth|cost|selling)\b");
        bool hasNegative = Regex.IsMatch(window, NegativePattern);
        bool hasContrast = Regex.IsMatch(window, ContrastPattern);

        int score = 0;

        // Proximity to offer verbs
        var keywordMatches = Regex.Matches(window, @"\b(buy|pay|offer|give|take|offering|get)\b");

        foreach (Match keyword in keywordMatches)
        {
            int distance = Math.Abs((start + keyword.Index) - index);

            if (distance < 10) score += 3;
            else if (distance < 20) score += 2;
            else if (distance < 30) score += 1;
        }

        // Looking for Limit phrases (strong)
        var limitMatches = Regex.Matches(window, LimitPattern);

        foreach (Match keyword in limitMatches)
        {
            int distance = Math.Abs((start + keyword.Index) - index);

            if (distance < 10) score += 4;
            else if (distance < 20) score += 3;
            else if (distance < 30) score += 2;
        }

        // Looking for proposal phrases (VERY strong signal)
        var proposalMatches = Regex.Matches(window, ProposalPattern);

        foreach (Match keyword in proposalMatches)
        {
            int distance = Math.Abs((start + keyword.Index) - index);

            if (distance < 10) score += 4;
            else if (distance < 20) score += 3;
        }

        // General signals
        if (Regex.IsMatch(window, @"\b(buy|pay|offer|give|take|offering|get)\b")) score += 2;

        if (Regex.IsMatch(window, @"\b(maybe|would|could|perhaps|guess)\b")) score += 1;

        // NEGATIVE CONTEXT (kills reference numbers like "930 is a scam")
        if (hasNegative && !hasLimit)
        {
            score -= 4;
        }

        // REJECTION (weaker than negative sentiment)
        if (hasRejection && !hasLimit) score -= 3;

        // Contrast slightly boosts later offers
        if (hasContrast)
        {
            score += 1;
        }

        // Reference price only if weak
        if (hasReference && score <= 0)
        {
            return (score, NumberContext.ReferencePrice);
        }

        if (score > 0)
            return (score, NumberContext.OfferCandidate);

        if (hasRejection)
            return (score, NumberContext.Rejection);

        return (score, NumberContext.Unclear);
    }


    static float ResolveFinalOffer(string input, List<(float value, int score, NumberContext context)> candidates)
    {
        string lowered = input.ToLower();

        // Get strong candidates first (limit / proposal / high confidence)
        var strongCandidates = candidates
            .Where(x => x.context == NumberContext.OfferCandidate && x.score >= 4)
            .ToList();

        if (strongCandidates.Count > 0)
        {
            return strongCandidates
                .OrderByDescending(x => x.score)
                .ThenByDescending(x => x.value)
                .First().value;
        }

        // Check for all non rejections and check if there's only one, if there is, it is most LIKELY an offer.
        var nonRejected = candidates
            .Where(x => x.context != NumberContext.Rejection && x.score >= 0)
            .ToList();

        if (nonRejected.Count == 1)
        {
            return nonRejected[0].value;
        }

        // Look for any change of mind in the sentence
        if (lowered.Contains("wait") || lowered.Contains("actually"))
        {
            return candidates.Last().value;
        }

        // Select the number candidate who has the most score (most likely to be the offer)
        return candidates
            .Select((x, i) => new { val = x, index = i })
            .OrderByDescending(x => x.val.score)
            .ThenByDescending(x => x.val.value)
            .ThenByDescending(x => x.index)
            .First().val.value;
    }


    // Rank numbers based on score
    static OfferConfidence ScoreConfidence(int score)
    {
        if (score >= 3) return OfferConfidence.High;
        if (score == 2) return OfferConfidence.Medium;
        if (score == 1) return OfferConfidence.Low;
        return OfferConfidence.None;
    }


    // Helper Functions
    static List<float> ExtractRanges(string input)
    {
        // 
        var ranges = new List<float>();

        // Matches: "200-300", "200 to 300", "between 200 and 300"
        var matches = Regex.Matches(input.ToLower(),
            @"(\d+)\s*(\-|to|and)\s*(\d+)");

        foreach (Match match in matches)
        {
            float a = float.Parse(match.Groups[1].Value);
            float b = float.Parse(match.Groups[3].Value);

            // Assume player leans toward the higher number
            ranges.Add(Math.Max(a, b));
        }

        return ranges;
    }
}
