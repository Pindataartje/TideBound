using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SpawnGroup
{
    [Header("Group Settings")]
    public string groupName;
    public GameObject prefab;           // The prefab to spawn.
    public int maxCount = 10;           // Maximum spawned objects allowed.
    public float spawnInterval = 300f;  // Time in seconds between spawns.

    [Header("Distance & Placement Settings")]
    public float minDistanceFromPlayer = 10f; // Minimum distance from the player.
    public float minSeparation = 5f;    // Minimum separation from other spawned objects.
    [Tooltip("0 = uniformly random within zone, 1 = always at center of zone")]
    public float centerBias = 0.5f;     // Bias toward the center of the spawn zone.

    [Header("Spawn Zone Overrides")]
    [Tooltip("Optional: Assign specific spawn zone GameObjects for this group. " +
             "If empty, the global spawn zones will be used.")]
    public GameObject[] allowedSpawnZones;

    [HideInInspector]
    public float spawnTimer = 0f;       // Internal timer for spawn intervals.
}

// This component is added to spawned objects so we can later identify which group they belong to.
public class SpawnGroupIdentifier : MonoBehaviour
{
    public string groupName;
}

public class TerrainSpawner : MonoBehaviour
{
    [Header("Global Spawn Zones")]
    [Tooltip("Assign the spawn zone GameObjects that define valid spawn areas. " +
             "These are used if a spawn group doesn't have its own allowed zones.")]
    public GameObject[] spawnZones;  // Global spawn zones (GameObjects with one or more colliders).

    [Header("Terrain Settings")]
    [Tooltip("Assign the terrains that cover your world. These are used for height sampling.")]
    public Terrain[] terrains;     // Array of terrains (for sampling height).

    [Header("Spawning Groups")]
    public SpawnGroup[] spawnGroups; // Different groups of spawns with their own settings.

    [Header("Other Settings")]
    [Tooltip("Layers that count as obstacles (e.g., buildings).")]
    public LayerMask obstacleMask;   // Layers to consider obstacles.
    public Transform player;         // Player reference.
    [Tooltip("How often (in seconds) to check for spawn opportunities.")]
    public float checkInterval = 30f;  // Frequency of spawn checks.

    void Start()
    {
        Debug.Log("Starting spawn routine.");
        StartCoroutine(SpawnRoutine());
    }

