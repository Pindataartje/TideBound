using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SpawnGroup
{
    [Header("Group Settings")]
    public string groupName;
    [Tooltip("Array of prefabs to spawn; one is chosen at random.")]
    public GameObject[] prefabs;
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

public class TerrainSpawner : MonoBehaviour
{
    [Header("Global Spawn Zones")]
    [Tooltip("Assign the spawn zone GameObjects that define valid spawn areas. " +
             "These are used if a spawn group doesn't have its own allowed zones.")]
    public GameObject[] spawnZones;  // Global spawn zones.

    [Header("Terrain Settings")]
    [Tooltip("Assign the terrains that cover your world. These are used for height sampling.")]
    public Terrain[] terrains;     // Array of terrains.

    [Header("Spawning Groups")]
    public SpawnGroup[] spawnGroups; // Different groups of spawns.

    [Header("Other Settings")]
    [Tooltip("Layers that count as obstacles (e.g., buildings).")]
    public LayerMask obstacleMask;
    public Transform player;
    [Tooltip("How often (in seconds) to check for spawn opportunities.")]
    public float checkInterval = 30f;

    void Start()
    {
        Debug.Log("Starting spawn routine.");
        StartCoroutine(SpawnRoutine());
    }

    // In play mode, periodically check each spawn group.
    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            foreach (SpawnGroup group in spawnGroups)
            {
                int currentCount = CountGroupSpawns(group);
                if (currentCount < group.maxCount)
                {
                    group.spawnTimer += checkInterval;
                    Debug.Log("Group: " + group.groupName + " timer: " + group.spawnTimer + " (" + currentCount + " spawned)");
                    if (group.spawnTimer >= group.spawnInterval)
                    {
                        if (CountGroupSpawns(group) < group.maxCount)
                        {
                            Debug.Log("Attempting spawn for group: " + group.groupName);
                            TrySpawn(group);
                        }
                        group.spawnTimer = 0f;
                    }
                }
            }
            yield return new WaitForSeconds(checkInterval);
        }
    }

    // Editor testing: spawn up to maxCount objects for each spawn group.
    [ContextMenu("Spawn Test")]
    public void SpawnTest()
    {
        Debug.Log("Spawn Test triggered.");
        foreach (SpawnGroup group in spawnGroups)
        {
            SpawnGroupTest(group);
        }
    }

    // Spawns objects for a single group until max count is reached.
    public void SpawnGroupTest(SpawnGroup group)
    {
        int safetyCounter = 0;
        // Loop until we reach maxCount or a safety limit (to avoid infinite loops)
        while (CountGroupSpawns(group) < group.maxCount && safetyCounter < group.maxCount * 10)
        {
            TrySpawn(group);
            safetyCounter++;
        }
        Debug.Log("Spawn Test finished for group: " + group.groupName + ". Spawned " + CountGroupSpawns(group) + " objects.");
    }


    // Counts spawned objects by checking if their name starts with the group identifier.
    int CountGroupSpawns(SpawnGroup group)
    {
        int count = 0;
        string identifier = group.groupName + "_Spawned";
        if (group.allowedSpawnZones != null && group.allowedSpawnZones.Length > 0)
        {
            foreach (GameObject zone in group.allowedSpawnZones)
            {
                foreach (Transform child in zone.transform)
                {
                    if (child.name.StartsWith(identifier))
                        count++;
                }
            }
        }
        else
        {
            foreach (Transform child in transform)
            {
                if (child.name.StartsWith(identifier))
                    count++;
            }
        }
        return count;
    }

    // Attempts to spawn one object for the given group.
    bool TrySpawn(SpawnGroup group)
    {
        // Use more attempts in Editor mode.
        int attempts = Application.isEditor ? 100 : 10;
        bool spawned = false;
        while (attempts > 0 && !spawned)
        {
            attempts--;

            // Determine which spawn zones to use.
            GameObject[] zonesToUse = (group.allowedSpawnZones != null && group.allowedSpawnZones.Length > 0)
                                      ? group.allowedSpawnZones
                                      : spawnZones;
            if (zonesToUse == null || zonesToUse.Length == 0)
            {
                Debug.LogWarning("No spawn zones available for group: " + group.groupName);
                return false;
            }

            // Pick a random spawn zone.
            GameObject zoneObj = zonesToUse[Random.Range(0, zonesToUse.Length)];
            Collider[] zoneColliders = zoneObj.GetComponentsInChildren<Collider>();
            if (zoneColliders.Length == 0)
            {
                Debug.LogWarning("Spawn zone " + zoneObj.name + " has no colliders.");
                continue;
            }
            Collider zone = zoneColliders[Random.Range(0, zoneColliders.Length)];

            // Get a random point inside the collider (with center bias).
            Vector3 pos = GetRandomPointInCollider(zone, group.centerBias);
            pos.y = GetTerrainHeight(pos);

            // Check conditions.
            if (player != null && Vector3.Distance(pos, player.position) < group.minDistanceFromPlayer)
            {
                Debug.Log("Position too close to player: " + pos);
                continue;
            }
            if (!IsFarFromSpawned(pos, group.minSeparation))
            {
                Debug.Log("Position too close to another spawned object: " + pos);
                continue;
            }
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

            // Spawn a random prefab from the group's array.
            if (group.prefabs == null || group.prefabs.Length == 0)
            {
                Debug.Log("No prefab assigned for group: " + group.groupName);
                return false;
            }
            GameObject chosenPrefab = group.prefabs[Random.Range(0, group.prefabs.Length)];
            // Apply random rotation on Y and slight random tilt on X and Z.
            float randomY = Random.Range(0f, 360f);
            float randomX = Random.Range(-2f, 2f);
            float randomZ = Random.Range(-2f, 2f);
            Quaternion randomRotation = Quaternion.Euler(randomX, randomY, randomZ);

            // Determine parent: if group has allowed zones, use the chosen zone; otherwise, use the spawner.
            Transform parentTransform = (group.allowedSpawnZones != null && group.allowedSpawnZones.Length > 0)
                                          ? zoneObj.transform
                                          : transform;
            GameObject spawnedObj = Instantiate(chosenPrefab, pos, randomRotation, parentTransform);
            spawnedObj.name = group.groupName + "_Spawned_" + chosenPrefab.name;
            Debug.Log("Spawned " + chosenPrefab.name + " at position: " + pos + " for group: " + group.groupName);
            spawned = true;
        }

        if (!spawned)
        {
            Debug.Log("Failed to spawn any object for group: " + group.groupName + " after maximum attempts.");
        }
        return spawned;
    }

    // Returns a random point within the collider's bounds, biased toward its center.
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

    // Clears all spawned objects for the specified group.
    public void ClearGroup(string groupName, SpawnGroup group)
    {
        string identifier = group.groupName + "_Spawned";
        if (group.allowedSpawnZones != null && group.allowedSpawnZones.Length > 0)
        {
            foreach (GameObject zoneObj in group.allowedSpawnZones)
            {
                ClearChildrenInParent(zoneObj, identifier);
            }
        }
        else
        {
            ClearChildrenInParent(this.gameObject, identifier);
        }
    }

    // Helper to clear children whose names start with the given identifier.
    private void ClearChildrenInParent(GameObject parent, string identifier)
    {
        List<Transform> toClear = new List<Transform>();
        foreach (Transform child in parent.transform)
        {
            if (child.name.StartsWith(identifier))
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
        Debug.Log("Cleared " + toClear.Count + " objects from group: " + identifier + " under " + parent.name);
    }
}
