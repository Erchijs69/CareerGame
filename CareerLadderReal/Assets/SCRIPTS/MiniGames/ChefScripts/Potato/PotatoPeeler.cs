using UnityEngine;

public class PotatoPeeler : MonoBehaviour
{
    public PotatoPeelerManager manager;
    public PotatoPeelSurface potato;

    [Header("Peeling Settings")]
    public float peelMultiplier = 1f;   // Base peel strength
    public float speedDampening = 5f;   // Reduces peel when moving fast

    private bool dragging = false;
    private Vector3 offset;
    private Vector3 lastPosition;

    void Update()
    {
        // Start dragging
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorld = GetMouseWorld();
            if (IsMouseOverPeeler(mouseWorld))
            {
                dragging = true;
                offset = transform.position - mouseWorld;
                lastPosition = transform.position;
            }
        }

        // Stop dragging
        if (Input.GetMouseButtonUp(0))
        {
            dragging = false;
        }

        if (dragging)
        {
            Vector3 mouseWorld = GetMouseWorld() + offset;
            transform.position = new Vector3(mouseWorld.x, mouseWorld.y, transform.position.z);

            // Compute distance moved since last frame
            float distance = Vector3.Distance(mouseWorld, lastPosition);

            // Peel factor decreases with speed
            float peelFactor = peelMultiplier / (1f + distance * speedDampening);

            // Check if over potato
            Collider2D hit = Physics2D.OverlapPoint(transform.position, LayerMask.GetMask("Potato"));
            if (hit != null && hit.gameObject == potato.gameObject)
            {
                Vector2 local = potato.transform.InverseTransformPoint(transform.position);
                potato.PeelAt(local, peelFactor);
            }

            lastPosition = mouseWorld;
        }
    }

    bool IsMouseOverPeeler(Vector3 mouseWorld)
    {
        Collider2D col = GetComponent<Collider2D>();
        return col == null || col.OverlapPoint(mouseWorld);
    }

    Vector3 GetMouseWorld()
    {
        Vector3 mPos = Input.mousePosition;
        mPos.z = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);
        return Camera.main.ScreenToWorldPoint(mPos);
    }
}






