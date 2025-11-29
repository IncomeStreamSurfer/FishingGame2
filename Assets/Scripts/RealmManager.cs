using UnityEngine;
using System.Collections;

public enum RealmType
{
    TropicalIsland,
    IceRealm,
    JungleRealm,
    VolcanicRealm,
    VoidRealm
}

public class RealmManager : MonoBehaviour
{
    public static RealmManager Instance { get; private set; }

    public RealmType CurrentRealm { get; private set; } = RealmType.TropicalIsland;

    // Realm positions (offset from origin)
    public static readonly Vector3 TropicalIslandOrigin = Vector3.zero;
    public static readonly Vector3 IceRealmOrigin = new Vector3(500f, 0f, 0f);
    public static readonly Vector3 JungleRealmOrigin = new Vector3(1000f, 0f, 0f);
    public static readonly Vector3 VolcanicRealmOrigin = new Vector3(1500f, 0f, 0f);
    public static readonly Vector3 VoidRealmOrigin = new Vector3(2000f, 0f, 0f);

    // Transition state
    private bool isTransitioning = false;
    private float transitionAlpha = 0f;
    private Texture2D fadeTexture;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Create fade texture
        fadeTexture = new Texture2D(1, 1);
        fadeTexture.SetPixel(0, 0, Color.black);
        fadeTexture.Apply();
    }

    public Vector3 GetRealmOrigin(RealmType realm)
    {
        switch (realm)
        {
            case RealmType.TropicalIsland: return TropicalIslandOrigin;
            case RealmType.IceRealm: return IceRealmOrigin;
            case RealmType.JungleRealm: return JungleRealmOrigin;
            case RealmType.VolcanicRealm: return VolcanicRealmOrigin;
            case RealmType.VoidRealm: return VoidRealmOrigin;
            default: return TropicalIslandOrigin;
        }
    }

    public void TravelToRealm(RealmType targetRealm, Vector3 spawnOffset)
    {
        if (isTransitioning) return;
        StartCoroutine(RealmTransition(targetRealm, spawnOffset));
    }

    IEnumerator RealmTransition(RealmType targetRealm, Vector3 spawnOffset)
    {
        isTransitioning = true;
        GameObject player = GameObject.Find("Player");

        // Fade to black
        float fadeTime = 0.5f;
        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            transitionAlpha = Mathf.Clamp01(elapsed / fadeTime);
            yield return null;
        }
        transitionAlpha = 1f;

        // Teleport player
        Vector3 realmOrigin = GetRealmOrigin(targetRealm);
        Vector3 newPosition = realmOrigin + spawnOffset;

        if (player != null)
        {
            // Disable physics temporarily
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
            }

            player.transform.position = newPosition;
            player.transform.rotation = Quaternion.identity;

            // Reset camera
            CameraController cam = Camera.main?.GetComponent<CameraController>();
            if (cam != null)
            {
                cam.transform.position = newPosition + new Vector3(0, 4f, -8f);
            }
        }

        CurrentRealm = targetRealm;
        Debug.Log($"Arrived at {targetRealm}!");

        // Small delay at black
        yield return new WaitForSeconds(0.3f);

        // Fade back in
        elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            transitionAlpha = 1f - Mathf.Clamp01(elapsed / fadeTime);
            yield return null;
        }
        transitionAlpha = 0f;

        isTransitioning = false;
    }

    void OnGUI()
    {
        if (transitionAlpha > 0f)
        {
            GUI.color = new Color(0, 0, 0, transitionAlpha);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), fadeTexture);
            GUI.color = Color.white;
        }

        // Show current realm in corner (for testing)
        if (MainMenu.GameStarted && CurrentRealm != RealmType.TropicalIsland)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 14;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = GetRealmColor(CurrentRealm);
            GUI.Label(new Rect(10, 10, 200, 25), $"Current Realm: {CurrentRealm}", style);
        }
    }

    Color GetRealmColor(RealmType realm)
    {
        switch (realm)
        {
            case RealmType.IceRealm: return new Color(0.6f, 0.85f, 1f);
            case RealmType.JungleRealm: return new Color(0.2f, 0.8f, 0.3f);
            case RealmType.VolcanicRealm: return new Color(1f, 0.4f, 0.2f);
            case RealmType.VoidRealm: return new Color(0.6f, 0.3f, 0.9f);
            default: return Color.white;
        }
    }

    public bool IsTransitioning()
    {
        return isTransitioning;
    }

    void OnDestroy()
    {
        if (fadeTexture != null)
        {
            Destroy(fadeTexture);
        }
    }
}
