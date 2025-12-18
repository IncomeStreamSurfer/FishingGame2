using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor window for testing the Night Sky Overlay system
/// Allows quick time-of-day changes to verify the black sky effect
/// </summary>
public class NightSkyOverlayTester : EditorWindow
{
    private DayNightCycle dayNightCycle;
    private NightSkyOverlay nightSkyOverlay;
    private float testTimeOfDay = 20f; // Default to 8 PM (night)

    [MenuItem("Tools/Sky System/Night Sky Overlay Tester")]
    static void ShowWindow()
    {
        NightSkyOverlayTester window = GetWindow<NightSkyOverlayTester>("Night Sky Tester");
        window.minSize = new Vector2(350, 400);
        window.Show();
    }

    void OnEnable()
    {
        FindComponents();
    }

    void FindComponents()
    {
        dayNightCycle = FindObjectOfType<DayNightCycle>();
        nightSkyOverlay = FindObjectOfType<NightSkyOverlay>();
    }

    void OnGUI()
    {
        GUILayout.Label("Night Sky Overlay Tester", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // Check for required components
        if (dayNightCycle == null)
        {
            EditorGUILayout.HelpBox("DayNightCycle not found in scene!", MessageType.Error);
            if (GUILayout.Button("Refresh"))
            {
                FindComponents();
            }
            return;
        }

        if (nightSkyOverlay == null)
        {
            EditorGUILayout.HelpBox("NightSkyOverlay not found in scene!", MessageType.Warning);
            if (GUILayout.Button("Create NightSkyOverlay"))
            {
                NightSkyOverlaySetup.AddNightSkyOverlayMenuItem();
                FindComponents();
            }
            GUILayout.Space(10);
        }

        // Display current status
        GUILayout.Label("Current Status", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");

        float currentHour = Application.isPlaying ? dayNightCycle.GetCurrentHour() : testTimeOfDay;
        EditorGUILayout.LabelField("Current Time:", FormatTime(currentHour));

        if (nightSkyOverlay != null)
        {
            float alpha = nightSkyOverlay.GetNightOverlayAlpha();
            EditorGUILayout.LabelField("Overlay Alpha:", alpha.ToString("F3"));
            EditorGUILayout.LabelField("Sky Appearance:", alpha > 0.5f ? "BLACK" : (alpha > 0.01f ? "TRANSITIONING" : "BLUE"));
        }

        EditorGUILayout.EndVertical();
        GUILayout.Space(10);

        // Time control
        GUILayout.Label("Time Control", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");

        if (Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Game is running. Use the slider to change time in real-time.", MessageType.Info);

            float newTime = EditorGUILayout.Slider("Time of Day", dayNightCycle.GetCurrentHour(), 0f, 24f);
            if (newTime != dayNightCycle.GetCurrentHour())
            {
                dayNightCycle.SetTimeOfDay(newTime);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Enter Play Mode to test the overlay in real-time.", MessageType.Info);
            testTimeOfDay = EditorGUILayout.Slider("Preview Time", testTimeOfDay, 0f, 24f);
        }

        EditorGUILayout.EndVertical();
        GUILayout.Space(10);

        // Quick time presets
        GUILayout.Label("Quick Time Presets", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Midnight (12 AM)"))
        {
            SetTime(0f);
        }
        if (GUILayout.Button("Sunrise (6 AM)"))
        {
            SetTime(6f);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Noon (12 PM)"))
        {
            SetTime(12f);
        }
        if (GUILayout.Button("Sunset (6 PM)"))
        {
            SetTime(18f);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Night (8 PM)"))
        {
            SetTime(20f);
        }
        if (GUILayout.Button("Late Night (2 AM)"))
        {
            SetTime(2f);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
        GUILayout.Space(10);

        // Transition testing
        GUILayout.Label("Transition Testing", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");

        if (GUILayout.Button("Test Sunrise Transition (6 AM → 8 AM)"))
        {
            if (Application.isPlaying)
            {
                EditorCoroutineUtility.StartCoroutine(AnimateTimeTransition(6f, 8f, 5f), this);
            }
            else
            {
                EditorUtility.DisplayDialog("Play Mode Required", "Enter Play Mode to test animated transitions.", "OK");
            }
        }

        if (GUILayout.Button("Test Sunset Transition (6 PM → 8 PM)"))
        {
            if (Application.isPlaying)
            {
                EditorCoroutineUtility.StartCoroutine(AnimateTimeTransition(18f, 20f, 5f), this);
            }
            else
            {
                EditorUtility.DisplayDialog("Play Mode Required", "Enter Play Mode to test animated transitions.", "OK");
            }
        }

        EditorGUILayout.EndVertical();
        GUILayout.Space(10);

        // Manual overlay control
        if (nightSkyOverlay != null)
        {
            GUILayout.Label("Manual Overlay Control", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.HelpBox("Override automatic blending for testing", MessageType.Info);

            float manualAlpha = EditorGUILayout.Slider("Force Alpha",
                nightSkyOverlay.GetNightOverlayAlpha(), 0f, 1f);

            if (GUILayout.Button("Apply Manual Alpha"))
            {
                if (Application.isPlaying)
                {
                    nightSkyOverlay.SetOverlayAlpha(manualAlpha);
                }
                else
                {
                    EditorUtility.DisplayDialog("Play Mode Required", "Enter Play Mode to manually control the overlay.", "OK");
                }
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(10);
        }

        // Refresh button
        if (GUILayout.Button("Refresh Components"))
        {
            FindComponents();
        }
    }

    void SetTime(float hour)
    {
        if (Application.isPlaying && dayNightCycle != null)
        {
            dayNightCycle.SetTimeOfDay(hour);
        }
        else
        {
            testTimeOfDay = hour;
        }
    }

    string FormatTime(float hour)
    {
        int hours = Mathf.FloorToInt(hour);
        int minutes = Mathf.FloorToInt((hour - hours) * 60);
        string ampm = hours >= 12 ? "PM" : "AM";
        int displayHour = hours % 12;
        if (displayHour == 0) displayHour = 12;
        return $"{displayHour}:{minutes:D2} {ampm} ({hour:F2})";
    }

    System.Collections.IEnumerator AnimateTimeTransition(float startHour, float endHour, float duration)
    {
        if (dayNightCycle == null) yield break;

        float elapsed = 0f;
        dayNightCycle.SetTimeOfDay(startHour);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float currentTime = Mathf.Lerp(startHour, endHour, t);
            dayNightCycle.SetTimeOfDay(currentTime);
            yield return null;
        }

        dayNightCycle.SetTimeOfDay(endHour);
    }

    void OnInspectorUpdate()
    {
        // Repaint the window during play mode to show live updates
        if (Application.isPlaying)
        {
            Repaint();
        }
    }
}

/// <summary>
/// Simple coroutine utility for editor scripts
/// </summary>
public static class EditorCoroutineUtility
{
    public static EditorCoroutine StartCoroutine(System.Collections.IEnumerator routine, EditorWindow window)
    {
        EditorCoroutine coroutine = new EditorCoroutine(routine, window);
        coroutine.Start();
        return coroutine;
    }
}

public class EditorCoroutine
{
    private System.Collections.IEnumerator routine;
    private EditorWindow window;

    public EditorCoroutine(System.Collections.IEnumerator routine, EditorWindow window)
    {
        this.routine = routine;
        this.window = window;
    }

    public void Start()
    {
        EditorApplication.update += Update;
    }

    public void Stop()
    {
        EditorApplication.update -= Update;
    }

    void Update()
    {
        if (!routine.MoveNext())
        {
            Stop();
        }
    }
}
