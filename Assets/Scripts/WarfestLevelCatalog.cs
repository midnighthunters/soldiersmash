using System.Collections.Generic;
using UnityEngine;

public static class WarfestLevelCatalog
{
    public struct BlockSpec
    {
        public Vector2 position;
        public Vector2 scale;
        public float rotation;
        public Color color;
        public int spriteIndex;

        public BlockSpec(Vector2 position, Vector2 scale, float rotation, Color color, int spriteIndex = -1)
        {
            this.position = position;
            this.scale = scale;
            this.rotation = rotation;
            this.color = color;
            this.spriteIndex = spriteIndex;
        }
    }

    public struct LevelDefinition
    {
        public int number;
        public string title;
        public string subtitle;
        public string motif;
        public Color background;
        public Color accent;
        public Color secondary;
        public int designType;
        public int blockCount;
        public int difficulty;
    }

    private static readonly string[] Titles =
    {
        "Copper District", "Neon Harbor", "Solar Foundry", "Moonbase Relay", "Rust Cathedral",
        "Violet Bastion", "Glassway Station", "Ember Outpost", "Midnight Arcade", "Quartz Ridge"
    };

    private static readonly string[] Motifs =
    {
        "WARMUP", "TIDELOCK", "OVERDRIVE", "LOW GRAV", "IRON AGE", "NIGHTFALL", "SKYLINE", "HEATWAVE", "GLITCHRUN", "CRYSTAL"
    };

    private static readonly Color[] Backgrounds =
    {
        new Color(0.035f, 0.055f, 0.105f), new Color(0.025f, 0.075f, 0.115f), new Color(0.10f, 0.045f, 0.04f),
        new Color(0.045f, 0.035f, 0.12f), new Color(0.095f, 0.06f, 0.035f), new Color(0.07f, 0.035f, 0.105f),
        new Color(0.025f, 0.09f, 0.10f), new Color(0.11f, 0.045f, 0.025f), new Color(0.025f, 0.035f, 0.08f),
        new Color(0.045f, 0.075f, 0.09f)
    };

    private static readonly Color[] Accents =
    {
        new Color(1f, 0.50f, 0.20f), new Color(0.10f, 0.82f, 0.95f), new Color(1f, 0.30f, 0.18f),
        new Color(0.58f, 0.40f, 1f), new Color(0.96f, 0.72f, 0.20f), new Color(0.92f, 0.28f, 0.78f),
        new Color(0.18f, 0.92f, 0.70f), new Color(1f, 0.40f, 0.12f), new Color(0.44f, 0.64f, 1f),
        new Color(0.35f, 0.92f, 0.96f)
    };

    private static readonly Color[] LevelOneFortressColors =
    {
        new Color(1.00f, 0.64f, 0.16f),
        new Color(0.16f, 0.48f, 0.92f),
        new Color(0.36f, 0.70f, 0.18f),
        new Color(0.88f, 0.28f, 0.14f),
        new Color(0.72f, 0.74f, 0.70f)
    };

    public static LevelDefinition Get(int zeroBasedLevel)
    {
        int index = Mathf.Clamp(zeroBasedLevel, 0, WarfestSession.LevelCount - 1);
        int theme = index % Titles.Length;
        return new LevelDefinition
        {
            number = index + 1,
            title = Titles[theme],
            subtitle = "OPERATION " + (index + 1).ToString("00") + " // " + Motifs[theme],
            motif = Motifs[theme],
            background = Backgrounds[theme],
            accent = Accents[theme],
            secondary = Color.Lerp(Accents[theme], Color.white, 0.25f),
            designType = index % 10,
            blockCount = index == 0 ? 56 : Mathf.Clamp(7 + index / 3, 7, 19),
            difficulty = 1 + index / 8
        };
    }

