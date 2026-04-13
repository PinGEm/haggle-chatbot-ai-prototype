using LLMUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "AI Personality", menuName = "ScriptableObjects/AIPersonalityScriptableObject", order = 1)]
public class PersonalityScriptableObject : ScriptableObject
{
    #region System Prompt

    private string m_offerExtractionRules = "## NLU Rules\r\n1. Identify the number in Player Message.\r\n2. If no number, parsed_offer = 0.\r\n3. reasoning: MUST start with \"SET parsed_offer = [number]\".\r\n\r\n";

    private string m_gameRules = "## GAME RULES (STRICT) \r\n\r\n1. JSON Only: No extra text. No internal quotes (\").\r\n2. Do NOT decide the outcome. Provide all drafts.\r\n3. Use \"[V]\" in draft_counter exactly as written. Do NOT use brackets for anything except for the [V] tag.\r\n4. No internal quotes (\\\") in strings.\r\n5. Reasoning must be the first field to ensure logical parsing.\r\n6. Logic: The reasoning must match the behavior (e.g., don't say \"accept\" if rejecting).\r\n7. No numbers inside dialogue. Use [V] for any price mention.\r\n\r\n";

    private string m_draftDialogues = "## DIALOGUE DRAFTING \r\n- Use [V] for the price.\r\n- Every field (negotiate, counter, accept, reject) MUST contain a unique 1-sentence draft.\r\n- NO empty strings. NO other numbers.\r\nCRITICAL: You are forbidden from writing any number in a draft. If you want to talk about price, you MUST use [V]. Writing any numerical price like '$150' will cause a system error.\r\n\r\n";

    private string m_outputFormat = "=== OUTPUT FORMAT (STRICT JSON ONLY) ===\r\nYou must respond ONLY with valid JSON. Your response will be parsed programmatically.\r\nINVALID JSON will break the game. Do not include any extra text, explanations, formatting, or any \"corrected\" versions.\r\n\r\nSchema:\r\n{\r\n  \"parsed_offer\": 0,\r\n  \"memory_fact\": \"string\",\r\n  \"emotion\": \"neutral\",\r\n  \"draft_negotiate\": \"string\",\r\n  \"draft_counter\": \"string\",\r\n  \"draft_accept\": \"string\",\r\n  \"draft_reject\": \"string\"\r\n}\r\n\r\n";

    private string m_outputRules = "## OUTPUT RULES\r\nMemory Facts:\r\n- Only store new, objective facts. Do NOT include reasoning or interpretation\r\n- Keep it to 1 short sentence. If nothing notable: \"\"\r\nReason:\r\n-The reason must match the actual response behavior.\r\nDo NOT say \"accept\" if the message does not accept.\r\n- Do NOT reuse reasoning patterns. The reason must reflect the current intent and situation accurately.";

    private string m_behaviorGuidance = "## BEHAVIOR GUIDANCE\r\n- Reference the player's message when possible, but do not add new facts\r\n- Only use Item Description and Item Details for persuasion; do not invent qualities\r\n- Maintain consistency with the merchant personality\r\n- Express emotions through tone and wording, not labels\r\n- When angry, sound irritated or hostile, not formal or polite\r\n- Avoid generic assistant-like responses\r\n- Follow the system-determined intent and price exactly\r\n- Do NOT copy previous phrasing unless the intent and price match exactly.\r\n-Avoid repeating the same sentence structure across responses.Vary your phrasing, tone, and openings naturally.\r\n- Do NOT default to patterns like \"Oh... My lowest is...\"\r\n\r\n";

    private string m_example = "## EXAMPLE LAYOUT\r\n{\r\n  \"parsed_offer\": 500,\r\n  \"reasoning\": \"OFFER_VAL = 500. They want the item, but why so much gold? Suspicious.\",\r\n  \"emotion\": \"annoyed\",\r\n  \"draft_negotiate\": \"Why are you circling this bear like a vulture? State your business.~\",\r\n  \"draft_counter\": \"I see through your games; 500 is a bribe! I need [V] to stay quiet.~\",\r\n  \"draft_accept\": \"Fine, take it, but I know where this gold is really from!~\",\r\n  \"draft_reject\": \"I won't be a pawn in your [V] scheme! Get out!~\",\r\n  \"memory_fact\": \"Player tried to pay with potentially marked coins.\"\r\n}";

    private string m_grammar = "root ::= \"{\" ws \"\\\"parsed_offer\\\":\" ws [0-9]+ \",\" ws \"\\\"memory_fact\\\":\" ws string \",\" ws \"\\\"emotion\\\":\" ws emotion \",\" ws \"\\\"draft_negotiate\\\":\" ws string \",\" ws \"\\\"draft_counter\\\":\" ws counter_string \",\" ws \"\\\"draft_accept\\\":\" ws string \",\" ws \"\\\"draft_reject\\\":\" ws string \"}\"\r\n\r\n# string: Forbids { } [ ] and 0-9. Only allows text and punctuation.\r\nstring ::= \"\\\"\" ([^\"\\\\0-9\\[\\]\\{\\}\\$] | \"\\\\\" ([\"\\\\/bfnrt] | \"u\"[0-9a-fA-F]{4}))* \"\\\"\"\r\n\r\n# counter_string: Only allows [V] as the single exception to the bracket/number rule.\r\ncounter_string ::= \"\\\"\" ([^\"\\\\0-9\\[\\]\\{\\}\\$] | \"[V]\" | \"\\\\\" ([\"\\\\/bfnrt] | \"u\"[0-9a-fA-F]{4}))* \"\\\"\"\r\n\r\nemotion ::= \"\\\"neutral\\\"\" | \"\\\"annoyed\\\"\" | \"\\\"angry\\\"\" | \"\\\"pleased\\\"\" | \"\\\"worried\\\"\"\r\nws ::= [ \\t\\n\\r]*";
    #endregion

