using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor tool to create the Poll Booth object in the scene
/// Menu: GameObject > Fish or Die > Create Poll Booth
/// </summary>
public class PollBoothCreator : Editor
{
    [MenuItem("GameObject/Fish or Die/Create Poll Booth", false, 10)]
    static void CreatePollBooth()
    {
        // Create main poll booth object
        GameObject pollBooth = new GameObject("PollBooth");

        // Position at dock area (adjust as needed)
        // Default position: near the docks, to the right
        pollBooth.transform.position = new Vector3(15f, 0f, -5f);

        // Add the PollBooth component
        PollBooth pb = pollBooth.AddComponent<PollBooth>();
        pb.interactionRange = 3f;
        pb.boothName = "Poll Booth";

        // Create the postbox visual
        CreatePostboxVisual(pollBooth.transform);

        // Select the new object
        Selection.activeGameObject = pollBooth;

        Debug.Log("Poll Booth created! Adjust position as needed (recommended: base of docks to the right)");
        EditorUtility.SetDirty(pollBooth);
    }

    static void CreatePostboxVisual(Transform parent)
    {
        // Materials
        Material postboxMat = new Material(Shader.Find("Standard"));
        postboxMat.color = new Color(0.1f, 0.3f, 0.6f); // Blue postbox color

        Material poleMat = new Material(Shader.Find("Standard"));
        poleMat.color = new Color(0.3f, 0.3f, 0.35f); // Metal gray

        Material envelopeMat = new Material(Shader.Find("Standard"));
        envelopeMat.color = new Color(0.95f, 0.95f, 0.9f); // White/cream

        Material textMat = new Material(Shader.Find("Standard"));
        textMat.color = new Color(0.9f, 0.85f, 0.2f); // Gold text

        // === POLE ===
        GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pole.name = "Pole";
        pole.transform.SetParent(parent);
        pole.transform.localPosition = new Vector3(0, 0.6f, 0);
        pole.transform.localScale = new Vector3(0.08f, 0.6f, 0.08f);
        pole.GetComponent<Renderer>().material = poleMat;
        DestroyImmediate(pole.GetComponent<Collider>()); // Remove default collider

        // === MAIN POSTBOX BODY ===
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "PostboxBody";
        body.transform.SetParent(parent);
        body.transform.localPosition = new Vector3(0, 1.35f, 0);
        body.transform.localScale = new Vector3(0.5f, 0.6f, 0.35f);
        body.GetComponent<Renderer>().material = postboxMat;
        DestroyImmediate(body.GetComponent<Collider>());

        // === ROUNDED TOP (half sphere approximation with stretched cube) ===
        GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cube);
        top.name = "PostboxTop";
        top.transform.SetParent(parent);
        top.transform.localPosition = new Vector3(0, 1.7f, 0);
        top.transform.localScale = new Vector3(0.52f, 0.15f, 0.37f);
        top.transform.localRotation = Quaternion.Euler(0, 0, 0);
        top.GetComponent<Renderer>().material = postboxMat;
        DestroyImmediate(top.GetComponent<Collider>());

        // === MAIL SLOT ===
        GameObject slot = GameObject.CreatePrimitive(PrimitiveType.Cube);
        slot.name = "MailSlot";
        slot.transform.SetParent(parent);
        slot.transform.localPosition = new Vector3(0, 1.55f, 0.18f);
        slot.transform.localScale = new Vector3(0.35f, 0.04f, 0.02f);

        Material slotMat = new Material(Shader.Find("Standard"));
        slotMat.color = new Color(0.05f, 0.15f, 0.35f); // Darker blue
        slot.GetComponent<Renderer>().material = slotMat;
        DestroyImmediate(slot.GetComponent<Collider>());

