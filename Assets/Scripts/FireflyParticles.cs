using UnityEngine;

/// <summary>
/// Creates ambient firefly particles floating in the jungle
/// Glowing yellow-green dots that float and pulse
/// </summary>
public class FireflyParticles : MonoBehaviour
{
    private GameObject[] fireflies;
    private Vector3[] velocities;
    private float[] phases;
    private float[] pauseTimers;
    private float[] speedMultipliers;
    private Vector3[] targetDirections;
    private float[] directionChangeTimers;

    private int fireflyCount = 40;
    private float areaSize = 80f;

    private Material glowMat;

    void Start()
    {
        glowMat = new Material(Shader.Find("Standard"));
        glowMat.color = new Color(0.8f, 1f, 0.3f);
        glowMat.EnableKeyword("_EMISSION");
        glowMat.SetColor("_EmissionColor", new Color(0.8f, 1f, 0.3f) * 2f);

        fireflies = new GameObject[fireflyCount];
        velocities = new Vector3[fireflyCount];
        phases = new float[fireflyCount];
        pauseTimers = new float[fireflyCount];
        speedMultipliers = new float[fireflyCount];
        targetDirections = new Vector3[fireflyCount];
        directionChangeTimers = new float[fireflyCount];

        for (int i = 0; i < fireflyCount; i++)
        {
            CreateFirefly(i);
        }
    }

    void CreateFirefly(int index)
    {
        GameObject firefly = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        firefly.name = "Firefly_" + index;
        firefly.transform.SetParent(transform);
        firefly.transform.localPosition = new Vector3(
            Random.Range(-areaSize / 2, areaSize / 2),
            Random.Range(2f, 8f),
            Random.Range(-areaSize / 2, areaSize / 2)
        );
        // Much smaller fireflies - reduced by 50-60%
        firefly.transform.localScale = Vector3.one * Random.Range(0.03f, 0.04f);
        firefly.GetComponent<Renderer>().sharedMaterial = glowMat;
        Object.Destroy(firefly.GetComponent<Collider>());

        // Add point light for glow effect (smaller range for smaller fireflies)
        Light light = firefly.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(0.8f, 1f, 0.3f);
        light.intensity = 0.2f;
        light.range = 1.5f;

        fireflies[index] = firefly;

        // Initial random velocity
        velocities[index] = new Vector3(
            Random.Range(-0.5f, 0.5f),
            Random.Range(-0.3f, 0.3f),
            Random.Range(-0.5f, 0.5f)
        );
        phases[index] = Random.Range(0f, Mathf.PI * 2f);
        pauseTimers[index] = Random.Range(0f, 3f);
        speedMultipliers[index] = Random.Range(0.4f, 1.2f);
        targetDirections[index] = Random.onUnitSphere;
        directionChangeTimers[index] = Random.Range(0f, 2f);
    }

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        // Only update when player is in Jungle Realm
        RealmManager rm = FindObjectOfType<RealmManager>();
        bool inJungle = false;

        if (rm != null)
        {
            inJungle = rm.CurrentRealm == RealmType.JungleRealm;
        }
        else
        {
            // Fallback: check player X position (jungle starts at X > 900)
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                inJungle = player.transform.position.x > 900f;
            }
        }

        // Hide fireflies when not in jungle
        if (!inJungle)
        {
            for (int i = 0; i < fireflyCount; i++)
            {
                if (fireflies[i] != null)
                {
                    fireflies[i].SetActive(false);
                }
            }
            return;
        }

        float time = Time.time;

        for (int i = 0; i < fireflyCount; i++)
        {
            if (fireflies[i] == null) continue;

            // Ensure firefly is visible in jungle
            if (!fireflies[i].activeSelf)
            {
                fireflies[i].SetActive(true);
            }

            Vector3 pos = fireflies[i].transform.localPosition;

            // Handle pause behavior (fireflies occasionally stop)
            if (pauseTimers[i] > 0f)
            {
                pauseTimers[i] -= Time.deltaTime;
                // During pause, only gentle bobbing
                pos.y += Mathf.Sin(time * 2f + phases[i]) * 0.01f;
            }
            else
            {
                // Check if we need a new direction
                directionChangeTimers[i] -= Time.deltaTime;
                if (directionChangeTimers[i] <= 0f)
                {
                    // Pick new target direction
                    targetDirections[i] = new Vector3(
                        Random.Range(-1f, 1f),
                        Random.Range(-0.5f, 0.5f),
                        Random.Range(-1f, 1f)
                    ).normalized;

                    // Occasional speed bursts or slow drifts
                    speedMultipliers[i] = Random.Range(0.3f, 1.5f);

                    // Time until next direction change
                    directionChangeTimers[i] = Random.Range(1.5f, 4f);

                    // 20% chance to pause after direction change
                    if (Random.value < 0.2f)
                    {
                        pauseTimers[i] = Random.Range(0.5f, 2f);
                    }
                }

                // Smoothly steer toward target direction
                velocities[i] = Vector3.Lerp(
                    velocities[i],
                    targetDirections[i] * speedMultipliers[i],
                    Time.deltaTime * 1.5f
                );

                // Add gentle bobbing motion (sine wave)
                float bobAmount = Mathf.Sin(time * 1.5f + phases[i]) * 0.015f;
                velocities[i].y += bobAmount;

                // Add subtle random flutter
                velocities[i] += new Vector3(
                    Random.Range(-0.1f, 0.1f) * Time.deltaTime,
                    Random.Range(-0.05f, 0.05f) * Time.deltaTime,
                    Random.Range(-0.1f, 0.1f) * Time.deltaTime
                );

                // Clamp velocity to realistic firefly speeds
                velocities[i] = Vector3.ClampMagnitude(velocities[i], 1.2f);

                // Apply movement
                pos += velocities[i] * Time.deltaTime;
            }

            // Keep in bounds with smooth direction reversal
            if (Mathf.Abs(pos.x) > areaSize / 2)
            {
                velocities[i].x *= -0.8f;
                targetDirections[i].x *= -1f;
            }
            if (Mathf.Abs(pos.z) > areaSize / 2)
            {
                velocities[i].z *= -0.8f;
                targetDirections[i].z *= -1f;
            }
            if (pos.y < 1f || pos.y > 10f)
            {
                velocities[i].y *= -0.8f;
                targetDirections[i].y *= -1f;
            }

            pos.x = Mathf.Clamp(pos.x, -areaSize / 2, areaSize / 2);
            pos.y = Mathf.Clamp(pos.y, 1f, 10f);
            pos.z = Mathf.Clamp(pos.z, -areaSize / 2, areaSize / 2);

            fireflies[i].transform.localPosition = pos;

            // Pulsing glow (natural firefly blink)
            float pulse = 0.5f + 0.5f * Mathf.Sin(time * 2.5f + phases[i]);

            Light light = fireflies[i].GetComponent<Light>();
            if (light != null)
            {
                light.intensity = 0.05f + pulse * 0.25f;
            }

            // Subtle scale pulse to match glow
            float baseScale = Random.Range(0.03f, 0.04f);
            float scale = baseScale * (0.9f + pulse * 0.2f);
            fireflies[i].transform.localScale = Vector3.one * scale;
        }
    }
}
