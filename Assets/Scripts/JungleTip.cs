using UnityEngine;

/// <summary>
/// Shows helpful tip messages when player enters the Jungle Realm
/// </summary>
public class JungleTip : MonoBehaviour
{
    private bool hasShownTip = false;
    private float tipTimer = 0f;
    private bool showingTip = false;
    private int currentTipIndex = 0;

    private string[] tipMessages = {
        "Welcome to the Jungle!\nWatch out for snakes - they're venomous!",
        "Sick of these dirty snakes? Press 'G' to STOMP!",
        "Tip: Climb on boulders or docks to escape snakes!\nThey can't reach you when you're elevated.",
        "Tip: Talk to Rena the Cumbia Queen for quests.\nThe tribal shopkeeper sells jungle gear!"
    };
    private float[] tipDurations = { 6f, 5f, 8f, 10f };

    private GUIStyle tipStyle;
    private GUIStyle shadowStyle;

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        // Check if player is in Jungle Realm
        RealmManager rm = FindObjectOfType<RealmManager>();
        if (rm == null || rm.CurrentRealm != RealmType.JungleRealm) return;

        if (!hasShownTip && !showingTip)
        {
            showingTip = true;
            tipTimer = 0f;
            currentTipIndex = 0;
        }

        if (showingTip)
        {
            tipTimer += Time.deltaTime;

            // Check if current tip duration is over
            float currentDuration = currentTipIndex < tipDurations.Length ? tipDurations[currentTipIndex] : 8f;
            if (tipTimer >= currentDuration)
            {
                currentTipIndex++;
                tipTimer = 0f;

                // All tips shown?
                if (currentTipIndex >= tipMessages.Length)
                {
                    showingTip = false;
                    hasShownTip = true;
                }
            }
        }
    }

    void OnGUI()
    {
        if (!showingTip || !MainMenu.GameStarted) return;
        if (currentTipIndex >= tipMessages.Length) return;

        if (tipStyle == null)
        {
            tipStyle = new GUIStyle(GUI.skin.label);
            tipStyle.fontSize = 24;
            tipStyle.fontStyle = FontStyle.Bold;
            tipStyle.alignment = TextAnchor.MiddleCenter;
            tipStyle.normal.textColor = new Color(0.4f, 1f, 0.4f); // Jungle green

            shadowStyle = new GUIStyle(tipStyle);
            shadowStyle.normal.textColor = Color.black;
        }

        string message = tipMessages[currentTipIndex];

        // Calculate fade
        float currentDuration = currentTipIndex < tipDurations.Length ? tipDurations[currentTipIndex] : 8f;
        float alpha = 1f;
        if (tipTimer < 0.5f)
            alpha = tipTimer / 0.5f;
        else if (tipTimer > currentDuration - 1f)
            alpha = (currentDuration - tipTimer) / 1f;

        Color tipColor = tipStyle.normal.textColor;
        tipColor.a = alpha;
        tipStyle.normal.textColor = tipColor;

        Color shadowColor = Color.black;
        shadowColor.a = alpha;
        shadowStyle.normal.textColor = shadowColor;

        float boxWidth = 500;
        float boxHeight = 80;
        float x = (Screen.width - boxWidth) / 2;
        float y = Screen.height * 0.2f;

        // Shadow
        GUI.Label(new Rect(x + 2, y + 2, boxWidth, boxHeight), message, shadowStyle);
        // Main text
        GUI.Label(new Rect(x, y, boxWidth, boxHeight), message, tipStyle);
    }
}
