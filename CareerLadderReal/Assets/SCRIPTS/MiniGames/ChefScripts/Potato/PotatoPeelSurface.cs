using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PotatoPeelSurface : MonoBehaviour
{
    [Header("Peel Settings")]
    public Texture2D peelMask;
    public int brushSize = 32;
    public float eraseThreshold = 0.05f;

    private SpriteRenderer sr;
    private Texture2D potatoTexture;
    private int texWidth, texHeight;
    private bool isPeeled = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        Sprite sprite = sr.sprite;
        Rect rect = sprite.textureRect;

        texWidth = (int)rect.width;
        texHeight = (int)rect.height;

        potatoTexture = new Texture2D(texWidth, texHeight, TextureFormat.ARGB32, false);
        potatoTexture.wrapMode = TextureWrapMode.Clamp;
        potatoTexture.filterMode = sprite.texture.filterMode;

        Color[] pixels = sprite.texture.GetPixels((int)rect.x, (int)rect.y, texWidth, texHeight);
        potatoTexture.SetPixels(pixels);
        potatoTexture.Apply();

        sr.sprite = Sprite.Create(potatoTexture, new Rect(0, 0, texWidth, texHeight), new Vector2(0.5f, 0.5f), sprite.pixelsPerUnit);
    }

    public void PeelAt(Vector2 localHit, float peelFactor = 1f)
    {
        if (isPeeled) return;

        int px = Mathf.RoundToInt((localHit.x + 0.5f) * (texWidth - 1));
        int py = Mathf.RoundToInt((localHit.y + 0.5f) * (texHeight - 1));

        if (px < 0 || px >= texWidth || py < 0 || py >= texHeight) return;

        EraseAt(px, py, peelFactor);

        if (IsFullyPeeled())
        {
            isPeeled = true;
            var manager = FindObjectOfType<PotatoPeelerManager>();
            if (manager != null)
                manager.PotatoPeeled();
        }
    }

    void EraseAt(int cx, int cy, float peelFactor)
    {
        if (peelMask == null) return;

        Color32[] pixels = potatoTexture.GetPixels32();
        int maskW = peelMask.width;
        int maskH = peelMask.height;
        int half = brushSize / 2;

        for (int by = 0; by < brushSize; by++)
        {
            int y = cy - half + by;
            if (y < 0 || y >= texHeight) continue;

            for (int bx = 0; bx < brushSize; bx++)
            {
                int x = cx - half + bx;
                if (x < 0 || x >= texWidth) continue;

                float u = (float)bx / (brushSize - 1);
                float v = (float)by / (brushSize - 1);
                int mu = Mathf.Clamp(Mathf.RoundToInt(u * (maskW - 1)), 0, maskW - 1);
                int mv = Mathf.Clamp(Mathf.RoundToInt(v * (maskH - 1)), 0, maskH - 1);

                float maskAlpha = peelMask.GetPixel(mu, mv).a;
                if (maskAlpha <= 0f) continue;

                int idx = y * texWidth + x;
                Color32 c = pixels[idx];
                float newAlpha = c.a / 255f * (1f - maskAlpha * peelFactor);
                c.a = (byte)(newAlpha * 255);
                pixels[idx] = c;
            }
        }

        potatoTexture.SetPixels32(pixels);
        potatoTexture.Apply();
    }

    bool IsFullyPeeled()
    {
        Color32[] pixels = potatoTexture.GetPixels32();
        foreach (var p in pixels)
        {
            if (p.a / 255f > eraseThreshold)
                return false;
        }
        return true;
    }
}


