using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class playerMovementScript : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // Ensure no gravity for top-down movement
        rb.freezeRotation = true; // Prevent unwanted rotation
    }

    void Update()
    {
        // Get movement input (WASD or Arrow Keys)
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // Normalize to prevent faster diagonal movement
        moveInput = moveInput.normalized;
    }

    void FixedUpdate()
    {
        // Apply movement in physics step
        rb.velocity = moveInput * moveSpeed;
    }
}

