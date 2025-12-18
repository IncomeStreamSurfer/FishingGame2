using UnityEngine;

/// <summary>
/// EXAMPLE MIGRATION: This shows how PicketSignText.cs would be updated to use ResolutionManager.
/// This is a reference example - the original file has NOT been modified.
///
/// CHANGES MADE:
/// 1. Line 31-35: Replaced Screen.width/height with ResolutionManager helpers
/// 2. Line 52: Added font size scaling based on resolution (optional but recommended)
/// </summary>
public class PicketSignText_Migrated_Example : MonoBehaviour
{
    public string message = "";
    private bool playerNearby = false;
    private float interactionDistance = 4f;

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        if (!GameCache.IsPlayerValid()) return;

        float distance = Vector3.Distance(transform.position, GameCache.Player.position);
        playerNearby = distance < interactionDistance;
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted) return;
        if (!playerNearby || string.IsNullOrEmpty(message)) return;

        // Sign background
        float boxWidth = 280;
        float boxHeight = 100;

        // OLD WAY (Lines 30-35 in original):
        // Rect boxRect = new Rect(
        //     Screen.width / 2 - boxWidth / 2,
        //     Screen.height - 200,
        //     boxWidth,
        //     boxHeight
        // );

        // NEW WAY (Using ResolutionManager):
        float x = ResolutionManager.GetViewportOffsetX() + (ResolutionManager.GetEffectiveScreenWidth() - boxWidth) / 2;
        float y = ResolutionManager.GetViewportOffsetY() + ResolutionManager.GetEffectiveScreenHeight() - 200;
        Rect boxRect = new Rect(x, y, boxWidth, boxHeight);

        // OR use the convenience method for centered elements:
        // Rect boxRect = ResolutionManager.GetCenteredRect(boxWidth, boxHeight);
        // boxRect.y = ResolutionManager.GetViewportOffsetY() + ResolutionManager.GetEffectiveScreenHeight() - 200;

        // Background
        GUI.color = new Color(0.2f, 0.15f, 0.1f, 0.9f);
        GUI.DrawTexture(boxRect, Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Border
        GUI.color = new Color(0.5f, 0.4f, 0.3f);
        GUI.DrawTexture(new Rect(boxRect.x - 2, boxRect.y - 2, boxRect.width + 4, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(boxRect.x - 2, boxRect.y + boxRect.height, boxRect.width + 4, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(boxRect.x - 2, boxRect.y, 2, boxRect.height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(boxRect.x + boxRect.width, boxRect.y, 2, boxRect.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Text
        GUIStyle textStyle = new GUIStyle();

        // OLD WAY (Line 52 in original):
        // textStyle.fontSize = 14;

        // NEW WAY (Optional - scales font size with resolution):
        textStyle.fontSize = Mathf.RoundToInt(14 * ResolutionManager.GetScaleFactor());
        // Note: At 1920x1080 (reference), scale factor = 1.0, so fontSize = 14
        //       At 2560x1440, scale factor = 1.33, so fontSize = 18-19
        //       At 1280x720, scale factor = 0.67, so fontSize = 9

        textStyle.fontStyle = FontStyle.Bold;
        textStyle.alignment = TextAnchor.MiddleCenter;
        textStyle.wordWrap = true;
        textStyle.normal.textColor = new Color(0.95f, 0.9f, 0.8f);

        GUI.Label(new Rect(boxRect.x + 10, boxRect.y + 10, boxRect.width - 20, boxRect.height - 20), message, textStyle);
    }

    /*
    MIGRATION SUMMARY:

    BEFORE (2 changes needed):
    Line 31-35:
        Rect boxRect = new Rect(
            Screen.width / 2 - boxWidth / 2,        ← Direct Screen.width usage
            Screen.height - 200,                     ← Direct Screen.height usage
            boxWidth,
            boxHeight
        );

    AFTER:
        float x = ResolutionManager.GetViewportOffsetX() + (ResolutionManager.GetEffectiveScreenWidth() - boxWidth) / 2;
        float y = ResolutionManager.GetViewportOffsetY() + ResolutionManager.GetEffectiveScreenHeight() - 200;
        Rect boxRect = new Rect(x, y, boxWidth, boxHeight);

    OPTIONAL (font scaling):
    Line 52:
        textStyle.fontSize = 14;                     ← Fixed size

    After (scales with resolution):
        textStyle.fontSize = Mathf.RoundToInt(14 * ResolutionManager.GetScaleFactor());

    RESULT:
    - Sign box is properly centered regardless of resolution
    - Sign appears in correct position relative to screen bottom
    - Works correctly with letterboxing/pillarboxing
    - Font optionally scales for better readability at different resolutions
    */
}
