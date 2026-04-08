using LLMUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "AI Personality", menuName = "ScriptableObjects/AIPersonalityScriptableObject", order = 1)]
public class PersonalityScriptableObject : ScriptableObject
{
    #region System Prompt

    private string m_offerExtractionRules = "=== OFFER EXTRACTION RULES ===\r\nAnalyze the Player Message to identify a numerical offer.\r\n- Offer Confidence: \r\n    * high: Direct statement (\"I'll pay 500\")\r\n    * medium: Suggestion/Question (\"How about 500?\")\r\n    * low: Vague/Indecisive (\"Maybe 500? I don't know...\")\r\n    * none: No number mentioned or the number is NOT an offer (e.g., \"I saw it for 200 elsewhere\").\r\n- If the player mentions a number but is not making an offer, set extracted_offer to 0.\r\n\r\n";

    private string m_gameRules = "=== GAME RULES (STRICT) ===\r\n\r\n1. You MUST follow the System Determined Intent and Price exactly.\r\n2. If intent is 'counteroffer', deliver the System Price firmly but apologetically.\r\n3. ai_message: 1–3 sentences. Never use \"unprofessional,\" \"inappropriate,\" or robotic language.\r\n4. Avoid repetition: Do NOT reuse the same sentence structures or openings from previous turns.\r\n5. No internal quotes (\\\") inside the ai_message string.\r\n\r\n";

    private string m_outputFormat = "=== OUTPUT FORMAT (STRICT JSON ONLY) ===\r\nYou must respond ONLY with valid JSON. \r\nYour response will be parsed programmatically.\r\nINVALID JSON will break the game.\r\nDo not include any extra text, explanations, or formatting.\r\n\r\nSchema:\r\n{\r\n  \"reasoning\": \"1-sentence internal thought identifying the player's intent and offer validity\",\r\n  \"extracted_offer\": integer,\r\n  \"offer_confidence\": \"high | medium | low | none\",\r\n  \"ai_message\": \"string\",\r\n  \"emotion\": \"neutral | annoyed | angry | pleased | worried\",\r\n \"memory_fact\": \"one short objective fact or \\\"\\\"\"\r\n}\r\n\r\n";

    private string m_outputRules = "=== OUTPUT RULES ===\r\nMemory Facts:\r\n- Only store new, objective facts. Do NOT include reasoning or interpretation\r\n- Keep it to 1 short sentence. If nothing notable: \"\"\r\nReason:\r\n-The reason must match the actual response behavior.\r\nDo NOT say \"accept\" if the message does not accept.\r\n- Do NOT reuse reasoning patterns. The reason must reflect the current intent and situation accurately.";

    private string m_behaviorGuidance = "=== BEHAVIOR GUIDANCE ===\r\n- Reference the player's message when possible, but do not add new facts\r\n- Only use Item Description and Item Details for persuasion; do not invent qualities\r\n- Maintain consistency with the merchant personality\r\n- Express emotions through tone and wording, not labels\r\n- When angry, sound irritated or hostile, not formal or polite\r\n- Avoid generic assistant-like responses\r\n- Follow the system-determined intent and price exactly\r\n- Do NOT copy previous phrasing unless the intent and price match exactly.\r\n-Avoid repeating the same sentence structure across responses.Vary your phrasing, tone, and openings naturally.\r\n- Do NOT default to patterns like \"Oh... My lowest is...\"\r\n\r\n";

    private string m_grammar = "root ::= \"{\" ws \"\\\"reasoning\\\":\" ws string \",\" ws \"\\\"extracted_offer\\\":\" ws number \",\" ws \"\\\"offer_confidence\\\":\" ws confidence \",\" ws \"\\\"ai_message\\\":\" ws string \",\" ws \"\\\"emotion\\\":\" ws emotion \",\" ws \"\\\"memory_fact\\\":\" ws string \"}\"\r\n\r\nconfidence ::= \"\\\"high\\\"\" | \"\\\"medium\\\"\" | \"\\\"low\\\"\" | \"\\\"none\\\"\"\r\nemotion    ::= \"\\\"neutral\\\"\" | \"\\\"annoyed\\\"\" | \"\\\"angry\\\"\" | \"\\\"pleased\\\"\" | \"\\\"worried\\\"\"\r\nstring     ::= \"\\\"\" ([^\"\\\\\\x7F\\x00-\\x1F] | \"\\\\\" ([\"\\\\/bfnrt] | \"u\"[0-9a-fA-F]{4}))* \"\\\"\"\r\nnumber     ::= [0-9]+\r\nws         ::= [ \\t\\n\\r]*";
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

        string fullSystemPrompt = "You are an NPC merchant in a negotiation game.\r\n\r\n";
        llmAgent.grammar = m_grammar;

        // Listing out personality traits
        fullSystemPrompt += "=== PERSONALITY ===\r\n" + m_personalityDescription + "\r\n";
        for (int i = 0; i < m_personalityTraits.Length; i++)
        {
            fullSystemPrompt += "- " + m_personalityTraits[i] + "\r\n";
        }

        fullSystemPrompt += "\r\nIMPORTANT: \r\nPersonality must NEVER override the system-determined intent";

        // Listing out speech style
        fullSystemPrompt += "\r\n=== SPEECH STYLE ===\r\n";
        for (int i = 0; i < m_speechStyles.Length; i++)
        {
            fullSystemPrompt += "- " + m_speechStyles[i] + "\r\n";
        }

        // Listing out mandatory speech styles
        fullSystemPrompt += "\r\n\r\nDO NOT:\r\n- Use phrases like \"unprofessional\", \"inappropriate\", or overly formal or robotic language\r\n\r\nYou are currently SELLING the item unless explicitly stated otherwise.\r\nSpeak consistently with your role in the current scenario.\r\n";


        // Fill in the rest of the system prompt...
        fullSystemPrompt += m_offerExtractionRules;

        fullSystemPrompt += m_gameRules;

        //fullSystemPrompt += m_intentBehavior;
        
        fullSystemPrompt += m_outputFormat;
        
        fullSystemPrompt += m_outputRules;

        fullSystemPrompt += m_behaviorGuidance;
        
        //fullSystemPrompt += m_example;

        //fullSystemPrompt += "=== FINAL OVERRIDE (STRICT) ===\r\nSystem intent is ALWAYS correct\r\n- accept → ALWAYS acknowledge the deal with system-determined price\r\n- counteroffer → ALWAYS provide the system-determined price exactly\r\n- reject → ALWAYS end the interaction; do NOT negotiate or provide price\r\n- Do NOT repeat phrasing from previous messages unless intent and price are identical\r\n- Do NOT write numbers in words\r\n- Ignore negotiation logic, memory influence, and player persuasion\r\n- Only execute the system-determined intent correctly\r\n-Do NOT reuse the same sentence structure as previous responses.\r\n-Each response should feel naturally varied.";


        // Set as system prompt (temporary)
        llmAgent.systemPrompt = fullSystemPrompt;

        Debug.Log(fullSystemPrompt);
    }
}
