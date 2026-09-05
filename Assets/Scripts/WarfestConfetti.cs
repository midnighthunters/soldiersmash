using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class WarfestConfetti : MonoBehaviour
{
    private sealed class ConfettiPiece
    {
        public RectTransform rect;
        public Image image;
        public Vector2 velocity;
        public float angularVelocity;
        public float flipSpeed;
        public float flipPhase;
        public float flutterFreq;
        public float flutterPhase;
        public float flutterAmp;
        public float baseWidth;
        public float baseHeight;
    }

    private static readonly Color[] ConfettiColors =
    {
        new Color(1.00f, 0.84f, 0.00f, 1f), // Gold
        new Color(0.18f, 0.80f, 0.44f, 1f), // Emerald
        new Color(0.92f, 0.26f, 0.21f, 1f), // Ruby Red
        new Color(0.12f, 0.53f, 0.90f, 1f), // Azure Blue
        new Color(1.00f, 0.58f, 0.00f, 1f), // Tangerine Orange
        new Color(0.68f, 0.32f, 0.87f, 1f), // Purple
        new Color(1.00f, 0.96f, 0.30f, 1f), // Bright Yellow
        new Color(1.00f, 1.00f, 1.00f, 0.95f) // White
    };

    private readonly List<ConfettiPiece> pieces = new List<ConfettiPiece>();
    private Sprite singlePixelSprite;
    private const int ConfettiCount = 85;
    private const float Gravity = -680f;
    private const float HorizontalDrag = 0.96f;

    public static WarfestConfetti Create(Transform parent)
    {
        GameObject confettiRoot = new GameObject("Confetti Container", typeof(RectTransform));
        confettiRoot.transform.SetParent(parent, false);
        RectTransform rt = confettiRoot.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        WarfestConfetti confetti = confettiRoot.AddComponent<WarfestConfetti>();
        confetti.Initialize();
        return confetti;
    }

    private void Initialize()
    {
        // 1x1 white texture for clean vector-like crisp UI confetti quads
        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        tex.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
        tex.Apply();
        singlePixelSprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 100f);

        for (int i = 0; i < ConfettiCount; i++)
        {
            SpawnPiece(true);
        }
    }

    private void SpawnPiece(bool isInitialBurst)
    {
        GameObject obj = new GameObject("Confetti", typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(transform, false);

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);

        Image img = obj.GetComponent<Image>();
        img.sprite = singlePixelSprite;
        img.color = ConfettiColors[Random.Range(0, ConfettiColors.Length)];
        img.raycastTarget = false;

        bool isRibbon = Random.value > 0.45f;
        float w = isRibbon ? Random.Range(6f, 9f) : Random.Range(10f, 16f);
        float h = isRibbon ? Random.Range(18f, 28f) : Random.Range(10f, 16f);
        rt.sizeDelta = new Vector2(w, h);

        ConfettiPiece p = new ConfettiPiece
        {
            rect = rt,
            image = img,
            baseWidth = w,
            baseHeight = h,
            angularVelocity = Random.Range(-320f, 320f),
            flipSpeed = Random.Range(4f, 10f),
            flipPhase = Random.Range(0f, Mathf.PI * 2f),
            flutterFreq = Random.Range(3f, 7f),
            flutterPhase = Random.Range(0f, Mathf.PI * 2f),
            flutterAmp = Random.Range(35f, 85f)
        };

        if (isInitialBurst)
        {
            // Burst from upper screen center-left and center-right outward
            float startX = Random.Range(-160f, 160f);
            float startY = Random.Range(100f, 320f);
            rt.anchoredPosition = new Vector2(startX, startY);

            float angle = Random.Range(30f, 150f) * Mathf.Deg2Rad;
            float speed = Random.Range(350f, 850f);
            p.velocity = new Vector2(Mathf.Cos(angle) * speed * (Random.value > 0.5f ? 1f : -1f), Mathf.Sin(angle) * speed);
        }
        else
        {
            // Regular rain from top
            float startX = Random.Range(-210f, 210f);
            float startY = Random.Range(460f, 540f);
            rt.anchoredPosition = new Vector2(startX, startY);
            p.velocity = new Vector2(Random.Range(-50f, 50f), Random.Range(-120f, -40f));
        }

        pieces.Add(p);
    }

    private void Update()
    {
        float dt = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
        if (dt <= 0f) return;

        for (int i = 0; i < pieces.Count; i++)
        {
            ConfettiPiece p = pieces[i];
            if (p.rect == null) continue;

            // Apply gravity & drag
            p.velocity.y += Gravity * dt;
            p.velocity.x *= Mathf.Pow(HorizontalDrag, dt * 60f);

            // Aerodynamic flutter
            float flutter = Mathf.Sin(Time.unscaledTime * p.flutterFreq + p.flutterPhase) * p.flutterAmp;
            Vector2 pos = p.rect.anchoredPosition;
            pos.x += (p.velocity.x + flutter) * dt;
            pos.y += p.velocity.y * dt;

            // 2.5D tumbling flip simulation (scales X from -1 to 1)
            float flip = Mathf.Cos(Time.unscaledTime * p.flipSpeed + p.flipPhase);
            p.rect.localScale = new Vector3(flip, 1f, 1f);

            // Z-axis spin rotation
            p.rect.Rotate(0f, 0f, p.angularVelocity * dt);

            // Recycle if fallen below screen
            if (pos.y < -520f)
            {
                pos.x = Random.Range(-200f, 200f);
                pos.y = Random.Range(480f, 560f);
                p.velocity = new Vector2(Random.Range(-70f, 70f), Random.Range(-180f, -40f));
            }

            p.rect.anchoredPosition = pos;
        }
    }
}
