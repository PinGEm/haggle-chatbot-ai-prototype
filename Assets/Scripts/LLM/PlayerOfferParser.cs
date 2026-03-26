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

        // Get valid offer candidates (score-based, not just regex match)
        var valid = candidates
            .Where(x => x.context == NumberContext.OfferCandidate && x.score > 0)
            .ToList();

        if (valid.Count == 0)
        {
            return new OfferResult
            {
                OfferValue = null,
                Confidence = OfferConfidence.None,
                DebugReason = "No valid offer candidates"
            };
        }

        float selected = ResolveFinalOffer(input, valid);

        return new OfferResult
        {
            OfferValue = selected,
            Confidence = ScoreConfidence(valid.Max(x => x.score)),
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

        // Get tighter window for proximity scoring
        int start = Math.Max(0, index - 40);
        int length = Math.Min(80, lowered.Length - start);
        string window = lowered.Substring(start, length);

        int score = 0;

        // Check if the number is near any keyphrases

        int proximityBonus = 0;

        var keywordMatches = Regex.Matches(window, @"\b(buy|pay|offer|give|take|offering)\b");

        foreach (Match keyword in keywordMatches)
        {
            int distance = Math.Abs((start + keyword.Index) - index);

            if (distance < 10) proximityBonus += 3;
            else if (distance < 20) proximityBonus += 2;
            else if (distance < 30) proximityBonus += 1;
        }

        score += proximityBonus;

        // STRONG OFFER INDICATORS \\\

        if (Regex.IsMatch(window, @"\b(buy|pay|offer|give|take|offering)\b"))
            score += 2;

        // HESITATION LANGUAGE 

        if (Regex.IsMatch(window, @"\b(maybe|would|could|perhaps|guess)\b"))
            score += 1;

        // REJECTION LANGUAGE

        if (Regex.IsMatch(window, @"\b(not|never|won't|wouldn't)\b"))
        {
            score -= 3;
            return (score, NumberContext.Rejection);
        }

        // REFERENCE PRICE (neutral)

        if (Regex.IsMatch(window, @"\b(price|worth|cost|selling)\b"))
        {
            return (score, NumberContext.ReferencePrice);
        }

        if (score > 0)
            return (score, NumberContext.OfferCandidate);

        return (score, NumberContext.Unclear);
    }


    static float ResolveFinalOffer(string input, List<(float value, int score, NumberContext context)> candidates)
    {
        string lowered = input.ToLower();

        // If player "changes mind", take LAST number
        if (lowered.Contains("wait") || lowered.Contains("actually"))
        {
            return candidates.Last().value;
        }

        // Otherwise: take highest scored candidate first, then highest value
        return candidates
            .OrderByDescending(x => x.score)
            .ThenByDescending(x => x.value)
            .First().value;
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
