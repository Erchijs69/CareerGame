using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DirtEraser : MonoBehaviour
{
    public Texture2D brush;                
    public int brushSize = 64;             
    public float eraseThreshold = 0.01f;   
    public bool drawOnHold = true;         

    public Texture2D cursorTexture;        
    public Vector2 hotSpot = Vector2.zero; 

    SpriteRenderer sr;
    Texture2D dirtTex;
    int texWidth, texHeight;

    // Debug variables
    public bool showDebug = true;
    private Vector3 lastMouseWorld;
    private Vector2 lastLocalPos;
    private int lastPx, lastPy;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        Sprite sprite = sr.sprite;
        Rect rect = sprite.textureRect;

        texWidth = (int)rect.width;
        texHeight = (int)rect.height;

        dirtTex = new Texture2D(texWidth, texHeight, TextureFormat.ARGB32, false);
        dirtTex.wrapMode = TextureWrapMode.Clamp;
        dirtTex.filterMode = sprite.texture.filterMode;

        Color[] pixels = sprite.texture.GetPixels((int)rect.x, (int)rect.y, texWidth, texHeight);
        dirtTex.SetPixels(pixels);
        dirtTex.Apply();

        Sprite newSprite = Sprite.Create(dirtTex, new Rect(0, 0, texWidth, texHeight),
            new Vector2(0.5f, 0.5f), sprite.pixelsPerUnit);
        sr.sprite = newSprite;
    }

    void Update()
    {
        if ((Input.GetMouseButton(0) && drawOnHold) || (Input.GetMouseButtonDown(0) && !drawOnHold))
        {
            if (Camera.main == null) return;

            // Mouse world position
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.WorldToScreenPoint(transform.position).z));


            // Local position relative to sprite
            Vector2 localPos = transform.InverseTransformPoint(mouseWorld);

            // Convert local position to texture pixels using pivot
            Sprite sprite = sr.sprite;
            Vector2 pivot = sprite.pivot;
            float pixelsPerUnit = sprite.pixelsPerUnit;

            int px = Mathf.RoundToInt(localPos.x * pixelsPerUnit + pivot.x);
            int py = Mathf.RoundToInt(localPos.y * pixelsPerUnit + pivot.y);

            // Store for debug
            lastMouseWorld = mouseWorld;
            lastLocalPos = localPos;
            lastPx = px;
            lastPy = py;

            if (px >= 0 && px < texWidth && py >= 0 && py < texHeight)
            {
                EraseAt(px, py);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!showDebug || sr == null) return;

        // Mouse world position
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(lastMouseWorld, 0.05f);

        // Local position relative to sprite
        Vector3 localWorld = transform.TransformPoint(lastLocalPos);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(localWorld, 0.05f);

        // Brush pixel approximate world position
        if (sr.sprite != null)
        {
            Vector2 spriteSize = new Vector2(sr.sprite.rect.width, sr.sprite.rect.height) / sr.sprite.pixelsPerUnit;
            float unitPerPixelX = spriteSize.x / texWidth;
            float unitPerPixelY = spriteSize.y / texHeight;

            Vector3 brushPos = transform.position + new Vector3(
                (lastPx + 0.5f) * unitPerPixelX - spriteSize.x * 0.5f,
                (lastPy + 0.5f) * unitPerPixelY - spriteSize.y * 0.5f,
                0f);
            Gizmos.color = Color.red;
            Gizmos.DrawCube(brushPos, Vector3.one * 0.05f);
        }
    }

    void EraseAt(int centerX, int centerY)
    {
        if (brush == null) return;

        Color32[] pixels = dirtTex.GetPixels32();
        int bW = brush.width;
        int bH = brush.height;
        int half = brushSize / 2;

        for (int by = 0; by < brushSize; by++)
        {
            int y = centerY - half + by;
            if (y < 0 || y >= texHeight) continue;

            for (int bx = 0; bx < brushSize; bx++)
            {
                int x = centerX - half + bx;
                if (x < 0 || x >= texWidth) continue;

                float u = (float)bx / (brushSize - 1);
                float v = (float)by / (brushSize - 1);
                int bu = Mathf.Clamp(Mathf.RoundToInt(u * (bW - 1)), 0, bW - 1);
                int bv = Mathf.Clamp(Mathf.RoundToInt(v * (bH - 1)), 0, bH - 1);

                float brushAlpha = brush.GetPixel(bu, bv).a;
                if (brushAlpha <= 0f) continue;

                int idx = y * texWidth + x;
                Color32 c = pixels[idx];
                float newAlpha = (c.a / 255f) * (1f - brushAlpha);
                pixels[idx].a = (byte)(newAlpha * 255f);
            }
        }

        dirtTex.SetPixels32(pixels);
        dirtTex.Apply();
    }

    void OnMouseEnter() => Cursor.SetCursor(cursorTexture, hotSpot, CursorMode.Auto);
    void OnMouseExit() => Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    void OnDisable() => Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
}




