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
    private playerMovementScript playerMovementScript; // ✅ use correct type

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

        // Switch cameras
        if (mainCamera != null && minigameCamera != null)
        {
            mainCamera.enabled = false;
            minigameCamera.enabled = true;
            mainCamera.depth = 0;
            minigameCamera.depth = 1;
        }

        // ✅ Stop movement cleanly
        if (playerMovementScript != null)
            playerMovementScript.DisableMovement();

        Debug.Log("Entered minigame view");
    }

    public void ExitMinigame()
    {
        inMinigame = false;

        // Switch cameras back
        if (mainCamera != null && minigameCamera != null)
        {
            minigameCamera.enabled = false;
            mainCamera.enabled = true;
            minigameCamera.depth = 0;
            mainCamera.depth = 1;
        }

        // ✅ Re-enable movement
        if (playerMovementScript != null)
            playerMovementScript.EnableMovement();

        Debug.Log("Exited minigame view");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
            player = other.gameObject;

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



