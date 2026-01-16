using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages player model variations like Rust - randomizes skin tone and body proportions
/// when a new game is started. Each player gets a unique, persistent appearance.
/// </summary>
public class PlayerModelVariations : MonoBehaviour
{
    public static PlayerModelVariations Instance { get; private set; }

    // Realistic human skin tone palette (8 tones from light to dark)
    public static readonly Color[] SkinTones = new Color[]
    {
        new Color(1.00f, 0.87f, 0.77f),    // Very light / pale
        new Color(0.96f, 0.80f, 0.69f),    // Light
        new Color(0.87f, 0.72f, 0.58f),    // Light-medium / olive
        new Color(0.80f, 0.64f, 0.49f),    // Medium
        new Color(0.70f, 0.49f, 0.35f),    // Medium-tan
        new Color(0.55f, 0.38f, 0.26f),    // Tan / brown
        new Color(0.42f, 0.28f, 0.18f),    // Dark brown
        new Color(0.30f, 0.20f, 0.13f)     // Very dark
    };

    // Body scale variation ranges (subtle to keep human proportions)
    [System.Serializable]
    public struct BodyProportions
    {
        public float torsoScaleX;   // Width (0.9 - 1.1)
        public float torsoScaleY;   // Height (0.95 - 1.05)
        public float torsoScaleZ;   // Depth (0.9 - 1.1)
        public float armScale;      // Overall arm size (0.9 - 1.1)
        public float legScaleX;     // Leg width (0.9 - 1.1)
        public float legScaleY;     // Leg length (0.95 - 1.05)
        public float headScale;     // Head size (0.95 - 1.05)
        public float hipsScale;     // Hip width (0.9 - 1.1)
    }

    // Current player appearance (persists during session)
    private int currentSkinToneIndex = -1;
    private Color currentSkinColor;
    private BodyProportions currentProportions;
    private bool hasBeenRandomized = false;

    // References to body parts
    private Transform torso;
    private Transform head;
    private Transform hips;
    private Transform leftArm;
    private Transform rightArm;
    private List<Transform> legParts = new List<Transform>();
    private List<Renderer> allSkinRenderers = new List<Renderer>();

    // Original scales for restoration
    private Dictionary<Transform, Vector3> originalScales = new Dictionary<Transform, Vector3>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    void Start()
    {
        FindBodyParts();
        StoreOriginalScales();
    }

    void FindBodyParts()
    {
        // Find main body parts
        torso = transform.Find("Torso");
        head = transform.Find("Head");
        hips = transform.Find("Hips");
        leftArm = transform.Find("LeftArm");
        rightArm = transform.Find("RightArm");

        // Find all leg parts
        legParts.Clear();
        foreach (Transform child in transform)
        {
            if (child.name.Contains("Leg") || child.name.Contains("Foot") || child.name.Contains("Thigh") || child.name.Contains("Calf"))
            {
                legParts.Add(child);
            }
        }

        // Collect all renderers that show skin
        allSkinRenderers.Clear();

        if (torso != null)
        {
            Renderer r = torso.GetComponent<Renderer>();
            if (r != null) allSkinRenderers.Add(r);
        }
        if (head != null)
        {
            Renderer r = head.GetComponent<Renderer>();
            if (r != null) allSkinRenderers.Add(r);
        }
        if (hips != null)
        {
            Renderer r = hips.GetComponent<Renderer>();
            if (r != null) allSkinRenderers.Add(r);
        }
        if (leftArm != null)
        {
            Renderer r = leftArm.GetComponent<Renderer>();
            if (r != null) allSkinRenderers.Add(r);
            // Check for child arm parts
            foreach (Transform child in leftArm)
            {
                Renderer childR = child.GetComponent<Renderer>();
                if (childR != null) allSkinRenderers.Add(childR);
            }
        }
        if (rightArm != null)
        {
            Renderer r = rightArm.GetComponent<Renderer>();
            if (r != null) allSkinRenderers.Add(r);
            // Check for child arm parts
            foreach (Transform child in rightArm)
            {
                Renderer childR = child.GetComponent<Renderer>();
                if (childR != null) allSkinRenderers.Add(childR);
            }
        }

        // Add leg renderers
        foreach (Transform leg in legParts)
        {
            Renderer r = leg.GetComponent<Renderer>();
            if (r != null) allSkinRenderers.Add(r);
        }

        // Look for hands
        Transform leftHand = transform.Find("LeftHand");
        Transform rightHand = transform.Find("RightHand");
        if (leftHand != null)
        {
            Renderer r = leftHand.GetComponent<Renderer>();
            if (r != null) allSkinRenderers.Add(r);
        }
        if (rightHand != null)
        {
            Renderer r = rightHand.GetComponent<Renderer>();
            if (r != null) allSkinRenderers.Add(r);
        }
    }

