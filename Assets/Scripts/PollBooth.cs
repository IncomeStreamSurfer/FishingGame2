using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Poll Booth - American-style postbox where players can submit suggestions, bug reports, and feedback
/// Press E to interact when nearby, opens a letter-style UI window
/// Game pauses while writing the letter
/// </summary>
public class PollBooth : MonoBehaviour
{
    public static PollBooth Instance { get; private set; }

    [Header("Interaction Settings")]
    public float interactionRange = 3f;
    public string boothName = "Poll Booth";

    [Header("Categories")]
    public string[] categories = { "Suggestion", "Bug Report", "Feedback", "General" };

    private bool isWindowOpen = false;
    private bool playerNearby = false;
    private Transform playerTransform;

    // UI State
    private int selectedCategory = 0;
    private string playerName = "";
    private string playerEmail = "";
    private string messageSubject = "";
    private string messageBody = "";
    private bool showSubmitConfirmation = false;
    private float confirmationTimer = 0f;
    private string confirmationMessage = "";

    // Cached textures for performance
    private Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();
    private bool texturesInitialized = false;

    // Scroll position for message body
    private Vector2 scrollPosition = Vector2.zero;

    // Saved time scale to restore after closing
    private float savedTimeScale = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        if (GameCache.IsPlayerValid())
            playerTransform = GameCache.Player;

        CreateCachedTextures();

