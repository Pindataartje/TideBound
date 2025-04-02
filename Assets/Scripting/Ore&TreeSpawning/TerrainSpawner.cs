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

    // Preallocated buffer for Physics.OverlapSphereNonAlloc (adjust size as needed)
    private Collider[] overlapBuffer = new Collider[20];

    // Reusable list to avoid allocations in GetComponentsInChildren.
    private static List<Collider> tempZoneColliders = new List<Collider>();

    void Start()
    {
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
                    if (group.spawnTimer >= group.spawnInterval)
                    {
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

    // Editor testing: spawn up to maxCount objects for each spawn group.
    [ContextMenu("Spawn Test")]
    public void SpawnTest()
    {
        foreach (SpawnGroup group in spawnGroups)
        {
            SpawnGroupTest(group);
        }
    }

    // Spawns objects for a single group until max count is reached.
    public void SpawnGroupTest(SpawnGroup group)
    {
        int safetyCounter = 0;
        while (CountGroupSpawns(group) < group.maxCount && safetyCounter < group.maxCount * 10)
        {
            TrySpawn(group);
            safetyCounter++;
        }
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
                Transform zoneTransform = zone.transform;
                int childCount = zoneTransform.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    if (zoneTransform.GetChild(i).name.StartsWith(identifier))
                        count++;
                }
            }
        }
        else
        {
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                if (transform.GetChild(i).name.StartsWith(identifier))
                    count++;
            }
        }
        return count;
    }

    // Attempts to spawn one object for the given group.
    bool TrySpawn(SpawnGroup group)
    {
        int attempts = Application.isEditor ? 100 : 10;
        bool spawned = false;
        while (attempts > 0 && !spawned)
        {
            attempts--;

            GameObject[] zonesToUse = (group.allowedSpawnZones != null && group.allowedSpawnZones.Length > 0)
                                      ? group.allowedSpawnZones
                                      : spawnZones;
            if (zonesToUse == null || zonesToUse.Length == 0)
            {
                return false;
            }

            GameObject zoneObj = zonesToUse[Random.Range(0, zonesToUse.Length)];
            tempZoneColliders.Clear();
            zoneObj.GetComponentsInChildren<Collider>(tempZoneColliders);
            if (tempZoneColliders.Count == 0)
            {
                continue;
            }

            // Pick a random collider from the list.
            Collider zone = tempZoneColliders[Random.Range(0, tempZoneColliders.Count)];

            // Get a random spawn point inside the collider
            Vector3 pos = GetRandomPointInCollider(zone, group.centerBias);

            // If no valid spawn position was found (e.g., outside the terrain area), skip this attempt
            if (pos == Vector3.zero)
            {
                continue;
            }

            pos.y = GetTerrainHeight(pos);

            // Check distance from player and separation from other objects
            if (player != null && Vector3.Distance(pos, player.position) < group.minDistanceFromPlayer)
            {
                continue;
            }

            if (!IsFarFromSpawned(pos, group.minSeparation))
            {
                continue;
            }

            // Check for obstacles
            int overlapCount = Physics.OverlapSphereNonAlloc(pos, group.minSeparation, overlapBuffer, obstacleMask);
            bool foundObstacle = false;
            for (int i = 0; i < overlapCount; i++)
            {
                Collider col = overlapBuffer[i];
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
                continue;
            }

            if (group.prefabs == null || group.prefabs.Length == 0)
            {
                return false;
            }

            GameObject chosenPrefab = group.prefabs[Random.Range(0, group.prefabs.Length)];
            string prefabName = chosenPrefab.name;
            float randomY = Random.Range(0f, 360f);
            float randomX = Random.Range(-2f, 2f);
            float randomZ = Random.Range(-2f, 2f);
            Quaternion randomRotation = Quaternion.Euler(randomX, randomY, randomZ);

            Transform parentTransform = (group.allowedSpawnZones != null && group.allowedSpawnZones.Length > 0)
                                          ? zoneObj.transform
                                          : transform;
            GameObject spawnedObj = Instantiate(chosenPrefab, pos, randomRotation, parentTransform);
            spawnedObj.name = group.groupName + "_Spawned_" + prefabName;
            spawned = true;
        }

        return spawned;
    }


    // Returns a random point within the collider's bounds, biased toward its center.
    // Returns a random point within the collider's bounds, biased toward its center, and adjusted to the terrain height.
    // Returns a random point within the collider's bounds, biased toward its center, and adjusted to the terrain height.
    // Returns a random point within the collider's bounds, biased toward its center, and adjusted to the terrain height.
    Vector3 GetRandomPointInCollider(Collider col, float centerBias)
    {
        for (int i = 0; i < 10; i++)
        {
            // Generate a random point within the collider's bounds
            Vector3 randomPoint = new Vector3(
                Random.Range(col.bounds.min.x, col.bounds.max.x),
                Random.Range(col.bounds.min.y, col.bounds.max.y),
                Random.Range(col.bounds.min.z, col.bounds.max.z)
            );

            // Get the closest point on the collider's surface
            Vector3 closest = col.ClosestPoint(randomPoint);

            // Adjust the point to be biased towards the center if needed
            if ((randomPoint - closest).sqrMagnitude < 0.001f)
            {
                // Bias the position to the center of the collider if centerBias is > 0
                randomPoint = Vector3.Lerp(randomPoint, col.bounds.center, centerBias);

                // Get terrain height at this point
                float terrainHeight = GetTerrainHeight(randomPoint);

                // Ensure the spawn position is above the terrain, not below it
                randomPoint.y = Mathf.Max(randomPoint.y, terrainHeight);

                // If the terrain height is too low (e.g., over the ocean), do not spawn the object
                if (randomPoint.y < terrainHeight)
                {
                    randomPoint.y = terrainHeight;  // Correct to the terrain height
                }

                // If the spawn position is too far above the terrain (e.g., ocean), skip this spawn
                if (randomPoint.y < col.bounds.min.y || randomPoint.y > col.bounds.max.y)
                {
                    return Vector3.zero;  // Don't spawn if it's outside the valid area
                }

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
        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            if (Vector3.Distance(pos, transform.GetChild(i).position) < minSeparation)
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
            ClearChildrenInParent(gameObject, identifier);
        }
    }

    // Helper to clear children whose names start with the given identifier.
    private void ClearChildrenInParent(GameObject parent, string identifier)
    {
        List<Transform> toClear = new List<Transform>();
        Transform parentTransform = parent.transform;
        int childCount = parentTransform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform child = parentTransform.GetChild(i);
            if (child.name.StartsWith(identifier))
                toClear.Add(child);
        }
        for (int i = 0; i < toClear.Count; i++)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(toClear[i].gameObject);
            else
                Destroy(toClear[i].gameObject);
#else
            Destroy(toClear[i].gameObject);
#endif
        }
    }
}
