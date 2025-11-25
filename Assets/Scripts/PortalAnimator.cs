using UnityEngine;

public class PortalAnimator : MonoBehaviour
{
    private Transform portalSurface;
    private Transform[] smokeParticles;
    private Transform[] runes;
    private Transform lockSymbol;

    private float rotationSpeed = 30f;
    private float pulseSpeed = 2f;
    private float smokeSpeed = 1.5f;

    private Vector3[] smokeOriginalPositions;
    private float[] smokeOffsets;

    void Start()
    {
        portalSurface = transform.Find("PortalSurface");
        lockSymbol = transform.Find("LockSymbol");

        // Find smoke particles
        smokeParticles = new Transform[5];
        smokeOriginalPositions = new Vector3[5];
        smokeOffsets = new float[5];

        int smokeIndex = 0;
        foreach (Transform child in transform)
        {
            if (child.name == "MysticalSmoke" && smokeIndex < 5)
            {
                smokeParticles[smokeIndex] = child;
                smokeOriginalPositions[smokeIndex] = child.localPosition;
                smokeOffsets[smokeIndex] = Random.Range(0f, Mathf.PI * 2f);
                smokeIndex++;
            }
        }

        // Find runes
        runes = new Transform[6];
        int runeIndex = 0;
        foreach (Transform child in transform)
        {
            if (child.name == "Rune" && runeIndex < 6)
            {
                runes[runeIndex] = child;
                runeIndex++;
            }
        }
    }

    void Update()
    {
        AnimatePortalSurface();
        AnimateSmoke();
        AnimateRunes();
        AnimateLock();
    }

    void AnimatePortalSurface()
    {
        if (portalSurface == null) return;

        // Subtle pulsing glow effect
        Renderer rend = portalSurface.GetComponent<Renderer>();
        if (rend != null && rend.material.HasProperty("_EmissionColor"))
        {
            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
            Color baseColor = rend.material.color;
            Color emissionColor = new Color(baseColor.r, baseColor.g, baseColor.b) * (0.3f + pulse * 0.4f);
            rend.material.SetColor("_EmissionColor", emissionColor);
        }

        // Subtle swirl effect via UV offset (if supported)
        // For now, just a gentle scale pulse
        float scalePulse = 1f + Mathf.Sin(Time.time * pulseSpeed * 0.5f) * 0.02f;
        portalSurface.localScale = new Vector3(
            portalSurface.localScale.x,
            portalSurface.localScale.y,
            scalePulse
        );
    }

    void AnimateSmoke()
    {
        for (int i = 0; i < smokeParticles.Length; i++)
        {
            if (smokeParticles[i] == null) continue;

            float time = Time.time * smokeSpeed + smokeOffsets[i];

            // Floating up and swirling motion
            Vector3 newPos = smokeOriginalPositions[i];
            newPos.y += Mathf.Sin(time) * 0.3f + (Time.time * 0.1f) % 2f;
            newPos.x += Mathf.Sin(time * 1.3f) * 0.15f;
            newPos.z += Mathf.Cos(time * 0.8f) * 0.1f;

            // Reset when too high
            if (newPos.y > 4.5f)
            {
                smokeOriginalPositions[i] = new Vector3(
                    smokeOriginalPositions[i].x,
                    Random.Range(0.2f, 1f),
                    smokeOriginalPositions[i].z
                );
            }

            smokeParticles[i].localPosition = newPos;

            // Fade based on height
            Renderer rend = smokeParticles[i].GetComponent<Renderer>();
            if (rend != null)
            {
                Color c = rend.material.color;
                float heightFade = Mathf.Clamp01(1f - (newPos.y - 0.5f) / 4f);
                c.a = 0.3f * heightFade;
                rend.material.color = c;
            }

            // Scale pulse
            float scalePulse = 1f + Mathf.Sin(time * 2f) * 0.2f;
            float baseScale = 0.25f;
            smokeParticles[i].localScale = Vector3.one * baseScale * scalePulse;
        }
    }

    void AnimateRunes()
    {
        for (int i = 0; i < runes.Length; i++)
        {
            if (runes[i] == null) continue;

            Renderer rend = runes[i].GetComponent<Renderer>();
            if (rend != null && rend.material.HasProperty("_EmissionColor"))
            {
                // Alternating pulse pattern
                float offset = i * Mathf.PI / 3f;
                float pulse = (Mathf.Sin(Time.time * 3f + offset) + 1f) / 2f;
                Color baseColor = rend.material.color;
                Color emissionColor = baseColor * (0.5f + pulse * 1.5f);
                rend.material.SetColor("_EmissionColor", emissionColor);
            }
        }
    }

    void AnimateLock()
    {
        if (lockSymbol == null || !lockSymbol.gameObject.activeSelf) return;

        // Gentle hovering motion
        float hover = Mathf.Sin(Time.time * 2f) * 0.05f;
        Vector3 pos = lockSymbol.localPosition;
        pos.y = 2.3f + hover;
        lockSymbol.localPosition = pos;

        // Slow rotation
        lockSymbol.Rotate(Vector3.up * 20f * Time.deltaTime);
    }
}
