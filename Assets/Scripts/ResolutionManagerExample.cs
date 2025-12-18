using UnityEngine;

/// <summary>
/// Example script showing how to migrate from Screen.width/height to ResolutionManager.
/// This demonstrates the before/after patterns for common UI positioning scenarios.
/// </summary>
public class ResolutionManagerExample : MonoBehaviour
{
    void OnGUI()
    {
        // ============================================
        // EXAMPLE 1: Centered Panel
        // ============================================

        // OLD WAY (Direct Screen.width/height):
        // float panelX = (Screen.width - panelWidth) / 2;
        // float panelY = (Screen.height - panelHeight) / 2;

        // NEW WAY (Using ResolutionManager):
        float panelWidth = 400;
        float panelHeight = 300;
        Rect centeredPanel = ResolutionManager.GetCenteredRect(panelWidth, panelHeight);
        // OR manually:
        // float panelX = (ResolutionManager.GetEffectiveScreenWidth() - panelWidth) / 2 + ResolutionManager.GetViewportOffsetX();
        // float panelY = (ResolutionManager.GetEffectiveScreenHeight() - panelHeight) / 2 + ResolutionManager.GetViewportOffsetY();


        // ============================================
        // EXAMPLE 2: Full-Screen Overlay
        // ============================================

        // OLD WAY:
        // GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), overlayTexture);

        // NEW WAY:
        // GUI.DrawTexture(new Rect(
        //     ResolutionManager.GetViewportOffsetX(),
        //     ResolutionManager.GetViewportOffsetY(),
        //     ResolutionManager.GetEffectiveScreenWidth(),
        //     ResolutionManager.GetEffectiveScreenHeight()
        // ), overlayTexture);


        // ============================================
        // EXAMPLE 3: Anchored to Top-Right Corner
        // ============================================

        // OLD WAY:
        // GUI.Label(new Rect(Screen.width - 110, 35, 100, 25), "Time: 12:00", timeStyle);

        // NEW WAY:
        // float x = ResolutionManager.GetViewportOffsetX() + ResolutionManager.GetEffectiveScreenWidth() - 110;
        // float y = ResolutionManager.GetViewportOffsetY() + 35;
        // GUI.Label(new Rect(x, y, 100, 25), "Time: 12:00", timeStyle);


        // ============================================
        // EXAMPLE 4: Bottom-Center Prompt
        // ============================================

        // OLD WAY:
        // float promptY = Screen.height * 0.7f;
        // GUI.Label(new Rect(0, promptY, Screen.width, 30), "Press E to interact", style);

        // NEW WAY:
        // float promptY = ResolutionManager.GetViewportOffsetY() + ResolutionManager.GetEffectiveScreenHeight() * 0.7f;
        // float promptX = ResolutionManager.GetViewportOffsetX();
        // GUI.Label(new Rect(promptX, promptY, ResolutionManager.GetEffectiveScreenWidth(), 30), "Press E to interact", style);


        // ============================================
        // EXAMPLE 5: Scaling Font Sizes and Spacing
        // ============================================

        // OLD WAY:
        // GUIStyle style = new GUIStyle();
        // style.fontSize = 24;

        // NEW WAY (scale font size based on resolution):
        // GUIStyle style = new GUIStyle();
        // style.fontSize = Mathf.RoundToInt(24 * ResolutionManager.GetScaleFactor());

        // OR for values that should scale with resolution:
        // float scaledSpacing = ResolutionManager.Scale(10); // 10px at 1920x1080


        // ============================================
        // EXAMPLE 6: Clamping Draggable Window
        // ============================================

        // OLD WAY:
        // newPos.x = Mathf.Clamp(newPos.x, 0, Screen.width - rect.width);
        // newPos.y = Mathf.Clamp(newPos.y, 0, Screen.height - rect.height);

        // NEW WAY:
        // float minX = ResolutionManager.GetViewportOffsetX();
        // float maxX = ResolutionManager.GetViewportOffsetX() + ResolutionManager.GetEffectiveScreenWidth() - rect.width;
        // float minY = ResolutionManager.GetViewportOffsetY();
        // float maxY = ResolutionManager.GetViewportOffsetY() + ResolutionManager.GetEffectiveScreenHeight() - rect.height;
        // newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
        // newPos.y = Mathf.Clamp(newPos.y, minY, maxY);


        // ============================================
        // QUICK REFERENCE GUIDE
        // ============================================
        /*

        REPLACE THIS:                           WITH THIS:
        ---------------------------------------------------------------------------------
        Screen.width                       ->   ResolutionManager.GetEffectiveScreenWidth()
        Screen.height                      ->   ResolutionManager.GetEffectiveScreenHeight()

        (Screen.width - w) / 2             ->   Use ResolutionManager.GetCenteredRect(w, h)
        (Screen.height - h) / 2            ->   OR add ResolutionManager.GetViewportOffsetX/Y()

        new Rect(0, 0, SW, SH)             ->   Add GetViewportOffsetX/Y() to position,
        (fullscreen overlay)                    use GetEffectiveScreenWidth/Height() for size

        fontSize = 24                      ->   fontSize = Mathf.RoundToInt(24 * GetScaleFactor())
        spacing = 10                       ->   spacing = ResolutionManager.Scale(10)

        Screen.width * 0.5f                ->   GetViewportOffsetX() + GetEffectiveScreenWidth() * 0.5f
        Screen.height * 0.7f               ->   GetViewportOffsetY() + GetEffectiveScreenHeight() * 0.7f


        MIGRATION CHECKLIST:
        ☐ Add ResolutionManager to your main scene (attach to empty GameObject)
        ☐ Set it to execute early (execution order -100 in meta file)
        ☐ Search your codebase for "Screen.width" and "Screen.height"
        ☐ Replace each instance with the appropriate ResolutionManager method
        ☐ For centered elements, use GetCenteredRect() for simplicity
        ☐ For anchored elements, add viewport offsets to positions
        ☐ For fullscreen overlays, use viewport offset + effective screen size
        ☐ Consider scaling font sizes and spacing values
        ☐ Test at different resolutions (1920x1080, 2560x1440, 1280x720)
        ☐ Verify letterboxing/pillarboxing appears correctly on non-16:9 screens


        WHY THIS MATTERS:
        - 1920x1080 (16:9) is used by 54.44% of Steam users
        - 2560x1440 (16:9) is used by 20.19% of Steam users
        - Together, 16:9 resolutions cover 74%+ of PC gamers
        - Without aspect ratio enforcement, UI breaks on ultrawide/portrait displays
        - ResolutionManager ensures consistent UI regardless of screen size
        - Letterboxing/pillarboxing maintains the intended game presentation

        */
    }
}
