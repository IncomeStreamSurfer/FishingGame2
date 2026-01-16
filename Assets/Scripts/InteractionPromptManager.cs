using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Centralized manager for interaction prompts displayed on screen.
/// All prompts are neatly stacked on the left side of the screen without overlapping.
///
/// Usage:
/// - Call InteractionPromptManager.Instance.RegisterPrompt("uniqueId", "[E] Use BBQ", Color.yellow);
/// - Call InteractionPromptManager.Instance.UnregisterPrompt("uniqueId");
/// - Prompts are automatically drawn and stacked each frame.
/// </summary>
public class InteractionPromptManager : MonoBehaviour
{
    public static InteractionPromptManager Instance { get; private set; }

    // Prompt data structure
    private class PromptData
    {
        public string id;
        public string text;
        public string keyText;  // The key indicator (e.g., "E", "F", "R")
        public Color textColor;
        public Color keyColor;
        public int priority;    // Higher priority = higher on screen
        public float registeredTime;

        public PromptData(string id, string text, string keyText, Color textColor, Color keyColor, int priority)
        {
            this.id = id;
            this.text = text;
            this.keyText = keyText;
            this.textColor = textColor;
            this.keyColor = keyColor;
            this.priority = priority;
            this.registeredTime = Time.time;
        }
    }

    // Active prompts
    private Dictionary<string, PromptData> activePrompts = new Dictionary<string, PromptData>();
    private List<PromptData> sortedPrompts = new List<PromptData>();
    private bool needsSort = false;

    // Layout settings
    private float promptX = 15f;
    private float promptStartY = 0f;  // Will be calculated based on screen height
    private float promptWidth = 220f;
    private float promptHeight = 38f;
    private float promptSpacing = 8f;
    private float startYPercent = 0.35f;  // Start at 35% from top of screen

