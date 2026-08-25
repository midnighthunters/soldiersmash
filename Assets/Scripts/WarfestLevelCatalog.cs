using System.Collections.Generic;
using UnityEngine;

public static class WarfestLevelCatalog
{
    public struct BlockSpec
    {
        public Vector2 position;
        public Vector2 size;      // desired world-space footprint (sprite is fitted into this box)
        public float rotation;
        public Color color;
        public int spriteIndex;
        public bool kinematic;

        public BlockSpec(Vector2 position, Vector2 size, float rotation, Color color, int spriteIndex, bool kinematic)
        {
            this.position = position;
            this.size = size;
            this.rotation = rotation;
            this.color = color;
            this.spriteIndex = spriteIndex;
            this.kinematic = kinematic;
        }
    }

    // Describes one physical 3D construction piece. The width and height are explicit so
    // unlike models can share a precise modular grid without visual gaps or distorted spacing.
    public struct ModelBlockSpec
    {
        public float x;
        public float yOffset;
        public int variant;     // 0 = box, 1 = box2, 2 = box3, 3 = long_box, 4 = soldier, 5 = cannister
        public float width;
        public float height;
        public int depthLayer;  // 0 = front, 1 = rear
        public int tableIndex;

        public ModelBlockSpec(float x, float yOffset, int variant)
            : this(x, yOffset, variant, variant == 2 ? 0.36f : 0.72f, variant == 3 ? 0.36f : 0.72f, 0, 0)
        {
        }

        public ModelBlockSpec(float x, float yOffset, int variant, float width, float height)
            : this(x, yOffset, variant, width, height, 0, 0)
        {
        }

        public ModelBlockSpec(float x, float yOffset, int variant, float width, float height, int depthLayer, int tableIndex)
        {
            this.x = x;
            this.yOffset = yOffset;
            this.variant = variant;
            this.width = width;
            this.height = height;
            this.depthLayer = depthLayer;
            this.tableIndex = tableIndex;
        }
    }

    public struct ModelTableSpec
    {
        public float x;
        public float width;
        public float visibleTopY;
        public float depth;

        public ModelTableSpec(float x, float width, float visibleTopY, float depth = 2f)
        {
            this.x = x;
            this.width = width;
            this.visibleTopY = visibleTopY;
            this.depth = depth;
        }
    }

    // Square pieces occupy a 0.72 world-unit cell. Tall and long pieces occupy a half-cell
    // on their narrow axis, allowing every visible edge to meet on the same modular grid.
    public const float ModelColPitch = 0.72f;
    public const float ModelRowStep = 0.72f;

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

    // ---- Block sprite indices inside Resources/blocks -------------------------------------------
    private const int SMALL_ORANGE = 0; // small orange crate w/ diagonal plank
    private const int WIDE_ORANGE = 1;  // wide banded orange crate
    private const int ORANGE_X = 2;     // tall orange crate w/ X brace
    private const int STONE = 3;        // 2x2 cream stone cube cluster
    private const int BEAM = 4;         // horizontal orange beam on metal legs
    private const int SANDBAG = 5;      // sandbag pile
    private const int PILLAR = 6;       // narrow tall grey stone pillar
    private const int CANNON = 7;       // grey block with round cannon hole
    private const int METAL = 8;        // grey riveted metal plate crate
    private const int GREEN = 9;        // green wood crate

    // Grid geometry ------------------------------------------------------------------------------
    private const float ColPitch = 0.44f;      // fits every authored base row on the table collider
    private const float Cell = 0.42f;          // normal block footprint
    private const float BeamHeight = 0.16f;    // shallow support shelf footprint
    private const float TallHeight = 0.82f;    // X crates and pillars occupy a dedicated tall row
    private const float BlockGap = 0.025f;     // prevents colliders spawning interpenetrated
    private const float TableTopY = -0.351f;   // matches the visible tabletop in the table sprite

    private static readonly Color White = Color.white;

