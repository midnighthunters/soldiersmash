using UnityEngine;
using System.Linq;

public static class TempContactSheet
{
    public static string Execute()
    {
        var existing = GameObject.Find("__ContactSheet");
        if (existing != null) Object.DestroyImmediate(existing);

        var root = new GameObject("__ContactSheet");
        var textures = Resources.LoadAll<Texture2D>("").OrderBy(t => t.name).ToArray();
        int cols = 4;
        float cell = 3.2f;
        int i = 0;
        foreach (var tex in textures)
        {
            var go = new GameObject(tex.name);
            go.transform.SetParent(root.transform);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), tex.width / 2f);
            float cx = (i % cols) * cell;
            float cy = -(i / cols) * cell;
            go.transform.localPosition = new Vector3(cx, cy, 0);
            Debug.Log($"[ContactSheet] {i}: '{tex.name}' {tex.width}x{tex.height}");
            i++;
        }
        return $"Created {i} sprites in contact sheet";
    }
}
