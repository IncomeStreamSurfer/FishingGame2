using UnityEngine;

public static class MaterialGenerator
{
    // Realistic weathered wood plank texture for dock/boardwalk
    public static Material CreateWoodMaterial()
    {
        Material mat = new Material(Shader.Find("Standard"));
        Texture2D woodTex = GenerateRealisticWoodTexture(512, 512);
        Texture2D woodNormal = GenerateWoodNormalMap(512, 512);

        mat.mainTexture = woodTex;
        mat.SetTexture("_BumpMap", woodNormal);
        mat.SetFloat("_BumpScale", 0.8f);
        mat.SetFloat("_Glossiness", 0.15f);
        mat.SetFloat("_Metallic", 0f);
        mat.EnableKeyword("_NORMALMAP");

        return mat;
    }

    // Fabric texture for clothing with weave pattern
    public static Material CreateFabricMaterial(Color baseColor)
    {
        Material mat = new Material(Shader.Find("Standard"));
        Texture2D fabricTex = GenerateRealisticFabricTexture(256, 256, baseColor);

        mat.mainTexture = fabricTex;
        mat.SetFloat("_Glossiness", 0.15f);
        mat.SetFloat("_Metallic", 0f);

        return mat;
    }

    // Skin material with subtle variation
    public static Material CreateSkinMaterial()
    {
        Material mat = new Material(Shader.Find("Standard"));
        Texture2D skinTex = GenerateSkinTexture(128, 128);
        mat.mainTexture = skinTex;
        mat.SetFloat("_Glossiness", 0.35f);
        mat.SetFloat("_Metallic", 0f);

        return mat;
    }

    // Realistic grass texture
    public static Material CreateGrassMaterial()
    {
        Material mat = new Material(Shader.Find("Standard"));
        Texture2D grassTex = GenerateRealisticGrassTexture(512, 512);

        mat.mainTexture = grassTex;
        mat.SetFloat("_Glossiness", 0.05f);
        mat.SetFloat("_Metallic", 0f);

        return mat;
    }

    // Hat material (straw/woven look)
    public static Material CreateStrawMaterial()
    {
        Material mat = new Material(Shader.Find("Standard"));
        Texture2D strawTex = GenerateStrawTexture(256, 256);

        mat.mainTexture = strawTex;
        mat.SetFloat("_Glossiness", 0.1f);
        mat.SetFloat("_Metallic", 0f);

        return mat;
    }

    // Tree bark texture
    public static Material CreateBarkMaterial()
    {
        Material mat = new Material(Shader.Find("Standard"));
        Texture2D barkTex = GenerateBarkTexture(256, 256);
        Texture2D barkNormal = GenerateBarkNormalMap(256, 256);

        mat.mainTexture = barkTex;
        mat.SetTexture("_BumpMap", barkNormal);
        mat.SetFloat("_BumpScale", 1.0f);
        mat.SetFloat("_Glossiness", 0.1f);
        mat.SetFloat("_Metallic", 0f);
        mat.EnableKeyword("_NORMALMAP");

        return mat;
    }

    // Tree leaves material
    public static Material CreateLeavesMaterial()
    {
        Material mat = new Material(Shader.Find("Standard"));
        Texture2D leavesTex = GenerateLeavesTexture(256, 256);

        mat.mainTexture = leavesTex;
        mat.SetFloat("_Glossiness", 0.2f);
        mat.SetFloat("_Metallic", 0f);

        return mat;
    }