    public static void FillLayout(int zeroBasedLevel, List<BlockSpec> blocks)
    {
        blocks.Clear();
        if (zeroBasedLevel == 0)
        {
            FillLevelOneFortress(blocks);
            return;
        }

        LevelDefinition level = Get(zeroBasedLevel);
        int count = level.blockCount;
        float phase = zeroBasedLevel * 0.47f;
        for (int i = 0; i < count; i++)
        {
            Vector2 position;
            Vector2 scale = new Vector2(0.62f, 0.52f);
            float rotation = 0f;
            float t = count <= 1 ? 0f : i / (float)(count - 1);

            switch (level.designType)
            {
                case 0:
                    position = new Vector2(-3.9f + (i % 5) * 1.95f, 1.0f + (i / 5) * 0.62f);
                    break;
                case 1:
                    position = new Vector2(-4.3f + (i % 8) * 1.22f, 1.05f + (i / 8) * 0.72f);
                    scale = new Vector2(0.82f, 0.42f);
                    rotation = (i % 2 == 0 ? -1f : 1f) * 5f;
                    break;
                case 2:
                    int row = i / 6;
                    int column = i % 6;
                    position = new Vector2((column - 2.5f) * 1.2f + row * 0.18f, 1.0f + row * 0.58f);
                    break;
                case 3:
                    float angle = i / (float)count * Mathf.PI * 2f + phase;
                    position = new Vector2(Mathf.Cos(angle) * 3.1f, 2.1f + Mathf.Sin(angle) * 1.45f);
                    scale = new Vector2(0.56f, 0.42f);
                    rotation = angle * Mathf.Rad2Deg + 90f;
                    break;
                case 4:
                    position = new Vector2(-4.2f + t * 8.4f, 1.15f + Mathf.Sin(t * Mathf.PI * 4f + phase) * 1.25f + (i % 3) * 0.28f);
                    rotation = Mathf.Sin(t * Mathf.PI * 4f + phase) * 14f;
                    break;
                case 5:
                    float diamond = Mathf.Abs(i - (count - 1) * 0.5f);
                    position = new Vector2((i % 2 == 0 ? -1f : 1f) * diamond * 0.42f, 1.15f + i * 0.28f);
                    break;
                case 6:
                    position = new Vector2(i % 2 == 0 ? -2.6f - (i / 2) * 0.16f : 2.6f + (i / 2) * 0.16f, 1.1f + (i % 6) * 0.52f);
                    rotation = (i % 2 == 0 ? -1f : 1f) * (6f + i % 4 * 3f);
                    break;
                case 7:
                    position = new Vector2(-4.4f + (i % 10) * 0.92f, 1.0f + (i % 10) * 0.40f);
                    rotation = (i % 10) * 2.2f;
                    break;
                case 8:
                    position = new Vector2(-4.3f + t * 8.6f, 2.0f + Mathf.Cos(t * Mathf.PI * 2f + phase) * 1.05f);
                    scale = new Vector2(0.48f, 0.65f);
                    rotation = Mathf.Cos(t * Mathf.PI * 2f + phase) * 16f;
                    break;
                default:
                    float crownAngle = t * Mathf.PI * 1.18f - Mathf.PI * 0.59f;
                    position = new Vector2(Mathf.Sin(crownAngle) * 4.0f, 1.05f + (1f - Mathf.Cos(crownAngle)) * 1.65f);
                    rotation = crownAngle * Mathf.Rad2Deg;
                    break;
            }

            Color blockColor = Color.Lerp(level.accent, level.secondary, (i % 4) / 3f);
            blocks.Add(new BlockSpec(position, scale, rotation, blockColor));
        }
    }

private static void FillLevelOneFortress(List<BlockSpec> blocks)
    {
        Vector2 blockScale = new Vector2(0.70f, 0.68f);
        for (int row = 0; row < 9; row++)
        {
            for (int column = 0; column < 6; column++)
            {
                float x = (column - 2.5f) * 0.54f;
                float y = 1.48f + row * 0.48f;
                AddFortressBlock(blocks, x, y, blockScale, GetFortressSpriteIndex(row, column));
            }
        }

        AddFortressBlock(blocks, -1.35f, 5.55f, blockScale, 3);
        AddFortressBlock(blocks, 1.35f, 5.55f, blockScale, 3);
    }

private static void AddFortressBlock(List<BlockSpec> blocks, float x, float y, Vector2 scale, int spriteIndex)
    {
        int colorIndex = Mathf.Abs(Mathf.RoundToInt(x * 10f) + Mathf.RoundToInt(y * 10f)) % LevelOneFortressColors.Length;
        blocks.Add(new BlockSpec(new Vector2(x, y), scale, 0f, LevelOneFortressColors[colorIndex], spriteIndex));
    }

    private static int GetFortressSpriteIndex(int row, int column)
    {
        if (column == 0 || column == 5) return row >= 7 ? 3 : 2;
        if (row == 8) return 1;
        if (row == 0) return column == 2 || column == 3 ? 4 : 0;
        if (row == 4 && (column == 1 || column == 4)) return 4;
        if (column == 2 || column == 3) return row >= 3 && row <= 6 ? 5 : 0;
        return (row + column) % 3 == 0 ? 3 : 0;
    }
}