    void StoreOriginalScales()
    {
        originalScales.Clear();

        if (torso != null) originalScales[torso] = torso.localScale;
        if (head != null) originalScales[head] = head.localScale;
        if (hips != null) originalScales[hips] = hips.localScale;
        if (leftArm != null) originalScales[leftArm] = leftArm.localScale;
        if (rightArm != null) originalScales[rightArm] = rightArm.localScale;

        foreach (Transform leg in legParts)
        {
            if (leg != null) originalScales[leg] = leg.localScale;
        }
    }

    /// <summary>
    /// Randomizes the player's appearance - called when starting a new game
    /// </summary>
    public void RandomizeAppearance()
    {
        // Ensure we have body part references
        if (torso == null)
        {
            FindBodyParts();
            StoreOriginalScales();
        }

        // Randomize skin tone
        currentSkinToneIndex = Random.Range(0, SkinTones.Length);
        currentSkinColor = SkinTones[currentSkinToneIndex];

        // Randomize body proportions (subtle variations)
        currentProportions = GenerateRandomProportions();

        // Apply the variations
        ApplySkinTone();
        ApplyBodyProportions();

        hasBeenRandomized = true;

        Debug.Log($"Player appearance randomized: Skin tone {currentSkinToneIndex + 1}/{SkinTones.Length}, " +
                  $"Torso scale ({currentProportions.torsoScaleX:F2}, {currentProportions.torsoScaleY:F2}, {currentProportions.torsoScaleZ:F2})");
    }

    BodyProportions GenerateRandomProportions()
    {
        return new BodyProportions
        {
            // Subtle variations to maintain human proportions
            torsoScaleX = Random.Range(0.90f, 1.10f),  // Width variation
            torsoScaleY = Random.Range(0.95f, 1.05f),  // Height variation (more subtle)
            torsoScaleZ = Random.Range(0.90f, 1.10f),  // Depth variation
            armScale = Random.Range(0.90f, 1.10f),
            legScaleX = Random.Range(0.90f, 1.10f),
            legScaleY = Random.Range(0.95f, 1.05f),
            headScale = Random.Range(0.95f, 1.05f),   // Very subtle head variation
            hipsScale = Random.Range(0.90f, 1.10f)
        };
    }

    void ApplySkinTone()
    {
        foreach (Renderer renderer in allSkinRenderers)
        {
            if (renderer != null && renderer.material != null)
            {
                // Only apply to parts that should show skin
                // Check if it's not covered by clothing
                string partName = renderer.gameObject.name.ToLower();

                // Always apply to head and hands
                if (partName.Contains("head") || partName.Contains("hand"))
                {
                    renderer.material.color = currentSkinColor;
                }
                // For body parts, we apply the base skin color
                // The clothing system will override this when clothes are equipped
                else
                {
                    renderer.material.color = currentSkinColor;
                }
            }
        }

        // Update PlayerClothingVisuals with the new skin color
        if (PlayerClothingVisuals.Instance != null)
        {
            // The clothing visuals system stores the skin color and uses it when showing bare skin
            UpdateClothingVisualsSkinColor();
        }
    }

