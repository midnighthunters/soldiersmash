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
        public int variant;     // 0 = box, 1 = box2, 2 = box3, 3 = long_box, 4 = soldier, 5 = cannister, 6 = bomb
        public float width;
        public float height;
        public int depthLayer;  // 0 = front, 1 = rear
        public int tableIndex;
        public float rotation;  // z-axis rotation in degrees (0 = upright). Used for tilted layouts.

        public ModelBlockSpec(float x, float yOffset, int variant)
            : this(x, yOffset, variant, variant == 2 ? 0.36f : 0.72f, variant == 3 ? 0.36f : 0.72f, 0, 0, 0f)
        {
        }

        public ModelBlockSpec(float x, float yOffset, int variant, float width, float height)
            : this(x, yOffset, variant, width, height, 0, 0, 0f)
        {
        }

        public ModelBlockSpec(float x, float yOffset, int variant, float width, float height, int depthLayer, int tableIndex, float rotation = 0f)
        {
            this.x = x;
            this.yOffset = yOffset;
            this.variant = variant;
            this.width = width;
            this.height = height;
            this.depthLayer = depthLayer;
            this.tableIndex = tableIndex;
            this.rotation = rotation;
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

    // Every model level renders its pedestal table at this exact world width so the table always
    // appears at the same size, tilt and camera distance (see CreateModelTable, which scales the
    // table model uniformly to this width). The reference framing has clear sand margins on both
    // sides of the table; a value of 4.8 reproduces it. TableSpanMargin is the breathing room kept
    // between the structure footprint and the table edge, so MaxStructureSpan is the widest a
    // structure may be before it is scaled down to sit fully on the fixed-size table.
    public const float TargetTableWidth = 4.8f;
    private const float TableSpanMargin = 1.2f;
    private const float MaxStructureSpan = TargetTableWidth - TableSpanMargin; // 3.6

    // World-space z of the two visual crate planes. Must stay in sync with CreateModelBox so the
    // table can be centred under the stack in depth.
    public const float FrontLayerZ = 0.08f;
    public const float RearLayerZ = 0.72f;

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

    // The complete campaign uses deterministic, authored 3D construction rules.
    public const int AuthoredLevelCount = 100;

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
    // Authored 3D layouts (levels 1-50).
    //
    // Every level is a single, perfectly mirror-symmetric structure built from a right-half
    // column-height profile that is reflected across x = 0. Ten silhouette families cycle every
    // ten levels, while the width, height, block density and rear skyline all scale up with the
    // level number - so the campaign climbs from a small starter wall to a towering citadel.
    //
    // Asset roles (variant -> Resources model):
    //   0 box       light structural brick    |  1 box2     heavy structural brick
    //   2 box3      slim corner turret         |  3 long_box horizontal capstone / lintel
    //   4 soldier   objective topper           |  5 cannister barrel topper
    //   6 bomb      chain-reaction target (injected after the layout is built)
    // ------------------------------------------------------------------------------------------

    private static float ColX(float column) => column * ModelColPitch;
    private static float RowY(int row) => row * ModelRowStep;

    private static void AddModel(List<ModelBlockSpec> b, float x, float y, int variant,
        float width = 0.72f, float height = 0.72f, int layer = 0, int table = 0, float rotation = 0f)
    {
        b.Add(new ModelBlockSpec(x, y, variant, width, height, layer, table, rotation));
    }

    public static void FillModelLayout(int zeroBasedLevel, List<ModelBlockSpec> blocks)
    {
        blocks.Clear();

        // Levels 1-20 are individually hand-authored (see AuthoredCampaign). Levels 21-100 use a
        // multi-family generator that keeps producing visibly different symmetric structures.
        if (!BuildAuthoredLayout(zeroBasedLevel, blocks))
        {
            BuildGeneratedLayout(zeroBasedLevel, blocks);
        }

        // Guarantee every structure fits the fixed-size pedestal table so the table renders at the
        // same width, tilt and distance in every level regardless of how wide the design was authored.
        NormalizeLayoutToTable(blocks);
    }

    // Uniformly scales the whole structure down (never up) about its own centre until its widest
    // point fits within MaxStructureSpan. Scaling x, y, width and height by the same factor keeps
    // every crate square and every tilt angle intact, so a wide design simply becomes a smaller,
    // proportional copy that rests fully on the standard table instead of forcing an oversized one.
    private static void NormalizeLayoutToTable(List<ModelBlockSpec> blocks)
    {
        if (blocks.Count == 0) return;

        float minX = float.MaxValue;
        float maxX = float.MinValue;
        for (int i = 0; i < blocks.Count; i++)
        {
            ModelBlockSpec s = blocks[i];
            float rad = s.rotation * Mathf.Deg2Rad;
            float halfWidth = 0.5f * (Mathf.Abs(s.width * Mathf.Cos(rad)) + Mathf.Abs(s.height * Mathf.Sin(rad)));
            minX = Mathf.Min(minX, s.x - halfWidth);
            maxX = Mathf.Max(maxX, s.x + halfWidth);
        }

        float span = maxX - minX;
        if (span <= MaxStructureSpan || span <= 0.0001f) return;

        float scale = MaxStructureSpan / span;
        float centerX = (minX + maxX) * 0.5f;
        for (int i = 0; i < blocks.Count; i++)
        {
            ModelBlockSpec s = blocks[i];
            s.x = centerX + (s.x - centerX) * scale;
            s.yOffset *= scale;   // yOffset is the height above the tabletop, so vertical stacking shrinks too
            s.width *= scale;
            s.height *= scale;
            blocks[i] = s;
        }
    }

    public static void FillModelTables(int zeroBasedLevel, List<ModelTableSpec> tables)
    {
        tables.Clear();

        // Size the table to the exact footprint of whatever structure the level builds, so wide,
        // asymmetric or tilted designs still rest fully on the surface.
        List<ModelBlockSpec> layout = new List<ModelBlockSpec>();
        FillModelLayout(zeroBasedLevel, layout);

        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minZ = float.MaxValue;
        float maxZ = float.MinValue;
        const float halfDepth = 0.36f; // half-depth of a standard crate plane
        for (int i = 0; i < layout.Count; i++)
        {
            ModelBlockSpec s = layout[i];
            float rad = s.rotation * Mathf.Deg2Rad;
            float halfWidth = 0.5f * (Mathf.Abs(s.width * Mathf.Cos(rad)) + Mathf.Abs(s.height * Mathf.Sin(rad)));
            minX = Mathf.Min(minX, s.x - halfWidth);
            maxX = Mathf.Max(maxX, s.x + halfWidth);

            float layerZ = s.depthLayer == 0 ? FrontLayerZ : RearLayerZ;
            minZ = Mathf.Min(minZ, layerZ - halfDepth);
            maxZ = Mathf.Max(maxZ, layerZ + halfDepth);
        }
        if (layout.Count == 0)
        {
            minX = -1.5f; maxX = 1.5f;
            minZ = FrontLayerZ - halfDepth; maxZ = RearLayerZ + halfDepth;
        }

        float center = (minX + maxX) * 0.5f;
        // The layout was already normalized to fit MaxStructureSpan, so lock the table to the exact
        // target width. This makes the pedestal render at an identical size, tilt and distance in
        // every level (matching the reference framing) instead of growing with the structure.
        float width = TargetTableWidth;
        // Centre the table under the crate stack in depth so the blocks visibly rest ON the
        // tabletop rather than floating in front of it (see CreateModelTable / CreateModelBox).
        float depthCenter = (minZ + maxZ) * 0.5f;
        tables.Add(new ModelTableSpec(center, width, -0.351f, depthCenter));
    }

    // ==========================================================================================
    // #region AuthoredCampaign  --  levels 1-100
    //
    // Levels 1-20 are individually authored (1-4 follow the exact requested shapes). Levels
    // 21-100 are produced by BuildGeneratedLayout, which cycles a dozen distinct, symmetric
    // structure families and grows them with the level number, so the campaign keeps looking
    // fresh instead of repeating one silhouette. Every layout is mirror-symmetric about x = 0
    // (or built from mirrored pairs) and uses at least 18 blocks.
    //
    // Grid: columns sit on a 0.72 pitch; RowY(r) = r * 0.72 is the bottom of row r. Variants:
    // 0 box, 1 box2 (heavy), 2 box3 (turret), 3 long_box (lintel/cap/plank), 4 soldier
    // (objective), 5 cannister (barrel), 6 bomb (chain-reaction target). A block may carry a
    // z-rotation for tilted layouts.
    // ==========================================================================================

    private static bool BuildAuthoredLayout(int zeroBasedLevel, List<ModelBlockSpec> b)
    {
        switch (zeroBasedLevel)
        {
            case 0: Level01_FrontRearBlock(b); return true;
            case 1: Level02_CannisterPyramid(b); return true;
            case 2: Level03_FigureEights(b); return true;
            case 3: Level04_TiltedTables(b); return true;
            case 4: Level05_HollowFort(b); return true;
            case 5: Level06_DoubleGate(b); return true;
            case 6: Level07_BombCheckerboard(b); return true;
            case 7: Level08_BarrelPyramid(b); return true;
            case 8: Level09_TripleTowers(b); return true;
            case 9: Level10_TwinDiamonds(b); return true;
            case 10: Level11_LayeredWall(b); return true;
            case 11: Level12_TwinPeaks(b); return true;
            case 12: Level13_Battlements(b); return true;
            case 13: Level14_Colonnade(b); return true;
            case 14: Level15_StepPyramid(b); return true;
            case 15: Level16_TowerBridges(b); return true;
            case 16: Level17_Ziggurat(b); return true;
            case 17: Level18_HollowKeep(b); return true;
            case 18: Level19_Bastion(b); return true;
            case 19: Level20_GrandBastion(b); return true;
            default: return false;
        }
    }

    // ---- authoring helpers ---------------------------------------------------------------------

    private static void Box(List<ModelBlockSpec> b, float x, int row, bool heavy)
        => AddModel(b, x, RowY(row), heavy ? 1 : 0);

    // Alternating heavy/light column of `rows` crates (heavy on the ground for stability).
    private static void Column(List<ModelBlockSpec> b, float x, int rows)
    {
        for (int r = 0; r < rows; r++) AddModel(b, x, RowY(r), (r % 2 == 0) ? 1 : 0);
    }

    // Same, but the crate at `bombRow` becomes a bomb.
    private static void ColumnB(List<ModelBlockSpec> b, float x, int rows, int bombRow)
    {
        for (int r = 0; r < rows; r++)
        {
            if (r == bombRow) AddModel(b, x, RowY(r), 6, 0.56f, 0.72f);
            else AddModel(b, x, RowY(r), (r % 2 == 0) ? 1 : 0);
        }
    }

    private static void Soldier(List<ModelBlockSpec> b, float x, int row) => AddModel(b, x, RowY(row), 4, 0.72f, 0.72f);
    private static void Cannister(List<ModelBlockSpec> b, float x, int row) => AddModel(b, x, RowY(row), 5, 0.56f, 0.72f);
    private static void Turret(List<ModelBlockSpec> b, float x, int row) => AddModel(b, x, RowY(row), 2, 0.36f, 0.72f);
    private static void Lintel(List<ModelBlockSpec> b, float x, int row, float width) => AddModel(b, x, RowY(row), 3, width, 0.36f);
    private static void Bomb(List<ModelBlockSpec> b, float x, int row) => AddModel(b, x, RowY(row), 6, 0.56f, 0.72f);
    private static void At(List<ModelBlockSpec> b, float x, float y, int variant, float width, float height) => AddModel(b, x, y, variant, width, height);

    private static void RearColumn(List<ModelBlockSpec> b, float x, int rows)
    {
        for (int r = 0; r < rows; r++) AddModel(b, x, RowY(r), (r % 2 == 0) ? 1 : 0, 0.72f, 0.72f, 1);
    }
    private static void RearTurret(List<ModelBlockSpec> b, float x, int row) => AddModel(b, x, RowY(row), 2, 0.36f, 0.72f, 1);

    // One topper per column of a symmetric wall: soldier centre, turrets on the ends, barrels and
    // capstones alternating between.
    private static void CrownColumns(List<ModelBlockSpec> b, int half, int topRow)
    {
        for (int c = -half; c <= half; c++)
        {
            float x = c * 0.72f;
            int a = Mathf.Abs(c);
            if (c == 0) Soldier(b, x, topRow);
            else if (a == half) Turret(b, x, topRow);
            else if (a % 2 == 1) Cannister(b, x, topRow);
            else Lintel(b, x, topRow, 0.72f);
        }
    }

    // A solid k x k block of crates rotated 45 degrees so it reads as a diamond, with a bomb core.
    private static void DiamondBlock(List<ModelBlockSpec> b, float cx, float cy, int k, float spacing)
    {
        int half = (k - 1) / 2;
        for (int i = -half; i <= half; i++)
            for (int j = -half; j <= half; j++)
            {
                float ox = (i - j) * spacing * 0.7071f;
                float oy = (i + j) * spacing * 0.7071f;
                int variant = (i == 0 && j == 0) ? 6 : (((i + j) % 2 == 0) ? 1 : 0);
                AddModel(b, cx + ox, cy + oy, variant, spacing, spacing, 0, 0, 45f);
            }
    }

    // A tilted "table" (a long_box plank) at `angleDeg` carrying an n x n grid of cannisters on
    // its upper face. Used by level 4.
    private static void TiltedCannisterTable(List<ModelBlockSpec> b, float cx, float cy, float angleDeg, float spacing, int n)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        float dx = Mathf.Cos(rad), dy = Mathf.Sin(rad);    // along the plank
        float px = -Mathf.Sin(rad), py = Mathf.Cos(rad);   // out of the plank (up)
        AddModel(b, cx, cy, 3, spacing * n, 0.32f, 0, 0, angleDeg);   // the tilted table
        int half = (n - 1) / 2;
        for (int col = -half; col <= half; col++)
            for (int row = 0; row < n; row++)
            {
                float t = row + 0.85f;
                float ox = col * spacing * dx + t * spacing * px;
                float oy = col * spacing * dy + t * spacing * py;
                AddModel(b, cx + ox, cy + oy, 5, spacing * 0.92f, spacing * 1.2f, 0, 0, angleDeg);
            }
    }

    // A single figure-eight built from long_box rungs and short box uprights (two stacked loops).
    private static void FigureEight(List<ModelBlockSpec> b, float cx)
    {
        const float rs = 0.5f;
        AddModel(b, cx, 0 * rs, 3, 1.1f, 0.34f);
        AddModel(b, cx, 3 * rs, 3, 1.1f, 0.34f);
        AddModel(b, cx, 6 * rs, 3, 1.1f, 0.34f);
        int[] rows = { 1, 2, 4, 5 };
        for (int i = 0; i < rows.Length; i++)
        {
            AddModel(b, cx - 0.34f, rows[i] * rs, 0, 0.6f, 0.5f);
            AddModel(b, cx + 0.34f, rows[i] * rs, 0, 0.6f, 0.5f);
        }
    }

    // ---- levels 1-4 : exactly the requested shapes --------------------------------------------

    // Front 3x3 wall, an identical 3x3 wall directly behind it, and a soldier objective on top.
    private static void Level01_FrontRearBlock(List<ModelBlockSpec> b)
    {
        float[] xs = { -0.72f, 0f, 0.72f };
        for (int c = 0; c < 3; c++)
            for (int r = 0; r < 3; r++)
            {
                AddModel(b, xs[c], RowY(r), (r % 2 == 0) ? 1 : 0);              // front 3x3
                AddModel(b, xs[c], RowY(r), (r % 2 == 0) ? 0 : 1, 0.72f, 0.72f, 1); // rear 3x3
            }
        Soldier(b, 0f, 3);
    }

    // A pyramid of cannisters only: 6 across the bottom rising to a single one at the apex.
    private static void Level02_CannisterPyramid(List<ModelBlockSpec> b)
    {
        const float pitch = 0.62f;
        const float rowStep = 0.6f;
        for (int r = 0; r < 6; r++)
        {
            int count = 6 - r;
            for (int i = 0; i < count; i++)
            {
                float x = (i - (count - 1) * 0.5f) * pitch;
                AddModel(b, x, r * rowStep, 5, 0.56f, 0.72f);
            }
        }
    }

    // Two figure-eight patterns made only of long blocks and short blocks.
    private static void Level03_FigureEights(List<ModelBlockSpec> b)
    {
        FigureEight(b, -1.35f);
        FigureEight(b, 1.35f);
    }

    // Two 45-degree tilted tables, one leaning each way, each carrying a 5x5 grid of cannisters.
    private static void Level04_TiltedTables(List<ModelBlockSpec> b)
    {
        TiltedCannisterTable(b, -1.9f, 0.6f, -45f, 0.4f, 5);
        TiltedCannisterTable(b, 1.9f, 0.6f, 45f, 0.4f, 5);
    }

    // ---- levels 5-11 : bespoke set pieces -----------------------------------------------------

    // A hollow fort: full base and top courses, side walls, a bomb in the courtyard, crenellations.
    private static void Level05_HollowFort(List<ModelBlockSpec> b)
    {
        float[] xs = { -1.44f, -0.72f, 0f, 0.72f, 1.44f };
        for (int i = 0; i < xs.Length; i++)
        {
            if (xs[i] == 0f) Bomb(b, 0f, 0); else Box(b, xs[i], 0, true);   // base course + courtyard bomb
            Box(b, xs[i], 3, false);                                        // top course
        }
        Box(b, -1.44f, 1, true); Box(b, -1.44f, 2, false);                  // left wall
        Box(b, 1.44f, 1, true); Box(b, 1.44f, 2, false);                    // right wall
        CrownColumns(b, 2, 4);
    }

    // Two arches side by side over a three-pillar colonnade, objectives sheltering under each.
    private static void Level06_DoubleGate(List<ModelBlockSpec> b)
    {
        Column(b, -1.6f, 3); Column(b, 0f, 3); Column(b, 1.6f, 3);
        Lintel(b, -0.8f, 3, 1.9f); Lintel(b, 0.8f, 3, 1.9f);
        Soldier(b, -0.8f, 0); Soldier(b, 0.8f, 0);
        Turret(b, -1.6f, 3); Turret(b, 1.6f, 3);
        At(b, 0f, 2.52f, 4, 0.72f, 0.72f);
        At(b, -0.8f, 2.52f, 5, 0.56f, 0.72f); At(b, 0.8f, 2.52f, 5, 0.56f, 0.72f);
    }

    // A 5x5 grid with bombs seeded on a checkerboard through its core.
    private static void Level07_BombCheckerboard(List<ModelBlockSpec> b)
    {
        float[] xs = { -1.44f, -0.72f, 0f, 0.72f, 1.44f };
        for (int c = 0; c < 5; c++)
            for (int r = 0; r < 5; r++)
            {
                bool bomb = ((r + c) % 2 == 0) && r > 0 && r < 4 && c > 0 && c < 4;
                if (bomb) Bomb(b, xs[c], r);
                else AddModel(b, xs[c], RowY(r), (r % 2 == 0) ? 1 : 0);
            }
        Soldier(b, 0f, 5);
    }

    // A wide heavy base carrying stacked rows of cannisters up to a lone objective.
    private static void Level08_BarrelPyramid(List<ModelBlockSpec> b)
    {
        for (int c = -4; c <= 4; c++) Box(b, c * 0.72f, 0, true);          // 9-wide base
        for (int c = -2; c <= 2; c++) AddModel(b, c * 0.72f, RowY(1), 5, 0.56f, 0.72f);
        for (int c = -1; c <= 1; c++) AddModel(b, c * 0.72f, RowY(2), 5, 0.56f, 0.72f);
        Soldier(b, 0f, 3);
    }

    // Three towers linked by two sky-bridges; the centre tower stands on a bomb.
    private static void Level09_TripleTowers(List<ModelBlockSpec> b)
    {
        ColumnB(b, 0f, 4, 0);
        Column(b, -1.8f, 4); Column(b, 1.8f, 4);
        Lintel(b, -0.9f, 4, 1.95f); Lintel(b, 0.9f, 4, 1.95f);
        Turret(b, -1.8f, 4); Turret(b, 1.8f, 4);
        At(b, 0f, 3.24f, 4, 0.72f, 0.72f);
        At(b, -0.9f, 3.24f, 5, 0.56f, 0.72f); At(b, 0.9f, 3.24f, 5, 0.56f, 0.72f);
    }

    // Two diamonds: solid blocks of crates rotated 45 degrees, each with a bomb at its core.
    private static void Level10_TwinDiamonds(List<ModelBlockSpec> b)
    {
        DiamondBlock(b, -1.5f, 1.3f, 3, 0.62f);
        DiamondBlock(b, 1.5f, 1.3f, 3, 0.62f);
    }

    // A crowned front wall with a bomb core, doubled by a full wall directly behind it.
    private static void Level11_LayeredWall(List<ModelBlockSpec> b)
    {
        float[] xs = { -1.44f, -0.72f, 0f, 0.72f, 1.44f };
        for (int i = 0; i < xs.Length; i++)
        {
            if (xs[i] == 0f) { Bomb(b, 0f, 0); Box(b, 0f, 1, false); Box(b, 0f, 2, false); }
            else Column(b, xs[i], 3);
            AddModel(b, xs[i], RowY(0), 1, 0.72f, 0.72f, 1);
            AddModel(b, xs[i], RowY(1), 0, 0.72f, 0.72f, 1);
        }
        CrownColumns(b, 2, 3);
        RearTurret(b, -1.44f, 2); RearTurret(b, 1.44f, 2);
    }

    // ---- levels 12-20 : larger set pieces built from the shared family library ----------------

    private static void Level12_TwinPeaks(List<ModelBlockSpec> b) => FamTwinPeaks(b, 3, 5);
    private static void Level13_Battlements(List<ModelBlockSpec> b) => FamWall(b, 4, 3, 2);
    private static void Level14_Colonnade(List<ModelBlockSpec> b) => FamColonnade(b, 3, 4);
    private static void Level15_StepPyramid(List<ModelBlockSpec> b) => FamPyramid(b, 4, 2);
    private static void Level16_TowerBridges(List<ModelBlockSpec> b) => FamTowers(b, 4, 4);
    private static void Level17_Ziggurat(List<ModelBlockSpec> b) => FamZiggurat(b, 4, 6);
    private static void Level18_HollowKeep(List<ModelBlockSpec> b) => FamFort(b, 3, 5);
    private static void Level19_Bastion(List<ModelBlockSpec> b) => FamBastion(b, 3, 6, 2);

    private static void Level20_GrandBastion(List<ModelBlockSpec> b)
    {
        FamBastion(b, 4, 6, 3);
        RearColumn(b, -1.44f, 4); RearColumn(b, 1.44f, 4);   // extra rear skyline towers
        RearTurret(b, -1.44f, 4); RearTurret(b, 1.44f, 4);
    }

    // ==========================================================================================
    // Shared symmetric structure families. Each is mirror-symmetric and, at the parameter ranges
    // used, always spawns at least 18 blocks. They back both the later authored levels and the
    // 21-100 generator.
    // ==========================================================================================

    private static void FamWall(List<ModelBlockSpec> b, int half, int rows, int bombCols)
    {
        half = Mathf.Clamp(half, 2, 4);
        rows = Mathf.Clamp(rows, 2, 5);
        int br = Mathf.Clamp(rows / 2, 1, rows - 1);
        for (int c = -half; c <= half; c++)
        {
            bool bomb = bombCols > 0 && (Mathf.Abs(c) == 1 || (bombCols >= 3 && c == 0));
            if (bomb) ColumnB(b, c * 0.72f, rows, br);
            else Column(b, c * 0.72f, rows);
        }
        CrownColumns(b, half, rows);
    }

    private static void FamPyramid(List<ModelBlockSpec> b, int baseHalf, int bombs)
    {
        baseHalf = Mathf.Clamp(baseHalf, 3, 4);
        for (int c = -baseHalf; c <= baseHalf; c++)
        {
            int h = baseHalf + 1 - Mathf.Abs(c);
            if (bombs > 0 && Mathf.Abs(c) <= bombs - 1) ColumnB(b, c * 0.72f, h, 0);
            else Column(b, c * 0.72f, h);
        }
        Soldier(b, 0f, baseHalf + 1);
    }

    private static void FamGate(List<ModelBlockSpec> b, int half, int height, int bombCols = 0)
    {
        half = Mathf.Clamp(half, 2, 4);
        height = Mathf.Clamp(height, 3, 6);
        int[] h = new int[half + 1];
        for (int c = 0; c <= half; c++) h[c] = Mathf.Clamp(Mathf.RoundToInt(1 + (height - 1) * (float)c / half), 1, height);
        for (int c = -half; c <= half; c++)
        {
            int d = Mathf.Abs(c);
            if (d == 0) continue;                 // keep the archway open
            if (bombCols > 0 && d <= bombCols) ColumnB(b, c * 0.72f, h[d], 0);   // bomb at the tower base
            else Column(b, c * 0.72f, h[d]);
        }
        float bridgeY = RowY(h[1]);
        AddModel(b, 0f, bridgeY, 3, 3f * 0.72f, 0.36f);   // lintel over the opening
        Soldier(b, 0f, 0);                                // objective under the arch
        Turret(b, -half * 0.72f, h[half]); Turret(b, half * 0.72f, h[half]);
        At(b, 0f, bridgeY + 0.36f, 4, 0.72f, 0.72f);      // objective on the bridge
    }

    private static void FamTwinPeaks(List<ModelBlockSpec> b, int half, int peak)
    {
        half = Mathf.Clamp(half, 2, 4);
        peak = Mathf.Clamp(peak, 4, 6);
        int m = Mathf.Max(1, half / 2 + 1);
        for (int c = -half; c <= half; c++)
        {
            int d = Mathf.Abs(c);
            int hh = Mathf.Clamp(peak - 2 * Mathf.Abs(d - m), 1, peak);
            if (d == 0) ColumnB(b, 0f, Mathf.Max(1, hh), 0);
            else Column(b, c * 0.72f, Mathf.Max(1, hh));
        }
        Turret(b, -m * 0.72f, peak); Turret(b, m * 0.72f, peak);
        Soldier(b, 0f, 1);
    }

    private static void FamFort(List<ModelBlockSpec> b, int half, int height)
    {
        half = Mathf.Clamp(half, 2, 4);
        height = Mathf.Clamp(height, 4, 5);
        for (int c = -half; c <= half; c++)
        {
            if (c == 0) Bomb(b, 0f, 0); else Box(b, c * 0.72f, 0, true);   // base course
            Box(b, c * 0.72f, height - 1, false);                          // top course
        }
        for (int r = 1; r < height - 1; r++)
        {
            Box(b, -half * 0.72f, r, true);
            Box(b, half * 0.72f, r, true);
        }
        CrownColumns(b, half, height);
    }

    private static void FamTowers(List<ModelBlockSpec> b, int count, int height)
    {
        count = Mathf.Clamp(count, 3, 5);
        height = Mathf.Clamp(height, 3, 5);
        const float span = 4.4f;
        for (int i = 0; i < count; i++)
        {
            float x = -span * 0.5f + span * i / (count - 1);
            bool bombBase = (i == 0 || i == count - 1 || (count % 2 == 1 && i == count / 2));
            if (bombBase) ColumnB(b, x, height, 0); else Column(b, x, height);
            if (count % 2 == 1 && i == count / 2) Soldier(b, x, height);
            else if (i == 0 || i == count - 1) Turret(b, x, height);
            else Cannister(b, x, height);
        }
    }

    private static void FamColonnade(List<ModelBlockSpec> b, int bays, int height)
    {
        bays = Mathf.Clamp(bays, 2, 4);
        height = Mathf.Clamp(height, 3, 4);
        int pillars = bays + 1;
        const float pitch = 0.95f;
        float x0 = -(pillars - 1) * pitch * 0.5f;
        for (int i = 0; i < pillars; i++)
        {
            float x = x0 + i * pitch;
            Column(b, x, height);
            Turret(b, x, height);
        }
        for (int i = 0; i < bays; i++)
        {
            float xc = x0 + (i + 0.5f) * pitch;
            Lintel(b, xc, height, pitch + 0.4f);
            Soldier(b, xc, 0);
            At(b, xc, RowY(height) + 0.36f, 5, 0.56f, 0.72f);
        }
    }

    private static void FamZiggurat(List<ModelBlockSpec> b, int half, int peak)
    {
        half = Mathf.Clamp(half, 3, 4);
        peak = Mathf.Clamp(peak, 4, 6);
        for (int c = -half; c <= half; c++)
        {
            int h = Mathf.Clamp(peak - (Mathf.Abs(c) + 1) / 2, 1, peak);
            if (Mathf.Abs(c) <= 1) ColumnB(b, c * 0.72f, h, 0);
            else Column(b, c * 0.72f, h);
        }
        Soldier(b, 0f, peak);
        Turret(b, -half * 0.72f, Mathf.Clamp(peak - (half + 1) / 2, 1, peak));
        Turret(b, half * 0.72f, Mathf.Clamp(peak - (half + 1) / 2, 1, peak));
    }

    private static void FamBastion(List<ModelBlockSpec> b, int half, int peak, int bombCols = 0)
    {
        FamGate(b, half, peak, bombCols);
        RearColumn(b, 0f, peak + 1); RearTurret(b, 0f, peak + 1);
        RearColumn(b, -half * 0.72f, Mathf.Max(2, peak - 1));
        RearColumn(b, half * 0.72f, Mathf.Max(2, peak - 1));
    }

    private static void FamDiamonds(List<ModelBlockSpec> b, int k)
    {
        k = Mathf.Clamp(k, 3, 4);
        DiamondBlock(b, -1.6f, 1.4f, k, 0.6f);
        DiamondBlock(b, 1.6f, 1.4f, k, 0.6f);
    }

    private static void FamChecker(List<ModelBlockSpec> b, int half, int rows)
    {
        half = Mathf.Clamp(half, 2, 4);
        rows = Mathf.Clamp(rows, 3, 5);
        for (int c = -half; c <= half; c++)
            for (int r = 0; r < rows; r++)
            {
                bool bomb = ((r + c + 100) % 2 == 0) && r > 0 && r < rows - 1 && Mathf.Abs(c) < half;
                if (bomb) Bomb(b, c * 0.72f, r);
                else AddModel(b, c * 0.72f, RowY(r), (r % 2 == 0) ? 1 : 0);
            }
        Soldier(b, 0f, rows);
    }

    private static void FamLayeredWall(List<ModelBlockSpec> b, int half, int rows)
    {
        half = Mathf.Clamp(half, 2, 4);
        rows = Mathf.Clamp(rows, 2, 4);
        int rearRows = Mathf.Max(2, rows);
        for (int c = -half; c <= half; c++)
        {
            if (c == 0) { Bomb(b, 0f, 0); for (int r = 1; r < rows; r++) AddModel(b, 0f, RowY(r), (r % 2 == 0) ? 1 : 0); }
            else Column(b, c * 0.72f, rows);
            for (int r = 0; r < rearRows; r++) AddModel(b, c * 0.72f, RowY(r), (r % 2 == 0) ? 1 : 0, 0.72f, 0.72f, 1);
        }
        CrownColumns(b, half, rows);
    }

    // ==========================================================================================
    // Levels 21-100 : cycle the families with size that grows with the level number.
    // ==========================================================================================

    private static void BuildGeneratedLayout(int zeroBasedLevel, List<ModelBlockSpec> b)
    {
        int fam = zeroBasedLevel % 12;
        int t = zeroBasedLevel / 12;              // grows every twelve levels
        int half = 3 + (t % 2);                   // 3 or 4 (keeps the footprint on-screen)
        int peak = 4 + (t % 3);                   // 4..6

        switch (fam)
        {
            case 0: FamWall(b, half, Mathf.Clamp(peak - 1, 3, 5), 1 + t % 3); break;
            case 1: FamPyramid(b, Mathf.Clamp(half + 1, 3, 4), 1 + t % 2); break;
            case 2: FamGate(b, half, peak, 1 + t % 2); break;
            case 3: FamTwinPeaks(b, half, peak); break;
            case 4: FamFort(b, half, Mathf.Clamp(peak - 1, 4, 5)); break;
            case 5: FamTowers(b, 3 + t % 3, Mathf.Clamp(peak - 1, 3, 5)); break;
            case 6: FamColonnade(b, 2 + t % 3, Mathf.Clamp(peak - 2, 3, 4)); break;
            case 7: FamBastion(b, half, peak, 1 + t % 2); break;
            case 8: FamZiggurat(b, Mathf.Clamp(half + 1, 3, 4), peak); break;
            case 9: FamDiamonds(b, 3 + t % 2); break;
            case 10: FamChecker(b, half, Mathf.Clamp(peak - 1, 3, 5)); break;
            default: FamLayeredWall(b, half, Mathf.Clamp(peak - 2, 2, 4)); break;
        }

        EnsureMinBlocks(b, 18);
    }

    // Safety net: symmetric filler so no generated level ever dips below the minimum block count.
    private static void EnsureMinBlocks(List<ModelBlockSpec> b, int min)
    {
        float x = 0.72f;
        int guard = 0;
        while (b.Count < min && guard++ < 24)
        {
            AddModel(b, -x, 0f, 5, 0.56f, 0.72f, 1);
            AddModel(b, x, 0f, 5, 0.56f, 0.72f, 1);
            x += 0.72f;
        }
    }

    // #endregion  --  AuthoredCampaign

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
