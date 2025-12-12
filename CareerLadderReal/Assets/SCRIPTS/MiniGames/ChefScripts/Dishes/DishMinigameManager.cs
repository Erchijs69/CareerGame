using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DishMinigameManager : MonoBehaviour
{
    [Header("Plate Settings")]
    public GameObject platePrefab;
    public Transform plateSpawnParent;
    public GameObject[] platePrefabs;
    public int maxPlates = 8;

    [Header("Level Setup")]
    public int levelIndex;
    public int LevelIndex => levelIndex;

    [Header("Spawn Timing")]
    public float minSpawnDelay = 10f;
    public float maxSpawnDelay = 30f;

    [Header("Stacking (optional)")]
    public float zOffsetPerPlate = 0.05f;

    [Header("Progress Bar (shared)")]
    public ProgressBar progressBar; // shared across minigames
    public float dishDrainSpeed = 0.05f; // drain while plates are capped
    public float dishGainAmount = 0.1f;  // gain per plate cleaned

    private List<GameObject> spawnedPlates = new List<GameObject>();
    private GameObject currentActivePlate = null;
    private Coroutine spawnRoutine;
    private bool active = false;

    void OnEnable()
    {
        StageCameraMover.OnCameraStageSwitched += HandleStageSwitch;
    }

    void OnDisable()
    {
        StageCameraMover.OnCameraStageSwitched -= HandleStageSwitch;
    }

    private void HandleStageSwitch()
    {
        if (StageCameraMover.CurrentLevel == levelIndex)
            ActivateMinigame();
        else
            DeactivateMinigame();
    }

    void Start()
    {
        if (StageCameraMover.CurrentLevel == levelIndex)
            ActivateMinigame();
        else
            DeactivateMinigame();
    }

    private void Update()
    {
        if (!active) return;

        if (progressBar != null)
        {
            if (spawnedPlates.Count >= maxPlates)
                progressBar.StartDraining(this, dishDrainSpeed);
            else
                progressBar.StopDraining(this);
        }
    }

    private void ActivateMinigame()
    {
        if (active) return;
        active = true;

        // Enable all plates
        foreach (var p in spawnedPlates)
            if (p != null) p.SetActive(true);

        // Set current active plate if null
        if (currentActivePlate == null && spawnedPlates.Count > 0)
            currentActivePlate = spawnedPlates[0];

        // Start spawning
        if (spawnRoutine == null)
            spawnRoutine = StartCoroutine(SpawnPlatesOverTime());

        // Start draining
        if (progressBar != null)
            progressBar.StartDraining(this, dishDrainSpeed);
    }

    private void DeactivateMinigame()
    {
        if (!active) return;
        active = false;

        // Stop draining
        if (progressBar != null)
            progressBar.StopDraining(this);

        // Stop spawning
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        // Disable all plates
        foreach (var p in spawnedPlates)
            if (p != null) p.SetActive(false);

        currentActivePlate = null;
    }

    private IEnumerator SpawnPlatesOverTime()
    {
        while (active)
        {
            if (spawnedPlates.Count < maxPlates)
            {
                float waitTime = Random.Range(minSpawnDelay, maxSpawnDelay);
                yield return new WaitForSeconds(waitTime);

                if (!active) yield break;

                if (spawnedPlates.Count < maxPlates)
                    SpawnAndPoolPlate();

                if (spawnedPlates.Count < maxPlates && progressBar != null)
                    progressBar.StopDraining(this);
            }
            else
            {
                if (progressBar != null)
                    progressBar.StartDraining(this, dishDrainSpeed);

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

        bool shouldActivateNow = (currentActivePlate == null && active);
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
        if (!active || cleanedPlate == null) return;

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

        if (progressBar != null)
        {
            progressBar.AddProgress(this, dishGainAmount);
            progressBar.StopDraining(this);
        }
    }

    public GameObject GetCurrentActivePlate()
    {
        return currentActivePlate;
    }
}












