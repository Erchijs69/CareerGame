using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class PotatoPeelerManager : MonoBehaviour
{
    [Header("Setup")]
    public GameObject[] potatoPrefabs;
    public GameObject peelerPrefab;
    public Transform potatoSpawnPoint;
    public Transform peelerStartPoint;

    [Header("Spawn Settings")]
    public int batchSize = 5;
    public float nextBatchDelay = 20f; // Can be changed
    public TMP_Text[] countdownTexts;  // Assign all TMP texts here

    private PotatoPeeler peeler;
    private PotatoPeelSurface activePotato;
    private Queue<GameObject> potatoQueue = new Queue<GameObject>();
    private Vector3 peelerInitialPosition;
    private Coroutine countdownCoroutine;

    public bool InMinigame => minigameZone != null && minigameZone.InMinigame;
    public MinigameZone minigameZone;

    void Start()
    {
        if (minigameZone == null)
            minigameZone = FindObjectOfType<MinigameZone>();

        SpawnPeeler();

        // Start first batch countdown
        StartCountdown(nextBatchDelay);
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

        // Clear leftover potatoes
        potatoQueue.Clear();

        // Spawn batchSize potatoes
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

        // Hide countdown texts when batch spawns
        foreach (var text in countdownTexts)
        {
            if (text != null)
                text.gameObject.SetActive(false);
        }
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

        // Promote next potato
        if (potatoQueue.Count > 0)
        {
            GameObject next = potatoQueue.Dequeue();
            next.SetActive(true);
            activePotato = next.GetComponent<PotatoPeelSurface>();
            peeler.SetActivePotato(activePotato);
        }
        else
        {
            // Batch finished
            activePotato = null;
            peeler.ClearPotato();

            // Start next batch countdown
            StartCountdown(nextBatchDelay);
        }
    }

    private void StartCountdown(float duration)
    {
        if (countdownCoroutine != null) StopCoroutine(countdownCoroutine);
        countdownCoroutine = StartCoroutine(CountdownCoroutine(duration));
    }

    private IEnumerator CountdownCoroutine(float duration)
    {
        // Enable all countdown texts
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

        // Spawn next batch
        SpawnNewBatch();
    }
}













