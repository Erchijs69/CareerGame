using UnityEngine;

public class PotatoPeeler : MonoBehaviour
{
    public PotatoPeelerManager manager;
    public PotatoPeelSurface potato;

    private bool dragging = false;
    private Vector3 offset;
    private Vector3 lastPosition;

    [Header("Peeling Settings")]
    public float horizontalPeelMultiplier = 1f;

    void Update()
    {
        if (manager == null || manager.spawnedPotatoes.Count == 0) return;
        if (manager.minigameZone == null || !manager.minigameZone.InMinigame) return;

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

        if (Input.GetMouseButtonUp(0)) dragging = false;

        if (dragging)
        {
            Vector3 mouseWorld = GetMouseWorld() + offset;
            transform.position = new Vector3(mouseWorld.x, mouseWorld.y, transform.position.z);

            float deltaX = mouseWorld.x - lastPosition.x;
            if (Mathf.Abs(deltaX) > 0f)
            {
                float peelAmount = horizontalPeelMultiplier * Mathf.Abs(deltaX);
                foreach (var potatoGO in manager.spawnedPotatoes)
                {
                    if (potatoGO == null) continue;
                    PotatoPeelSurface peelSurface = potatoGO.GetComponent<PotatoPeelSurface>();
                    if (peelSurface != null) peelSurface.PeelAt(transform.position, peelAmount);
                }
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