        // Add collider if not present
        if (GetComponent<Collider>() == null)
        {
            BoxCollider col = gameObject.AddComponent<BoxCollider>();
            col.size = new Vector3(0.5f, 1.5f, 0.3f);
            col.center = new Vector3(0, 0.75f, 0);
        }
    }

    void CreateCachedTextures()
    {
        if (texturesInitialized) return;

        CacheTexture("letterBg", new Color(0.95f, 0.92f, 0.85f, 1f)); // Cream/paper color
        CacheTexture("letterBorder", new Color(0.6f, 0.5f, 0.4f, 1f)); // Brown border
        CacheTexture("headerBg", new Color(0.3f, 0.5f, 0.7f, 1f)); // Blue header
        CacheTexture("buttonNormal", new Color(0.25f, 0.45f, 0.65f, 1f));
        CacheTexture("buttonHover", new Color(0.35f, 0.55f, 0.75f, 1f));
        CacheTexture("buttonSubmit", new Color(0.2f, 0.6f, 0.3f, 1f));
        CacheTexture("buttonSubmitHover", new Color(0.3f, 0.7f, 0.4f, 1f));
        CacheTexture("buttonCancel", new Color(0.6f, 0.25f, 0.25f, 1f));
        CacheTexture("buttonCancelHover", new Color(0.7f, 0.35f, 0.35f, 1f));
        CacheTexture("inputField", new Color(1f, 1f, 1f, 1f));
        CacheTexture("inputFieldBorder", new Color(0.7f, 0.7f, 0.7f, 1f));
        CacheTexture("categorySelected", new Color(0.3f, 0.5f, 0.7f, 1f));
        CacheTexture("categoryNormal", new Color(0.85f, 0.85f, 0.85f, 1f));
        CacheTexture("stamp", new Color(0.8f, 0.2f, 0.2f, 1f));
        CacheTexture("confirmBg", new Color(0.2f, 0.5f, 0.3f, 0.95f));
        CacheTexture("white", Color.white);

        texturesInitialized = true;
    }

    void CacheTexture(string name, Color color)
    {
        if (!textureCache.ContainsKey(name))
        {
            Texture2D tex = new Texture2D(2, 2);
            Color[] pixels = new Color[4];
            for (int i = 0; i < 4; i++) pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();
            textureCache[name] = tex;
        }
    }

    Texture2D GetTexture(string name)
    {
        return textureCache.TryGetValue(name, out Texture2D tex) ? tex : Texture2D.whiteTexture;
    }

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        // Update player reference if needed
        if (playerTransform == null && GameCache.IsPlayerValid())
            playerTransform = GameCache.Player;

        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        playerNearby = distance <= interactionRange;

        // Handle E key to open/close
        if (playerNearby && Input.GetKeyDown(KeyCode.E) && !isWindowOpen)
        {
            OpenPollWindow();
        }

        // Handle Escape to close
        if (isWindowOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePollWindow();
        }

        // Update confirmation timer
        if (showSubmitConfirmation)
        {
            confirmationTimer -= Time.unscaledDeltaTime;
            if (confirmationTimer <= 0f)
            {
                showSubmitConfirmation = false;
            }
        }
    }

    void OpenPollWindow()
    {
        isWindowOpen = true;
        savedTimeScale = Time.timeScale;
        Time.timeScale = 0f; // Pause game

        // Reset form
        selectedCategory = 0;
        messageSubject = "";
        messageBody = "";
        scrollPosition = Vector2.zero;

        // Unlock cursor for UI interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Poll Booth opened - game paused");
    }

    public bool IsWindowOpen()
    {
        return isWindowOpen;
    }

    void ClosePollWindow()
    {
        isWindowOpen = false;
        Time.timeScale = savedTimeScale; // Restore time

        // Lock cursor back if needed
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("Poll Booth closed - game resumed");
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted) return;

        // PERFORMANCE: Only draw if there's something to show
        if (!playerNearby && !isWindowOpen && !showSubmitConfirmation) return;

        // Show interaction prompt when nearby
        if (playerNearby && !isWindowOpen)
        {
            DrawInteractionPrompt();
        }

        // Draw the poll window
        if (isWindowOpen)
        {
            DrawPollWindow();
        }

        // Draw submission confirmation
        if (showSubmitConfirmation)
        {
            DrawConfirmation();
        }
    }

    void DrawInteractionPrompt()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 16;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.MiddleCenter;

        // Envelope icon representation
        style.normal.textColor = new Color(0.4f, 0.6f, 0.8f);
        GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 80, 200, 25), "[E] Poll Booth", style);

        style.fontSize = 12;
        style.normal.textColor = Color.white;
        GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height / 2 - 55, 300, 20), "Submit suggestions, bug reports & feedback", style);
    }

    void DrawPollWindow()
    {
        // Letter dimensions
        float letterWidth = 650;
        float letterHeight = 580;
        float letterX = (Screen.width - letterWidth) / 2;
        float letterY = (Screen.height - letterHeight) / 2;

        // Draw shadow
        GUI.color = new Color(0, 0, 0, 0.3f);
        GUI.DrawTexture(new Rect(letterX + 8, letterY + 8, letterWidth, letterHeight), GetTexture("white"));
        GUI.color = Color.white;

        // Draw letter border
        GUI.DrawTexture(new Rect(letterX - 4, letterY - 4, letterWidth + 8, letterHeight + 8), GetTexture("letterBorder"));

        // Draw letter background (paper)
        GUI.DrawTexture(new Rect(letterX, letterY, letterWidth, letterHeight), GetTexture("letterBg"));

        // Draw decorative lines on the paper
        GUI.color = new Color(0.8f, 0.75f, 0.7f, 0.3f);
        for (int i = 0; i < 15; i++)
        {
            GUI.DrawTexture(new Rect(letterX + 40, letterY + 150 + i * 28, letterWidth - 80, 1), GetTexture("white"));
        }
        GUI.color = Color.white;

        // Draw header
        GUI.DrawTexture(new Rect(letterX, letterY, letterWidth, 50), GetTexture("headerBg"));

        // Draw stamp in top right
        GUI.DrawTexture(new Rect(letterX + letterWidth - 70, letterY + 60, 55, 65), GetTexture("stamp"));
        GUIStyle stampStyle = new GUIStyle();
        stampStyle.fontSize = 10;
        stampStyle.fontStyle = FontStyle.Bold;
        stampStyle.alignment = TextAnchor.MiddleCenter;
        stampStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(letterX + letterWidth - 70, letterY + 75, 55, 20), "FISH", stampStyle);
        GUI.Label(new Rect(letterX + letterWidth - 70, letterY + 90, 55, 20), "OR DIE", stampStyle);
        GUI.Label(new Rect(letterX + letterWidth - 70, letterY + 105, 55, 15), "MAIL", stampStyle);

        // Title
        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = 22;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(letterX, letterY + 10, letterWidth, 35), "POLL BOOTH - Player Feedback", titleStyle);

        // Close button
        if (DrawButton(new Rect(letterX + letterWidth - 40, letterY + 10, 30, 30), "X", "buttonCancel", "buttonCancelHover"))
        {
            ClosePollWindow();
        }

        float contentY = letterY + 65;
        float labelWidth = 100;
        float inputWidth = letterWidth - 180;

        // Label style
        GUIStyle labelStyle = new GUIStyle();
        labelStyle.fontSize = 14;
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.normal.textColor = new Color(0.3f, 0.3f, 0.3f);

        // === Category Selection ===
        GUI.Label(new Rect(letterX + 30, contentY, labelWidth, 25), "Category:", labelStyle);

        float categoryX = letterX + 30;
        float categoryY = contentY + 25;
        float categoryBtnWidth = 130;
        float categoryBtnHeight = 28;

        for (int i = 0; i < categories.Length; i++)
        {
            bool isSelected = (selectedCategory == i);
            Rect btnRect = new Rect(categoryX + i * (categoryBtnWidth + 10), categoryY, categoryBtnWidth, categoryBtnHeight);

            // Draw category button
            bool hover = btnRect.Contains(Event.current.mousePosition);
            GUI.DrawTexture(btnRect, isSelected ? GetTexture("categorySelected") : GetTexture("categoryNormal"));

            GUIStyle catStyle = new GUIStyle();
            catStyle.fontSize = 12;
            catStyle.fontStyle = isSelected ? FontStyle.Bold : FontStyle.Normal;
            catStyle.alignment = TextAnchor.MiddleCenter;
            catStyle.normal.textColor = isSelected ? Color.white : new Color(0.3f, 0.3f, 0.3f);
            GUI.Label(btnRect, categories[i], catStyle);

            if (GUI.Button(btnRect, "", GUIStyle.none))
            {
                selectedCategory = i;
            }
        }

        contentY += 65;

        // === Name Field ===
        GUI.Label(new Rect(letterX + 30, contentY, labelWidth, 25), "Your Name:", labelStyle);
        playerName = DrawTextField(new Rect(letterX + 130, contentY, inputWidth - 80, 25), playerName, "Enter your name (optional)");
        contentY += 35;

        // === Email Field ===
        GUI.Label(new Rect(letterX + 30, contentY, labelWidth, 25), "Email:", labelStyle);
        playerEmail = DrawTextField(new Rect(letterX + 130, contentY, inputWidth - 80, 25), playerEmail, "your@email.com (optional)");
        contentY += 35;

        // === Subject Field ===
        GUI.Label(new Rect(letterX + 30, contentY, labelWidth, 25), "Subject:", labelStyle);
        messageSubject = DrawTextField(new Rect(letterX + 130, contentY, inputWidth - 80, 25), messageSubject, "Brief summary of your message");
        contentY += 40;

        // === Message Body ===
        GUI.Label(new Rect(letterX + 30, contentY, labelWidth, 25), "Message:", labelStyle);
        contentY += 25;

        // Text area for message
        Rect textAreaRect = new Rect(letterX + 30, contentY, letterWidth - 60, 200);

        // Draw text area border
        GUI.DrawTexture(new Rect(textAreaRect.x - 2, textAreaRect.y - 2, textAreaRect.width + 4, textAreaRect.height + 4), GetTexture("inputFieldBorder"));
        GUI.DrawTexture(textAreaRect, GetTexture("inputField"));

        // Text area style
        GUIStyle textAreaStyle = new GUIStyle(GUI.skin.textArea);
        textAreaStyle.fontSize = 14;
        textAreaStyle.wordWrap = true;
        textAreaStyle.normal.textColor = new Color(0.2f, 0.2f, 0.2f);
        textAreaStyle.focused.textColor = new Color(0.1f, 0.1f, 0.1f);
        textAreaStyle.padding = new RectOffset(8, 8, 8, 8);

        messageBody = GUI.TextArea(textAreaRect, messageBody, 2000, textAreaStyle);

        // Placeholder text
        if (string.IsNullOrEmpty(messageBody))
        {
            GUIStyle placeholderStyle = new GUIStyle();
            placeholderStyle.fontSize = 14;
            placeholderStyle.fontStyle = FontStyle.Italic;
            placeholderStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
            placeholderStyle.padding = new RectOffset(8, 8, 8, 8);
            GUI.Label(textAreaRect, "Write your message here...\n\nBe as detailed as you'd like. We read every submission!", placeholderStyle);
        }

        contentY += 215;

        // Character count
        GUIStyle charCountStyle = new GUIStyle();
        charCountStyle.fontSize = 11;
        charCountStyle.alignment = TextAnchor.MiddleRight;
        charCountStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
        GUI.Label(new Rect(letterX + 30, contentY, letterWidth - 60, 20), $"{messageBody.Length}/2000 characters", charCountStyle);

        contentY += 30;

        // === Buttons ===
        float buttonWidth = 140;
        float buttonHeight = 40;
        float buttonSpacing = 20;
        float buttonsX = letterX + (letterWidth - buttonWidth * 2 - buttonSpacing) / 2;

        // Cancel button
        if (DrawButton(new Rect(buttonsX, contentY, buttonWidth, buttonHeight), "Cancel", "buttonCancel", "buttonCancelHover"))
        {
            ClosePollWindow();
        }

        // Submit button
        if (DrawButton(new Rect(buttonsX + buttonWidth + buttonSpacing, contentY, buttonWidth, buttonHeight), "Submit Letter", "buttonSubmit", "buttonSubmitHover"))
        {
            SubmitFeedback();
        }

        // Footer note
        GUIStyle footerStyle = new GUIStyle();
        footerStyle.fontSize = 10;
        footerStyle.fontStyle = FontStyle.Italic;
        footerStyle.alignment = TextAnchor.MiddleCenter;
        footerStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
        GUI.Label(new Rect(letterX, letterY + letterHeight - 25, letterWidth, 20), "Your feedback helps us improve Fish or Die!", footerStyle);
    }

    string DrawTextField(Rect rect, string value, string placeholder)
    {
        // Draw border and background
        GUI.DrawTexture(new Rect(rect.x - 2, rect.y - 2, rect.width + 4, rect.height + 4), GetTexture("inputFieldBorder"));
        GUI.DrawTexture(rect, GetTexture("inputField"));

        // Text field style
        GUIStyle fieldStyle = new GUIStyle(GUI.skin.textField);
        fieldStyle.fontSize = 13;
        fieldStyle.normal.textColor = new Color(0.2f, 0.2f, 0.2f);
        fieldStyle.focused.textColor = new Color(0.1f, 0.1f, 0.1f);
        fieldStyle.padding = new RectOffset(6, 6, 4, 4);

        string result = GUI.TextField(rect, value, fieldStyle);

        // Show placeholder if empty
        if (string.IsNullOrEmpty(value))
        {
            GUIStyle placeholderStyle = new GUIStyle();
            placeholderStyle.fontSize = 12;
            placeholderStyle.fontStyle = FontStyle.Italic;
            placeholderStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
            placeholderStyle.padding = new RectOffset(8, 6, 6, 4);
            GUI.Label(rect, placeholder, placeholderStyle);
        }

        return result;
    }

    bool DrawButton(Rect rect, string text, string normalTex, string hoverTex)
    {
        bool hover = rect.Contains(Event.current.mousePosition);
        GUI.DrawTexture(rect, hover ? GetTexture(hoverTex) : GetTexture(normalTex));

        GUIStyle btnStyle = new GUIStyle();
        btnStyle.fontSize = 14;
        btnStyle.fontStyle = FontStyle.Bold;
        btnStyle.alignment = TextAnchor.MiddleCenter;
        btnStyle.normal.textColor = Color.white;
        GUI.Label(rect, text, btnStyle);

        return GUI.Button(rect, "", GUIStyle.none);
    }

    void DrawConfirmation()
    {
        float boxWidth = 400;
        float boxHeight = 100;
        float boxX = (Screen.width - boxWidth) / 2;
        float boxY = Screen.height - 150;

        // Draw confirmation box
        GUI.DrawTexture(new Rect(boxX - 3, boxY - 3, boxWidth + 6, boxHeight + 6), GetTexture("letterBorder"));
        GUI.DrawTexture(new Rect(boxX, boxY, boxWidth, boxHeight), GetTexture("confirmBg"));

        GUIStyle confirmStyle = new GUIStyle();
        confirmStyle.fontSize = 16;
        confirmStyle.fontStyle = FontStyle.Bold;
        confirmStyle.alignment = TextAnchor.MiddleCenter;
        confirmStyle.normal.textColor = Color.white;
        confirmStyle.wordWrap = true;
        GUI.Label(new Rect(boxX + 20, boxY + 20, boxWidth - 40, boxHeight - 40), confirmationMessage, confirmStyle);
    }

    void SubmitFeedback()
    {
        // Validate
        if (string.IsNullOrWhiteSpace(messageBody))
        {
            ShowConfirmation("Please write a message before submitting!", 3f);
            return;
        }

        // Create submission data
        PollSubmission submission = new PollSubmission
        {
            category = categories[selectedCategory],
            playerName = string.IsNullOrEmpty(playerName) ? "Anonymous" : playerName,
            playerEmail = playerEmail,
            subject = string.IsNullOrEmpty(messageSubject) ? $"{categories[selectedCategory]} from player" : messageSubject,
            message = messageBody,
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            gameVersion = "0.2 Open Testing",
            playerLevel = LevelingSystem.Instance != null ? LevelingSystem.Instance.GetLevel() : 1
        };

        // Save submission
        PollBoothEmailService.SaveSubmission(submission);

        // Show confirmation
        ShowConfirmation("Thank you! Your feedback has been submitted successfully!", 3f);

        // Clear form
        playerName = "";
        playerEmail = "";
        messageSubject = "";
        messageBody = "";
        selectedCategory = 0;

        // Close window after brief delay
        Invoke("ClosePollWindow", 2f);
    }

    void ShowConfirmation(string message, float duration)
    {
        confirmationMessage = message;
        confirmationTimer = duration;
        showSubmitConfirmation = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }

    void OnDestroy()
    {
        foreach (var tex in textureCache.Values)
        {
            if (tex != null) Destroy(tex);
        }
        textureCache.Clear();
    }
}

/// <summary>
/// Data structure for poll submissions
/// </summary>
[Serializable]
public class PollSubmission
{
    public string category;
    public string playerName;
    public string playerEmail;
    public string subject;
    public string message;
    public string timestamp;
    public string gameVersion;
    public int playerLevel;
}