        // === ENVELOPE ICON ===
        // Main envelope body
        GameObject envelope = GameObject.CreatePrimitive(PrimitiveType.Cube);
        envelope.name = "EnvelopeIcon";
        envelope.transform.SetParent(parent);
        envelope.transform.localPosition = new Vector3(0, 1.25f, 0.18f);
        envelope.transform.localScale = new Vector3(0.2f, 0.12f, 0.01f);
        envelope.GetComponent<Renderer>().material = envelopeMat;
        DestroyImmediate(envelope.GetComponent<Collider>());

        // Envelope flap (triangle approximation)
        GameObject flap = GameObject.CreatePrimitive(PrimitiveType.Cube);
        flap.name = "EnvelopeFlap";
        flap.transform.SetParent(parent);
        flap.transform.localPosition = new Vector3(0, 1.3f, 0.185f);
        flap.transform.localScale = new Vector3(0.12f, 0.06f, 0.005f);
        flap.transform.localRotation = Quaternion.Euler(0, 0, 45);
        flap.GetComponent<Renderer>().material = envelopeMat;
        DestroyImmediate(flap.GetComponent<Collider>());

        // === TEXT LABEL - "POLL BOOTH" ===
        // Using 3D text or a simple plate with emissive material
        GameObject textPlate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        textPlate.name = "TextPlate";
        textPlate.transform.SetParent(parent);
        textPlate.transform.localPosition = new Vector3(0, 1.05f, 0.18f);
        textPlate.transform.localScale = new Vector3(0.4f, 0.08f, 0.01f);

        Material plateMat = new Material(Shader.Find("Standard"));
        plateMat.color = new Color(0.9f, 0.85f, 0.3f); // Gold plate
        plateMat.EnableKeyword("_EMISSION");
        plateMat.SetColor("_EmissionColor", new Color(0.3f, 0.25f, 0.1f));
        textPlate.GetComponent<Renderer>().material = plateMat;
        DestroyImmediate(textPlate.GetComponent<Collider>());

        // Add 3D text for "POLL BOOTH"
        GameObject textObj = new GameObject("PollBoothText");
        textObj.transform.SetParent(parent);
        textObj.transform.localPosition = new Vector3(0, 1.05f, 0.195f);
        textObj.transform.localRotation = Quaternion.Euler(0, 180, 0);
        textObj.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);

        TextMesh textMesh = textObj.AddComponent<TextMesh>();
        textMesh.text = "POLL BOOTH";
        textMesh.fontSize = 50;
        textMesh.fontStyle = FontStyle.Bold;
        textMesh.alignment = TextAlignment.Center;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.color = new Color(0.15f, 0.1f, 0.05f);

        // === BASE PLATE ===
        GameObject basePlate = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        basePlate.name = "BasePlate";
        basePlate.transform.SetParent(parent);
        basePlate.transform.localPosition = new Vector3(0, 0.02f, 0);
        basePlate.transform.localScale = new Vector3(0.4f, 0.02f, 0.4f);
        basePlate.GetComponent<Renderer>().material = poleMat;
        DestroyImmediate(basePlate.GetComponent<Collider>());

        // === Add proper collider to the main object ===
        BoxCollider col = parent.gameObject.AddComponent<BoxCollider>();
        col.size = new Vector3(0.6f, 1.8f, 0.4f);
        col.center = new Vector3(0, 0.9f, 0);
    }

    [MenuItem("GameObject/Fish or Die/Poll Booth - View Submissions Folder", false, 11)]
    static void OpenSubmissionsFolder()
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, "PollSubmissions");

        if (!System.IO.Directory.Exists(path))
        {
            System.IO.Directory.CreateDirectory(path);
        }

        EditorUtility.RevealInFinder(path);
        Debug.Log($"Submissions folder: {path}");
    }

    [MenuItem("GameObject/Fish or Die/Poll Booth - Export All Submissions", false, 12)]
    static void ExportSubmissions()
    {
        string result = PollBoothEmailService.ExportAllSubmissions();
        EditorUtility.DisplayDialog("Export Complete", $"Submissions exported to:\n{result}", "OK");
    }
}
