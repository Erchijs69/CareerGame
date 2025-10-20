using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PotatoPeelerManager : MonoBehaviour
{
    [Header("Peeler Minigame Setup")]
    public GameObject[] potatoPrefabs;
    public GameObject peelerPrefab;
    public Transform potatoSpawnPoint;
    public Transform peelerStartPoint;

    [Header("Spawn Settings")]
    public int maxPotatoes = 5;
    public float minSpawnDelay = 5f;
    public float maxSpawnDelay = 15f;
    public float zOffsetPerPotato = 0.05f;

    [Header("Spawn Areas (Relative to potatoSpawnPoint)")]
    public Vector2 spawnArea1Min = new Vector2(-3f, -3f);
    public Vector2 spawnArea1Max = new Vector2(3f, 3f);
    public Vector2 spawnArea2Min = new Vector2(-3f, -3f);
    public Vector2 spawnArea2Max = new Vector2(3f, 3f);
    public Vector2 spawnArea3Min = new Vector2(-3f, -3f);
    public Vector2 spawnArea3Max = new Vector2(3f, 3f);
    public Vector2 spawnArea4Min = new Vector2(-3f, -3f);
    public Vector2 spawnArea4Max = new Vector2(3f, 3f);

    [HideInInspector] public List<GameObject> spawnedPotatoes = new List<GameObject>();

    private GameObject currentPeeler;
    public MinigameZone minigameZone;
    private Coroutine spawnRoutine;

    void Start()
    {
        if (minigameZone == null)
            minigameZone = FindObjectOfType<MinigameZone>();

        spawnRoutine = StartCoroutine(SpawnPotatoesOverTime());
    }

    private IEnumerator SpawnPotatoesOverTime()
    {
        while (true)
        {
            if (spawnedPotatoes.Count < maxPotatoes)
            {
                float waitTime = Random.Range(minSpawnDelay, maxSpawnDelay);
                yield return new WaitForSeconds(waitTime);
                if (spawnedPotatoes.Count < maxPotatoes)
                    SpawnAndPoolPotato();
            }
            else
            {
                yield return null;
            }
        }
    }

    private void SpawnAndPoolPotato()
    {
        if (potatoPrefabs.Length == 0)
        {
            Debug.LogWarning("No potato prefabs assigned!");
            return;
        }

        GameObject prefab = potatoPrefabs[Random.Range(0, potatoPrefabs.Length)];
        GameObject newPotato = Instantiate(prefab, potatoSpawnPoint.position, Quaternion.identity);

        // Pick a random area
        int areaIndex = Random.Range(1, 5);
        Vector2 min, max;
        switch (areaIndex)
        {
            case 1: min = spawnArea1Min; max = spawnArea1Max; break;
            case 2: min = spawnArea2Min; max = spawnArea2Max; break;
            case 3: min = spawnArea3Min; max = spawnArea3Max; break;
            case 4: min = spawnArea4Min; max = spawnArea4Max; break;
            default: min = spawnArea1Min; max = spawnArea1Max; break;
        }

        // Find a valid spawn position
        Vector3 spawnPos = Vector3.zero;
        int attempts = 0;
        bool foundValidPos = false;
        CircleCollider2D newCollider = newPotato.GetComponent<CircleCollider2D>();
        float newRadius = newCollider != null ? newCollider.radius : 0.5f;

        while (attempts < 100)
        {
            float randomX = Random.Range(min.x, max.x);
            float randomY = Random.Range(min.y, max.y);
            float zOffset = spawnedPotatoes.Count * zOffsetPerPotato;
            spawnPos = potatoSpawnPoint.position + new Vector3(randomX, randomY, zOffset);

            bool overlaps = false;
            foreach (var potato in spawnedPotatoes)
            {
                if (potato == null) continue;
                CircleCollider2D otherCol = potato.GetComponent<CircleCollider2D>();
                float otherRadius = otherCol != null ? otherCol.radius : 0.5f;
                if (Vector3.Distance(spawnPos, potato.transform.position) < newRadius + otherRadius)
                {
                    overlaps = true;
                    break;
                }
            }

            if (!overlaps)
            {
                foundValidPos = true;
                break;
            }
            attempts++;
        }

        if (!foundValidPos)
            Debug.LogWarning("Could not find a valid spawn position, spawning anyway.");

        newPotato.transform.position = spawnPos;
        spawnedPotatoes.Add(newPotato);

        // Spawn peeler if needed
        if (currentPeeler == null)
            SpawnPeeler();

        // Always assign the *latest* spawned potato as the active one
        if (currentPeeler != null)
        {
            var peelerScript = currentPeeler.GetComponent<PotatoPeeler>();
            if (peelerScript != null)
            {
                peelerScript.potato = newPotato.GetComponent<PotatoPeelSurface>();
            }
        }

        Debug.Log($"Spawned potato #{spawnedPotatoes.Count} from prefab {prefab.name} in area {areaIndex}");
    }

    private void SpawnPeeler()
    {
        Vector3 peelerPos = peelerStartPoint.position;
        currentPeeler = Instantiate(peelerPrefab, peelerPos, Quaternion.identity);
        PotatoPeeler peelerScript = currentPeeler.GetComponent<PotatoPeeler>();
        if (peelerScript != null)
            peelerScript.manager = this;

        SpriteRenderer sr = currentPeeler.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "Foreground";
            sr.sortingOrder = 10;
        }
    }

    public void PotatoPeeled(GameObject peeledPotato)
    {
        if (peeledPotato == null) return;

        if (spawnedPotatoes.Contains(peeledPotato))
            spawnedPotatoes.Remove(peeledPotato);

        Destroy(peeledPotato);
        Debug.Log($"Potato peeled! Pool size now: {spawnedPotatoes.Count}");

        // Assign the new "latest" potato as the next peel target
        if (currentPeeler != null)
        {
            var peelerScript = currentPeeler.GetComponent<PotatoPeeler>();
            if (peelerScript != null)
            {
                if (spawnedPotatoes.Count > 0)
                    peelerScript.potato = spawnedPotatoes[spawnedPotatoes.Count - 1].GetComponent<PotatoPeelSurface>();
                else
                    peelerScript.potato = null;
            }
        }

        if (minigameZone != null && spawnedPotatoes.Count == 0)
            minigameZone.ExitMinigame();
    }
}





