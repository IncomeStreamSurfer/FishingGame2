using UnityEngine;

/// <summary>
/// Helper class for creating draggable and resizable GUI windows
/// Usage: Create instance, call UpdateWindow() in OnGUI, use WindowRect for positioning
/// </summary>
public class DraggableWindow
{
    public Rect WindowRect { get; private set; }
    public bool IsDragging { get; private set; }
    public bool IsResizing { get; private set; }

    private Vector2 dragOffset;
    private Vector2 minSize;
    private Vector2 maxSize;
    private float resizeHandleSize = 15f;
    private float titleBarHeight = 25f;

    // Cached textures
    private static Texture2D resizeHandleTex;
    private static Texture2D titleBarTex;

    public DraggableWindow(Rect initialRect, Vector2 minSize, Vector2 maxSize)
    {
        WindowRect = initialRect;
        this.minSize = minSize;
        this.maxSize = maxSize;
        CreateTextures();
    }

    public DraggableWindow(Rect initialRect) : this(initialRect, new Vector2(200, 150), new Vector2(800, 600))
    {
    }

    static void CreateTextures()
    {
        if (resizeHandleTex == null)
        {
            resizeHandleTex = new Texture2D(1, 1);
            resizeHandleTex.SetPixel(0, 0, new Color(0.5f, 0.5f, 0.5f, 0.8f));
            resizeHandleTex.Apply();
        }

        if (titleBarTex == null)
        {
            titleBarTex = new Texture2D(1, 1);
            titleBarTex.SetPixel(0, 0, new Color(0.15f, 0.15f, 0.18f, 1f));
            titleBarTex.Apply();
        }
    }

    /// <summary>
    /// Call this at the start of your OnGUI window drawing
    /// Returns true if window should be drawn (handles drag/resize input)
    /// </summary>
    public bool UpdateWindow()
    {
        Event e = Event.current;
        Rect rect = WindowRect;

        // Title bar area for dragging
        Rect titleBar = new Rect(rect.x, rect.y, rect.width - resizeHandleSize, titleBarHeight);

        // Resize handle area (bottom-right corner)
        Rect resizeHandle = new Rect(
            rect.x + rect.width - resizeHandleSize,
            rect.y + rect.height - resizeHandleSize,
            resizeHandleSize,
            resizeHandleSize
        );

        // Handle mouse down
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            if (resizeHandle.Contains(e.mousePosition))
            {
                IsResizing = true;
                e.Use();
            }
            else if (titleBar.Contains(e.mousePosition))
            {
                IsDragging = true;
                dragOffset = e.mousePosition - new Vector2(rect.x, rect.y);
                e.Use();
            }
        }

        // Handle mouse up
        if (e.type == EventType.MouseUp && e.button == 0)
        {
            IsDragging = false;
            IsResizing = false;
        }

        // Handle dragging
        if (IsDragging && e.type == EventType.MouseDrag)
        {
            Vector2 newPos = e.mousePosition - dragOffset;

            // Clamp to screen
            newPos.x = Mathf.Clamp(newPos.x, 0, Screen.width - rect.width);
            newPos.y = Mathf.Clamp(newPos.y, 0, Screen.height - rect.height);

            rect.x = newPos.x;
            rect.y = newPos.y;
            WindowRect = rect;
            e.Use();
        }

        // Handle resizing
        if (IsResizing && e.type == EventType.MouseDrag)
        {
            float newWidth = e.mousePosition.x - rect.x;
            float newHeight = e.mousePosition.y - rect.y;

            // Clamp to min/max size
            newWidth = Mathf.Clamp(newWidth, minSize.x, maxSize.x);
            newHeight = Mathf.Clamp(newHeight, minSize.y, maxSize.y);

            // Clamp to screen
            newWidth = Mathf.Min(newWidth, Screen.width - rect.x);
            newHeight = Mathf.Min(newHeight, Screen.height - rect.y);

            rect.width = newWidth;
            rect.height = newHeight;
            WindowRect = rect;
            e.Use();
        }

        return true;
    }

    /// <summary>
    /// Draw the resize handle indicator (call at end of window drawing)
    /// </summary>
    public void DrawResizeHandle()
    {
        Rect rect = WindowRect;
        Rect resizeHandle = new Rect(
            rect.x + rect.width - resizeHandleSize,
            rect.y + rect.height - resizeHandleSize,
            resizeHandleSize,
            resizeHandleSize
        );

        // Draw resize grip lines
        GUI.color = new Color(0.6f, 0.6f, 0.6f, 0.8f);
        for (int i = 0; i < 3; i++)
        {
            float offset = i * 4f;
            GUI.DrawTexture(new Rect(
                resizeHandle.x + 3 + offset,
                resizeHandle.y + resizeHandle.height - 3,
                resizeHandle.width - 6 - offset,
                1
            ), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(
                resizeHandle.x + resizeHandle.width - 3,
                resizeHandle.y + 3 + offset,
                1,
                resizeHandle.height - 6 - offset
            ), Texture2D.whiteTexture);
        }
        GUI.color = Color.white;
    }

    /// <summary>
    /// Set the window position (useful for centering)
    /// </summary>
    public void SetPosition(float x, float y)
    {
        Rect rect = WindowRect;
        rect.x = x;
        rect.y = y;
        WindowRect = rect;
    }

    /// <summary>
    /// Center the window on screen
    /// </summary>
    public void CenterOnScreen()
    {
        Rect rect = WindowRect;
        rect.x = (Screen.width - rect.width) / 2;
        rect.y = (Screen.height - rect.height) / 2;
        WindowRect = rect;
    }

    /// <summary>
    /// Set window size
    /// </summary>
    public void SetSize(float width, float height)
    {
        Rect rect = WindowRect;
        rect.width = Mathf.Clamp(width, minSize.x, maxSize.x);
        rect.height = Mathf.Clamp(height, minSize.y, maxSize.y);
        WindowRect = rect;
    }
}
