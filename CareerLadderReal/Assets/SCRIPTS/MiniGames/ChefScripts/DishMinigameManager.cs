using UnityEngine;

public class DishMinigameManager : MonoBehaviour
{
    [Header("Plates")]
    public GameObject[] plates; // Each plate should have the dirt child or dirt script
    private int currentPlateIndex = 0;

    [Header("Minigame Zone")]
    public MinigameZone minigameZone; // Reference to your existing MinigameZone

    void Start()
    {
        // Disable all plates except the first one
        for (int i = 0; i < plates.Length; i++)
        {
            plates[i].SetActive(i == 0);
        }
        currentPlateIndex = 0;
    }

    // Call this from your DirtEraser / cleaning logic when a plate is fully cleaned
    public void PlateCleaned()
    {
        if (currentPlateIndex >= plates.Length) return;

        // Disable current plate
        plates[currentPlateIndex].SetActive(false);

        currentPlateIndex++;

        if (currentPlateIndex < plates.Length)
        {
            // Enable next plate
            plates[currentPlateIndex].SetActive(true);
        }
        else
        {
            // All plates cleaned → exit minigame
            if (minigameZone != null)
            {
                minigameZone.ExitMinigame();
            }
            Debug.Log("All plates cleaned!");
        }
    }

    // Optional: helper to get the current active plate
    public GameObject GetCurrentPlate()
    {
        if (currentPlateIndex < plates.Length)
            return plates[currentPlateIndex];
        return null;
    }
}