    void UpdateClothingVisualsSkinColor()
    {
        // Use reflection or a public method to update the skin color in PlayerClothingVisuals
        // Since PlayerClothingVisuals has a skinColor field, we need to update it
        var clothingVisuals = PlayerClothingVisuals.Instance;
        if (clothingVisuals != null)
        {
            // Access the private skinColor field via reflection
            var skinColorField = typeof(PlayerClothingVisuals).GetField("skinColor",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (skinColorField != null)
            {
                skinColorField.SetValue(clothingVisuals, currentSkinColor);
            }

            // Also update skinMaterial if it exists
            var skinMaterialField = typeof(PlayerClothingVisuals).GetField("skinMaterial",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (skinMaterialField != null)
            {
                Material mat = skinMaterialField.GetValue(clothingVisuals) as Material;
                if (mat != null)
                {
                    mat.color = currentSkinColor;
                }
            }
        }
    }

    void ApplyBodyProportions()
    {
        // Apply torso scaling
        if (torso != null && originalScales.ContainsKey(torso))
        {
            Vector3 original = originalScales[torso];
            torso.localScale = new Vector3(
                original.x * currentProportions.torsoScaleX,
                original.y * currentProportions.torsoScaleY,
                original.z * currentProportions.torsoScaleZ
            );
        }

        // Apply head scaling (very subtle)
        if (head != null && originalScales.ContainsKey(head))
        {
            Vector3 original = originalScales[head];
            head.localScale = original * currentProportions.headScale;
        }

        // Apply hips scaling
        if (hips != null && originalScales.ContainsKey(hips))
        {
            Vector3 original = originalScales[hips];
            hips.localScale = new Vector3(
                original.x * currentProportions.hipsScale,
                original.y,
                original.z * currentProportions.hipsScale
            );
        }

        // Apply arm scaling
        if (leftArm != null && originalScales.ContainsKey(leftArm))
        {
            Vector3 original = originalScales[leftArm];
            leftArm.localScale = original * currentProportions.armScale;
        }
        if (rightArm != null && originalScales.ContainsKey(rightArm))
        {
            Vector3 original = originalScales[rightArm];
            rightArm.localScale = original * currentProportions.armScale;
        }

        // Apply leg scaling
        foreach (Transform leg in legParts)
        {
            if (leg != null && originalScales.ContainsKey(leg))
            {
                Vector3 original = originalScales[leg];
                leg.localScale = new Vector3(
                    original.x * currentProportions.legScaleX,
                    original.y * currentProportions.legScaleY,
                    original.z * currentProportions.legScaleX
                );
            }
        }
    }

    /// <summary>
    /// Resets the player to default appearance
    /// </summary>
    public void ResetToDefault()
    {
        // Reset all scales to original
        foreach (var kvp in originalScales)
        {
            if (kvp.Key != null)
            {
                kvp.Key.localScale = kvp.Value;
            }
        }

        // Reset skin color to default
        Color defaultSkin = new Color(0.85f, 0.7f, 0.55f);
        foreach (Renderer renderer in allSkinRenderers)
        {
            if (renderer != null && renderer.material != null)
            {
                renderer.material.color = defaultSkin;
            }
        }

        hasBeenRandomized = false;
    }

    // Getters for current appearance
    public Color GetCurrentSkinColor() => currentSkinColor;
    public int GetSkinToneIndex() => currentSkinToneIndex;
    public BodyProportions GetCurrentProportions() => currentProportions;
    public bool HasBeenRandomized() => hasBeenRandomized;

    /// <summary>
    /// Sets a specific skin tone by index (0-7)
    /// </summary>
    public void SetSkinTone(int index)
    {
        if (index >= 0 && index < SkinTones.Length)
        {
            currentSkinToneIndex = index;
            currentSkinColor = SkinTones[index];
            ApplySkinTone();
        }
    }

    /// <summary>
    /// Sets specific body proportions
    /// </summary>
    public void SetBodyProportions(BodyProportions proportions)
    {
        currentProportions = proportions;
        ApplyBodyProportions();
    }
}
