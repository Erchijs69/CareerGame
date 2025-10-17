using UnityEngine;

public class MinigameZone : MonoBehaviour
{
    [Header("Cameras")]
    public Camera mainCamera;
    public Camera minigameCamera;

    public bool InMinigame => inMinigame;

    private bool playerInZone = false;
    private bool inMinigame = false;
    private GameObject player;
    private MonoBehaviour playerMovementScript;

    void Start()
    {
        if (mainCamera != null)
        {
            mainCamera.enabled = true;
            mainCamera.depth = 1;
        }

        if (minigameCamera != null)
        {
            minigameCamera.enabled = false;
            minigameCamera.depth = 0;
        }
    }

    void Update()
    {
        if (playerInZone && Input.GetKeyDown(KeyCode.Space))
        {
            if (!inMinigame)
                EnterMinigame();
            else
                ExitMinigame();
        }
    }

    void EnterMinigame()
    {
        inMinigame = true;

        // Disable main, enable mini
        if (mainCamera != null && minigameCamera != null)
        {
            mainCamera.enabled = false;
            minigameCamera.enabled = true;

            // Adjust depth for safety/consistency
            mainCamera.depth = 0;
            minigameCamera.depth = 1;
        }

        if (playerMovementScript != null)
            playerMovementScript.enabled = false;

        Debug.Log("Entered minigame view");
    }

    public void ExitMinigame()
    {
        inMinigame = false;

        // Disable mini, enable main
        if (mainCamera != null && minigameCamera != null)
        {
            minigameCamera.enabled = false;
            mainCamera.enabled = true;

            minigameCamera.depth = 0;
            mainCamera.depth = 1;
        }

        if (playerMovementScript != null)
            playerMovementScript.enabled = true;

        Debug.Log("Exited minigame view");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
            player = other.gameObject;

            // Replace with your actual movement script
            playerMovementScript = player.GetComponent<playerMovementScript>();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
        }
    }
}


