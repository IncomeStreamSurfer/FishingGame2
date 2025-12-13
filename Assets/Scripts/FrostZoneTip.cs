using UnityEngine;

/// <summary>
/// Shows small tip messages on the left side when player enters the Ice Realm
/// Non-disruptive small squares - only shows in Ice Realm
/// </summary>
public class FrostZoneTip : MonoBehaviour
{
    private bool hasShownTip = false;
    private float tipTimer = 0f;
    private bool showingTip = false;
    private int currentTipIndex = 0;
    private bool isInIceRealm = false;

    // Performance: Frame skip for OnGUI
    private int guiFrameSkip = 0;

    private string[] tipMessages = {
        "Bears attack humans here!",
        "Press CTRL to play dead",
        "Visit Bjork for warm gear"
    };
    private float tipDuration = 4f;
    private float tipGap = 0.5f;

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        // Check if player is in Ice Realm
        bool wasInIce = isInIceRealm;
        isInIceRealm = IsPlayerInIceRealm();

        // Only show tips in ice realm
        if (!isInIceRealm)
        {
            showingTip = false;
            return;
        }

        // Start tips when entering ice realm for first time
        if (isInIceRealm && !wasInIce && !hasShownTip)
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

    bool IsPlayerInIceRealm()
    {
        // Use cached realm reference for performance
        return GameCache.IsInRealm(RealmType.IceRealm);
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted) return;

        // Performance: Skip frames when not showing tips
        if (!showingTip)
        {
            guiFrameSkip++;
            if (guiFrameSkip % 3 != 0) return; // Skip 2 out of 3 frames
        }

        if (!showingTip) return;
        if (!isInIceRealm) return;
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

        // Background - ice blue tint
        GUI.color = new Color(0.1f, 0.15f, 0.25f, 0.9f * alpha);
        GUI.DrawTexture(new Rect(boxX, boxY, boxSize, boxSize), Texture2D.whiteTexture);

        // Border - ice blue
        GUI.color = new Color(0.5f, 0.7f, 0.9f, alpha);
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
        iconStyle.normal.textColor = new Color(0.6f, 0.85f, 1f, alpha);
        GUI.Label(new Rect(boxX, boxY + 8, boxSize, 25), "TIP", iconStyle);

        // Text
        GUIStyle textStyle = new GUIStyle();
        textStyle.fontSize = 11;
        textStyle.alignment = TextAnchor.MiddleCenter;
        textStyle.wordWrap = true;
        textStyle.normal.textColor = new Color(0.9f, 0.95f, 1f, alpha);
        GUI.Label(new Rect(boxX + 8, boxY + 35, boxSize - 16, boxSize - 45), tipMessages[currentTipIndex], textStyle);
    }
}
