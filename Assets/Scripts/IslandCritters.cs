using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages critters across the MAIN island - squirrels and ant colonies dispersed everywhere
/// </summary>
public class IslandCritters : MonoBehaviour
{
    public static IslandCritters Instance { get; private set; }

    [Header("Squirrel Settings")]
    public int maxSquirrels = 12;  // More squirrels spread across main island
    public float squirrelSpeed = 2f;

    [Header("Ant Settings")]
    public int numAntHills = 8;    // More ant hills spread across island
    public int antsPerHill = 15;

    private List<Squirrel> squirrels = new List<Squirrel>();
    private List<AntHill> antHills = new List<AntHill>();
    private List<Ant> ants = new List<Ant>();

    private Vector3 islandCenter;
    private float islandRadius = 45f;  // Main island radius
    private float groundY = 1.5f;      // Main island ground height

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Critters spawn across the MAIN island (centered at origin)
        islandCenter = Vector3.zero;

        Invoke("SpawnCritters", 1.5f);
    }

    void SpawnCritters()
    {
        SpawnSquirrels();
        SpawnAntHills();
    }

    #region Squirrels

    void SpawnSquirrels()
    {
        for (int i = 0; i < maxSquirrels; i++)
        {
            Vector3 pos = islandCenter + new Vector3(
                Random.Range(-islandRadius, islandRadius),
                groundY,
                Random.Range(-islandRadius, islandRadius)
            );
            squirrels.Add(CreateSquirrel(pos));
        }
    }

    Squirrel CreateSquirrel(Vector3 position)
    {
        GameObject squirrelObj = new GameObject("Squirrel");
        squirrelObj.transform.position = position;

        // Squirrel materials
        Material furMat = new Material(Shader.Find("Standard"));
        float variation = Random.Range(0.85f, 1.15f);
        furMat.color = new Color(0.55f * variation, 0.35f * variation, 0.2f * variation);
        furMat.SetFloat("_Glossiness", 0.2f);

        Material bellyMat = new Material(Shader.Find("Standard"));
        bellyMat.color = new Color(0.9f, 0.85f, 0.75f);

        float scale = Random.Range(0.9f, 1.1f);

        // Body
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        body.name = "Body";
        body.transform.SetParent(squirrelObj.transform);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(0.12f * scale, 0.1f * scale, 0.18f * scale);
        body.GetComponent<Renderer>().material = furMat;
        Destroy(body.GetComponent<Collider>());

        // Head
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(squirrelObj.transform);
        head.transform.localPosition = new Vector3(0, 0.03f * scale, 0.12f * scale);
        head.transform.localScale = new Vector3(0.09f * scale, 0.08f * scale, 0.09f * scale);
        head.GetComponent<Renderer>().material = furMat;
        Destroy(head.GetComponent<Collider>());

        // Ears
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject ear = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ear.name = "Ear";
            ear.transform.SetParent(head.transform);
            ear.transform.localPosition = new Vector3(side * 0.35f, 0.4f, -0.1f);
            ear.transform.localScale = new Vector3(0.3f, 0.5f, 0.2f);
            ear.GetComponent<Renderer>().material = furMat;
            Destroy(ear.GetComponent<Collider>());
        }

        // Eyes
        Material eyeMat = new Material(Shader.Find("Standard"));
        eyeMat.color = new Color(0.1f, 0.08f, 0.05f);
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.name = "Eye";
            eye.transform.SetParent(head.transform);
            eye.transform.localPosition = new Vector3(side * 0.3f, 0.1f, 0.4f);
            eye.transform.localScale = Vector3.one * 0.15f;
            eye.GetComponent<Renderer>().material = eyeMat;
            Destroy(eye.GetComponent<Collider>());
        }

        // Nose
        GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        nose.name = "Nose";
        nose.transform.SetParent(head.transform);
        nose.transform.localPosition = new Vector3(0, -0.1f, 0.5f);
        nose.transform.localScale = new Vector3(0.15f, 0.1f, 0.1f);
        nose.GetComponent<Renderer>().material = eyeMat;
        Destroy(nose.GetComponent<Collider>());

        // Bushy tail
        GameObject tail = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tail.name = "Tail";
        tail.transform.SetParent(squirrelObj.transform);
        tail.transform.localPosition = new Vector3(0, 0.08f * scale, -0.15f * scale);
        tail.transform.localScale = new Vector3(0.08f * scale, 0.15f * scale, 0.1f * scale);
        tail.transform.localRotation = Quaternion.Euler(-30, 0, 0);
        tail.GetComponent<Renderer>().material = furMat;
        Destroy(tail.GetComponent<Collider>());

        // Tail fluff (upper part)
        GameObject tailFluff = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tailFluff.name = "TailFluff";
        tailFluff.transform.SetParent(tail.transform);
        tailFluff.transform.localPosition = new Vector3(0, 0.7f, -0.3f);
        tailFluff.transform.localScale = new Vector3(1.2f, 1.5f, 1.3f);
        tailFluff.GetComponent<Renderer>().material = furMat;
        Destroy(tailFluff.GetComponent<Collider>());

        // Legs
        List<Transform> legs = new List<Transform>();
        float[][] legPositions = {
            new float[] { -0.05f, -0.04f, 0.06f },  // Front left
            new float[] { 0.05f, -0.04f, 0.06f },   // Front right
            new float[] { -0.05f, -0.04f, -0.06f }, // Back left
            new float[] { 0.05f, -0.04f, -0.06f }   // Back right
        };

        for (int i = 0; i < 4; i++)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leg.name = "Leg" + i;
            leg.transform.SetParent(squirrelObj.transform);
            leg.transform.localPosition = new Vector3(
                legPositions[i][0] * scale,
                legPositions[i][1] * scale,
                legPositions[i][2] * scale
            );
            leg.transform.localScale = new Vector3(0.025f * scale, 0.03f * scale, 0.025f * scale);
            leg.GetComponent<Renderer>().material = furMat;
            Destroy(leg.GetComponent<Collider>());
            legs.Add(leg.transform);
        }

        return new Squirrel
        {
            gameObject = squirrelObj,
            tail = tail.transform,
            legs = legs,
            moveTarget = position,
            state = SquirrelState.Idle,
            stateTimer = Random.Range(2f, 5f),
            legAnimTime = 0
        };
    }

    void UpdateSquirrel(Squirrel squirrel)
    {
        if (squirrel.gameObject == null) return;

        squirrel.stateTimer -= Time.deltaTime;

        switch (squirrel.state)
        {
            case SquirrelState.Idle:
                // Tail twitch
                if (squirrel.tail != null)
                {
                    float twitch = Mathf.Sin(Time.time * 8f) * 5f;
                    squirrel.tail.localRotation = Quaternion.Euler(-30 + twitch, twitch * 0.5f, 0);
                }

                if (squirrel.stateTimer <= 0)
                {
                    // Pick new activity
                    float rand = Random.value;
                    if (rand < 0.5f)
                    {
                        // Run to new location
                        squirrel.moveTarget = islandCenter + new Vector3(
                            Random.Range(-islandRadius, islandRadius),
                            groundY,
                            Random.Range(-islandRadius, islandRadius)
                        );
                        squirrel.state = SquirrelState.Running;
                    }
                    else if (rand < 0.75f)
                    {
                        squirrel.state = SquirrelState.Foraging;
                        squirrel.stateTimer = Random.Range(2f, 4f);
                    }
                    else
                    {
                        squirrel.stateTimer = Random.Range(1f, 3f);
                    }
                }
                break;

            case SquirrelState.Running:
                Vector3 toTarget = squirrel.moveTarget - squirrel.gameObject.transform.position;
                toTarget.y = 0;

                if (toTarget.magnitude < 0.3f)
                {
                    squirrel.state = SquirrelState.Idle;
                    squirrel.stateTimer = Random.Range(2f, 5f);
                }
                else
                {
                    // Move
                    Vector3 moveDir = toTarget.normalized;
                    squirrel.gameObject.transform.position += moveDir * squirrelSpeed * Time.deltaTime;
                    squirrel.gameObject.transform.rotation = Quaternion.Lerp(
                        squirrel.gameObject.transform.rotation,
                        Quaternion.LookRotation(moveDir),
                        Time.deltaTime * 8f
                    );

                    // Animate legs
                    squirrel.legAnimTime += Time.deltaTime * 20f;
                    for (int i = 0; i < squirrel.legs.Count; i++)
                    {
                        if (squirrel.legs[i] == null) continue;
                        float phase = (i % 2 == 0) ? 0 : Mathf.PI;
                        float swing = Mathf.Sin(squirrel.legAnimTime + phase) * 0.02f;
                        Vector3 basePos = squirrel.legs[i].localPosition;
                        squirrel.legs[i].localPosition = new Vector3(basePos.x, -0.04f + Mathf.Abs(swing), basePos.z);
                    }

                    // Bouncy tail while running
                    if (squirrel.tail != null)
                    {
                        float bounce = Mathf.Sin(squirrel.legAnimTime) * 10f;
                        squirrel.tail.localRotation = Quaternion.Euler(-30 + bounce, bounce * 0.3f, 0);
                    }
                }
                break;

            case SquirrelState.Foraging:
                // Head bob animation (looking for food)
                Transform head = squirrel.gameObject.transform.Find("Head");
                if (head != null)
                {
                    float bob = Mathf.Sin(Time.time * 6f) * 0.01f;
                    head.localPosition = new Vector3(0, 0.03f + bob, 0.12f);
                }

                if (squirrel.stateTimer <= 0)
                {
                    squirrel.state = SquirrelState.Idle;
                    squirrel.stateTimer = Random.Range(1f, 3f);
                }
                break;
        }
    }

    #endregion

    #region Ants

    void SpawnAntHills()
    {
        for (int i = 0; i < numAntHills; i++)
        {
            Vector3 pos = islandCenter + new Vector3(
                Random.Range(-islandRadius * 0.7f, islandRadius * 0.7f),
                groundY,
                Random.Range(-islandRadius * 0.7f, islandRadius * 0.7f)
            );
            AntHill hill = CreateAntHill(pos);
            antHills.Add(hill);

            // Create ants for this hill
            for (int j = 0; j < antsPerHill; j++)
            {
                ants.Add(CreateAnt(pos, hill));
            }
        }
    }

    AntHill CreateAntHill(Vector3 position)
    {
        GameObject hillObj = new GameObject("AntHill");
        hillObj.transform.position = position;

        Material dirtMat = new Material(Shader.Find("Standard"));
        dirtMat.color = new Color(0.4f, 0.3f, 0.2f);

        // Main mound
        GameObject mound = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        mound.name = "Mound";
        mound.transform.SetParent(hillObj.transform);
        mound.transform.localPosition = Vector3.zero;
        float size = Random.Range(0.2f, 0.35f);
        mound.transform.localScale = new Vector3(size, size * 0.5f, size);
        mound.GetComponent<Renderer>().material = dirtMat;
        Destroy(mound.GetComponent<Collider>());

        // Small dirt particles around
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f * Mathf.Deg2Rad;
            float dist = Random.Range(size * 0.8f, size * 1.5f);

            GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            particle.name = "DirtParticle";
            particle.transform.SetParent(hillObj.transform);
            particle.transform.localPosition = new Vector3(
                Mathf.Cos(angle) * dist,
                0,
                Mathf.Sin(angle) * dist
            );
            float pSize = Random.Range(0.02f, 0.05f);
            particle.transform.localScale = Vector3.one * pSize;
            particle.GetComponent<Renderer>().material = dirtMat;
            Destroy(particle.GetComponent<Collider>());
        }

        // Entrance hole
        GameObject hole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        hole.name = "Entrance";
        hole.transform.SetParent(hillObj.transform);
        hole.transform.localPosition = new Vector3(0, size * 0.25f, 0);
        hole.transform.localScale = new Vector3(0.04f, 0.01f, 0.04f);
        Material holeMat = new Material(Shader.Find("Standard"));
        holeMat.color = new Color(0.1f, 0.08f, 0.05f);
        hole.GetComponent<Renderer>().material = holeMat;
        Destroy(hole.GetComponent<Collider>());

        return new AntHill
        {
            gameObject = hillObj,
            position = position
        };
    }

    Ant CreateAnt(Vector3 hillPosition, AntHill home)
    {
        GameObject antObj = new GameObject("Ant");

        // Start at or near the hill
        float startDist = Random.Range(0f, 3f);
        float startAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 startPos = hillPosition + new Vector3(
            Mathf.Cos(startAngle) * startDist,
            0,
            Mathf.Sin(startAngle) * startDist
        );
        antObj.transform.position = startPos;

        Material antMat = new Material(Shader.Find("Standard"));
        antMat.color = new Color(0.15f, 0.1f, 0.08f);

        float scale = Random.Range(0.8f, 1.2f) * 0.02f;

        // Head
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(antObj.transform);
        head.transform.localPosition = new Vector3(0, 0, scale * 1.5f);
        head.transform.localScale = Vector3.one * scale * 0.8f;
        head.GetComponent<Renderer>().material = antMat;
        Destroy(head.GetComponent<Collider>());

        // Thorax
        GameObject thorax = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        thorax.name = "Thorax";
        thorax.transform.SetParent(antObj.transform);
        thorax.transform.localPosition = Vector3.zero;
        thorax.transform.localScale = new Vector3(scale * 0.7f, scale * 0.5f, scale);
        thorax.GetComponent<Renderer>().material = antMat;
        Destroy(thorax.GetComponent<Collider>());

        // Abdomen
        GameObject abdomen = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        abdomen.name = "Abdomen";
        abdomen.transform.SetParent(antObj.transform);
        abdomen.transform.localPosition = new Vector3(0, 0, -scale * 1.8f);
        abdomen.transform.localScale = new Vector3(scale * 0.9f, scale * 0.7f, scale * 1.2f);
        abdomen.GetComponent<Renderer>().material = antMat;
        Destroy(abdomen.GetComponent<Collider>());

        // Legs (6 total)
        for (int i = 0; i < 6; i++)
        {
            int side = (i % 2 == 0) ? -1 : 1;
            float zOffset = -0.3f + (i / 2) * 0.3f;

            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leg.name = "Leg" + i;
            leg.transform.SetParent(antObj.transform);
            leg.transform.localPosition = new Vector3(side * scale * 0.5f, -scale * 0.2f, zOffset * scale);
            leg.transform.localScale = new Vector3(scale * 0.8f, scale * 0.1f, scale * 0.1f);
            leg.transform.localRotation = Quaternion.Euler(0, 0, side * 30);
            leg.GetComponent<Renderer>().material = antMat;
            Destroy(leg.GetComponent<Collider>());
        }

        // Antennae
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject antenna = GameObject.CreatePrimitive(PrimitiveType.Cube);
            antenna.name = "Antenna";
            antenna.transform.SetParent(head.transform);
            antenna.transform.localPosition = new Vector3(side * 0.3f, 0.3f, 0.4f);
            antenna.transform.localScale = new Vector3(0.1f, 0.5f, 0.1f);
            antenna.transform.localRotation = Quaternion.Euler(-30, side * 20, 0);
            antenna.GetComponent<Renderer>().material = antMat;
            Destroy(antenna.GetComponent<Collider>());
        }

        // Determine trail target
        float trailLength = Random.Range(2f, 6f);
        float trailAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 trailEnd = hillPosition + new Vector3(
            Mathf.Cos(trailAngle) * trailLength,
            0,
            Mathf.Sin(trailAngle) * trailLength
        );

        return new Ant
        {
            gameObject = antObj,
            home = home,
            trailTarget = trailEnd,
            progress = Random.value, // Start at random point in patrol
            speed = Random.Range(0.3f, 0.6f),
            goingOut = Random.value > 0.5f
        };
    }

    void UpdateAnt(Ant ant)
    {
        if (ant.gameObject == null || ant.home == null) return;

        // Move along trail between hill and target
        Vector3 start = ant.home.position;
        Vector3 end = ant.trailTarget;

        if (ant.goingOut)
        {
            ant.progress += ant.speed * Time.deltaTime / Vector3.Distance(start, end);
            if (ant.progress >= 1f)
            {
                ant.progress = 1f;
                ant.goingOut = false;
            }
        }
        else
        {
            ant.progress -= ant.speed * Time.deltaTime / Vector3.Distance(start, end);
            if (ant.progress <= 0f)
            {
                ant.progress = 0f;
                ant.goingOut = true;

                // Occasionally pick new trail
                if (Random.value < 0.3f)
                {
                    float trailLength = Random.Range(2f, 6f);
                    float trailAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                    ant.trailTarget = ant.home.position + new Vector3(
                        Mathf.Cos(trailAngle) * trailLength,
                        0,
                        Mathf.Sin(trailAngle) * trailLength
                    );
                }
            }
        }

        // Interpolate position
        Vector3 pos = Vector3.Lerp(start, end, ant.progress);
        pos.y = groundY;

        // Add tiny wandering
        float wander = Mathf.Sin(Time.time * 10f + ant.gameObject.GetHashCode()) * 0.02f;
        pos.x += wander;

        ant.gameObject.transform.position = pos;

        // Face movement direction
        Vector3 dir = ant.goingOut ? (end - start).normalized : (start - end).normalized;
        if (dir.magnitude > 0.01f)
        {
            ant.gameObject.transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    #endregion

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        // Update squirrels
        foreach (Squirrel squirrel in squirrels)
        {
            UpdateSquirrel(squirrel);
        }

        // Update ants
        foreach (Ant ant in ants)
        {
            UpdateAnt(ant);
        }
    }

    void OnDestroy()
    {
        foreach (Squirrel s in squirrels)
        {
            if (s.gameObject != null) Destroy(s.gameObject);
        }
        foreach (AntHill h in antHills)
        {
            if (h.gameObject != null) Destroy(h.gameObject);
        }
        foreach (Ant a in ants)
        {
            if (a.gameObject != null) Destroy(a.gameObject);
        }
    }

    private enum SquirrelState { Idle, Running, Foraging }

    private class Squirrel
    {
        public GameObject gameObject;
        public Transform tail;
        public List<Transform> legs;
        public Vector3 moveTarget;
        public SquirrelState state;
        public float stateTimer;
        public float legAnimTime;
    }

    private class AntHill
    {
        public GameObject gameObject;
        public Vector3 position;
    }

    private class Ant
    {
        public GameObject gameObject;
        public AntHill home;
        public Vector3 trailTarget;
        public float progress;
        public float speed;
        public bool goingOut;
    }
}
