using UnityEngine;
using System.Collections.Generic;

public class BirdSpawner : MonoBehaviour
{
    [Header("Spawn Area Settings")]
    [Tooltip("The BoxCollider that defines the spawn area (sky area) for birds.")]
    public BoxCollider spawnArea;

    [Header("Bird Settings")]
    [Tooltip("The bird prefab (UI image or 3D quad) to spawn. It must have the Bird component attached.")]
    public GameObject birdPrefab;
    [Tooltip("The world-space Canvas that will contain the bird images.")]
    public Canvas birdCanvas;
    [Tooltip("Maximum number of birds to maintain.")]
    public int maxBirds = 10;
    [Tooltip("Time interval (in seconds) between spawn attempts.")]
    public float spawnInterval = 5f;
    [Tooltip("Maximum allowed spawn distance from the player.")]
    public float maxSpawnDistanceFromPlayer = 500f;

    [Header("Player Reference")]
    public Transform player;

    // Keep track of spawned birds.
    private List<GameObject> spawnedBirds = new List<GameObject>();

    void Start()
    {
        // Spawn initial birds.
        for (int i = 0; i < maxBirds; i++)
        {
            SpawnBird();
        }
        // Maintain the bird count over time.
        InvokeRepeating(nameof(MaintainBirds), spawnInterval, spawnInterval);
    }

    void MaintainBirds()
    {
        // Remove any destroyed (null) birds.
        spawnedBirds.RemoveAll(b => b == null);
        while (spawnedBirds.Count < maxBirds)
        {
            SpawnBird();
        }
    }

    void SpawnBird()
    {
        Vector3 spawnPos = GetValidSpawnPosition();
        if (spawnPos == Vector3.zero)
            return;
        // Instantiate the bird as a child of the canvas so it's visible.
        GameObject bird = Instantiate(birdPrefab, spawnPos, Quaternion.identity, birdCanvas.transform);
        // Set up the bird movement area so it stays inside the spawn area.
        Bird birdScript = bird.GetComponent<Bird>();
        if (birdScript != null)
        {
            birdScript.SetRandomSpeed();
            birdScript.movementArea = spawnArea;
        }
        spawnedBirds.Add(bird);
    }

    // Finds a valid spawn position inside the spawn area and within max distance from the player.
    Vector3 GetValidSpawnPosition()
    {
        Vector3 spawnPos = Vector3.zero;
        int attempts = 10;
        while (attempts > 0)
        {
            spawnPos = GetRandomPointInBox(spawnArea);
            if (player != null)
            {
                if (Vector3.Distance(spawnPos, player.position) <= maxSpawnDistanceFromPlayer)
                {
                    return spawnPos;
                }
            }
            else
            {
                return spawnPos;
            }
            attempts--;
        }
        return spawnPos;
    }

    // Returns a random point within the BoxCollider's world bounds.
    Vector3 GetRandomPointInBox(BoxCollider box)
    {
        Bounds bounds = box.bounds;
        float x = Random.Range(bounds.min.x, bounds.max.x);
        float y = Random.Range(bounds.min.y, bounds.max.y);
        float z = Random.Range(bounds.min.z, bounds.max.z);
        return new Vector3(x, y, z);
    }
}
