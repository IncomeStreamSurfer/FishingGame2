using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages coconuts on palm trees that can randomly fall and damage the player
/// </summary>
public class CoconutManager : MonoBehaviour
{
    public static CoconutManager Instance { get; private set; }

    [Header("Coconut Settings")]
    public float dropChancePerSecond = 0.02f;  // 2% chance per second per coconut
    public float coconutDamage = 10f;
    public float respawnTime = 30f;

    [Header("Visual Settings")]
    public Color coconutColor = new Color(0.45f, 0.30f, 0.15f);  // Brown
    public Color coconutHuskColor = new Color(0.55f, 0.40f, 0.20f);  // Lighter brown husk

    private List<Coconut> coconuts = new List<Coconut>();
    private Transform playerTransform;
    private float groundY = 1.5f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Wait for trees to be created then add coconuts
        Invoke("FindPalmTreesAndAddCoconuts", 2f);
    }

    void FindPalmTreesAndAddCoconuts()
    {
        // Find all palm trees in the scene
        GameObject treesParent = GameObject.Find("TreesParent");
        if (treesParent == null)
        {
            Debug.LogWarning("CoconutManager: No TreesParent found");
            return;
        }

        foreach (Transform tree in treesParent.transform)
        {
            if (tree.name.Contains("Palm"))
            {
                AddCoconutsToTree(tree);
            }
        }

        Debug.Log($"CoconutManager: Added coconuts to {coconuts.Count / 3} palm trees");
    }

    void AddCoconutsToTree(Transform tree)
    {
        // Find the crown position (top of tree where fronds attach)
        // Palm trees have their crown at the top
        Vector3 crownPos = tree.position;

        // Find highest point of the tree (look for fronds)
        float maxY = tree.position.y;
        foreach (Transform child in tree.GetComponentsInChildren<Transform>())
        {
            if (child.name.Contains("Frond") || child.name.Contains("Crown"))
            {
                if (child.position.y > maxY)
                {
                    maxY = child.position.y;
                    crownPos = child.position;
                }
            }
        }

        // If we didn't find fronds, estimate crown height
        if (maxY == tree.position.y)
        {
            crownPos = tree.position + Vector3.up * 10f;  // Estimate tree height
        }

        // Add 2-4 coconuts per tree
        int numCoconuts = Random.Range(2, 5);
        for (int i = 0; i < numCoconuts; i++)
        {
            // Position coconuts around the crown
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float dist = Random.Range(0.3f, 0.8f);
            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * dist,
                Random.Range(-0.5f, 0.2f),  // Slightly below crown
                Mathf.Sin(angle) * dist
            );

            Coconut coconut = CreateCoconut(crownPos + offset, tree);
            coconuts.Add(coconut);
        }
    }

    Coconut CreateCoconut(Vector3 position, Transform parentTree)
    {
        GameObject coconutObj = new GameObject("Coconut");
        coconutObj.transform.position = position;
        coconutObj.transform.SetParent(parentTree);  // Parent to tree so it moves with tree

        // Coconut materials
        Material coconutMat = new Material(Shader.Find("Standard"));
        coconutMat.color = coconutColor;
        coconutMat.SetFloat("_Glossiness", 0.2f);

        Material huskMat = new Material(Shader.Find("Standard"));
        huskMat.color = coconutHuskColor;
        huskMat.SetFloat("_Glossiness", 0.1f);

        float scale = Random.Range(0.18f, 0.25f);

        // Main coconut body (oval shape)
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        body.name = "CoconutBody";
        body.transform.SetParent(coconutObj.transform);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(scale, scale * 1.3f, scale);
        body.GetComponent<Renderer>().material = coconutMat;
        Destroy(body.GetComponent<Collider>());

        // Husk fibers (decorative)
        for (int i = 0; i < 3; i++)
        {
            GameObject fiber = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fiber.name = "Fiber";
            fiber.transform.SetParent(coconutObj.transform);
            float fAngle = i * 120f * Mathf.Deg2Rad;
            fiber.transform.localPosition = new Vector3(
                Mathf.Cos(fAngle) * scale * 0.4f,
                scale * 0.5f,
                Mathf.Sin(fAngle) * scale * 0.4f
            );
            fiber.transform.localScale = new Vector3(0.02f, 0.08f, 0.02f);
            fiber.transform.localRotation = Quaternion.Euler(Random.Range(-20f, 20f), fAngle * Mathf.Rad2Deg, Random.Range(-20f, 20f));
            fiber.GetComponent<Renderer>().material = huskMat;
            Destroy(fiber.GetComponent<Collider>());
        }

        return new Coconut
        {
            gameObject = coconutObj,
            originalPosition = position,
            parentTree = parentTree,
            isFalling = false,
            isOnGround = false,
            fallVelocity = 0f,
            respawnTimer = 0f
        };
    }

    void Update()
    {
        if (!MainMenu.GameStarted) return;
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
            return;
        }

        foreach (Coconut coconut in coconuts)
        {
            if (coconut.gameObject == null) continue;
            UpdateCoconut(coconut);
        }
    }

    void UpdateCoconut(Coconut coconut)
    {
        if (coconut.isOnGround)
        {
            // Wait for respawn
            coconut.respawnTimer -= Time.deltaTime;
            if (coconut.respawnTimer <= 0)
            {
                RespawnCoconut(coconut);
            }
            return;
        }

        if (coconut.isFalling)
        {
            // Apply gravity
            coconut.fallVelocity += 15f * Time.deltaTime;  // Gravity
            Vector3 pos = coconut.gameObject.transform.position;
            pos.y -= coconut.fallVelocity * Time.deltaTime;

            // Rotate while falling
            coconut.gameObject.transform.Rotate(
                Random.Range(100f, 200f) * Time.deltaTime,
                Random.Range(50f, 100f) * Time.deltaTime,
                0
            );

            // Check if hit ground
            if (pos.y <= groundY + 0.1f)
            {
                pos.y = groundY + 0.1f;
                coconut.isOnGround = true;
                coconut.isFalling = false;
                coconut.respawnTimer = respawnTime;

                // Unparent from tree so it stays on ground
                coconut.gameObject.transform.SetParent(null);
            }

            // Check if hit player
            if (playerTransform != null)
            {
                float distToPlayer = Vector3.Distance(pos, playerTransform.position);
                if (distToPlayer < 1f && pos.y > playerTransform.position.y - 0.5f)
                {
                    // Hit player!
                    HitPlayer(coconut);
                }
            }

            coconut.gameObject.transform.position = pos;
        }
        else
        {
            // Check if should drop (random chance when player is nearby)
            if (playerTransform != null)
            {
                float distToPlayer = Vector3.Distance(coconut.gameObject.transform.position, playerTransform.position);

                // Only drop if player is under the tree (within 5 units horizontally)
                Vector3 horizontalDist = coconut.gameObject.transform.position - playerTransform.position;
                horizontalDist.y = 0;

                if (horizontalDist.magnitude < 5f)
                {
                    // Random chance to drop
                    if (Random.value < dropChancePerSecond * Time.deltaTime)
                    {
                        DropCoconut(coconut);
                    }
                }
            }
        }
    }

    void DropCoconut(Coconut coconut)
    {
        coconut.isFalling = true;
        coconut.fallVelocity = 0f;

        // Unparent so it falls in world space
        Vector3 worldPos = coconut.gameObject.transform.position;
        coconut.gameObject.transform.SetParent(null);
        coconut.gameObject.transform.position = worldPos;

        Debug.Log("Coconut falling!");
    }

    void HitPlayer(Coconut coconut)
    {
        Debug.Log($"COCONUT HIT PLAYER! Damage: {coconutDamage}");

        // Find player health component if exists
        // For now, just log the damage - you can integrate with your health system

        // Visual feedback - could spawn particles here

        // The coconut continues falling to ground
    }

    void RespawnCoconut(Coconut coconut)
    {
        coconut.isOnGround = false;
        coconut.isFalling = false;
        coconut.fallVelocity = 0f;

        // Reparent to tree and reset position
        coconut.gameObject.transform.SetParent(coconut.parentTree);
        coconut.gameObject.transform.position = coconut.originalPosition;
        coconut.gameObject.transform.rotation = Quaternion.identity;
    }

    private class Coconut
    {
        public GameObject gameObject;
        public Vector3 originalPosition;
        public Transform parentTree;
        public bool isFalling;
        public bool isOnGround;
        public float fallVelocity;
        public float respawnTimer;
    }
}
