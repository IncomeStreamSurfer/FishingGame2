using UnityEngine;
using UnityEditor;

public class CleanMissingScripts : MonoBehaviour
{
    [MenuItem("Fishing Game/Clean Missing Scripts")]
    static void CleanUp()
    {
        int totalRemoved = 0;

        // Find all GameObjects in the scene
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>(true);

        foreach (GameObject go in allObjects)
        {
            // Get count of missing scripts
            int missingCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);

            if (missingCount > 0)
            {
                Debug.Log($"Removing {missingCount} missing scripts from: {go.name}");
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                totalRemoved += missingCount;
            }
        }

        if (totalRemoved > 0)
        {
            Debug.Log($"<color=green>Removed {totalRemoved} missing script references!</color>");
            // Mark scene as dirty so changes can be saved
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
        else
        {
            Debug.Log("No missing scripts found!");
        }
    }
}
