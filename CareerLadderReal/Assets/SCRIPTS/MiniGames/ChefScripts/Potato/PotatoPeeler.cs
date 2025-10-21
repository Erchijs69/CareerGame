using UnityEngine;

public class PotatoPeeler : MonoBehaviour
{
    [Header("References")]
    public PotatoPeelerManager manager;
    public PotatoPeelSurface activePotato;

    [Header("Peeling Settings")]
    public float horizontalPeelMultiplier = 1f;

    private bool dragging = false;
    private Vector3 offset;
    private Vector3 lastPosition;

    void Update()
    {
        if (manager == null || !manager.InMinigame || activePotato == null) return;

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

        if (Input.GetMouseButtonUp(0))
            dragging = false;

        if (dragging)
        {
            Vector3 mouseWorld = GetMouseWorld() + offset;
            transform.position = new Vector3(mouseWorld.x, mouseWorld.y, transform.position.z);

            float deltaX = mouseWorld.x - lastPosition.x;
            if (Mathf.Abs(deltaX) > 0.001f)
            {
                float peelAmount = horizontalPeelMultiplier * Mathf.Abs(deltaX);
                activePotato.PeelAt(transform.position, peelAmount);
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

    public void SetActivePotato(PotatoPeelSurface potato)
    {
        activePotato = potato;
    }

    public void ClearPotato()
    {
        activePotato = null;
    }
}













