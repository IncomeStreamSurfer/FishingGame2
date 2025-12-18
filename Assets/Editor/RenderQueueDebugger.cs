using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Debugger tool to verify render queue setup for the Night Sky Overlay system
/// Shows all renderers in the scene and their render queues
/// </summary>
public class RenderQueueDebugger : EditorWindow
{
    private Vector2 scrollPosition;
    private bool autoRefresh = true;
    private string filterText = "";

    [MenuItem("Tools/Sky System/Render Queue Debugger")]
    static void ShowWindow()
    {
        RenderQueueDebugger window = GetWindow<RenderQueueDebugger>("Render Queue Debugger");
        window.minSize = new Vector2(500, 400);
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Render Queue Debugger", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "This tool shows all materials in the scene and their render queues. " +
            "Verify that the Night Sky Overlay renders BEFORE stars/moon but AFTER the skybox.",
            MessageType.Info
        );
        GUILayout.Space(10);

        // Controls
        EditorGUILayout.BeginHorizontal();
        autoRefresh = EditorGUILayout.Toggle("Auto Refresh", autoRefresh);
        if (GUILayout.Button("Manual Refresh", GUILayout.Width(120)))
        {
            Repaint();
        }
        EditorGUILayout.EndHorizontal();

        // Filter
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Filter:", GUILayout.Width(50));
        filterText = EditorGUILayout.TextField(filterText);
        if (GUILayout.Button("Clear", GUILayout.Width(60)))
        {
            filterText = "";
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        // Expected render queue reference
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("Expected Render Queues:", EditorStyles.boldLabel);
        DrawRenderQueueReference();
        EditorGUILayout.EndVertical();

        GUILayout.Space(10);

        // Scene objects list
        GUILayout.Label("Scene Objects:", EditorStyles.boldLabel);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        DrawSceneObjects();

        EditorGUILayout.EndScrollView();
    }

    void DrawRenderQueueReference()
    {
        EditorGUILayout.BeginVertical("box");
        DrawQueueItem("Background", "0-1499", Color.gray);
        DrawQueueItem("Geometry (Opaque)", "2000", Color.white);
        DrawQueueItem("AlphaTest", "2450", Color.white);
        DrawQueueItem("⭐ SKYBOX", "2000", new Color(0.5f, 0.7f, 1f));
        DrawQueueItem("⭐ NIGHT BLACK OVERLAY", "2600", new Color(0.1f, 0.1f, 0.1f));
        DrawQueueItem("⭐ MOON", "2750", Color.yellow);
        DrawQueueItem("⭐ STARS", "2800", Color.white);
        DrawQueueItem("⭐ CLOUDS", "2900", new Color(0.8f, 0.8f, 0.8f));
        DrawQueueItem("⭐ SUN GLOW", "3000", new Color(1f, 0.9f, 0.6f));
        DrawQueueItem("Transparent", "3000", Color.white);
        DrawQueueItem("Overlay", "4000", Color.white);
        EditorGUILayout.EndVertical();
    }

    void DrawQueueItem(string name, string queue, Color color)
    {
        EditorGUILayout.BeginHorizontal();
        GUI.color = color;
        GUILayout.Label(name, GUILayout.Width(200));
        GUILayout.Label(queue, EditorStyles.boldLabel);
        GUI.color = Color.white;
        EditorGUILayout.EndHorizontal();
    }

    void DrawSceneObjects()
    {
        // Find all renderers in the scene
        Renderer[] allRenderers = FindObjectsOfType<Renderer>();

        if (allRenderers.Length == 0)
        {
            EditorGUILayout.HelpBox("No renderers found in scene", MessageType.Info);
            return;
        }

        // Group by render queue
        var groupedRenderers = new Dictionary<int, List<RendererInfo>>();

        foreach (Renderer renderer in allRenderers)
        {
            if (renderer == null || renderer.sharedMaterial == null) continue;

            Material mat = renderer.sharedMaterial;
            int queue = mat.renderQueue;
            string name = renderer.gameObject.name;

            // Apply filter
            if (!string.IsNullOrEmpty(filterText) &&
                !name.ToLower().Contains(filterText.ToLower()) &&
                !mat.name.ToLower().Contains(filterText.ToLower()))
            {
                continue;
            }

            if (!groupedRenderers.ContainsKey(queue))
            {
                groupedRenderers[queue] = new List<RendererInfo>();
            }

            groupedRenderers[queue].Add(new RendererInfo
            {
                gameObject = renderer.gameObject,
                materialName = mat.name,
                renderQueue = queue
            });
        }

        // Sort by render queue
        var sortedQueues = groupedRenderers.Keys.OrderBy(q => q).ToList();

        // Display grouped results
        foreach (int queue in sortedQueues)
        {
            EditorGUILayout.BeginVertical("box");

            // Queue header
            Color queueColor = GetQueueColor(queue);
            GUI.color = queueColor;
            GUILayout.Label($"Render Queue: {queue} ({GetQueueName(queue)})", EditorStyles.boldLabel);
            GUI.color = Color.white;

            // Objects in this queue
            foreach (RendererInfo info in groupedRenderers[queue])
            {
                EditorGUILayout.BeginHorizontal();

                // Object name (clickable)
                if (GUILayout.Button(info.gameObject.name, EditorStyles.linkLabel, GUILayout.Width(200)))
                {
                    Selection.activeGameObject = info.gameObject;
                    EditorGUIUtility.PingObject(info.gameObject);
                }

                // Material name
                GUILayout.Label(info.materialName, EditorStyles.miniLabel);

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(5);
        }
    }

    Color GetQueueColor(int queue)
    {
        if (queue < 2000) return Color.gray;
        if (queue == 2000) return new Color(0.5f, 0.7f, 1f); // Skybox
        if (queue == 2600) return new Color(0.1f, 0.1f, 0.1f); // Black overlay
        if (queue == 2750) return Color.yellow; // Moon
        if (queue == 2800) return Color.white; // Stars
        if (queue == 2900) return new Color(0.8f, 0.8f, 0.8f); // Clouds
        if (queue == 3000) return new Color(1f, 0.9f, 0.6f); // Sun glow
        if (queue >= 3000) return new Color(0.5f, 1f, 0.5f); // Transparent
        return Color.white;
    }

    string GetQueueName(int queue)
    {
        if (queue < 2000) return "Background";
        if (queue == 2000) return "Geometry/Skybox";
        if (queue == 2450) return "AlphaTest";
        if (queue == 2600) return "⭐ NIGHT OVERLAY";
        if (queue == 2750) return "⭐ MOON";
        if (queue == 2800) return "⭐ STARS";
        if (queue == 2900) return "⭐ CLOUDS";
        if (queue == 3000) return "Transparent/Sun Glow";
        if (queue >= 4000) return "Overlay";
        return "Custom";
    }

    void OnInspectorUpdate()
    {
        if (autoRefresh)
        {
            Repaint();
        }
    }

    private class RendererInfo
    {
        public GameObject gameObject;
        public string materialName;
        public int renderQueue;
    }
}
