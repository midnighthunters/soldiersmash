using System.Text;
using UnityEditor;
using UnityEngine;

public static class TableAnalysis
{
    public static string Execute()
    {
        var sb = new StringBuilder();
        Sprite[] sprites = Resources.LoadAll<Sprite>("table");
        sb.AppendLine($"table sprites loaded: {sprites.Length}");
        if (sprites.Length == 0) return sb.ToString();

        Sprite s = sprites[0];
        string assetPath = AssetDatabase.GetAssetPath(s);
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        bool wasReadable = importer != null && importer.isReadable;
        if (importer != null)
        {
            importer.isReadable = true;
            importer.SaveAndReimport();
        }
        try
        {
            Texture2D tex = s.texture;
            Rect r = s.textureRect;
            int x0 = Mathf.FloorToInt(r.xMin);
            int y0 = Mathf.FloorToInt(r.yMin);
            int w = Mathf.RoundToInt(r.width);
            int h = Mathf.RoundToInt(r.height);
            Color[] pixels = tex.GetPixels(x0, y0, w, h);

            int minX = w, maxX = -1, minY = h, maxY = -1;
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    if (pixels[y * w + x].a > 0.05f)
                    {
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                    }

            float ppu = s.pixelsPerUnit;
            float tightW = (maxX - minX + 1) / ppu;
            float tightH = (maxY - minY + 1) / ppu;
            float cx = ((minX + maxX + 1) * 0.5f - w * 0.5f) / ppu;
            float cy = ((minY + maxY + 1) * 0.5f - h * 0.5f) / ppu;

            sb.AppendLine($"{s.name}: rectPx={w}x{h} ppu={ppu} bounds={s.bounds.size.x:F3}x{s.bounds.size.y:F3} " +
                          $"tight={tightW:F3}x{tightH:F3} tightCenterOffset=({cx:F3},{cy:F3})");

            // World-space geometry with current placement: position y=-0.95, scale 0.315
            float scale = 0.315f;
            float posY = -0.95f;
            float visibleTop = posY + scale * (cy + tightH * 0.5f);
            float visibleBottom = posY + scale * (cy - tightH * 0.5f);
            float fullTop = posY + scale * (s.bounds.size.y * 0.5f);
            sb.AppendLine($"world fullRectTop={fullTop:F4} visibleTop={visibleTop:F4} visibleBottom={visibleBottom:F4}");
            sb.AppendLine($"current collider top = {-0.95f + scale * (1.64f + 0.52f * 0.5f):F4}");

            // Offset needed so collider top == visible top (collider half-height 0.26 local)
            float neededOffsetY = (visibleTop - posY) / scale - 0.26f;
            sb.AppendLine($"needed collider offsetY for top==visibleTop: {neededOffsetY:F4}");
        }
        finally
        {
            if (importer != null)
            {
                importer.isReadable = wasReadable;
                importer.SaveAndReimport();
            }
        }
        return sb.ToString();
    }
}
