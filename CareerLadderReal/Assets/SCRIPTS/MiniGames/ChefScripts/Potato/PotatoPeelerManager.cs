using UnityEngine;

public class PotatoPeelerManager : MonoBehaviour
{
    [Header("Peeler Minigame Setup")]
    public GameObject potatoPrefab;
    public GameObject peelerPrefab;

    public Transform potatoSpawnPoint;
    public Transform peelerStartPoint;

    private GameObject currentPotato;
    private GameObject currentPeeler;

    public MinigameZone minigameZone;

    void Start()
    {
        if (minigameZone == null)
            minigameZone = FindObjectOfType<MinigameZone>();
    }

    void Update()
    {
        if (minigameZone != null && minigameZone.InMinigame && currentPotato == null)
        {
            StartMinigame();
        }
    }

    void StartMinigame()
    {
        // Spawn potato
        currentPotato = Instantiate(potatoPrefab, potatoSpawnPoint.position, Quaternion.identity);
        SpriteRenderer potatoSR = currentPotato.GetComponent<SpriteRenderer>();
        if (potatoSR != null)
        {
            potatoSR.sortingLayerName = "Default";
            potatoSR.sortingOrder = 0;
        }

        // Spawn peeler slightly in front of potato
        Vector3 peelerPos = peelerStartPoint.position;
        peelerPos.z = potatoSpawnPoint.position.z - 0.1f;
        currentPeeler = Instantiate(peelerPrefab, peelerPos, Quaternion.identity);

        SpriteRenderer peelerSR = currentPeeler.GetComponent<SpriteRenderer>();
        if (peelerSR != null)
        {
            peelerSR.sortingLayerName = "Foreground";
            peelerSR.sortingOrder = 10;
        }

        // Assign references
        var peeler = currentPeeler.GetComponent<PotatoPeeler>();
        peeler.manager = this;
        peeler.potato = currentPotato.GetComponent<PotatoPeelSurface>();
    }

    public void PotatoPeeled()
    {
        Debug.Log("Potato peeled completely!");
        if (minigameZone != null)
            minigameZone.ExitMinigame();
    }
}


