using System.Text;
using UnityEditor;
using UnityEngine;

public static class SpriteAnalysis
{
    public static string Execute()
    {
        var sb = new StringBuilder();
        AnalyzeSheet(sb, "Assets/Resources/blocks.png");
        AnalyzeSingle(sb, "Assets/Resources/table.png");
        return sb.ToString();
    }

    private static void WithReadable(string assetPath, System.Action<Texture2D> action)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        bool wasReadable = importer.isReadable;
        importer.isReadable = true;
        importer.SaveAndReimport();
        try
        {
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            action(tex);
        }
        finally
        {
            importer.isReadable = wasReadable;
            importer.SaveAndReimport();
        }
    }

    private static void AnalyzeSheet(StringBuilder sb, string assetPath)
    {
        // Load sprites via the asset database (sub-assets of the sheet)
        Object[] assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);
        var sprites = new System.Collections.Generic.List<Sprite>();
        foreach (Object o in assets) if (o is Sprite s) sprites.Add(s);

        sb.AppendLine($"=== {assetPath}: {sprites.Count} sprites ===");

        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        bool wasReadable = importer.isReadable;
        importer.isReadable = true;
        importer.SaveAndReimport();
        try
        {
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            foreach (Sprite s in sprites)
            {
                AppendTightBounds(sb, s, tex);
            }
        }
        finally
        {
            importer.isReadable = wasReadable;
            importer.SaveAndReimport();
        }
    }

    private static void AnalyzeSingle(StringBuilder sb, string assetPath)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            sb.AppendLine($"=== {assetPath}: NOT FOUND ===");
            return;
        }
        sb.AppendLine($"=== {assetPath} ===");
        WithReadable(assetPath, tex =>
        {
            Sprite[] sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath) as Sprite[];
            if (sprites == null || sprites.Length == 0)
            {
                sb.AppendLine("no sprites found");
                return;
            }
            AppendTightBounds(sb, sprites[0], tex);
        });
    }

    private static void AppendTightBounds(StringBuilder sb, Sprite s, Texture2D tex)
    {
        Rect r = s.textureRect;
        int x0 = Mathf.FloorToInt(r.xMin);
        int y0 = Mathf.FloorToInt(r.yMin);
        int w = Mathf.RoundToInt(r.width);
        int h = Mathf.RoundToInt(r.height);
        Color[] pixels = tex.GetPixels(x0, y0, w, h);

        int minX = w, maxX = -1, minY = h, maxY = -1;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (pixels[y * w + x].a > 0.05f)
                {
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
        }

        float ppu = s.pixelsPerUnit;
        if (maxX < 0)
        {
            sb.AppendLine($"{s.name}: FULLY TRANSPARENT rect={w}x{h}");
            return;
        }

        float tightW = (maxX - minX + 1) / ppu;
        float tightH = (maxY - minY + 1) / ppu;
        float cx = ((minX + maxX + 1) * 0.5f - w * 0.5f) / ppu;
        float cy = ((minY + maxY + 1) * 0.5f - h * 0.5f) / ppu;

        float boundsW = s.bounds.size.x;
        float boundsH = s.bounds.size.y;
        sb.AppendLine($"{s.name}: rectPx={w}x{h} ppu={ppu} bounds={boundsW:F3}x{boundsH:F3} " +
                      $"tight={tightW:F3}x{tightH:F3} fillX={(boundsW > 0 ? tightW / boundsW * 100f : 0):F1}% fillY={(boundsH > 0 ? tightH / boundsH * 100f : 0):F1}% " +
                      $"tightCenterOffset=({cx:F3},{cy:F3})");
    }
}