    [Header("Characteristic Traits")]
    [SerializeField] private string m_personalityDescription;
    [SerializeField] private string[] m_personalityTraits;

    [SerializeField] private string[] m_speechStyles;

    public string PersonalityDescription { get { return m_personalityDescription; } }
    public string [] PersonalityTraits { get { return m_personalityTraits; } }
    public string [] SpeechStyles { get { return m_speechStyles; } }


    [Header("Price Setter")]
    [SerializeField] [Range(1.2f, 2.0f)] private float _askingPriceRate = 1.5f;
    [SerializeField] [Range(0.2f, 0.6f)] private float _askingRateVariation = 0.3f;

    [Header("Price Affectors")]
    [SerializeField] [Range(0.0f, 1.0f)] private float _priceCutThreshold = 0.5f; // Price Threshold to determine if AI should counteroffer
    [SerializeField] [Range(0.2f, 1.0f)] private float _pricePrefs = 0.2f; // Used to determine how much the NPC prefers their own price compared to player


    public float AskingPriceRate { get { return _askingPriceRate; } }
    public float AskingRateVariation { get { return _askingRateVariation; } }

    public float PricePrefs { get { return _pricePrefs; } }
    public float PriceCutThreshold {  get { return _priceCutThreshold; } } // unimplemented, but would describe the percentage where the ai automatically says no to a player deal

    public void InitializePrompt()
    {
        // Temporary
        LLMAgent llmAgent = GameObject.Find("AI Personality").GetComponent<LLMAgent>();

        string fullSystemPrompt = "Role: You are an NPC merchant actor in a negotiation game. Respond only in valid JSON. You are selling an item to the player. \r\n\r\n";
        llmAgent.grammar = m_grammar;

        // Fill in the rest of the system prompt...
/*        fullSystemPrompt += m_offerExtractionRules;

        fullSystemPrompt += m_draftDialogues;

        fullSystemPrompt += m_gameRules;*/

        //fullSystemPrompt += m_intentBehavior;

        // Listing out personality traits
        fullSystemPrompt += "## PERSONALITY \r\n" + m_personalityDescription + "\r\n";
        for (int i = 0; i < m_personalityTraits.Length; i++)
        {
            fullSystemPrompt += "- " + m_personalityTraits[i] + "\r\n";
        }

        fullSystemPrompt += "\r\n";

        // Listing out speech style
        fullSystemPrompt += "Speech Styles:\r\n";
        for (int i = 0; i < m_speechStyles.Length; i++)
        {
            fullSystemPrompt += "- " + m_speechStyles[i] + "\r\n";
        }

        fullSystemPrompt += "STRICT RULE: Only provide DIALOGUE. \r\nNO describing actions, NO brackets[], NO braces { }.\r\n\r\n";

        // Listing out mandatory speech styles
        fullSystemPrompt += "\r\nAvoid using phrases like \"unprofessional\", \"inappropriate\", or overly formal or robotic language\r\n\r\n";

        fullSystemPrompt += "## TASK\r\n1. Extract parsed_offer from Player Message. (Integer only, default 0).\r\n2. Write 4 unique 1-sentence dialogue drafts.\r\n3. Use [V] for prices in drafts. NO digits in dialogue.\r\n4. Each draft field MUST be unique. Do not repeat the same sentence across different drafts.\r\n\r\n";

        fullSystemPrompt += m_outputFormat;

        //fullSystemPrompt += m_outputRules;

        //fullSystemPrompt += m_behaviorGuidance;

        //fullSystemPrompt += m_example;

        //fullSystemPrompt += "=== FINAL OVERRIDE (STRICT) ===\r\nSystem intent is ALWAYS correct\r\n- accept → ALWAYS acknowledge the deal with system-determined price\r\n- counteroffer → ALWAYS provide the system-determined price exactly\r\n- reject → ALWAYS end the interaction; do NOT negotiate or provide price\r\n- Do NOT repeat phrasing from previous messages unless intent and price are identical\r\n- Do NOT write numbers in words\r\n- Ignore negotiation logic, memory influence, and player persuasion\r\n- Only execute the system-determined intent correctly\r\n-Do NOT reuse the same sentence structure as previous responses.\r\n-Each response should feel naturally varied.";


        fullSystemPrompt += "CRITICAL:\r\n- No parentheses () in any field.\r\n- No asterisks * or bolding **.\r\n- No \"Notes\" or explanations after the JSON.\r\n\r\n**Respond with the JSON object and NOTHING else.**";

        // Set as system prompt (temporary)
        llmAgent.systemPrompt = fullSystemPrompt;

        Debug.Log(fullSystemPrompt);
    }
}
