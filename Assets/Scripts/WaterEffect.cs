using UnityEngine;
using System.Collections;

public class WaterEffect : MonoBehaviour
{
    [Header("Water Color")]
    public Color waterColor = new Color(0.1f, 0.4f, 0.7f, 0.85f);

    [Header("Wave Settings")]
    public float waveSpeed = 0.3f;
    public float waveHeight = 0.05f;

    private Vector3 startPos;
    private Material waterMat;

    void Start()
    {
        startPos = transform.position;
        SetupWaterMaterial();
    }

    void SetupWaterMaterial()
    {
        Renderer rend = GetComponent<Renderer>();
        waterMat = new Material(Shader.Find("Standard"));

        // Setup transparency
        waterMat.SetFloat("_Mode", 3);
        waterMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        waterMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        waterMat.SetInt("_ZWrite", 0);
        waterMat.DisableKeyword("_ALPHATEST_ON");
        waterMat.EnableKeyword("_ALPHABLEND_ON");
        waterMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        waterMat.renderQueue = 3000;

        // Simple blue water
        waterMat.color = waterColor;
        waterMat.SetFloat("_Metallic", 0.0f);
        waterMat.SetFloat("_Glossiness", 0.8f);

        rend.material = waterMat;
    }

    void Update()
    {
        // Gentle wave motion - just subtle up/down movement
        float wave = Mathf.Sin(Time.time * waveSpeed) * waveHeight;
        transform.position = new Vector3(startPos.x, startPos.y + wave, startPos.z);
    }

    // Public method to create splash at position (for fishing)
    public void CreateSplash(Vector3 worldPos)
    {
        StartCoroutine(SplashEffect(worldPos));
    }

    IEnumerator SplashEffect(Vector3 pos)
    {
        // Create simple expanding ring
        GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "SplashRing";
        ring.transform.position = new Vector3(pos.x, transform.position.y + 0.05f, pos.z);
        ring.transform.localScale = new Vector3(0.5f, 0.02f, 0.5f);
        Destroy(ring.GetComponent<Collider>());

        Material mat = new Material(Shader.Find("Standard"));
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.renderQueue = 3010;
        mat.color = new Color(1f, 1f, 1f, 0.6f);
        ring.GetComponent<Renderer>().material = mat;

        // Animate ring expanding
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime;
            float scale = 0.5f + t * 3f;
            ring.transform.localScale = new Vector3(scale, 0.02f, scale);

            float alpha = Mathf.Max(0, 0.6f - t * 0.6f);
            Color c = mat.color;
            c.a = alpha;
            mat.color = c;

            yield return null;
        }

        Destroy(ring);
    }
}
