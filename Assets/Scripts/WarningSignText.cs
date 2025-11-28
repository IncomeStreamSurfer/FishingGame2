using UnityEngine;

/// <summary>
/// Displays "CAUTION. FAST CURRENT" text on warning signs
/// </summary>
public class WarningSignText : MonoBehaviour
{
    private Texture2D signTexture;

    void Start()
    {
        CreateSignTexture();
        ApplyTexture();
    }

    void CreateSignTexture()
    {
        // Create a texture with warning text
        int width = 128;
        int height = 80;
        signTexture = new Texture2D(width, height);

        // Yellow background
        Color yellow = new Color(0.95f, 0.85f, 0.3f);
        Color black = new Color(0.1f, 0.1f, 0.1f);
        Color red = new Color(0.9f, 0.2f, 0.2f);

        // Fill with yellow
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                signTexture.SetPixel(x, y, yellow);
            }
        }

        // Add red border
        int borderWidth = 4;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (x < borderWidth || x >= width - borderWidth ||
                    y < borderWidth || y >= height - borderWidth)
                {
                    signTexture.SetPixel(x, y, red);
                }
            }
        }

        // Draw simple "CAUTION" text (pixel art style)
        DrawText("CAUTION", 20, 50, black);
        DrawText("FAST", 35, 30, black);
        DrawText("CURRENT", 18, 12, black);

        signTexture.Apply();
        signTexture.filterMode = FilterMode.Point;
    }

    void DrawText(string text, int startX, int startY, Color color)
    {
        // Simple 5x7 pixel font for basic letters
        int charWidth = 6;
        int charHeight = 7;

        for (int i = 0; i < text.Length; i++)
        {
            DrawChar(text[i], startX + i * charWidth, startY, color);
        }
    }

    void DrawChar(char c, int x, int y, Color color)
    {
        // Very simplified pixel representations of letters
        bool[,] pixels = GetCharPixels(c);
        if (pixels == null) return;

        for (int py = 0; py < 7; py++)
        {
            for (int px = 0; px < 5; px++)
            {
                if (pixels[py, px])
                {
                    int texX = x + px;
                    int texY = y + (6 - py); // Flip Y
                    if (texX >= 0 && texX < signTexture.width &&
                        texY >= 0 && texY < signTexture.height)
                    {
                        signTexture.SetPixel(texX, texY, color);
                    }
                }
            }
        }
    }

    bool[,] GetCharPixels(char c)
    {
        // 5x7 pixel font patterns for common letters
        switch (char.ToUpper(c))
        {
            case 'C':
                return new bool[,] {
                    {false,true,true,true,false},
                    {true,false,false,false,true},
                    {true,false,false,false,false},
                    {true,false,false,false,false},
                    {true,false,false,false,false},
                    {true,false,false,false,true},
                    {false,true,true,true,false}
                };
            case 'A':
                return new bool[,] {
                    {false,true,true,true,false},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {true,true,true,true,true},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {true,false,false,false,true}
                };
            case 'U':
                return new bool[,] {
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {false,true,true,true,false}
                };
            case 'T':
                return new bool[,] {
                    {true,true,true,true,true},
                    {false,false,true,false,false},
                    {false,false,true,false,false},
                    {false,false,true,false,false},
                    {false,false,true,false,false},
                    {false,false,true,false,false},
                    {false,false,true,false,false}
                };
            case 'I':
                return new bool[,] {
                    {true,true,true,true,true},
                    {false,false,true,false,false},
                    {false,false,true,false,false},
                    {false,false,true,false,false},
                    {false,false,true,false,false},
                    {false,false,true,false,false},
                    {true,true,true,true,true}
                };
            case 'O':
                return new bool[,] {
                    {false,true,true,true,false},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {false,true,true,true,false}
                };
            case 'N':
                return new bool[,] {
                    {true,false,false,false,true},
                    {true,true,false,false,true},
                    {true,false,true,false,true},
                    {true,false,false,true,true},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {true,false,false,false,true}
                };
            case 'F':
                return new bool[,] {
                    {true,true,true,true,true},
                    {true,false,false,false,false},
                    {true,false,false,false,false},
                    {true,true,true,true,false},
                    {true,false,false,false,false},
                    {true,false,false,false,false},
                    {true,false,false,false,false}
                };
            case 'S':
                return new bool[,] {
                    {false,true,true,true,true},
                    {true,false,false,false,false},
                    {true,false,false,false,false},
                    {false,true,true,true,false},
                    {false,false,false,false,true},
                    {false,false,false,false,true},
                    {true,true,true,true,false}
                };
            case 'R':
                return new bool[,] {
                    {true,true,true,true,false},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {true,true,true,true,false},
                    {true,false,true,false,false},
                    {true,false,false,true,false},
                    {true,false,false,false,true}
                };
            case 'E':
                return new bool[,] {
                    {true,true,true,true,true},
                    {true,false,false,false,false},
                    {true,false,false,false,false},
                    {true,true,true,true,false},
                    {true,false,false,false,false},
                    {true,false,false,false,false},
                    {true,true,true,true,true}
                };
            case ' ':
                return new bool[,] {
                    {false,false,false,false,false},
                    {false,false,false,false,false},
                    {false,false,false,false,false},
                    {false,false,false,false,false},
                    {false,false,false,false,false},
                    {false,false,false,false,false},
                    {false,false,false,false,false}
                };
            default:
                return null;
        }
    }

    void ApplyTexture()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend != null && signTexture != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.mainTexture = signTexture;
            mat.SetFloat("_Glossiness", 0.2f);
            rend.material = mat;
        }
    }

    void OnDestroy()
    {
        if (signTexture != null)
        {
            Destroy(signTexture);
        }
    }
}