    // Each level is authored as ASCII art (top row first). Legend:
    //   .  empty            S stone cluster      C cannon-hole block   M metal crate
    //   G  green crate      o small orange crate D sandbag pile        X orange X crate (tall)
    //   I  stone pillar (tall)   B orange beam (runs of B merge into one wide shelf)
    private static readonly string[][] LevelGrids =
    {
        // ---------------------------------------------------------------- Level 1 (sample 1)
        new[]
        {
            "....BBB....",
            "....I.I....",
            "...BBBBB...",
            "....IGI....",
            "...BBBBB...",
            "...X.D.X...",
            "..BBBBBBB..",
            ".DD.MoM.DD.",
            ".BBBGGGBBB.",
            "..S.MoM.S..",
            "SSSS.C.SSSS",
        },
        // ---------------------------------------------------------------- Level 2 (sample 2)
        new[]
        {
            "....BBB....",
            "....I.I....",
            "....XGX....",
            "...BBBBB...",
            "..XM.D.MX..",
            "..BBBBBBB..",
            ".GX.MDM.XG.",
            ".GBBBBBBBG.",
            ".SGXMDMXGS.",
            "SSCSGGGSCSS",
        },
        // ---------------------------------------------------------------- Level 3 (sample 3)
        new[]
        {
            "....BBB....",
            "....I.I....",
            "..G.DDD.G..",
            "..BBBBBBB..",
            "..X.SSS.X..",
            "..BBBBBBB..",
            ".GD.MMM.DG.",
            ".GBBBBBBBG.",
            ".SD.M.M.DS.",
            ".C.SSXSS.C.",
        },
        // ---------------------------------------------------------------- Level 4 (sample 4)
        new[]
        {
            "....BBB....",
            "...o.X.o...",
            "...BBBBB...",
            "..SSMCMSS..",
            "..BBBBBBB..",
            ".X.MDDDM.X.",
            ".BBBBBBBBB.",
            "...GGGGG...",
            "..SDGGGDS..",
            "SSCSSXSSCSS",
        },
        // ---------------------------------------------------------------- Level 5 (sample 5, asymmetric)
        new[]
        {
            "...X.......",
            "...S..BBBB.",
            ".o.SS.G..S.",
            ".BBBB.MSS..",
            ".DDX..G.SS.",
            ".DD.GCG.o..",
            "SSSBBBBBGSS",
            "SS..ooo..SS",
        },
    };

    // The first ten campaign stages use carefully authored 3D constructions.
    public const int AuthoredLevelCount = 10;

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
            blockCount = Mathf.Clamp(7 + index / 3, 7, 19),
            difficulty = 1 + index / 8
        };
    }

    public static void FillLayout(int zeroBasedLevel, List<BlockSpec> blocks)
    {
        blocks.Clear();
        if (zeroBasedLevel >= 0 && zeroBasedLevel < LevelGrids.Length)
        {
            BuildFromGrid(LevelGrids[zeroBasedLevel], blocks);
            return;
        }

        FillProcedural(zeroBasedLevel, blocks);
    }

    // ------------------------------------------------------------------------------------------
    // Authored 3D layouts. Every piece snaps to the same 0.72-unit module. Rear-layer pieces
    // are offset deliberately so their silhouettes remain visible behind the front structure.
    // ------------------------------------------------------------------------------------------
    public static void FillModelLayout(int zeroBasedLevel, List<ModelBlockSpec> blocks)
    {
        blocks.Clear();
        switch (zeroBasedLevel)
        {
            case 0: BuildBrickWall(blocks); break;
            case 1: BuildTwinTowers(blocks); break;
            case 2: BuildZiggurat(blocks); break;
            case 3: BuildFortress(blocks); break;
            case 4: BuildStaircase(blocks); break;
            case 5: BuildCrossfireBastion(blocks); break;
            case 6: BuildSkylineRelay(blocks); break;
            case 7: BuildHourglassKeep(blocks); break;
            case 8: BuildTwinBarracks(blocks); break;
            default: BuildFinalCitadel(blocks); break;
        }
    }

    public static void FillModelTables(int zeroBasedLevel, List<ModelTableSpec> tables)
    {
        tables.Clear();
        switch (zeroBasedLevel)
        {
            case 3: // split gate
            case 8: // twin barracks
                tables.Add(new ModelTableSpec(-1.28f, 2.25f, -0.42f, 2.10f));
                tables.Add(new ModelTableSpec(1.28f, 2.25f, -0.42f, 2.05f));
                break;
            case 6: // staggered relay platforms
                tables.Add(new ModelTableSpec(-1.30f, 2.20f, -0.62f, 2.10f));
                tables.Add(new ModelTableSpec(1.25f, 2.20f, 0.05f, 2.05f));
                break;
            case 9: // final twin citadel
                tables.Add(new ModelTableSpec(-1.32f, 2.30f, -0.48f, 2.10f));
                tables.Add(new ModelTableSpec(1.32f, 2.30f, -0.48f, 2.05f));
                break;
            default:
                tables.Add(new ModelTableSpec(0f, 4.8f, -0.351f));
                break;
        }
    }

    private static float ColX(float column) => column * ModelColPitch;
    private static float RowY(int row) => row * ModelRowStep;

    private static void AddModel(List<ModelBlockSpec> b, float x, float y, int variant,
        float width = 0.72f, float height = 0.72f, int layer = 0, int table = 0)
    {
        b.Add(new ModelBlockSpec(x, y, variant, width, height, layer, table));
    }

    // Level 1: a solid brick wall, 5 crates wide and 3 tall, skins alternating per brick.
