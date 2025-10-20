using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DishMinigameManager : MonoBehaviour
{
    [Header("Plate Settings")]
    public GameObject platePrefab;
    public Transform plateSpawnParent;
    public GameObject[] platePrefabs;
    public int maxPlates = 8;

    [Header("Spawn Timing")]
    [Tooltip("Minimum and maximum delay before the next plate spawns (seconds)")]
    public float minSpawnDelay = 10f;
    public float maxSpawnDelay = 30f;

    [Header("Stacking (optional)")]
    [Tooltip("How far back each subsequent plate should appear on the Z-axis.")]
    public float zOffsetPerPlate = 0.05f;

    // Pool of all spawned plates (both active and inactive). The active one is tracked separately.
    private List<GameObject> spawnedPlates = new List<GameObject>();
    private GameObject currentActivePlate = null;
    private Coroutine spawnRoutine;

    void Start()
    {
        spawnRoutine = StartCoroutine(SpawnPlatesOverTime());
    }

    private IEnumerator SpawnPlatesOverTime()
    {
        while (true)
        {
            // If we haven't reached the pool limit, wait a random delay then spawn one.
            if (spawnedPlates.Count < maxPlates)
            {
                float waitTime = Random.Range(minSpawnDelay, maxSpawnDelay);
                yield return new WaitForSeconds(waitTime);

                // Double-check in case something changed while waiting
                if (spawnedPlates.Count < maxPlates)
                    SpawnAndPoolPlate();
            }
            else
            {
                // Pool full — wait until the next frame and check again
                yield return null;
            }
        }
    }

    private void SpawnAndPoolPlate()
{
    // Pick a random plate prefab from the array
    GameObject prefab = platePrefabs[Random.Range(0, platePrefabs.Length)];

    // Instantiate the chosen prefab
    GameObject newPlate = Instantiate(prefab, plateSpawnParent);

    // Optional Z offset stacking
    float zOffset = spawnedPlates.Count * zOffsetPerPlate;
    newPlate.transform.localPosition = new Vector3(0f, 0f, zOffset);
    newPlate.transform.localRotation = Quaternion.identity;
    newPlate.transform.localScale = Vector3.one;

    // Activate only if no plate is currently active
    bool shouldActivateNow = (currentActivePlate == null);
    newPlate.SetActive(shouldActivateNow);
    if (shouldActivateNow)
        currentActivePlate = newPlate;

    spawnedPlates.Add(newPlate);

    // Wire the DirtEraser
    DirtEraser eraser = newPlate.GetComponentInChildren<DirtEraser>();
    if (eraser != null)
    {
        eraser.manager = this;
        eraser.plateRoot = newPlate;
    }

    Debug.Log($"Spawned & pooled plate #{spawnedPlates.Count} ({prefab.name}) Active: {shouldActivateNow})");
}


    // Called by DirtEraser when a plate is fully cleaned (passes its plateRoot)
    public void PlateCleaned(GameObject cleanedPlate)
    {
        if (cleanedPlate == null) return;

        // Remove from pool if present
        if (spawnedPlates.Contains(cleanedPlate))
            spawnedPlates.Remove(cleanedPlate);

        // Destroy the cleaned plate
        Destroy(cleanedPlate);
        Debug.Log($"Plate cleaned! Pool size now: {spawnedPlates.Count}");

        // Clear currentActivePlate if it was the cleaned one
        if (currentActivePlate == cleanedPlate)
            currentActivePlate = null;

        // Activate the next pooled plate (the first one in the list), if any
        if (spawnedPlates.Count > 0)
        {
            GameObject next = spawnedPlates[0];
            // If next was inactive, activate it and set as current
            if (next != null && !next.activeSelf)
            {
                next.SetActive(true);
                currentActivePlate = next;
                Debug.Log("Activated next pooled plate.");
            }
            else if (next != null && next.activeSelf)
            {
                // safety: if it's already active, ensure currentActivePlate references it
                currentActivePlate = next;
            }
        }
        else
        {
            // No pooled plates available — currentActivePlate stays null until spawn fills pool again
            currentActivePlate = null;
        }

        // Reset spawn cooldown: restart the spawn coroutine so it will begin waiting again
        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);
        spawnRoutine = StartCoroutine(SpawnPlatesOverTime());
    }

    // Optional helper: returns the currently interactable plate (null if none)
    public GameObject GetCurrentActivePlate()
    {
        return currentActivePlate;
    }
}