    // Cached GUI elements
    private Texture2D bgTexture;
    private GUIStyle keyStyle;
    private GUIStyle textStyle;
    private bool stylesInitialized = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Create background texture
        bgTexture = new Texture2D(1, 1);
        bgTexture.SetPixel(0, 0, new Color(0.08f, 0.12f, 0.08f, 0.92f));
        bgTexture.Apply();
    }

    void InitializeStyles()
    {
        if (stylesInitialized) return;

        keyStyle = new GUIStyle();
        keyStyle.fontSize = 16;
        keyStyle.fontStyle = FontStyle.Bold;
        keyStyle.alignment = TextAnchor.MiddleCenter;
        keyStyle.normal.textColor = new Color(1f, 0.9f, 0.4f);

        textStyle = new GUIStyle();
        textStyle.fontSize = 13;
        textStyle.fontStyle = FontStyle.Bold;
        textStyle.alignment = TextAnchor.MiddleLeft;
        textStyle.normal.textColor = new Color(1f, 0.9f, 0.5f);

        stylesInitialized = true;
    }

    /// <summary>
    /// Register a simple prompt with just text (old format like "[E] Use BBQ")
    /// </summary>
    public void RegisterPrompt(string id, string fullText, Color color, int priority = 0)
    {
        // Parse the key from text like "[E] Use BBQ" -> key="E", text="Use BBQ"
        string keyText = "";
        string displayText = fullText;

        if (fullText.StartsWith("[") && fullText.Contains("]"))
        {
            int endBracket = fullText.IndexOf(']');
            keyText = fullText.Substring(1, endBracket - 1);
            displayText = fullText.Substring(endBracket + 1).Trim();
        }

        RegisterPromptWithKey(id, displayText, keyText, color, new Color(1f, 0.9f, 0.4f), priority);
    }

    /// <summary>
    /// Register a prompt with separate key indicator and text
    /// </summary>
    public void RegisterPromptWithKey(string id, string text, string keyText, Color textColor, Color keyColor, int priority = 0)
    {
        if (activePrompts.ContainsKey(id))
        {
            // Update existing prompt
            var prompt = activePrompts[id];
            prompt.text = text;
            prompt.keyText = keyText;
            prompt.textColor = textColor;
            prompt.keyColor = keyColor;
            if (prompt.priority != priority)
            {
                prompt.priority = priority;
                needsSort = true;
            }
        }
        else
        {
            // Add new prompt
            var prompt = new PromptData(id, text, keyText, textColor, keyColor, priority);
            activePrompts[id] = prompt;
            needsSort = true;
        }
    }

    /// <summary>
    /// Unregister/remove a prompt
    /// </summary>
    public void UnregisterPrompt(string id)
    {
        if (activePrompts.ContainsKey(id))
        {
            activePrompts.Remove(id);
            needsSort = true;
        }
    }

    /// <summary>
    /// Check if a prompt is currently registered
    /// </summary>
    public bool IsPromptRegistered(string id)
    {
        return activePrompts.ContainsKey(id);
    }

    /// <summary>
    /// Clear all prompts
    /// </summary>
    public void ClearAllPrompts()
    {
        activePrompts.Clear();
        sortedPrompts.Clear();
        needsSort = false;
    }

    void SortPrompts()
    {
        sortedPrompts.Clear();
        sortedPrompts.AddRange(activePrompts.Values);

        // Sort by priority (higher first), then by registration time (older first for consistent ordering)
        sortedPrompts.Sort((a, b) => {
            int priorityCompare = b.priority.CompareTo(a.priority);
            if (priorityCompare != 0) return priorityCompare;
            return a.registeredTime.CompareTo(b.registeredTime);
        });

        needsSort = false;
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted) return;
        if (PauseMenu.IsPaused) return;
        if (activePrompts.Count == 0) return;

        InitializeStyles();

        if (needsSort)
        {
            SortPrompts();
        }

        // Calculate starting Y position
        promptStartY = Screen.height * startYPercent;

        // Draw each prompt stacked vertically
        float currentY = promptStartY;

        foreach (var prompt in sortedPrompts)
        {
            DrawPrompt(prompt, promptX, currentY);
            currentY += promptHeight + promptSpacing;
        }
    }

    void DrawPrompt(PromptData prompt, float x, float y)
    {
        // Pulsing effect for visibility
        float pulse = 0.85f + Mathf.Sin(Time.time * 3f) * 0.15f;

        // Background
        GUI.color = new Color(0.1f, 0.15f, 0.1f, 0.9f * pulse);
        GUI.DrawTexture(new Rect(x, y, promptWidth, promptHeight), Texture2D.whiteTexture);

        // Border (gold color)
        Color borderColor = new Color(1f, 0.85f, 0.3f, pulse);
        GUI.color = borderColor;
        GUI.DrawTexture(new Rect(x, y, promptWidth, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(x, y + promptHeight - 2, promptWidth, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(x, y, 2, promptHeight), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(x + promptWidth - 2, y, 2, promptHeight), Texture2D.whiteTexture);

        GUI.color = Color.white;

        // Key indicator box
        if (!string.IsNullOrEmpty(prompt.keyText))
        {
            // Key background
            GUI.color = new Color(0.2f, 0.25f, 0.2f, 1f);
            GUI.DrawTexture(new Rect(x + 10, y + 7, 26, 24), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Key text
            keyStyle.normal.textColor = prompt.keyColor;
            GUI.Label(new Rect(x + 10, y + 7, 26, 24), prompt.keyText, keyStyle);

            // Prompt text (to the right of key)
            textStyle.normal.textColor = prompt.textColor;
            GUI.Label(new Rect(x + 42, y, promptWidth - 50, promptHeight), prompt.text, textStyle);
        }
        else
        {
            // No key - just text centered
            textStyle.normal.textColor = prompt.textColor;
            textStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(x, y, promptWidth, promptHeight), prompt.text, textStyle);
            textStyle.alignment = TextAnchor.MiddleLeft;  // Reset
        }
    }

    void OnDestroy()
    {
        if (bgTexture != null)
        {
            Destroy(bgTexture);
        }
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
