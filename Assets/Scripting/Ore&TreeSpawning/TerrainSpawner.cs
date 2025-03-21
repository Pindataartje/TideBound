using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SpawnGroup
{
    public string groupName;
    public GameObject prefab;           // The prefab to spawn.
    public int maxCount = 10;           // Maximum spawned objects allowed.
    public float spawnInterval = 300f;  // How many seconds to wait between spawns (default 5 minutes).
    public int targetTextureIndex = 0;  // Texture index in the terrain's splat map that must be present.
    public float textureThreshold = 0.5f; // Minimum alpha value for that texture.
    public float minDistanceFromPlayer = 10f; // Minimum distance from the player.
    public float minSeparation = 5f;    // Minimum separation from other spawned objects.

    [HideInInspector]
    public float spawnTimer = 0f;       // Timer tracking time passed.
}

public class TerrainSpawner : MonoBehaviour
{
    [Header("Spawn Area Settings")]
    // Define the rectangular area in world space where objects can be spawned.
    public Vector3 spawnAreaMin;  // e.g., bottom-left corner.
    public Vector3 spawnAreaMax;  // e.g., top-right corner.

    [Header("Terrain Settings")]
    public Terrain terrain;       // Reference to your Terrain.

    [Header("Spawning Groups")]
    public SpawnGroup[] spawnGroups; // Array of groups, each with its own settings.

    [Header("Other Settings")]
    public LayerMask obstacleMask;   // Layers to consider as obstacles (buildings, etc.).
    public Transform player;         // Reference to the player's transform.
    public float checkInterval = 30f;  // How often (in seconds) to check for spawning.

    void Start()
    {
        // Start the spawn-checking coroutine.
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            foreach (SpawnGroup group in spawnGroups)
            {
                // Count how many objects from this group have already been spawned.
                // Here we assume all spawned objects are children of this spawner.
                int currentCount = 0;
                foreach (Transform child in transform)
                {
                    // Optionally, you can filter by name/tag if needed.
                    currentCount++;
                }

                if (currentCount < group.maxCount)
                {
                    group.spawnTimer += checkInterval;
                    if (group.spawnTimer >= group.spawnInterval)
                    {
                        TrySpawn(group);
                        group.spawnTimer = 0f;
                    }
                }
            }
            yield return new WaitForSeconds(checkInterval);
        }
    }

    void TrySpawn(SpawnGroup group)
    {
        // We'll try up to a fixed number of attempts.
        int attempts = 10;
        bool spawned = false;
        while (attempts > 0 && !spawned)
        {
            attempts--;
            // Pick a random position within the spawn area.
            float x = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
            float z = Random.Range(spawnAreaMin.z, spawnAreaMax.z);
            Vector3 pos = new Vector3(x, 0f, z);
            // Sample the terrain's height.
            float height = terrain.SampleHeight(pos) + terrain.transform.position.y;
            pos.y = height;

            // Check terrain texture at pos.
            if (!IsValidTexture(pos, group.targetTextureIndex, group.textureThreshold))
                continue;

            // Ensure not too close to the player.
            if (player != null && Vector3.Distance(pos, player.position) < group.minDistanceFromPlayer)
                continue;

            // Ensure separation from other spawned objects.
            if (!IsFarFromSpawned(pos, group.minSeparation))
                continue;

            // Check for obstacles using an overlap sphere.
            Collider[] colliders = Physics.OverlapSphere(pos, group.minSeparation, obstacleMask);
            if (colliders.Length > 0)
                continue;

            // If all checks pass, spawn the prefab.
            GameObject spawnedObj = Instantiate(group.prefab, pos, Quaternion.identity, transform);
            // Optionally align the spawned object so its pivot is exactly at the terrain's surface.
            // For example, if the pivot is at the base, no further adjustment may be needed.
            spawned = true;
        }
    }

    bool IsValidTexture(Vector3 pos, int targetIndex, float threshold)
    {
        TerrainData tData = terrain.terrainData;
        Vector3 terrainPos = terrain.transform.position;
        // Convert world position to normalized terrain coordinates.
        float normX = (pos.x - terrainPos.x) / tData.size.x;
        float normZ = (pos.z - terrainPos.z) / tData.size.z;

        // Calculate the corresponding index in the alphamap.
        int mapX = Mathf.RoundToInt(normX * tData.alphamapWidth);
        int mapZ = Mathf.RoundToInt(normZ * tData.alphamapHeight);

        float[,,] splatmapData = tData.GetAlphamaps(mapX, mapZ, 1, 1);
        float value = splatmapData[0, 0, targetIndex];
        return (value >= threshold);
    }

    bool IsFarFromSpawned(Vector3 pos, float minSeparation)
    {
        foreach (Transform child in transform)
        {
            if (Vector3.Distance(pos, child.position) < minSeparation)
                return false;
        }
        return true;
    }
}