    static Texture2D GenerateRealisticWoodTexture(int width, int height)
    {
        Texture2D tex = new Texture2D(width, height);

        // Weathered dock wood colors
        Color[] woodColors = new Color[] {
            new Color(0.45f, 0.32f, 0.22f),  // Medium brown
            new Color(0.38f, 0.26f, 0.18f),  // Darker brown
            new Color(0.52f, 0.40f, 0.30f),  // Lighter weathered
            new Color(0.42f, 0.34f, 0.26f),  // Grey-brown weathered
            new Color(0.35f, 0.24f, 0.16f),  // Dark grain
        };

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Base wood color with large-scale variation
                float largeNoise = Mathf.PerlinNoise(x * 0.008f, y * 0.002f);
                Color baseColor = Color.Lerp(woodColors[0], woodColors[1], largeNoise);

                // Wood grain - long streaks along the plank
                float grainFreq = 0.15f;
                float grain1 = Mathf.PerlinNoise(x * 0.005f, y * grainFreq);
                float grain2 = Mathf.PerlinNoise(x * 0.003f + 50, y * grainFreq * 1.5f);
                float grainPattern = Mathf.Sin(y * 0.08f + grain1 * 8f + grain2 * 4f);
                grainPattern = Mathf.Pow((grainPattern + 1f) * 0.5f, 2);

                baseColor = Color.Lerp(baseColor, woodColors[4], grainPattern * 0.35f);

                // Knots (occasional dark spots)
                float knotNoise = Mathf.PerlinNoise(x * 0.02f + 100, y * 0.02f + 100);
                if (knotNoise > 0.75f)
                {
                    float knotStrength = (knotNoise - 0.75f) * 4f;
                    baseColor = Color.Lerp(baseColor, woodColors[4] * 0.6f, knotStrength * 0.5f);
                }

                // Weathering/aging spots
                float weatherNoise = Mathf.PerlinNoise(x * 0.03f + 200, y * 0.03f);
                if (weatherNoise > 0.6f)
                {
                    baseColor = Color.Lerp(baseColor, woodColors[3], (weatherNoise - 0.6f) * 1.5f);
                }

                // Fine detail noise
                float detail = Mathf.PerlinNoise(x * 0.1f, y * 0.1f) * 0.08f - 0.04f;
                baseColor.r = Mathf.Clamp01(baseColor.r + detail);
                baseColor.g = Mathf.Clamp01(baseColor.g + detail * 0.8f);
                baseColor.b = Mathf.Clamp01(baseColor.b + detail * 0.6f);

                // Plank gaps (dark lines between boards)
                int plankHeight = 64;
                int gapSize = 3;
                if (y % plankHeight < gapSize)
                {
                    float gapDepth = 1f - (float)(y % plankHeight) / gapSize;
                    baseColor = Color.Lerp(baseColor, new Color(0.1f, 0.08f, 0.05f), gapDepth * 0.7f);
                }

                // Nail holes
                int nailSpacing = 128;
                int nailY = plankHeight / 2;
                if ((x % nailSpacing < 4 || x % nailSpacing > nailSpacing - 4) &&
                    Mathf.Abs((y % plankHeight) - nailY) < 3)
                {
                    baseColor = Color.Lerp(baseColor, new Color(0.15f, 0.12f, 0.08f), 0.6f);
                }

                tex.SetPixel(x, y, baseColor);
            }
        }

        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Repeat;
        return tex;
    }

    static Texture2D GenerateWoodNormalMap(int width, int height)
    {
        Texture2D tex = new Texture2D(width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Wood grain bumps
                float grain1 = Mathf.PerlinNoise(x * 0.005f, y * 0.15f);
                float grain2 = Mathf.PerlinNoise(x * 0.003f + 50, y * 0.15f * 1.5f);
                float grainPattern = Mathf.Sin(y * 0.08f + grain1 * 8f + grain2 * 4f);

                float nx = grainPattern * 0.15f;
                float ny = Mathf.PerlinNoise(x * 0.05f, y * 0.05f) * 0.1f - 0.05f;

                // Plank gap normals
                int plankHeight = 64;
                if (y % plankHeight < 3)
                {
                    ny -= 0.3f;
                }

                Color normal = new Color(0.5f + nx, 0.5f + ny, 1f);
                tex.SetPixel(x, y, normal);
            }
        }

        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        return tex;
    }

    static Texture2D GenerateRealisticFabricTexture(int width, int height, Color baseColor)
    {
        Texture2D tex = new Texture2D(width, height);

        // Slightly vary the base color for natural look
        Color lightColor = baseColor * 1.15f;
        Color darkColor = baseColor * 0.85f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Canvas/cotton weave pattern
                int weaveSize = 4;
                bool warpThread = ((x / weaveSize) % 2 == 0);
                bool weftThread = ((y / weaveSize) % 2 == 0);

                float threadVariation;
                if (warpThread != weftThread)
                {
                    threadVariation = 0.05f;
                }
                else
                {
                    threadVariation = -0.03f;
                }

                // Thread texture within weave
                float threadDetail = Mathf.PerlinNoise(x * 0.3f, y * 0.3f) * 0.06f - 0.03f;

                // Fabric folds/wrinkles (larger scale variation)
                float foldNoise = Mathf.PerlinNoise(x * 0.02f, y * 0.02f) * 0.1f - 0.05f;

                Color color = baseColor;
                color.r = Mathf.Clamp01(color.r + threadVariation + threadDetail + foldNoise);
                color.g = Mathf.Clamp01(color.g + threadVariation + threadDetail + foldNoise);
                color.b = Mathf.Clamp01(color.b + threadVariation + threadDetail + foldNoise);

                tex.SetPixel(x, y, color);
            }
        }

        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Repeat;
        return tex;
    }

    static Texture2D GenerateSkinTexture(int width, int height)
    {
        Texture2D tex = new Texture2D(width, height);
        Color baseSkin = new Color(0.92f, 0.72f, 0.60f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Subtle skin variation
                float noise1 = Mathf.PerlinNoise(x * 0.05f, y * 0.05f) * 0.08f - 0.04f;
                float noise2 = Mathf.PerlinNoise(x * 0.1f + 50, y * 0.1f + 50) * 0.04f - 0.02f;

                Color color = baseSkin;
                color.r = Mathf.Clamp01(color.r + noise1 + noise2);
                color.g = Mathf.Clamp01(color.g + noise1 * 0.7f + noise2);
                color.b = Mathf.Clamp01(color.b + noise1 * 0.5f + noise2);

                tex.SetPixel(x, y, color);
            }
        }

        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        return tex;
    }

    static Texture2D GenerateRealisticGrassTexture(int width, int height)
    {
        Texture2D tex = new Texture2D(width, height);

        Color[] grassColors = new Color[] {
            new Color(0.18f, 0.38f, 0.12f),  // Dark grass
            new Color(0.28f, 0.52f, 0.18f),  // Medium grass
            new Color(0.35f, 0.58f, 0.22f),  // Light grass
            new Color(0.45f, 0.42f, 0.25f),  // Dried/yellow grass
            new Color(0.32f, 0.26f, 0.18f),  // Dirt/soil
        };

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Base grass with large variation
                float baseNoise = Mathf.PerlinNoise(x * 0.01f, y * 0.01f);
                Color baseColor = Color.Lerp(grassColors[0], grassColors[1], baseNoise);

                // Grass blade streaks (vertical-ish)
                float bladeNoise = Mathf.PerlinNoise(x * 0.08f, y * 0.02f);
                if (bladeNoise > 0.5f)
                {
                    baseColor = Color.Lerp(baseColor, grassColors[2], (bladeNoise - 0.5f) * 1.5f);
                }

                // Clumps of different grass
                float clumpNoise = Mathf.PerlinNoise(x * 0.025f + 100, y * 0.025f + 100);
                if (clumpNoise > 0.65f)
                {
                    baseColor = Color.Lerp(baseColor, grassColors[3], (clumpNoise - 0.65f) * 2f);
                }

                // Dirt patches
                float dirtNoise = Mathf.PerlinNoise(x * 0.015f + 200, y * 0.015f + 200);
                if (dirtNoise > 0.72f)
                {
                    baseColor = Color.Lerp(baseColor, grassColors[4], (dirtNoise - 0.72f) * 3f);
                }

                // Fine detail
                float detail = Mathf.PerlinNoise(x * 0.15f, y * 0.15f) * 0.1f - 0.05f;
                baseColor.r = Mathf.Clamp01(baseColor.r + detail * 0.5f);
                baseColor.g = Mathf.Clamp01(baseColor.g + detail);
                baseColor.b = Mathf.Clamp01(baseColor.b + detail * 0.3f);

                tex.SetPixel(x, y, baseColor);
            }
        }

        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Repeat;
        return tex;
    }

    static Texture2D GenerateStrawTexture(int width, int height)
    {
        Texture2D tex = new Texture2D(width, height);

        Color[] strawColors = new Color[] {
            new Color(0.85f, 0.72f, 0.48f),  // Light straw
            new Color(0.75f, 0.60f, 0.38f),  // Medium straw
            new Color(0.62f, 0.48f, 0.28f),  // Dark straw
            new Color(0.55f, 0.42f, 0.22f),  // Shadow
        };

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Woven pattern - interlocking horizontal and vertical strands
                int weaveX = (x / 6) % 2;
                int weaveY = (y / 3) % 2;

                Color color;
                if ((weaveX + weaveY) % 2 == 0)
                {
                    color = Color.Lerp(strawColors[0], strawColors[1],
                        Mathf.PerlinNoise(x * 0.1f, y * 0.1f) * 0.5f);
                }
                else
                {
                    color = Color.Lerp(strawColors[1], strawColors[2],
                        Mathf.PerlinNoise(x * 0.1f + 50, y * 0.1f + 50) * 0.5f);
                }

                // Individual strand variation
                float strandNoise = Mathf.PerlinNoise(x * 0.3f, y * 0.5f) * 0.15f - 0.075f;
                color.r = Mathf.Clamp01(color.r + strandNoise);
                color.g = Mathf.Clamp01(color.g + strandNoise * 0.8f);
                color.b = Mathf.Clamp01(color.b + strandNoise * 0.5f);

                tex.SetPixel(x, y, color);
            }
        }

        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Repeat;
        return tex;
    }

    static Texture2D GenerateBarkTexture(int width, int height)
    {
        Texture2D tex = new Texture2D(width, height);

        Color[] barkColors = new Color[] {
            new Color(0.28f, 0.20f, 0.14f),  // Dark bark
            new Color(0.38f, 0.28f, 0.18f),  // Medium bark
            new Color(0.45f, 0.35f, 0.25f),  // Light bark
            new Color(0.22f, 0.15f, 0.10f),  // Deep crevice
        };

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Vertical bark ridges
                float ridgeNoise = Mathf.PerlinNoise(x * 0.08f, y * 0.01f);
                float ridge = Mathf.Sin(x * 0.15f + ridgeNoise * 5f);
                ridge = (ridge + 1f) * 0.5f;

                Color baseColor = Color.Lerp(barkColors[3], barkColors[1], ridge);

                // Large scale variation
                float largeNoise = Mathf.PerlinNoise(x * 0.02f + 100, y * 0.02f);
                baseColor = Color.Lerp(baseColor, barkColors[2], largeNoise * 0.3f);

                // Horizontal cracks
                float crackNoise = Mathf.PerlinNoise(x * 0.01f, y * 0.05f + 50);
                if (crackNoise > 0.7f)
                {
                    baseColor = Color.Lerp(baseColor, barkColors[3], (crackNoise - 0.7f) * 2f);
                }

                // Fine detail
                float detail = Mathf.PerlinNoise(x * 0.2f, y * 0.2f) * 0.1f - 0.05f;
                baseColor.r = Mathf.Clamp01(baseColor.r + detail);
                baseColor.g = Mathf.Clamp01(baseColor.g + detail * 0.8f);
                baseColor.b = Mathf.Clamp01(baseColor.b + detail * 0.6f);

                tex.SetPixel(x, y, baseColor);
            }
        }

        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Repeat;
        return tex;
    }

    static Texture2D GenerateBarkNormalMap(int width, int height)
    {
        Texture2D tex = new Texture2D(width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Vertical ridge normals
                float ridgeNoise = Mathf.PerlinNoise(x * 0.08f, y * 0.01f);
                float ridge = Mathf.Cos(x * 0.15f + ridgeNoise * 5f);

                float nx = ridge * 0.4f;
                float ny = Mathf.PerlinNoise(x * 0.05f, y * 0.05f) * 0.2f - 0.1f;

                Color normal = new Color(0.5f + nx, 0.5f + ny, 1f);
                tex.SetPixel(x, y, normal);
            }
        }

        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        return tex;
    }

    static Texture2D GenerateLeavesTexture(int width, int height)
    {
        Texture2D tex = new Texture2D(width, height);

        Color[] leafColors = new Color[] {
            new Color(0.15f, 0.35f, 0.12f),  // Dark green
            new Color(0.22f, 0.48f, 0.18f),  // Medium green
            new Color(0.30f, 0.55f, 0.22f),  // Light green
            new Color(0.35f, 0.58f, 0.28f),  // Highlight
        };

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Leaf cluster pattern
                float noise1 = Mathf.PerlinNoise(x * 0.04f, y * 0.04f);
                float noise2 = Mathf.PerlinNoise(x * 0.08f + 50, y * 0.08f + 50);

                Color baseColor = Color.Lerp(leafColors[0], leafColors[1], noise1);
                baseColor = Color.Lerp(baseColor, leafColors[2], noise2 * 0.5f);

                // Highlight spots (sun hitting leaves)
                float highlight = Mathf.PerlinNoise(x * 0.1f + 100, y * 0.1f + 100);
                if (highlight > 0.7f)
                {
                    baseColor = Color.Lerp(baseColor, leafColors[3], (highlight - 0.7f) * 2f);
                }

                // Shadow spots
                float shadow = Mathf.PerlinNoise(x * 0.06f + 150, y * 0.06f + 150);
                if (shadow > 0.75f)
                {
                    baseColor = Color.Lerp(baseColor, leafColors[0] * 0.7f, (shadow - 0.75f) * 2f);
                }

                tex.SetPixel(x, y, baseColor);
            }
        }

        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Repeat;
        return tex;
    }
}
