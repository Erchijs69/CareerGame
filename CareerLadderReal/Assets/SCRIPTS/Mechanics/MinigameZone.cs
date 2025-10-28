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
    private playerMovementScript playerMovementScript;

    private void OnEnable()
    {
        // 👂 Listen for camera transitions from StageCameraMover
        StageCameraMover.OnCameraStageSwitched += HandleCameraSwitch;
    }

    private void OnDisable()
    {
        StageCameraMover.OnCameraStageSwitched -= HandleCameraSwitch;
    }

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

        // Disable movement
        if (playerMovementScript != null)
            playerMovementScript.DisableMovement();

        Debug.Log("Entered minigame view");
    }

    public void ExitMinigame()
    {
        if (!inMinigame)
            return;

        inMinigame = false;

        // Switch cameras back
        if (mainCamera != null && minigameCamera != null)
        {
            minigameCamera.enabled = false;
            mainCamera.enabled = true;
            minigameCamera.depth = 0;
            mainCamera.depth = 1;
        }

        // Re-enable movement
        if (playerMovementScript != null)
            playerMovementScript.EnableMovement();

        Debug.Log("Exited minigame view");
    }

    // 🔔 Automatically triggered when main camera switches (event)
    private void HandleCameraSwitch()
    {
        if (inMinigame)
        {
            Debug.Log("Minigame exited due to camera switch.");
            ExitMinigame();
        }
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




