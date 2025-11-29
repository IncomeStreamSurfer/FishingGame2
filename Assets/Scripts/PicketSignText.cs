using UnityEngine;

/// <summary>
/// Displays text when player approaches a picket sign
/// </summary>
public class PicketSignText : MonoBehaviour
{
    public string message = "";
    private bool playerNearby = false;
    private float interactionDistance = 4f;

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        GameObject player = GameObject.Find("Player");
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);
        playerNearby = distance < interactionDistance;
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted) return;
        if (!playerNearby || string.IsNullOrEmpty(message)) return;

        // Sign background
        float boxWidth = 280;
        float boxHeight = 100;
        Rect boxRect = new Rect(
            Screen.width / 2 - boxWidth / 2,
            Screen.height - 200,
            boxWidth,
            boxHeight
        );

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
        textStyle.fontSize = 14;
        textStyle.fontStyle = FontStyle.Bold;
        textStyle.alignment = TextAnchor.MiddleCenter;
        textStyle.wordWrap = true;
        textStyle.normal.textColor = new Color(0.95f, 0.9f, 0.8f);

        GUI.Label(new Rect(boxRect.x + 10, boxRect.y + 10, boxRect.width - 20, boxRect.height - 20), message, textStyle);
    }
}
