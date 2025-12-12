using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotatoPeelerManager : MonoBehaviour
{
    [Header("Setup")]
    public GameObject[] potatoPrefabs;
    public GameObject peelerPrefab;
    public Transform potatoSpawnPoint;
    public Transform peelerStartPoint;

    [Header("Level Setup")]
    public int levelIndex;
    public int LevelIndex => levelIndex;

    [Header("Spawn Settings")]
    public int batchSize = 5;
    public float nextBatchDelay = 20f; 
    public TMP_Text[] countdownTexts;

    [Header("Progress Bar")]
    public ProgressBar progressBar; // Shared across minigames
    public float potatoDrainSpeed = 0.03f; // How fast bar drains while waiting
    public float potatoGainAmount = 0.2f;  // How much it gains per finished batch

    private PotatoPeeler peeler;
    private PotatoPeelSurface activePotato;
    private Queue<GameObject> potatoQueue = new Queue<GameObject>();
    private Vector3 peelerInitialPosition;
    private Coroutine countdownCoroutine;

    private bool countdownActive = false;
    private bool isActive = false; // <-- Added

    public bool InMinigame => isActive; // Needed for PotatoPeeler

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
        SpawnPeeler();

        if (StageCameraMover.CurrentLevel == levelIndex)
            ActivateMinigame();
        else
            DeactivateMinigame();
    }

    private void SpawnPeeler()
    {
        if (peeler != null) return; // Already spawned

        GameObject peelerObj = Instantiate(peelerPrefab, peelerStartPoint.position, Quaternion.identity);
        peeler = peelerObj.GetComponent<PotatoPeeler>();
        peeler.manager = this;

        peelerInitialPosition = peelerStartPoint.position;

        SpriteRenderer sr = peelerObj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "Foreground";
            sr.sortingOrder = 10;
        }
    }

    private void SpawnNewBatch()
    {
        if (!isActive) return;

        if (peeler != null)
            peeler.transform.position = peelerInitialPosition;

        potatoQueue.Clear();

        for (int i = 0; i < batchSize; i++)
        {
            GameObject potato = InstantiateRandomPotato();
            var surface = potato.GetComponent<PotatoPeelSurface>();
            surface.OnFullyPeeled += HandlePotatoPeeled;

            if (i == 0)
            {
                activePotato = surface;
                peeler.SetActivePotato(activePotato);
            }
            else
            {
                potatoQueue.Enqueue(potato);
                potato.SetActive(false);
            }
        }

        foreach (var text in countdownTexts)
            if (text != null) text.gameObject.SetActive(false);

        progressBar?.StartDraining(this, potatoDrainSpeed);
    }

    private GameObject InstantiateRandomPotato()
    {
        if (potatoPrefabs.Length == 0) return null;
        GameObject prefab = potatoPrefabs[Random.Range(0, potatoPrefabs.Length)];
        return Instantiate(prefab, potatoSpawnPoint.position, Quaternion.identity);
    }

    private void HandlePotatoPeeled(PotatoPeelSurface peeled)
    {
        if (!isActive) return;

        peeled.OnFullyPeeled -= HandlePotatoPeeled;
        Destroy(peeled.gameObject);

        if (potatoQueue.Count > 0)
        {
            GameObject next = potatoQueue.Dequeue();
            next.SetActive(true);
            activePotato = next.GetComponent<PotatoPeelSurface>();
            peeler.SetActivePotato(activePotato);
        }
        else
        {
            activePotato = null;
            peeler.ClearPotato();

            if (progressBar != null)
            {
                progressBar.StopDraining(this);
                progressBar.AddProgress(this, potatoGainAmount);
            }

            StartCountdown(nextBatchDelay);
        }
    }

    private void StartCountdown(float duration)
    {
        if (!isActive) return;

        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);

        countdownActive = true;
        progressBar?.StopDraining(this);

        countdownCoroutine = StartCoroutine(CountdownCoroutine(duration));
    }

    private IEnumerator CountdownCoroutine(float duration)
    {
        foreach (var text in countdownTexts)
            if (text != null) text.gameObject.SetActive(true);

        float remaining = duration;
        while (remaining > 0)
        {
            if (!isActive) yield break;

            string displayTime = Mathf.CeilToInt(remaining).ToString();
            foreach (var text in countdownTexts)
                if (text != null) text.text = displayTime;

            remaining -= Time.deltaTime;
            yield return null;
        }

        countdownActive = false;
        SpawnNewBatch();
    }

    private void ActivateMinigame()
    {
        if (isActive) return;
        isActive = true;

        SpawnPeeler();
        progressBar?.StartDraining(this, potatoDrainSpeed);
        StartCountdown(nextBatchDelay);
    }

    private void DeactivateMinigame()
    {
        if (!isActive) return;
        isActive = false;

        progressBar?.StopDraining(this);

        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);

        foreach (var t in countdownTexts)
            if (t != null) t.gameObject.SetActive(false);

        foreach (var p in potatoQueue)
            if (p != null) Destroy(p);
        potatoQueue.Clear();

        if (activePotato != null)
            Destroy(activePotato.gameObject);

        if (peeler != null)
            Destroy(peeler.gameObject);
    }
}


















