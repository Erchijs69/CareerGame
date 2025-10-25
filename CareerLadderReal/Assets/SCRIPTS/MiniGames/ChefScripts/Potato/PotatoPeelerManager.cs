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

    [Header("Spawn Settings")]
    public int batchSize = 5;
    public float nextBatchDelay = 20f; 
    public TMP_Text[] countdownTexts;

    [Header("Progress Bar")]
    public ProgressBar progressBar; // Assign in Inspector
    public float potatoDrainSpeed = 0.03f; // How fast bar drains while waiting
    public float potatoGainAmount = 0.2f;  // How much it gains per finished batch

    private PotatoPeeler peeler;
    private PotatoPeelSurface activePotato;
    private Queue<GameObject> potatoQueue = new Queue<GameObject>();
    private Vector3 peelerInitialPosition;
    private Coroutine countdownCoroutine;

    private bool countdownActive = false;


    public bool InMinigame => minigameZone != null && minigameZone.InMinigame;
    public MinigameZone minigameZone;

    void Start()
    {
        if (minigameZone == null)
            minigameZone = FindObjectOfType<MinigameZone>();

        SpawnPeeler();

        // Configure progress bar for this minigame
        if (progressBar != null)
        {
            progressBar.SetRates(potatoDrainSpeed, potatoGainAmount);
            progressBar.StartDraining(); // Start draining until potatoes are peeled
        }

        // Start first batch countdown
        StartCountdown(nextBatchDelay);
    }

    private void Update()
{
    if (potatoQueue.Count > 0 || activePotato != null)
        progressBar.StartDraining();
    else
        progressBar.StopDraining();
}



    private void SpawnPeeler()
    {
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
    // Reset peeler position
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
    {
        if (text != null)
            text.gameObject.SetActive(false);
    }

    // Only start draining if countdown is not active
    if (progressBar != null && !countdownActive)
        progressBar.StartDraining();
}



    private GameObject InstantiateRandomPotato()
    {
        if (potatoPrefabs.Length == 0) return null;
        GameObject prefab = potatoPrefabs[Random.Range(0, potatoPrefabs.Length)];
        GameObject potato = Instantiate(prefab, potatoSpawnPoint.position, Quaternion.identity);
        return potato;
    }

    private void HandlePotatoPeeled(PotatoPeelSurface peeled)
{
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
            progressBar.AddProgress();
        }

        // Start next countdown
        StartCountdown(nextBatchDelay);
    }
}


    private void StartCountdown(float duration)
{
    if (countdownCoroutine != null)
        StopCoroutine(countdownCoroutine);

    countdownActive = true;

    // Only stop draining if there are no active potatoes
    if (progressBar != null && activePotato == null)
        progressBar.StopDraining();

    countdownCoroutine = StartCoroutine(CountdownCoroutine(duration));
}



    private IEnumerator CountdownCoroutine(float duration)
{
    foreach (var text in countdownTexts)
    {
        if (text != null)
            text.gameObject.SetActive(true);
    }

    float remaining = duration;
    while (remaining > 0)
    {
        string displayTime = Mathf.CeilToInt(remaining).ToString();
        foreach (var text in countdownTexts)
        {
            if (text != null)
                text.text = displayTime;
        }

        remaining -= Time.deltaTime;
        yield return null;
    }

    countdownActive = false;

    // Spawn next batch
    SpawnNewBatch();
}

}
















