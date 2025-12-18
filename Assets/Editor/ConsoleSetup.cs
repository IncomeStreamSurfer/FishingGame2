using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor utility for quickly adding the Developer Console to the current scene
/// Access via: Tools > Add Developer Console to Scene
/// </summary>
public class ConsoleSetup
{
    [MenuItem("Tools/Add Developer Console to Scene")]
    public static void AddConsoleToScene()
    {
        // Check if already exists
        ConsoleCommands existing = GameObject.FindObjectOfType<ConsoleCommands>();
        if (existing != null)
        {
            Debug.LogWarning("ConsoleCommands already exists in scene!");
            Selection.activeGameObject = existing.gameObject;
            EditorGUIUtility.PingObject(existing.gameObject);
            return;
        }

        // Create GameObject
        GameObject consoleObj = new GameObject("ConsoleCommands");
        ConsoleCommands console = consoleObj.AddComponent<ConsoleCommands>();

        // Mark the scene as dirty so changes are saved
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );

        // Select the new object
        Selection.activeGameObject = consoleObj;

        Debug.Log("Developer Console added to scene! Press ~ or F1 to open during play mode.");
    }

    [MenuItem("Tools/Developer Console/Add to Scene")]
    public static void AddConsoleToSceneAlt()
    {
        AddConsoleToScene();
    }

    [MenuItem("Tools/Developer Console/Documentation")]
    public static void OpenDocumentation()
    {
        string path = System.IO.Path.Combine(Application.dataPath, "..", "CONSOLE_COMMANDS.md");
        if (System.IO.File.Exists(path))
        {
            Application.OpenURL("file:///" + path);
        }
        else
        {
            Debug.LogWarning("Documentation not found at: " + path);
        }
    }

    [MenuItem("Tools/Developer Console/Quick Start Guide")]
    public static void OpenQuickStart()
    {
        string path = System.IO.Path.Combine(Application.dataPath, "..", "CONSOLE_SETUP_QUICK_START.md");
        if (System.IO.File.Exists(path))
        {
            Application.OpenURL("file:///" + path);
        }
        else
        {
            Debug.LogWarning("Quick start guide not found at: " + path);
        }
    }
}
