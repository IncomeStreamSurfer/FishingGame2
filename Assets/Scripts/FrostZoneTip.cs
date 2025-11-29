using UnityEngine;

/// <summary>
/// Shows tip messages when the player first arrives in the Frost Zone
/// First tip: Warning about bears
/// Second tip: How to play dead (CTRL key)
/// </summary>
public class FrostZoneTip : MonoBehaviour
{
    private bool tipsStarted = false;
    private int currentTipIndex = 0;
    private bool showingTip = false;
    private float tipDisplayTime = 0f;

    // Tip configuration
    private string[] tipMessages = {
        "You better head on over to Bjork the Huntsman quick!\nBears attack humans here.",
        "Did you know? If you press the CTRL key, you can play dead!\nBears will leave you alone."
    };
    private float[] tipDurations = { 8f, 10f };

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        // Check if player is in Ice Realm to start tips
        if (!tipsStarted)
        {
            GameObject player = GameObject.Find("Player");
            if (player == null) return;

            float distanceToCenter = Vector3.Distance(player.transform.position, transform.parent.position);
            if (distanceToCenter < 50f)
            {
                StartTips();
            }
        }

        // Update tip display
        if (showingTip)
        {
            tipDisplayTime += Time.deltaTime;
            if (tipDisplayTime > tipDurations[currentTipIndex])
            {
                showingTip = false;

                // Show next tip after a short delay
                if (currentTipIndex < tipMessages.Length - 1)
                {
                    currentTipIndex++;
                    Invoke("ShowNextTip", 1.5f);
                }
            }
        }
    }

    void StartTips()
    {
        tipsStarted = true;
        showingTip = true;
        tipDisplayTime = 0f;
        currentTipIndex = 0;
    }

    void ShowNextTip()
    {
        showingTip = true;
        tipDisplayTime = 0f;
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted) return;
        if (!showingTip || currentTipIndex >= tipMessages.Length) return;

        float duration = tipDurations[currentTipIndex];

        // Tip box (like cat message)
        float boxWidth = 450;
        float boxHeight = 80;
        Rect boxRect = new Rect(
            Screen.width / 2 - boxWidth / 2,
            80,
            boxWidth,
            boxHeight
        );

        // Fade in/out
        float alpha = 1f;
        if (tipDisplayTime < 0.5f)
            alpha = tipDisplayTime / 0.5f;
        else if (tipDisplayTime > duration - 1f)
            alpha = (duration - tipDisplayTime) / 1f;

        // Background with ice blue tint
        GUI.color = new Color(0.15f, 0.25f, 0.35f, 0.95f * alpha);
        GUI.DrawTexture(boxRect, Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Ice blue border
        GUI.color = new Color(0.5f, 0.7f, 0.9f, alpha);
        GUI.DrawTexture(new Rect(boxRect.x - 2, boxRect.y - 2, boxRect.width + 4, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(boxRect.x - 2, boxRect.y + boxRect.height, boxRect.width + 4, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(boxRect.x - 2, boxRect.y, 2, boxRect.height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(boxRect.x + boxRect.width, boxRect.y, 2, boxRect.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Icon
        GUIStyle iconStyle = new GUIStyle();
        iconStyle.fontSize = 28;
        iconStyle.fontStyle = FontStyle.Bold;
        iconStyle.alignment = TextAnchor.MiddleCenter;
        iconStyle.normal.textColor = new Color(0.6f, 0.85f, 1f, alpha);

        string icon = currentTipIndex == 0 ? "!" : "?";
        GUI.Label(new Rect(boxRect.x + 10, boxRect.y + 10, 40, boxRect.height - 20), icon, iconStyle);

        // Tip text
        GUIStyle textStyle = new GUIStyle();
        textStyle.fontSize = 15;
        textStyle.fontStyle = FontStyle.Bold;
        textStyle.alignment = TextAnchor.MiddleLeft;
        textStyle.wordWrap = true;
        textStyle.normal.textColor = new Color(0.9f, 0.95f, 1f, alpha);

        GUI.Label(new Rect(boxRect.x + 55, boxRect.y + 10, boxRect.width - 70, boxRect.height - 20), tipMessages[currentTipIndex], textStyle);
    }
}
