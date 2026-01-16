using UnityEngine;

/// <summary>
/// Helper class to randomize player appearance when starting a new game.
/// Call RandomizePlayerAppearance() from MainMenu.StartNewGame()
/// </summary>
public static class PlayerModelVariationHelper
{
    /// <summary>
    /// Finds the player and randomizes their appearance.
    /// Call this from MainMenu.StartNewGame() after EnableGameSystems()
    /// </summary>
    public static void RandomizePlayerAppearance()
    {
        // Find the player and randomize their appearance
        if (GameCache.IsPlayerValid() && GameCache.PlayerObject != null)
        {
            PlayerModelVariations modelVariations = GameCache.PlayerObject.GetComponent<PlayerModelVariations>();

            // Add the component if it doesn't exist
            if (modelVariations == null)
            {
                modelVariations = GameCache.PlayerObject.AddComponent<PlayerModelVariations>();
            }

            // Randomize the player's skin tone and body proportions
            modelVariations.RandomizeAppearance();

            Debug.Log("Player appearance randomized!");
        }
        else
        {
            // Try to find player by tag if GameCache isn't ready
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                PlayerModelVariations modelVariations = player.GetComponent<PlayerModelVariations>();
                if (modelVariations == null)
                {
                    modelVariations = player.AddComponent<PlayerModelVariations>();
                }
                modelVariations.RandomizeAppearance();
                Debug.Log("Player appearance randomized (found via tag)!");
            }
        }
    }
}
