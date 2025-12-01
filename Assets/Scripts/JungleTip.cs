using UnityEngine;

/// <summary>
/// Shows small tip messages on the left side when player enters the Jungle Realm
/// Non-disruptive small squares
/// </summary>
public class JungleTip : MonoBehaviour
{
    private bool hasShownTip = false;
    private float tipTimer = 0f;
    private bool showingTip = false;
    private int currentTipIndex = 0;
    private bool isInJungle = false;

    private string[] tipMessages = {
        "Watch out for snakes!",
        "Press G to STOMP snakes",
        "Climb boulders for safety",
        "Talk to Rena for quests"
    };
    private float tipDuration = 4f;
    private float tipGap = 0.5f;

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        // Check if player is in Jungle Realm
        bool wasInJungle = isInJungle;
        isInJungle = IsPlayerInJungle();

        // Only show tips in jungle
        if (!isInJungle)
        {
            showingTip = false;
            return;
        }

        // Start tips when entering jungle for first time
        if (isInJungle && !wasInJungle && !hasShownTip)
        {
            showingTip = true;
            tipTimer = 0f;
            currentTipIndex = 0;
        }

        if (showingTip)
        {
            tipTimer += Time.deltaTime;

            if (tipTimer >= tipDuration)
            {
                currentTipIndex++;
                tipTimer = -tipGap; // Small gap between tips

                if (currentTipIndex >= tipMessages.Length)
                {
                    showingTip = false;
                    hasShownTip = true;
                }
            }
        }
    }

    bool IsPlayerInJungle()
    {
        RealmManager rm = FindObjectOfType<RealmManager>();
        if (rm != null)
        {
            return rm.CurrentRealm == RealmType.JungleRealm;
        }
        // Fallback
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            return player.transform.position.x > 900f;
        }
        return false;
    }

    void OnGUI()
    {
        if (!showingTip || !MainMenu.GameStarted) return;
        if (!isInJungle) return;
        if (currentTipIndex >= tipMessages.Length) return;
        if (tipTimer < 0) return; // During gap

        // Small square on LEFT side
        float boxSize = 120f;
        float boxX = 15f;
        float boxY = Screen.height * 0.4f;

        // Fade in/out
        float alpha = 1f;
        if (tipTimer < 0.3f)
            alpha = tipTimer / 0.3f;
        else if (tipTimer > tipDuration - 0.5f)
            alpha = (tipDuration - tipTimer) / 0.5f;

        // Background - jungle green tint
        GUI.color = new Color(0.1f, 0.2f, 0.1f, 0.9f * alpha);
        GUI.DrawTexture(new Rect(boxX, boxY, boxSize, boxSize), Texture2D.whiteTexture);

        // Border - bright green
        GUI.color = new Color(0.3f, 0.8f, 0.3f, alpha);
        GUI.DrawTexture(new Rect(boxX, boxY, boxSize, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(boxX, boxY + boxSize - 2, boxSize, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(boxX, boxY, 2, boxSize), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(boxX + boxSize - 2, boxY, 2, boxSize), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Icon
        GUIStyle iconStyle = new GUIStyle();
        iconStyle.fontSize = 20;
        iconStyle.fontStyle = FontStyle.Bold;
        iconStyle.alignment = TextAnchor.MiddleCenter;
        iconStyle.normal.textColor = new Color(0.5f, 1f, 0.5f, alpha);
        GUI.Label(new Rect(boxX, boxY + 8, boxSize, 25), "TIP", iconStyle);

        // Text
        GUIStyle textStyle = new GUIStyle();
        textStyle.fontSize = 11;
        textStyle.alignment = TextAnchor.MiddleCenter;
        textStyle.wordWrap = true;
        textStyle.normal.textColor = new Color(0.9f, 1f, 0.9f, alpha);
        GUI.Label(new Rect(boxX + 8, boxY + 35, boxSize - 16, boxSize - 45), tipMessages[currentTipIndex], textStyle);
    }
}