private static void BuildBrickWall(List<ModelBlockSpec> b)
    {
        const float cell = 0.36f;
        const float square = 0.72f;
        const float half = 0.36f;

        // Five-piece foundation: box and box2 alternate for a strong, colorful base.
        for (int i = 0; i < 5; i++)
        {
            float x = (i - 2) * square;
            b.Add(new ModelBlockSpec(x, 0f, i % 2, square, square));
        }

        // A continuous belt of long_box beams locks the foundation together.
        for (int i = 0; i < 5; i++)
        {
            float x = (i - 2) * square;
            b.Add(new ModelBlockSpec(x, square, 3, square, half));
        }

        // Solid middle storey: paired tall box3 pillars frame three square blocks.
        float[] edgePillars = { -4.5f, -3.5f, 3.5f, 4.5f };
        for (int i = 0; i < edgePillars.Length; i++)
        {
            b.Add(new ModelBlockSpec(edgePillars[i] * cell, square + half, 2, half, square));
        }
        b.Add(new ModelBlockSpec(-square, square + half, 1, square, square));
        b.Add(new ModelBlockSpec(0f, square + half, 4, square, square));
        b.Add(new ModelBlockSpec(square, square + half, 1, square, square));

        // Second locking belt.
        for (int i = 0; i < 5; i++)
        {
            float x = (i - 2) * square;
            b.Add(new ModelBlockSpec(x, square * 2f + half, 3, square, half));
        }

        // Upper gate: close-set pillar pairs and two square shoulders leave one deliberate arch.
        for (int i = 0; i < edgePillars.Length; i++)
        {
            b.Add(new ModelBlockSpec(edgePillars[i] * cell, square * 2f + half * 2f, 2, half, square));
        }
        b.Add(new ModelBlockSpec(-square, square * 2f + half * 2f, 0, square, square));
        b.Add(new ModelBlockSpec(square, square * 2f + half * 2f, 1, square, square));

        // Three overlapping-span cap beams bridge the gate and rest securely on both shoulders.
        float capY = square * 3f + half * 2f;
        b.Add(new ModelBlockSpec(-1.26f, capY, 3, 1.08f, half));
        b.Add(new ModelBlockSpec(0f, capY, 3, 1.44f, half));
        b.Add(new ModelBlockSpec(1.26f, capY, 3, 1.08f, half));
    }

    // Level 2: a symmetrical guardhouse with a visible rear watchtower.
    private static void BuildTwinTowers(List<ModelBlockSpec> b)
    {
        // Rear watchtower: warm blocks peeking through the gate opening.
        for (int row = 0; row < 3; row++) AddModel(b, 0.18f, RowY(row) + 0.18f, row == 1 ? 4 : 0, 0.72f, 0.72f, 1);
        AddModel(b, -0.54f, RowY(3) + 0.18f, 3, 1.08f, 0.36f, 1);
        AddModel(b, 0.72f, RowY(3) + 0.18f, 3, 1.08f, 0.36f, 1);

        for (int row = 0; row < 4; row++)
        {
            AddModel(b, -1.44f, RowY(row), row % 2);
            AddModel(b, 1.44f, RowY(row), (row + 1) % 2);
        }
        AddModel(b, -0.72f, 0f, 2, 0.36f, 0.72f);
        AddModel(b, 0f, 0f, 4);
        AddModel(b, 0.72f, 0f, 2, 0.36f, 0.72f);
        AddModel(b, -0.90f, RowY(4), 3, 1.08f, 0.36f);
        AddModel(b, 0f, RowY(4), 3, 0.72f, 0.36f);
        AddModel(b, 0.90f, RowY(4), 3, 1.08f, 0.36f);
    }

    // Level 3: a colorful stepped ziggurat with a golden rear spine.
    private static void BuildZiggurat(List<ModelBlockSpec> b)
    {
        for (int row = 0; row < 4; row++)
            AddModel(b, 0.22f, RowY(row) + 0.18f, row == 2 ? 4 : 1, 0.72f, 0.72f, 1);
        AddModel(b, -0.68f, RowY(4) + 0.18f, 3, 1.08f, 0.36f, 1);
        AddModel(b, 0.58f, RowY(4) + 0.18f, 3, 1.08f, 0.36f, 1);

        int[] widths = { 5, 4, 3, 2, 1 };
        for (int row = 0; row < widths.Length; row++)
        {
            int w = widths[row];
            float start = -(w - 1) * 0.5f;
            for (int i = 0; i < w; i++)
                AddModel(b, ColX(start + i), RowY(row), (row + i) % 3);
        }
        AddModel(b, 0f, RowY(5), 4);
    }

    // Level 4: two independent gate towers, each on its own table.
    private static void BuildFortress(List<ModelBlockSpec> b)
    {
        for (int table = 0; table < 2; table++)
        {
            float center = table == 0 ? -1.28f : 1.28f;
            int flip = table == 0 ? 0 : 1;
            AddModel(b, center - 0.36f, 0f, flip, 0.72f, 0.72f, 0, table);
            AddModel(b, center + 0.36f, 0f, 1 - flip, 0.72f, 0.72f, 0, table);
            AddModel(b, center - 0.54f, RowY(1), 2, 0.36f, 0.72f, 0, table);
            AddModel(b, center, RowY(1), 4, 0.72f, 0.72f, 0, table);
            AddModel(b, center + 0.54f, RowY(1), 2, 0.36f, 0.72f, 0, table);
            AddModel(b, center, RowY(2), 3, 1.44f, 0.36f, 0, table);
            AddModel(b, center - 0.38f, 0.18f, 1, 0.72f, 0.72f, 1, table);
            AddModel(b, center + 0.38f, RowY(1) + 0.18f, 0, 0.72f, 0.72f, 1, table);
        }
    }

    // Level 5: interlocking front and rear staircases climbing in opposite directions.
    private static void BuildStaircase(List<ModelBlockSpec> b)
    {
        int[] frontHeights = { 1, 2, 3, 4, 5 };
        int[] rearHeights = { 5, 4, 3, 2, 1 };
        for (int col = 0; col < frontHeights.Length; col++)
        {
            float x = ColX(col - 2);
            for (int row = 0; row < rearHeights[col]; row++)
                AddModel(b, x + 0.18f, RowY(row) + 0.18f, (row + col + 1) % 2, 0.72f, 0.72f, 1);
            for (int row = 0; row < frontHeights[col]; row++)
            {
                bool useCannister = (col == 1 && row == 1) || (col == 3 && row == 1);
                AddModel(b, x, RowY(row), useCannister ? 5 : (row + col) % 3,
                    useCannister ? 0.56f : 0.72f, 0.72f);
            }
        }
        AddModel(b, ColX(-1.5f), RowY(5), 3, 1.44f, 0.36f);
        AddModel(b, ColX(2), RowY(5), 4);
    }

    // Level 6: broad armored bastion with a soldier corridor and rear cross-bracing.
    private static void BuildCrossfireBastion(List<ModelBlockSpec> b)
    {
        for (int i = -2; i <= 2; i++)
            AddModel(b, ColX(i), 0f, Mathf.Abs(i) == 1 ? 5 : Mathf.Abs(i) % 2,
                Mathf.Abs(i) == 1 ? 0.56f : 0.72f, 0.72f);
        AddModel(b, -1.44f, RowY(1), 2, 0.36f, 0.72f);
        AddModel(b, -0.72f, RowY(1), 4);
        AddModel(b, 0f, RowY(1), 3, 0.72f, 0.36f);
        AddModel(b, 0.72f, RowY(1), 4);
        AddModel(b, 1.44f, RowY(1), 2, 0.36f, 0.72f);
        for (int i = -2; i <= 2; i++) AddModel(b, ColX(i), RowY(2), (i + 5) % 3);
        AddModel(b, -1.08f, RowY(3), 3, 1.44f, 0.36f);
        AddModel(b, 0f, RowY(3), 4);
        AddModel(b, 1.08f, RowY(3), 3, 1.44f, 0.36f);

        for (int i = -2; i <= 2; i += 2)
        {
            AddModel(b, ColX(i) + 0.18f, RowY(1) + 0.18f, 1, 0.72f, 0.72f, 1);
            AddModel(b, ColX(i) + 0.18f, RowY(2) + 0.18f, i == 0 ? 4 : 0, 0.72f, 0.72f, 1);
        }
    }

    // Level 7: two staggered relay platforms with distinct tower profiles.
    private static void BuildSkylineRelay(List<ModelBlockSpec> b)
    {
        for (int row = 0; row < 4; row++)
        {
            AddModel(b, -1.62f, RowY(row), row == 1 ? 5 : row % 2,
                row == 1 ? 0.56f : 0.72f, 0.72f, 0, 0);
            if (row < 3) AddModel(b, -0.90f, RowY(row), row == 1 ? 4 : 2, row == 1 ? 0.72f : 0.36f, 0.72f, 0, 0);
        }
        AddModel(b, -1.26f, RowY(4), 3, 1.44f, 0.36f, 0, 0);

        for (int row = 0; row < 3; row++)
        {
            AddModel(b, 0.90f, RowY(row), row == 1 ? 4 : 1, 0.72f, 0.72f, 0, 1);
            AddModel(b, 1.62f, RowY(row), row == 1 ? 5 : row % 2,
                row == 1 ? 0.56f : 0.72f, 0.72f, 0, 1);
        }
        AddModel(b, 1.26f, RowY(3), 3, 1.44f, 0.36f, 0, 1);

        AddModel(b, -1.44f, 0.18f, 2, 0.36f, 0.72f, 1, 0);
        AddModel(b, -0.72f, RowY(2) + 0.18f, 0, 0.72f, 0.72f, 1, 0);
        AddModel(b, 1.44f, 0.18f, 2, 0.36f, 0.72f, 1, 1);
        AddModel(b, 0.72f, RowY(2) + 0.18f, 1, 0.72f, 0.72f, 1, 1);
    }

    // Level 8: hourglass silhouette, pinched around a soldier core.
    private static void BuildHourglassKeep(List<ModelBlockSpec> b)
    {
        int[] widths = { 5, 3, 1, 3, 5 };
        for (int row = 0; row < widths.Length; row++)
        {
            int w = widths[row];
            float start = -(w - 1) * 0.5f;
            for (int i = 0; i < w; i++)
            {
                bool useCannister = row == 0 && (i == 1 || i == 3);
                AddModel(b, ColX(start + i), RowY(row), useCannister ? 5 : (row == 2 ? 4 : (row + i) % 3),
                    useCannister ? 0.56f : 0.72f, 0.72f);
            }
        }
        AddModel(b, -1.26f, RowY(5), 3, 1.08f, 0.36f);
        AddModel(b, 0f, RowY(5), 3, 1.44f, 0.36f);
        AddModel(b, 1.26f, RowY(5), 3, 1.08f, 0.36f);
        for (int row = 0; row < 4; row++)
        {
            AddModel(b, -1.26f, RowY(row) + 0.18f, row % 2, 0.72f, 0.72f, 1);
            AddModel(b, 1.26f, RowY(row) + 0.18f, (row + 1) % 2, 0.72f, 0.72f, 1);
        }
    }

    // Level 9: paired barracks with rear supply stacks and front soldiers.
    private static void BuildTwinBarracks(List<ModelBlockSpec> b)
    {
        for (int table = 0; table < 2; table++)
        {
            float center = table == 0 ? -1.28f : 1.28f;
            AddModel(b, center - 0.36f, 0f, 0, 0.72f, 0.72f, 0, table);
            AddModel(b, center + 0.36f, 0f, 1, 0.72f, 0.72f, 0, table);
            AddModel(b, center - 0.54f, RowY(1), 2, 0.36f, 0.72f, 0, table);
            AddModel(b, center, RowY(1), 4, 0.72f, 0.72f, 0, table);
            AddModel(b, center + 0.54f, RowY(1), 2, 0.36f, 0.72f, 0, table);
            AddModel(b, center, RowY(2), 3, 1.44f, 0.36f, 0, table);
            AddModel(b, center - 0.38f, 0.18f, 1, 0.72f, 0.72f, 1, table);
            AddModel(b, center + 0.38f, 0.18f, 0, 0.72f, 0.72f, 1, table);
            AddModel(b, center, RowY(1) + 0.18f, 5, 0.56f, 0.72f, 1, table);
        }
    }

    // Level 10: final twin citadel with rear guard towers, soldiers, and long parapets.
    private static void BuildFinalCitadel(List<ModelBlockSpec> b)
    {
        for (int table = 0; table < 2; table++)
        {
            float center = table == 0 ? -1.32f : 1.32f;
            for (int row = 0; row < 4; row++)
            {
                bool leftCannister = row == 1 && table == 0;
                bool rightCannister = row == 1 && table == 1;
                AddModel(b, center - 0.36f, RowY(row), leftCannister ? 5 : (row + table) % 2,
                    leftCannister ? 0.56f : 0.72f, 0.72f, 0, table);
                AddModel(b, center + 0.36f, RowY(row), rightCannister ? 5 : (row + table + 1) % 2,
                    rightCannister ? 0.56f : 0.72f, 0.72f, 0, table);
            }
            AddModel(b, center - 0.54f, RowY(4), 2, 0.36f, 0.72f, 0, table);
            AddModel(b, center, RowY(4), 4, 0.72f, 0.72f, 0, table);
            AddModel(b, center + 0.54f, RowY(4), 2, 0.36f, 0.72f, 0, table);
            AddModel(b, center, RowY(5), 3, 1.44f, 0.36f, 0, table);

            AddModel(b, center, 0.18f, 4, 0.72f, 0.72f, 1, table);
            AddModel(b, center - 0.38f, RowY(1) + 0.18f, 0, 0.72f, 0.72f, 1, table);
            AddModel(b, center + 0.38f, RowY(2) + 0.18f, 1, 0.72f, 0.72f, 1, table);
            AddModel(b, center, RowY(3) + 0.18f, 4, 0.72f, 0.72f, 1, table);
        }
    }

    // ------------------------------------------------------------------------------------------
    // ASCII grid -> block specifications
    // ------------------------------------------------------------------------------------------
    private static void BuildFromGrid(string[] grid, List<BlockSpec> blocks)
    {
        int rows = grid.Length;
        int cols = 0;
        for (int r = 0; r < rows; r++) cols = Mathf.Max(cols, grid[r].Length);

        float centerCol = (cols - 1) * 0.5f;
        float[] rowBottomY = new float[rows];
        float nextRowBottom = TableTopY + BlockGap;
        for (int fb = 0; fb < rows; fb++)
        {
            int r = rows - 1 - fb;
            rowBottomY[r] = nextRowBottom;
            nextRowBottom += GetRowHeight(grid[r]) + BlockGap;
        }

        for (int r = 0; r < rows; r++)
        {
            string line = grid[r];
            float rowBottom = rowBottomY[r];
            for (int c = 0; c < line.Length; c++)
            {
                char token = line[c];
                if (token == '.' || token == ' ') continue;

                float colX = (c - centerCol) * ColPitch;
                if (token == 'B')
                {
                    int start = c;
                    while (c + 1 < line.Length && line[c + 1] == 'B') c++;
                    int runLength = c - start + 1;
                    float beamCenterX = ((start + c) * 0.5f - centerCol) * ColPitch;
                    float beamWidth = runLength * ColPitch - BlockGap;
                    blocks.Add(new BlockSpec(
                        new Vector2(beamCenterX, rowBottom + BeamHeight * 0.5f),
                        new Vector2(beamWidth, BeamHeight),
                        0f, White, BEAM, false));
                    continue;
                }

                AddToken(blocks, token, colX, rowBottom);
            }
        }
    }

    private static float GetRowHeight(string line)
    {
        bool hasTall = false;
        bool hasNonBeam = false;
        bool hasBeam = false;
        foreach (char token in line)
        {
            hasTall |= token == 'X' || token == 'I';
            hasNonBeam |= token != '.' && token != ' ' && token != 'B';
            hasBeam |= token == 'B';
        }

        if (hasTall) return TallHeight;
        return hasBeam && !hasNonBeam ? BeamHeight : Cell;
    }

    private static void AddToken(List<BlockSpec> blocks, char token, float x, float rowBottom)
    {
        float normalCenterY = rowBottom + Cell * 0.5f;
        switch (token)
        {
            case 'S':
                blocks.Add(new BlockSpec(new Vector2(x, normalCenterY), new Vector2(Cell, Cell), 0f, White, STONE, false));
                break;
            case 'C':
                blocks.Add(new BlockSpec(new Vector2(x, normalCenterY), new Vector2(Cell, Cell), 0f, White, CANNON, false));
                break;
            case 'M':
                blocks.Add(new BlockSpec(new Vector2(x, normalCenterY), new Vector2(Cell, Cell), 0f, White, METAL, false));
                break;
            case 'G':
                blocks.Add(new BlockSpec(new Vector2(x, normalCenterY), new Vector2(Cell, Cell), 0f, White, GREEN, false));
                break;
            case 'o':
                blocks.Add(new BlockSpec(new Vector2(x, normalCenterY), new Vector2(Cell * 0.92f, Cell * 0.92f), 0f, White, SMALL_ORANGE, false));
                break;
            case 'D':
                blocks.Add(new BlockSpec(new Vector2(x, normalCenterY), new Vector2(Cell, Cell), 0f, White, SANDBAG, false));
                break;
            case 'X':
                blocks.Add(new BlockSpec(new Vector2(x, rowBottom + TallHeight * 0.5f), new Vector2(Cell * 0.72f, TallHeight), 0f, White, ORANGE_X, false));
                break;
            case 'I':
                blocks.Add(new BlockSpec(new Vector2(x, rowBottom + TallHeight * 0.5f), new Vector2(Cell * 0.62f, TallHeight), 0f, White, PILLAR, false));
                break;
            case 'W':
                blocks.Add(new BlockSpec(new Vector2(x, normalCenterY), new Vector2(Cell * 1.5f, Cell), 0f, White, WIDE_ORANGE, false));
                break;
        }
    }

    // ------------------------------------------------------------------------------------------
    // Procedural fallback for levels 11+ (keeps the original 50-level campaign functional)
    // ------------------------------------------------------------------------------------------
    private static void FillProcedural(int zeroBasedLevel, List<BlockSpec> blocks)
    {
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
                    int rowIndex = i / 6;
                    int columnIndex = i % 6;
                    position = new Vector2((columnIndex - 2.5f) * 1.2f + rowIndex * 0.18f, 1.0f + rowIndex * 0.58f);
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

            Vector2 worldSize = new Vector2(Mathf.Max(0.42f, scale.x * 0.85f), Mathf.Max(0.42f, scale.y * 0.95f));
            int spriteIndex = i % 10;
            blocks.Add(new BlockSpec(position, worldSize, rotation, White, spriteIndex, false));
        }
    }
}