    // In play mode, this coroutine checks each spawn group periodically.
    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            foreach (SpawnGroup group in spawnGroups)
            {
                // Count spawned objects for this group.
                int currentCount = CountGroupSpawns(group);
                if (currentCount < group.maxCount)
                {
                    group.spawnTimer += checkInterval;
                    Debug.Log("Group: " + group.groupName + " timer: " + group.spawnTimer + " (" + currentCount + " spawned)");
                    if (group.spawnTimer >= group.spawnInterval)
                    {
                        Debug.Log("Attempting spawn for group: " + group.groupName);
                        // Only try to spawn if group hasn't reached max count.
                        if (CountGroupSpawns(group) < group.maxCount)
                        {
                            TrySpawn(group);
                        }
                        group.spawnTimer = 0f;
                    }
                }
            }
            yield return new WaitForSeconds(checkInterval);
        }
    }

    // Editor testing: attempts to spawn the maximum number of objects for each spawn group.
    [ContextMenu("Spawn Test")]
    public void SpawnTest()
    {
        Debug.Log("Spawn Test triggered.");
        foreach (SpawnGroup group in spawnGroups)
        {
            int spawnedCount = 0;
            for (int i = 0; i < group.maxCount; i++)
            {
                // Only attempt spawn if current count is below max.
                if (CountGroupSpawns(group) >= group.maxCount)
                    break;
                bool success = TrySpawn(group);
                if (success)
                    spawnedCount++;
                else
                {
                    Debug.Log("Failed to spawn for group: " + group.groupName + " on iteration " + i);
                    break;
                }
            }
            Debug.Log("Spawn Test finished for group: " + group.groupName + ". Spawned " + spawnedCount + " objects.");
        }
    }

    // Counts how many spawned objects belong to the given group.
    int CountGroupSpawns(SpawnGroup group)
    {
        int count = 0;
        // If this group uses its own spawn zones, check each one.
        if (group.allowedSpawnZones != null && group.allowedSpawnZones.Length > 0)
        {
            foreach (GameObject zone in group.allowedSpawnZones)
            {
                foreach (Transform child in zone.transform)
                {
                    SpawnGroupIdentifier id = child.GetComponent<SpawnGroupIdentifier>();
                    if (id != null && id.groupName == group.groupName)
                        count++;
                }
            }
        }
        else
        {
            // Otherwise, check all children of this spawner.
            foreach (Transform child in transform)
            {
                SpawnGroupIdentifier id = child.GetComponent<SpawnGroupIdentifier>();
                if (id != null && id.groupName == group.groupName)
                    count++;
            }
        }
        return count;
    }

    // Attempts to spawn one object for the given spawn group.
    // Returns true if an object was spawned.
    bool TrySpawn(SpawnGroup group)
    {
        int attempts = 10;
        bool spawned = false;
        while (attempts > 0 && !spawned)
        {
            attempts--;

            // --- Determine which spawn zones to use ---
            GameObject[] zonesToUse = (group.allowedSpawnZones != null && group.allowedSpawnZones.Length > 0)
                                      ? group.allowedSpawnZones
                                      : spawnZones;

            if (zonesToUse == null || zonesToUse.Length == 0)
            {
                Debug.LogWarning("No spawn zones available for group: " + group.groupName);
                return false;
            }

            // Pick a random spawn zone from the chosen set.
            GameObject zoneObj = zonesToUse[Random.Range(0, zonesToUse.Length)];
            Collider[] zoneColliders = zoneObj.GetComponentsInChildren<Collider>();
            if (zoneColliders.Length == 0)
            {
                Debug.LogWarning("Spawn zone " + zoneObj.name + " has no colliders.");
                continue;
            }
            Collider zone = zoneColliders[Random.Range(0, zoneColliders.Length)];

            // --- Get a random point inside the collider (with center bias) ---
            Vector3 pos = GetRandomPointInCollider(zone, group.centerBias);

            // --- Adjust the position's Y by sampling the appropriate terrain ---
            pos.y = GetTerrainHeight(pos);

            // --- Check conditions ---
            // (1) Distance from the player.
            if (player != null && Vector3.Distance(pos, player.position) < group.minDistanceFromPlayer)
            {
                Debug.Log("Position too close to player: " + pos);
                continue;
            }
            // (2) Separation from other spawned objects.
            if (!IsFarFromSpawned(pos, group.minSeparation))
            {
                Debug.Log("Position too close to another spawned object: " + pos);
                continue;
            }
            // (3) Check for obstacles. Ignore colliders that belong to assigned terrains.
            Collider[] overlapping = Physics.OverlapSphere(pos, group.minSeparation, obstacleMask);
            bool foundObstacle = false;
            foreach (Collider col in overlapping)
            {
                bool isTerrain = false;
                foreach (Terrain t in terrains)
                {
                    if (col.gameObject == t.gameObject)
                    {
                        isTerrain = true;
                        break;
                    }
                }
                if (!isTerrain)
                {
                    foundObstacle = true;
                    break;
                }
            }
            if (foundObstacle)
            {
                Debug.Log("Obstacle detected at position: " + pos);
                continue;
            }

            // --- Spawn the prefab ---
            if (group.prefab == null)
            {
                Debug.Log("No prefab assigned for group: " + group.groupName);
                return false;
            }
            // Parent under the allowed spawn zone if set; otherwise, parent to this spawner.
            Transform parentTransform = (group.allowedSpawnZones != null && group.allowedSpawnZones.Length > 0)
                                          ? zoneObj.transform
                                          : transform;
            GameObject spawnedObj = Instantiate(group.prefab, pos, Quaternion.identity, parentTransform);
            // Tag it with its group.
            SpawnGroupIdentifier id = spawnedObj.AddComponent<SpawnGroupIdentifier>();
            id.groupName = group.groupName;

            Debug.Log("Spawned " + group.prefab.name + " at position: " + pos + " for group: " + group.groupName);
            spawned = true;
        }

        if (!spawned)
        {
            Debug.Log("Failed to spawn any object for group: " + group.groupName + " after 10 attempts.");
        }
        return spawned;
    }

    // Returns a random point within the collider's bounds,
    // and biases it toward the collider's center based on centerBias (0 = no bias, 1 = always center).
    Vector3 GetRandomPointInCollider(Collider col, float centerBias)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomPoint = new Vector3(
                Random.Range(col.bounds.min.x, col.bounds.max.x),
                Random.Range(col.bounds.min.y, col.bounds.max.y),
                Random.Range(col.bounds.min.z, col.bounds.max.z)
            );
            Vector3 closest = col.ClosestPoint(randomPoint);
            if ((randomPoint - closest).sqrMagnitude < 0.001f)
            {
                Vector3 center = col.bounds.center;
                randomPoint = Vector3.Lerp(randomPoint, center, centerBias);
                return randomPoint;
            }
        }
        return col.bounds.center;
    }

    // Returns the terrain height at the given (x,z) position.
    float GetTerrainHeight(Vector3 pos)
    {
        foreach (Terrain t in terrains)
        {
            Vector3 terrainPos = t.transform.position;
            Vector3 terrainSize = t.terrainData.size;
            if (pos.x >= terrainPos.x && pos.x <= terrainPos.x + terrainSize.x &&
                pos.z >= terrainPos.z && pos.z <= terrainPos.z + terrainSize.z)
            {
                return t.SampleHeight(pos) + t.transform.position.y;
            }
        }
        return pos.y;
    }

    // Checks that the spawn point is far enough from other spawned objects.
    bool IsFarFromSpawned(Vector3 pos, float minSeparation)
    {
        foreach (Transform child in transform)
        {
            if (Vector3.Distance(pos, child.position) < minSeparation)
                return false;
        }
        return true;
    }
    // Clears all spawned objects that belong to the specified group.
    public void ClearGroup(string groupName, SpawnGroup group)
    {
        // If this group uses its own spawn zones, clear children from those objects.
        if (group.allowedSpawnZones != null && group.allowedSpawnZones.Length > 0)
        {
            foreach (GameObject zoneObj in group.allowedSpawnZones)
            {
                ClearChildrenInParent(zoneObj, groupName);
            }
        }
        else
        {
            // Otherwise, clear matching children from this spawner.
            ClearChildrenInParent(this.gameObject, groupName);
        }
    }

    // Helper method to clear children with matching group name.
    private void ClearChildrenInParent(GameObject parent, string groupName)
    {
        List<Transform> toClear = new List<Transform>();
        foreach (Transform child in parent.transform)
        {
            SpawnGroupIdentifier id = child.GetComponent<SpawnGroupIdentifier>();
            if (id != null && id.groupName == groupName)
                toClear.Add(child);
        }
        foreach (Transform child in toClear)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(child.gameObject);
            else
                Destroy(child.gameObject);
#else
        Destroy(child.gameObject);
#endif
        }
        Debug.Log("Cleared " + toClear.Count + " objects from group: " + groupName + " under " + parent.name);
    }

}
