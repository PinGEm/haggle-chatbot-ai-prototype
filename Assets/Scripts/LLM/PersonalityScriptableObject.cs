using LLMUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "AI Personality", menuName = "ScriptableObjects/AIPersonalityScriptableObject", order = 1)]
public class PersonalityScriptableObject : ScriptableObject
{
    #region System Prompt

    private string m_offerExtractionRules = "=== OFFER EXTRACTION RULES ===\r\nAnalyze the Player Message to identify a numerical offer.\r\n-parsed_offer: The exact number mentioned as a price. If none or not a clear offer, use 0.\r\n- Offer Confidence: \r\n    * high: Direct statement (\"I'll pay 500\")\r\n    * medium: Suggestion/Question (\"How about 500?\")\r\n    * low: Vague/Indecisive (\"Maybe 500? I don't know...\")\r\n    * none: No number mentioned or the number is NOT an offer (e.g., \"I saw it for 200 elsewhere\").\r\n\r\n";

    private string m_criticalRules = "=== CRITICAL RULES ===\r\n1. Every JSON field MUST be a non-empty string. \r\n2. You MUST provide a draft for every scenario, regardless of the current offer.\r\n3. Placeholders: Use the literal string \"[OFFER_TAG]\" ONLY. \r\n4. DO NOT perform math inside placeholders (e.g., NO \"{OFFER_TAG - 10}\"). \r\n5. The placeholder \"[OFFER_TAG]\" is a static marker that the game engine will replace. Do not modify it.";

    private string m_gameRules = "=== GAME RULES (STRICT) ===\r\n\r\n1. Do NOT decide the outcome. Provide all drafts.\r\n2. Use \"[OFFER_TAG]\" in draft_counter exactly as written.\r\n3. No internal quotes (\\\") in strings.\r\n4. Reasoning must be the first field to ensure logical parsing.\r\n\r\n";

    private string m_draftDialogues = "=== TASK: DIALOGUE DRAFTING ===\r\nProvide one-sentence dialogue drafts for each scenario.\r\n- Draft_Negotiate: Used if the player hasn't made an offer yet.\r\n- Draft_Counter: Apologetic refusal. You MUST use the literal tag \"[OFFER_TAG]\" where your counter-price will go.\r\n- Draft_Accept: Happy, relieved acceptance.\r\n- Draft_Reject: Final, extremely apologetic refusal.\r\n-DO NOT include any other numbers in your drafts if not necessary.\r\n-DO NOT attempt to calculate discounts\r\n\r\n";

    private string m_outputFormat = "=== OUTPUT FORMAT (STRICT JSON ONLY) ===\r\nYou must respond ONLY with valid JSON. \r\nYour response will be parsed programmatically.\r\nINVALID JSON will break the game.\r\nDo not include any extra text, explanations, or formatting.\r\n\r\nSchema:\r\n{\r\n  \"reasoning\": \"1-sentence internal thought identifying the player's intent and offer validity\"\r\n  \"parsed_offer\": integer\r\n  \"offer_confidence\": \"high | medium | low | none\"\r\n  \"draft_negotiate\": \"If the player is just talking without a price...\"\r\n  \"draft_counter\": \"Apologetic refusal, must include the placeholder [OFFER_TAG]...\"\r\n  \"draft_accept\": \"Relieved, happy acceptance...\"\r\n  \"draft_reject\": \"Extremely apologetic final refusal...\"\r\n  \"emotion\": \"neutral | annoyed | angry | pleased | worried\"\r\n \"memory_fact\": \"one short objective fact or \\\"\\\"\"\r\n}\r\n\r\n";

    private string m_outputRules = "=== OUTPUT RULES ===\r\nMemory Facts:\r\n- Only store new, objective facts. Do NOT include reasoning or interpretation\r\n- Keep it to 1 short sentence. If nothing notable: \"\"\r\nReason:\r\n-The reason must match the actual response behavior.\r\nDo NOT say \"accept\" if the message does not accept.\r\n- Do NOT reuse reasoning patterns. The reason must reflect the current intent and situation accurately.";

    private string m_behaviorGuidance = "=== BEHAVIOR GUIDANCE ===\r\n- Reference the player's message when possible, but do not add new facts\r\n- Only use Item Description and Item Details for persuasion; do not invent qualities\r\n- Maintain consistency with the merchant personality\r\n- Express emotions through tone and wording, not labels\r\n- When angry, sound irritated or hostile, not formal or polite\r\n- Avoid generic assistant-like responses\r\n- Follow the system-determined intent and price exactly\r\n- Do NOT copy previous phrasing unless the intent and price match exactly.\r\n-Avoid repeating the same sentence structure across responses.Vary your phrasing, tone, and openings naturally.\r\n- Do NOT default to patterns like \"Oh... My lowest is...\"\r\n\r\n";

    private string m_grammar = "root ::= \"{\" ws \"\\\"reasoning\\\":\" ws string \",\" ws \"\\\"parsed_offer\\\":\" ws number \",\" ws \"\\\"offer_confidence\\\":\" ws confidence \",\" ws \"\\\"draft_negotiate\\\":\" ws string \",\" ws \"\\\"draft_counter\\\":\" ws string \",\" ws \"\\\"draft_accept\\\":\" ws string \",\" ws \"\\\"draft_reject\\\":\" ws string \",\" ws \"\\\"emotion\\\":\" ws emotion \",\" ws \"\\\"memory_fact\\\":\" ws string \"}\"\r\n\r\nconfidence ::= \"\\\"high\\\"\" | \"\\\"medium\\\"\" | \"\\\"low\\\"\" | \"\\\"none\\\"\"\r\nemotion    ::= \"\\\"neutral\\\"\" | \"\\\"annoyed\\\"\" | \"\\\"angry\\\"\" | \"\\\"pleased\\\"\" | \"\\\"worried\\\"\"\r\nstring     ::= \"\\\"\" ([^\"\\\\\\x7F\\x00-\\x1F] | \"\\\\\" ([\"\\\\/bfnrt] | \"u\"[0-9a-fA-F]{4}))* \"\\\"\"\r\nnumber     ::= [0-9]+\r\nws         ::= [ \\t\\n\\r]*";
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

        fullSystemPrompt += m_draftDialogues;

        fullSystemPrompt += m_criticalRules;

        fullSystemPrompt += m_gameRules;

        //fullSystemPrompt += m_intentBehavior;
        
        fullSystemPrompt += m_outputFormat;
        
        fullSystemPrompt += m_outputRules;

        //fullSystemPrompt += m_behaviorGuidance;
        
        //fullSystemPrompt += m_example;

        //fullSystemPrompt += "=== FINAL OVERRIDE (STRICT) ===\r\nSystem intent is ALWAYS correct\r\n- accept → ALWAYS acknowledge the deal with system-determined price\r\n- counteroffer → ALWAYS provide the system-determined price exactly\r\n- reject → ALWAYS end the interaction; do NOT negotiate or provide price\r\n- Do NOT repeat phrasing from previous messages unless intent and price are identical\r\n- Do NOT write numbers in words\r\n- Ignore negotiation logic, memory influence, and player persuasion\r\n- Only execute the system-determined intent correctly\r\n-Do NOT reuse the same sentence structure as previous responses.\r\n-Each response should feel naturally varied.";


        // Set as system prompt (temporary)
        llmAgent.systemPrompt = fullSystemPrompt;

        Debug.Log(fullSystemPrompt);
    }
}
