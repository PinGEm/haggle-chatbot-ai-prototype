using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

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

        if (numbers.Count == 0)
        {
            return new OfferResult
            {
                OfferValue = null,
                Confidence = OfferConfidence.None,
                DebugReason = "No numbers found"
            };
        }

        var classified = new List<(float value, NumberContext context)>();

        foreach (var num in numbers)
        {
            var context = ClassifyNumber(input, num);
            classified.Add((num, context));
        }

        // Get valid offer candidates
        var candidates = classified.Where(x => x.context == NumberContext.OfferCandidate).Select(x => x.value).ToList();

        if (candidates.Count == 0)
        {
            return new OfferResult
            {
                OfferValue = null,
                Confidence = OfferConfidence.None,
                DebugReason = "No valid offer candidates"
            };
        }

        float selected = ResolveFinalOffer(input, candidates);

        return new OfferResult
        {
            OfferValue = selected,
            Confidence = ScoreConfidence(input, selected),
            DebugReason = "Offer detected"
        };
    }


    static List<float> ExtractAllNumbers(string input)
    {
        var matches = Regex.Matches(input, @"\$?\b\d+(\.\d+)?\b");

        return matches
            .Select(m => float.Parse(m.Value))
            .ToList();
    }


    static NumberContext ClassifyNumber(string input, float number)
    {
        // Find what the number means in the player input

        string lowered = input.ToLower();

        // Find position of the number
        int index = lowered.IndexOf(number.ToString()); // find where the number is located
        if (index == -1) return NumberContext.Unclear;

        // Get a small "window" around the number to determine context of the sentence
        int start = Math.Max(0, index - 25);
        int length = Math.Min(50, lowered.Length - start);
        string window = lowered.Substring(start, length);

        // REJECTION PATTERNS
        if (Regex.IsMatch(window, @"(wouldn't|would not|never|not).*" + number))
            return NumberContext.Rejection;

        // REFERENCE PRICE
        if (Regex.IsMatch(window, @"(selling|price|worth|cost).*" + number))
            return NumberContext.ReferencePrice;

        // OFFER PATTERNS
        if (Regex.IsMatch(window, @"(buy|take|offer|give).*" + number) ||
            Regex.IsMatch(window, number + @".*(deal|offer)"))
            return NumberContext.OfferCandidate;

        // If no matches, set to unclear
        return NumberContext.Unclear;
    }

    static float ResolveFinalOffer(string input, List<float> candidates)
    {
        // If multiple numbers exist, find the number which is most likely the offer
        string lowered = input.ToLower();

        // If player "changes mind", take LAST number
        if (lowered.Contains("wait") || lowered.Contains("actually"))
        {
            return candidates.Last();
        }

        // Otherwise: take highest (players usually negotiate upward)
        return candidates.Max();
    }

    // Check for confidence
    static OfferConfidence ScoreConfidence(string input, float value)
    {
        string lowered = input.ToLower();

        bool hasIntent = Regex.IsMatch(lowered,
            @"(i('| a)m|i will|i'd|i can|offer|give|take|buy)");

        bool hasQuestion = lowered.Contains("?");

        if (hasIntent && !hasQuestion) return OfferConfidence.High;

        if (hasIntent && hasQuestion) return OfferConfidence.Medium;

        return OfferConfidence.Low;
    }
}
