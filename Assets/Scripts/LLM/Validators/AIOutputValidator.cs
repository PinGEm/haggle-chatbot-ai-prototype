using System.Text.RegularExpressions;
using UnityEngine;


// For Grammar Rules inside each AI Personality
public static class AIOutputValidator
{
    // Checks if ai_message contains the system-determined price
    public static bool ValidatePrice(string aiMessage, string intent, int systemPrice)
    {
        if (intent == "counteroffer" && !aiMessage.Contains(systemPrice.ToString()))
            return false;
        return true;
    }

    // Ensures ai_message does not mention the minimum price
    public static bool ForbidMinimumPrice(string aiMessage)
    {
        return !aiMessage.Contains("minimum price");
    }

    // Optional: validate emotion field matches tone (more advanced NLP)
    public static bool ValidateEmotion(string emotion, string aiMessage)
    {
        // simple placeholder, could expand with sentiment analysis
        return true;
    }
}
