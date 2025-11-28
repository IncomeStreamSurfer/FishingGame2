using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Spawns and manages beach critters - crabs that scuttle around the sand
/// </summary>
public class BeachCritters : MonoBehaviour
{
    public static BeachCritters Instance { get; private set; }

    [Header("Crab Settings")]
    public int maxCrabs = 15;
    public float crabSpeed = 1.5f;
    public float beachRadius = 30f;

    private List<Crab> crabs = new List<Crab>();
    private float groundY = 1.26f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        Invoke("SpawnCrabs", 1f);
    }

    void SpawnCrabs()
    {
        for (int i = 0; i < maxCrabs; i++)
        {
            SpawnCrab();
        }
    }

    void SpawnCrab()
    {
        // Spawn crabs ONLY on the beach/sand area (ring around the island edge)
        // Sand area is roughly between radius 25-40 from center
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float sandInnerRadius = 28f;  // Inner edge of sand
        float sandOuterRadius = 42f;  // Outer edge (near water)
        float dist = Random.Range(sandInnerRadius, sandOuterRadius);

        Vector3 pos = new Vector3(
            Mathf.Cos(angle) * dist,
            groundY,
            Mathf.Sin(angle) * dist - 10f  // Offset to match island center
        );

        Crab crab = CreateCrabModel(pos);
        crabs.Add(crab);
    }

    Crab CreateCrabModel(Vector3 position)
    {
        GameObject crabObj = new GameObject("Crab");
        crabObj.transform.position = position;
        crabObj.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

        // Crab colors
        Material bodyMat = new Material(Shader.Find("Standard"));
        float colorVariation = Random.Range(0.8f, 1.2f);
        bodyMat.color = new Color(0.8f * colorVariation, 0.4f * colorVariation, 0.3f * colorVariation);
        bodyMat.SetFloat("_Glossiness", 0.4f);

        Material legMat = new Material(Shader.Find("Standard"));
        legMat.color = new Color(0.7f * colorVariation, 0.35f * colorVariation, 0.25f * colorVariation);

        Material clawMat = new Material(Shader.Find("Standard"));
        clawMat.color = new Color(0.9f * colorVariation, 0.45f * colorVariation, 0.35f * colorVariation);

        float scale = Random.Range(0.8f, 1.2f);

        // Main body (oval)
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        body.name = "CrabBody";
        body.transform.SetParent(crabObj.transform);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(0.15f * scale, 0.06f * scale, 0.12f * scale);
        body.GetComponent<Renderer>().material = bodyMat;
        Destroy(body.GetComponent<Collider>());

        // Eyes on stalks
        for (int side = -1; side <= 1; side += 2)
        {
            // Eye stalk
            GameObject stalk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stalk.name = "EyeStalk";
            stalk.transform.SetParent(crabObj.transform);
            stalk.transform.localPosition = new Vector3(side * 0.04f * scale, 0.04f * scale, 0.04f * scale);
            stalk.transform.localScale = new Vector3(0.015f * scale, 0.02f * scale, 0.015f * scale);
            stalk.GetComponent<Renderer>().material = bodyMat;
            Destroy(stalk.GetComponent<Collider>());

            // Eye ball
            GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.name = "Eye";
            eye.transform.SetParent(crabObj.transform);
            eye.transform.localPosition = new Vector3(side * 0.04f * scale, 0.065f * scale, 0.04f * scale);
            eye.transform.localScale = Vector3.one * 0.02f * scale;
            Material eyeMat = new Material(Shader.Find("Standard"));
            eyeMat.color = Color.black;
            eye.GetComponent<Renderer>().material = eyeMat;
            Destroy(eye.GetComponent<Collider>());
        }

        // Legs (4 on each side)
        List<Transform> legs = new List<Transform>();
        for (int side = -1; side <= 1; side += 2)
        {
            for (int i = 0; i < 4; i++)
            {
                float zOffset = -0.03f + i * 0.025f;

                // Upper leg segment
                GameObject upperLeg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                upperLeg.name = "CrabLeg";
                upperLeg.transform.SetParent(crabObj.transform);
                upperLeg.transform.localPosition = new Vector3(side * 0.08f * scale, 0.01f * scale, zOffset * scale);
                upperLeg.transform.localScale = new Vector3(0.06f * scale, 0.01f * scale, 0.015f * scale);
                upperLeg.transform.localRotation = Quaternion.Euler(0, 0, side * 20);
                upperLeg.GetComponent<Renderer>().material = legMat;
                Destroy(upperLeg.GetComponent<Collider>());

                // Lower leg segment
                GameObject lowerLeg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                lowerLeg.name = "CrabLegLower";
                lowerLeg.transform.SetParent(upperLeg.transform);
                lowerLeg.transform.localPosition = new Vector3(side * 0.4f, -0.3f, 0);
                lowerLeg.transform.localScale = new Vector3(0.8f, 1.5f, 0.8f);
                lowerLeg.transform.localRotation = Quaternion.Euler(0, 0, side * 40);
                lowerLeg.GetComponent<Renderer>().material = legMat;
                Destroy(lowerLeg.GetComponent<Collider>());

                legs.Add(upperLeg.transform);
            }
        }

        // Claws (front)
        List<Transform> claws = new List<Transform>();
        for (int side = -1; side <= 1; side += 2)
        {
            // Claw arm
            GameObject clawArm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            clawArm.name = "ClawArm";
            clawArm.transform.SetParent(crabObj.transform);
            clawArm.transform.localPosition = new Vector3(side * 0.06f * scale, 0.02f * scale, 0.06f * scale);
            clawArm.transform.localScale = new Vector3(0.04f * scale, 0.015f * scale, 0.03f * scale);
            clawArm.transform.localRotation = Quaternion.Euler(0, side * -30, 0);
            clawArm.GetComponent<Renderer>().material = clawMat;
            Destroy(clawArm.GetComponent<Collider>());

            // Claw pincer (upper)
            GameObject upperPincer = GameObject.CreatePrimitive(PrimitiveType.Cube);
            upperPincer.name = "UpperPincer";
            upperPincer.transform.SetParent(clawArm.transform);
            upperPincer.transform.localPosition = new Vector3(side * 0.8f, 0.3f, 0.5f);
            upperPincer.transform.localScale = new Vector3(0.8f, 0.5f, 1.2f);
            upperPincer.GetComponent<Renderer>().material = clawMat;
            Destroy(upperPincer.GetComponent<Collider>());

            // Claw pincer (lower)
            GameObject lowerPincer = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lowerPincer.name = "LowerPincer";
            lowerPincer.transform.SetParent(clawArm.transform);
            lowerPincer.transform.localPosition = new Vector3(side * 0.8f, -0.2f, 0.5f);
            lowerPincer.transform.localScale = new Vector3(0.6f, 0.4f, 1f);
            lowerPincer.GetComponent<Renderer>().material = clawMat;
            Destroy(lowerPincer.GetComponent<Collider>());

            claws.Add(clawArm.transform);
        }

        Crab crab = new Crab
        {
            gameObject = crabObj,
            legs = legs,
            claws = claws,
            moveTarget = position,
            waitTime = Random.Range(1f, 4f),
            isMoving = false,
            legAnimTime = 0
        };

        return crab;
    }

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        foreach (Crab crab in crabs)
        {
            if (crab.gameObject == null) continue;
            UpdateCrab(crab);
        }
    }

    void UpdateCrab(Crab crab)
    {
        // Behavior: wait, then scuttle to new location, repeat
        if (crab.isMoving)
        {
            // Move towards target (sideways scuttle)
            Vector3 toTarget = crab.moveTarget - crab.gameObject.transform.position;
            toTarget.y = 0;

            if (toTarget.magnitude < 0.2f)
            {
                // Reached target
                crab.isMoving = false;
                crab.waitTime = Random.Range(2f, 6f);
            }
            else
            {
                // Scuttle sideways
                Vector3 moveDir = toTarget.normalized;
                crab.gameObject.transform.position += moveDir * crabSpeed * Time.deltaTime;

                // Face perpendicular to movement (crabs walk sideways)
                Vector3 sideDir = Vector3.Cross(moveDir, Vector3.up);
                if (Random.value > 0.5f) sideDir = -sideDir;
                crab.gameObject.transform.rotation = Quaternion.Lerp(
                    crab.gameObject.transform.rotation,
                    Quaternion.LookRotation(sideDir),
                    Time.deltaTime * 5f
                );

                // Animate legs
                crab.legAnimTime += Time.deltaTime * 15f;
                AnimateCrabLegs(crab);
            }
        }
        else
        {
            // Waiting
            crab.waitTime -= Time.deltaTime;

            // Occasional claw snap animation
            if (Random.value < 0.002f)
            {
                StartCoroutine(SnapClaws(crab));
            }

            if (crab.waitTime <= 0)
            {
                // Pick new target - keep crabs on sand area only
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float sandInnerRadius = 28f;
                float sandOuterRadius = 42f;
                float dist = Random.Range(sandInnerRadius, sandOuterRadius);

                crab.moveTarget = new Vector3(
                    Mathf.Cos(angle) * dist,
                    groundY,
                    Mathf.Sin(angle) * dist - 10f
                );
                crab.isMoving = true;
            }
        }
    }

    void AnimateCrabLegs(Crab crab)
    {
        for (int i = 0; i < crab.legs.Count; i++)
        {
            if (crab.legs[i] == null) continue;

            // Alternating leg movement
            float phase = (i % 2 == 0) ? 0 : Mathf.PI;
            float swing = Mathf.Sin(crab.legAnimTime + phase) * 15f;

            Vector3 rot = crab.legs[i].localEulerAngles;
            int side = (i < 4) ? -1 : 1;
            crab.legs[i].localRotation = Quaternion.Euler(swing, rot.y, side * 20 + swing * 0.5f);
        }
    }

    IEnumerator SnapClaws(Crab crab)
    {
        // Quick claw snap animation
        for (int snap = 0; snap < 2; snap++)
        {
            foreach (Transform claw in crab.claws)
            {
                if (claw == null) continue;
                Transform upperPincer = claw.Find("UpperPincer");
                if (upperPincer != null)
                {
                    upperPincer.localPosition = new Vector3(upperPincer.localPosition.x, 0.1f, upperPincer.localPosition.z);
                }
            }
            yield return new WaitForSeconds(0.1f);

            foreach (Transform claw in crab.claws)
            {
                if (claw == null) continue;
                Transform upperPincer = claw.Find("UpperPincer");
                if (upperPincer != null)
                {
                    upperPincer.localPosition = new Vector3(upperPincer.localPosition.x, 0.3f, upperPincer.localPosition.z);
                }
            }
            yield return new WaitForSeconds(0.15f);
        }
    }

    void OnDestroy()
    {
        foreach (Crab crab in crabs)
        {
            if (crab.gameObject != null)
                Destroy(crab.gameObject);
        }
        crabs.Clear();
    }

    private class Crab
    {
        public GameObject gameObject;
        public List<Transform> legs;
        public List<Transform> claws;
        public Vector3 moveTarget;
        public float waitTime;
        public bool isMoving;
        public float legAnimTime;
    }
}
