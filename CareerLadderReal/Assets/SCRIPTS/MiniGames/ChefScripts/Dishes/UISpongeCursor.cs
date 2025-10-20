using UnityEngine;
using UnityEngine.UI;

public class UISpongeCursor : MonoBehaviour
{
    [Header("Cursor Settings")]
    public RectTransform cursorImage;
    public float scale = 2f;

    [Header("Minigame Reference")]
    public MinigameZone minigameZone;

    void Start()
    {
        if (cursorImage == null)
        {
            Debug.LogError("Assign the UI Image RectTransform for the cursor!");
            enabled = false;
            return;
        }

        Cursor.visible = false;
        cursorImage.localScale = new Vector3(scale, scale, 1f);
        cursorImage.gameObject.SetActive(false);
    }

    void Update()
    {
        if (minigameZone == null) return;

        // Only show the sponge cursor when in minigame
        bool showCursor = minigameZone.InMinigame;
        cursorImage.gameObject.SetActive(showCursor);

        if (!showCursor) return;

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
