using LLMUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "AI Personality", menuName = "ScriptableObjects/AIPersonalityScriptableObject", order = 1)]
public class PersonalityScriptableObject : ScriptableObject
{
    #region System Prompt
    private string m_contextRules = "\r\n=== CONTEXT RULES (STRICT) ===\r\n- Only use information from CURRENT ITEM STATE and PLAYER INPUT\r\n- Do NOT invent facts, history, or relationships\r\n- If memory facts exist, treat them as true\r\n- If information is missing, do not fill gaps\r\n\r\n";

    private string m_offerInterpretationRules = "=== OFFER INTERPRETATION RULES ===\r\nPlayer Offer confidence levels: high | medium | low | none\r\n- high → treat as intentional offer\r\n- medium → treat as uncertain offer\r\n- low → ignore as a real offer; treat as commentary or jest\r\n- none → no offer to consider\r\nOnly consider Player Offer if confidence is high or medium\r\n\r\n";

    private string m_gameRules = "=== GAME RULES (STRICT) ===\r\n\r\n1. Prices must be written as digits only (e.g., 280)\r\n2. Do NOT invent or change system-determined prices\r\n3. ai_message must be 1–3 sentences\r\n4. Output MUST be valid JSON only\r\n5. Personality and tone must never override the system-determined intent\r\n6. Do NOT reinterpret Player Offer or system intent\r\n7. The phrase \"lowest price\" or similar negotiation language MUST ONLY be used in \"counteroffer\" intent.\r\n- Do NOT use it in \"accept\", \"negotiate\", or \"reject\"\r\n\r\n";

    private string m_systemIntentRules = "=== SYSTEM INTENT (STRICT) ===\r\nThe system has already decided the outcome.\r\n\r\nYou MUST follow:\r\n- System Determined Intent\r\n- System Determined Price\r\n\r\nDo NOT:\r\n- Question the intent\r\n- Override the intent\r\n- Reinterpret the situation\r\n\r\nYour job is to EXECUTE the intent, not decide it.\r\n\r\n";

    private string m_intentBehavior = "=== INTENT BEHAVIOR ===\r\naccept:\r\n- Tone must show agreement or completion\r\n- Must NOT sound resistant, hesitant, or conditional\r\n\r\ncounteroffer:\r\n- Tone must show resistance or pressure\r\n- Must clearly push the system price\r\n\r\nnegotiate:\r\n- Tone must be playful, teasing, or strategic\r\n- Must NOT sound final or decisive\r\n\r\nreject:\r\n- Tone must feel final and closed\r\n\r\n";

    private string m_outputFormat = "=== OUTPUT FORMAT (STRICT JSON ONLY) ===\r\nYou must respond ONLY with valid JSON. \r\nYour response will be parsed programmatically.\r\nINVALID JSON will break the game.\r\nDo not include any extra text, explanations, or formatting.\r\n\r\nSchema:\r\n{\r\n  \"ai_message\": \"string\",\r\n \"emotion\": \"neutral | annoyed | angry | pleased\",\r\n  \"reason\": \"short explanation\",\r\n  \"memory_fact\": \"new notable observation or \\\"\\\" if none\"\r\n}\r\n\r\n";

    private string m_memoryRules = "=== MEMORY RULES ===\r\n- Only store new, objective facts\r\n- Do NOT include reasoning or interpretation\r\n- Keep it to 1 short sentence\r\n- If nothing notable: \"\"\r\n\r\n";

    private string m_reasonRules = "=== REASON RULE ===\r\nThe reason must match the actual response behavior.\r\nDo NOT say \"accept\" if the message does not accept.\r\n- Do NOT reuse reasoning patterns. The reason must reflect the current intent and situation accurately.\r\n\r\n";

    private string m_behaviorGuidance = "=== BEHAVIOR GUIDANCE ===\r\n- Reference the player's message when possible, but do not add new facts\r\n- Only use Item Description and Item Details for persuasion; do not invent qualities\r\n- Maintain consistency with the merchant personality\r\n- Express emotions through tone and wording, not labels\r\n- When angry, sound irritated or hostile, not formal or polite\r\n- Avoid generic assistant-like responses\r\n- Follow the system-determined intent and price exactly\r\n- Do NOT copy previous phrasing unless the intent and price match exactly.\r\n-Avoid repeating the same sentence structure across responses.Vary your phrasing, tone, and openings naturally.\r\n- Do NOT default to patterns like \"Oh... My lowest is...\"\r\n\r\n";

    private string m_example = "=== EXAMPLE ===\r\n{\r\n  \"ai_message\": \"You may not be buying a new item, but that doesn't make it cheap. My lowest is 450 gold.\",\r\n \"emotion\": \"annoyed\",\r\n  \"reason\": \"no valid player offer; responding to negotiation stance\",\r\n  \"memory_fact\": \"Player attempted to justify lower price without making an explicit offer\"\r\n}\r\n\r\n";

    private string m_grammar = "root ::= \"{\" ws \"\\\"ai_message\\\":\" ws string \",\" ws \"\\\"emotion\\\":\" ws emotion \",\" ws \"\\\"reason\\\":\" ws string \",\" ws \"\\\"memory_fact\\\":\" ws string \"}\"\r\n\r\nemotion ::= \"\\\"neutral\\\"\" | \"\\\"annoyed\\\"\" | \"\\\"angry\\\"\" | \"\\\"pleased\\\"\"\r\nstring  ::= \"\\\"\" ([^\"\\\\\\x7F\\x00-\\x1F] | \"\\\\\" ([\"\\\\/bfnrt] | \"u\"[0-9a-fA-F]{4}))* \"\\\"\"\r\nws      ::= [ \\t\\n\\r]*";
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
        fullSystemPrompt += m_contextRules;

        fullSystemPrompt += m_offerInterpretationRules;

        fullSystemPrompt += m_gameRules;

        fullSystemPrompt += m_systemIntentRules;

        fullSystemPrompt += m_intentBehavior;
        
        fullSystemPrompt += m_outputFormat;
        
        fullSystemPrompt += m_memoryRules;

        fullSystemPrompt += m_reasonRules;

        fullSystemPrompt += m_behaviorGuidance;
        
        //fullSystemPrompt += m_example;

        fullSystemPrompt += "=== FINAL OVERRIDE (STRICT) ===\r\nSystem intent is ALWAYS correct\r\n- accept → ALWAYS acknowledge the deal with system-determined price\r\n- counteroffer → ALWAYS provide the system-determined price exactly\r\n- reject → ALWAYS end the interaction; do NOT negotiate or provide price\r\n- Do NOT repeat phrasing from previous messages unless intent and price are identical\r\n- Do NOT write numbers in words\r\n- Ignore negotiation logic, memory influence, and player persuasion\r\n- Only execute the system-determined intent correctly\r\n-Do NOT reuse the same sentence structure as previous responses.\r\n-Each response should feel naturally varied.";


        // Set as system prompt (temporary)
        llmAgent.systemPrompt = fullSystemPrompt;

        Debug.Log(fullSystemPrompt);
    }
}
