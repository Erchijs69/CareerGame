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
    public float minSpawnDelay = 10f;
    public float maxSpawnDelay = 30f;

    [Header("Stacking (optional)")]
    public float zOffsetPerPlate = 0.05f;

    [Header("Progress Bar")]
    public ProgressBar progressBar; // assign in Inspector
    public float drainSpeed = 0.05f; // drain while plates are capped
    public float gainAmount = 0.1f;  // gain per plate cleaned

    private List<GameObject> spawnedPlates = new List<GameObject>();
    private GameObject currentActivePlate = null;
    private Coroutine spawnRoutine;

    void Start()
    {
        // Set the progress bar rates from this script
        if (progressBar != null)
            progressBar.SetRates(drainSpeed, gainAmount);

        spawnRoutine = StartCoroutine(SpawnPlatesOverTime());
    }

    private void Update()
    {
        // Start/stop draining based on plate count
        if (progressBar != null)
        {
            if (spawnedPlates.Count >= maxPlates)
                progressBar.StartDraining();
            else
                progressBar.StopDraining();
        }
    }

    private IEnumerator SpawnPlatesOverTime()
    {
        while (true)
        {
            if (spawnedPlates.Count < maxPlates)
            {
                float waitTime = Random.Range(minSpawnDelay, maxSpawnDelay);
                yield return new WaitForSeconds(waitTime);

                if (spawnedPlates.Count < maxPlates)
                    SpawnAndPoolPlate();

                if (spawnedPlates.Count < maxPlates && progressBar != null)
                    progressBar.StopDraining();
            }
            else
            {
                if (progressBar != null)
                    progressBar.StartDraining();

                yield return null;
            }
        }
    }

    private void SpawnAndPoolPlate()
    {
        GameObject prefab = platePrefabs[Random.Range(0, platePrefabs.Length)];
        GameObject newPlate = Instantiate(prefab, plateSpawnParent);

        float zOffset = spawnedPlates.Count * zOffsetPerPlate;
        newPlate.transform.localPosition = new Vector3(0f, 0f, zOffset);
        newPlate.transform.localRotation = Quaternion.identity;
        newPlate.transform.localScale = Vector3.one;

        bool shouldActivateNow = (currentActivePlate == null);
        newPlate.SetActive(shouldActivateNow);
        if (shouldActivateNow)
            currentActivePlate = newPlate;

        spawnedPlates.Add(newPlate);

        DirtEraser eraser = newPlate.GetComponentInChildren<DirtEraser>();
        if (eraser != null)
        {
            eraser.manager = this;
            eraser.plateRoot = newPlate;
        }
    }

    public void PlateCleaned(GameObject cleanedPlate)
    {
        if (cleanedPlate == null) return;

        if (spawnedPlates.Contains(cleanedPlate))
            spawnedPlates.Remove(cleanedPlate);

        Destroy(cleanedPlate);

        if (currentActivePlate == cleanedPlate)
            currentActivePlate = null;

        if (spawnedPlates.Count > 0)
        {
            GameObject next = spawnedPlates[0];
            if (next != null && !next.activeSelf)
            {
                next.SetActive(true);
                currentActivePlate = next;
            }
        }
        else
        {
            currentActivePlate = null;
        }

        // Reward a bit of progress for cleaning a plate
        if (progressBar != null)
            progressBar.AddProgress();

        // Restart spawn routine
        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);
        spawnRoutine = StartCoroutine(SpawnPlatesOverTime());
    }

    public GameObject GetCurrentActivePlate()
    {
        return currentActivePlate;
    }
}








