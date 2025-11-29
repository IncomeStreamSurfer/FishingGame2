using UnityEngine;

/// <summary>
/// Shows a tip message when the player first arrives in the Frost Zone
/// Similar to the cat tip message in the tropical zone
/// </summary>
public class FrostZoneTip : MonoBehaviour
{
    private bool tipShown = false;
    private bool showingTip = false;
    private float tipDisplayTime = 0f;
    private float tipDuration = 8f;

    void Update()
    {
        if (!MainMenu.GameStarted) return;
        if (tipShown) return;

        // Check if player is in Ice Realm
        GameObject player = GameObject.Find("Player");
        if (player == null) return;

        // Check if player is near the Ice Realm (within 50 units of origin which is Ice Realm center)
        float distanceToCenter = Vector3.Distance(player.transform.position, transform.parent.position);
        if (distanceToCenter < 50f && !tipShown)
        {
            ShowTip();
        }

        // Auto-hide after duration
        if (showingTip)
        {
            tipDisplayTime += Time.deltaTime;
            if (tipDisplayTime > tipDuration)
            {
                showingTip = false;
            }
        }
    }

    void ShowTip()
    {
        tipShown = true;
        showingTip = true;
        tipDisplayTime = 0f;
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted) return;
        if (!showingTip) return;

        // Tip box (like cat message)
        float boxWidth = 420;
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
        else if (tipDisplayTime > tipDuration - 1f)
            alpha = (tipDuration - tipDisplayTime) / 1f;

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

        // Warning icon (snowflake symbol)
        GUIStyle iconStyle = new GUIStyle();
        iconStyle.fontSize = 28;
        iconStyle.fontStyle = FontStyle.Bold;
        iconStyle.alignment = TextAnchor.MiddleCenter;
        iconStyle.normal.textColor = new Color(0.6f, 0.85f, 1f, alpha);

        GUI.Label(new Rect(boxRect.x + 10, boxRect.y + 10, 40, boxRect.height - 20), "*", iconStyle);

        // Tip text
        GUIStyle textStyle = new GUIStyle();
        textStyle.fontSize = 16;
        textStyle.fontStyle = FontStyle.Bold;
        textStyle.alignment = TextAnchor.MiddleLeft;
        textStyle.wordWrap = true;
        textStyle.normal.textColor = new Color(0.9f, 0.95f, 1f, alpha);

        string tipMessage = "You better head on over to Bjork the Huntsman quick!\nBears attack humans here.";
        GUI.Label(new Rect(boxRect.x + 55, boxRect.y + 10, boxRect.width - 70, boxRect.height - 20), tipMessage, textStyle);
    }
}
