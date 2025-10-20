using UnityEngine;
using UnityEngine.UI;

public class UISpongeCursor : MonoBehaviour
{
    [Header("Cursor Settings")]
    public RectTransform cursorImage;
    public float scale = 2f;

    [Header("Minigame Reference")]
    public MinigameZone minigameZone;

    private bool usingSpongeCursor = false;

    void Start()
    {
        if (cursorImage == null)
        {
            Debug.LogError("Assign the UI Image RectTransform for the cursor!");
            enabled = false;
            return;
        }

        // Keep the normal system cursor visible by default
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        cursorImage.localScale = new Vector3(scale, scale, 1f);
        cursorImage.gameObject.SetActive(false);
    }

    void Update()
    {
        if (minigameZone == null) return;

        bool shouldUseSponge = minigameZone.InMinigame;

        if (shouldUseSponge && !usingSpongeCursor)
        {
            // Switch to sponge cursor
            Cursor.visible = false; // Hide default system cursor
            cursorImage.gameObject.SetActive(true);
            usingSpongeCursor = true;
        }
        else if (!shouldUseSponge && usingSpongeCursor)
        {
            // Switch back to normal cursor
            Cursor.visible = true;
            cursorImage.gameObject.SetActive(false);
            usingSpongeCursor = false;
        }

        // Update sponge cursor position if active
        if (usingSpongeCursor)
        {
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                cursorImage.parent as RectTransform,
                Input.mousePosition,
                null,
                out pos
            );
            cursorImage.localPosition = pos;
        }
    }
}

