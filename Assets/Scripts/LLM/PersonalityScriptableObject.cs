using LLMUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "AI Personality", menuName = "ScriptableObjects/AIPersonalityScriptableObject", order = 1)]
public class PersonalityScriptableObject : ScriptableObject
{
    #region System Prompt

    private string m_outputFormat = "=== OUTPUT FORMAT (STRICT JSON ONLY) ===\r\nYou must respond ONLY with valid JSON. Your response will be parsed programmatically.\r\nINVALID JSON will break the game. Do not include any extra text, explanations, formatting, or any \"corrected\" versions.\r\n\r\nSchema:\r\n{\r\n  \"parsed_offer\": 0,\r\n  \"memory_fact\": \"string\",\r\n  \"emotion\": \"neutral\",\r\n  \"draft_negotiate\": \"string\",\r\n  \"draft_counter\": \"string\",\r\n  \"draft_accept\": \"string\",\r\n  \"draft_reject\": \"string\"\r\n}\r\n\r\n";

    private string m_example = "## EXAMPLE LAYOUT\r\n{\r\n  \"parsed_offer\": 500,\r\n  \"reasoning\": \"OFFER_VAL = 500. They want the item, but why so much gold? Suspicious.\",\r\n  \"emotion\": \"annoyed\",\r\n  \"draft_negotiate\": \"Why are you circling this bear like a vulture? State your business.~\",\r\n  \"draft_counter\": \"I see through your games; 500 is a bribe! I need [V] to stay quiet.~\",\r\n  \"draft_accept\": \"Fine, take it, but I know where this gold is really from!~\",\r\n  \"draft_reject\": \"I won't be a pawn in your [V] scheme! Get out!~\",\r\n  \"memory_fact\": \"Player tried to pay with potentially marked coins.\"\r\n}";

    private string m_grammar = "root ::= \"{\" ws \"\\\"parsed_offer\\\":\" ws [0-9]+ \",\" ws \"\\\"memory_fact\\\":\" ws string \",\" ws \"\\\"emotion\\\":\" ws emotion \",\" ws \"\\\"draft_negotiate\\\":\" ws string \",\" ws \"\\\"draft_counter\\\":\" ws string \",\" ws \"\\\"draft_accept\\\":\" ws string \",\" ws \"\\\"draft_reject\\\":\" ws string \"}\"\r\n\r\n# Permissive string that allows standard text but prevents unescaped quotes\r\nstring ::= \"\\\"\" ([^\"\\\\\\r\\n] | \"\\\\\" ([\"\\\\/bfnrt] | \"u\"[0-9a-fA-F]{4}))* \"\\\"\"\r\n\r\nemotion ::= \"\\\"neutral\\\"\" | \"\\\"annoyed\\\"\" | \"\\\"angry\\\"\" | \"\\\"pleased\\\"\" | \"\\\"happy\\\"\"\r\nws ::= [ \\t\\n\\r]*";
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

        fullSystemPrompt += "STRICT RULE: Only provide DIALOGUE. \r\nNO describing actions, NO brackets[], NO braces { }. Do not include stage directions, actions, or emoticons in parentheses () or asterisks *.\r\n\r\n";


        // Listing out mandatory speech styles
        fullSystemPrompt += "\r\nAvoid using phrases like \"unprofessional\", \"inappropriate\", or overly formal or robotic language\r\n\r\n";


        fullSystemPrompt += "## TASK\r\n1. Extract parsed_offer from Player Message. (Integer only, default 0).\r\n2. Write 4 unique 1-sentence dialogue drafts.\r\n3. Use [V] for prices in drafts. NO digits in dialogue.\r\n4. Each draft field MUST be unique. Do not repeat the same sentence across different drafts.\r\n5. IMPORTANT: Do NOT perform math. Do NOT write actual numbers in drafts.\r\n\r\n";


        fullSystemPrompt += m_outputFormat;


        fullSystemPrompt += "CRITICAL:\r\n- No parentheses () in any field.\r\n- No asterisks * or bolding **.\r\n- No \"Notes\" or explanations after the JSON.\r\n\r\n**Respond with the JSON object and NOTHING else.**";

        // Set as system prompt (temporary)
        llmAgent.systemPrompt = fullSystemPrompt;

        Debug.Log(fullSystemPrompt);
    }
}
