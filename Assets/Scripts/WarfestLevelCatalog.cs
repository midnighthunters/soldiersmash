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

    // The complete campaign uses deterministic, authored 3D construction rules.
    public const int AuthoredLevelCount = 50;

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

    private const int MaxProfileHeight = 8;

    // Number of columns on each side of the centre column (total width = 2*half + 1 columns).
    private static int HalfWidth(int zeroBasedLevel) => 2 + Mathf.Clamp(zeroBasedLevel / 13, 0, 2);

    // Tallest column, measured in rows. Grows steadily so later levels are visibly bigger.
    private static int PeakHeight(int zeroBasedLevel) => Mathf.Clamp(3 + zeroBasedLevel / 9, 3, 6);

    private static float ColX(float column) => column * ModelColPitch;
    private static float RowY(int row) => row * ModelRowStep;

    private static void AddModel(List<ModelBlockSpec> b, float x, float y, int variant,
        float width = 0.72f, float height = 0.72f, int layer = 0, int table = 0)
    {
        b.Add(new ModelBlockSpec(x, y, variant, width, height, layer, table));
    }

    public static void FillModelLayout(int zeroBasedLevel, List<ModelBlockSpec> blocks)
    {
        blocks.Clear();

        // Levels 1-20 are individually hand-authored below (see AuthoredCampaign region) so each
        // one is a visibly different structure. Levels 21+ fall back to the procedural generator.
        if (BuildAuthoredLayout(zeroBasedLevel, blocks)) return;

        int tier = Mathf.Clamp(zeroBasedLevel / 10, 0, 4);
        int motif = ((zeroBasedLevel % 10) + 10) % 10;
        int half = HalfWidth(zeroBasedLevel);
        int peak = PeakHeight(zeroBasedLevel);

        int[] profile = BuildProfile(motif, half, peak);

        BuildFrontStructure(blocks, motif, profile);
        if (tier >= 2) BuildRearSkyline(blocks, tier, profile);

        InjectBombs(zeroBasedLevel, blocks);
    }

    public static void FillModelTables(int zeroBasedLevel, List<ModelTableSpec> tables)
    {
        tables.Clear();

        // Authored levels size their table to the exact footprint of the structure they hold,
        // so asymmetric or unusually wide designs still rest fully on the surface.
        List<ModelBlockSpec> authored = new List<ModelBlockSpec>();
        if (BuildAuthoredLayout(zeroBasedLevel, authored))
        {
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            for (int i = 0; i < authored.Count; i++)
            {
                float halfWidth = authored[i].width * 0.5f;
                minX = Mathf.Min(minX, authored[i].x - halfWidth);
                maxX = Mathf.Max(maxX, authored[i].x + halfWidth);
            }
            if (authored.Count == 0) { minX = -1f; maxX = 1f; }
            float center = (minX + maxX) * 0.5f;
            float authoredWidth = (maxX - minX) + 1.2f;
            tables.Add(new ModelTableSpec(center, authoredWidth, -0.351f));
            return;
        }

        int tier = Mathf.Clamp(zeroBasedLevel / 10, 0, 4);
        int half = HalfWidth(zeroBasedLevel);

        // Wide enough that the whole mirrored structure - including its corner turrets - rests
        // fully on the surface with a little margin to spare.
        float width = (2 * half + 1) * ModelColPitch + 1.0f;
        // Lower the table as the campaign gets taller so tall stacks stay clear of the frame top.
        float topY = -0.351f - Mathf.Max(0, tier - 1) * 0.22f;
        tables.Add(new ModelTableSpec(0f, width, topY));
    }

    // ==========================================================================================
    // #region AuthoredCampaign  --  levels 1-20 (zero-based 0-19)
    //
    // Every level below is a distinct, deliberately designed structure rather than a variation
    // of one silhouette. The campaign teaches the toolkit a piece at a time, in the spirit of
    // physics knock-down games:
    //   1-3   read the shot: small wall, lone tower, twin pillars
    //   4-6   structure: gate + lintel, pyramid, heavy bunker
    //   7-9   bombs & chains: bomb core, barrel depot, two-storey gatehouse
    //   10-12 fortress ideas: the keep, staircase, split bomb-towers
    //   13-15 set pieces: crenellated wall, grand arch, twin peaks valley
    //   16-20 the big stuff: layered bastion, citadel, suspended deck, colossus, grand bastion
    //
    // Grid: columns sit on a 0.72 pitch; RowY(r) = r * 0.72 is the bottom of row r. Toppers rest
    // at RowY(columnHeight). Odd yOffsets (e.g. 1.80, 2.52) place a piece on top of a 0.36-tall
    // lintel. Variants: 0 box, 1 box2 (heavy), 2 box3 (turret), 3 long_box (lintel/cap),
    // 4 soldier (objective), 5 cannister (barrel), 6 bomb (chain-reaction target).
    // ==========================================================================================

    private static bool BuildAuthoredLayout(int zeroBasedLevel, List<ModelBlockSpec> b)
    {
        switch (zeroBasedLevel)
        {
            case 0: Level01_LoneOutpost(b); return true;
            case 1: Level02_Watchtower(b); return true;
            case 2: Level03_TwinPillars(b); return true;
            case 3: Level04_TheGate(b); return true;
            case 4: Level05_Pyramid(b); return true;
            case 5: Level06_HeavyBunker(b); return true;
            case 6: Level07_BombCore(b); return true;
            case 7: Level08_BarrelDepot(b); return true;
            case 8: Level09_DoubleBridge(b); return true;
            case 9: Level10_TheKeep(b); return true;
            case 10: Level11_Staircase(b); return true;
            case 11: Level12_SplitTowers(b); return true;
            case 12: Level13_FortressWall(b); return true;
            case 13: Level14_TheArch(b); return true;
            case 14: Level15_TwinPeaks(b); return true;
            case 15: Level16_LayeredBastion(b); return true;
            case 16: Level17_TheCitadel(b); return true;
            case 17: Level18_SuspendedDeck(b); return true;
            case 18: Level19_TheColossus(b); return true;
            case 19: Level20_WarfestBastion(b); return true;
            default: return false;
        }
    }

    // ---- authoring helpers ---------------------------------------------------------------------

    // A single crate on the modular grid. heavy = box2 (variant 1), otherwise a light box.
    private static void Box(List<ModelBlockSpec> b, float x, int row, bool heavy)
        => AddModel(b, x, RowY(row), heavy ? 1 : 0);

    // An alternating heavy/light column of `rows` crates (heavy on the ground for stability).
    private static void Column(List<ModelBlockSpec> b, float x, int rows)
    {
        for (int r = 0; r < rows; r++) AddModel(b, x, RowY(r), (r % 2 == 0) ? 1 : 0);
    }

    private static void Soldier(List<ModelBlockSpec> b, float x, int row)
        => AddModel(b, x, RowY(row), 4, 0.72f, 0.72f);

    private static void Cannister(List<ModelBlockSpec> b, float x, int row)
        => AddModel(b, x, RowY(row), 5, 0.56f, 0.72f);

    private static void Turret(List<ModelBlockSpec> b, float x, int row)
        => AddModel(b, x, RowY(row), 2, 0.36f, 0.72f);

    // A horizontal long_box lintel / capstone spanning `width` world units, resting at row `row`.
    private static void Lintel(List<ModelBlockSpec> b, float x, int row, float width)
        => AddModel(b, x, RowY(row), 3, width, 0.36f);

    private static void Bomb(List<ModelBlockSpec> b, float x, int row)
        => AddModel(b, x, RowY(row), 6, 0.56f, 0.72f);

    // Free-height placement (used to stand a piece on top of a 0.36-tall lintel deck).
    private static void At(List<ModelBlockSpec> b, float x, float y, int variant, float width, float height)
        => AddModel(b, x, y, variant, width, height);

    // A rear-layer (depthLayer 1) column that rises behind the front silhouette for a skyline.
    private static void RearColumn(List<ModelBlockSpec> b, float x, int rows)
    {
        for (int r = 0; r < rows; r++) AddModel(b, x, RowY(r), (r % 2 == 0) ? 1 : 0, 0.72f, 0.72f, 1);
    }

    private static void RearTurret(List<ModelBlockSpec> b, float x, int row)
        => AddModel(b, x, RowY(row), 2, 0.36f, 0.72f, 1);

    // ---- 1-3 : reading the shot ---------------------------------------------------------------

    // A three-wide starter wall with a single objective on top. One good arc clears it.
    private static void Level01_LoneOutpost(List<ModelBlockSpec> b)
    {
        Box(b, -0.72f, 0, true); Box(b, 0f, 0, true); Box(b, 0.72f, 0, true);
        Soldier(b, 0f, 1);
    }

    // A lone four-high watchtower capped with a barrel - teaches vertical aim and toppling.
    private static void Level02_Watchtower(List<ModelBlockSpec> b)
    {
        Column(b, 0f, 4);
        Cannister(b, 0f, 4);
    }

    // Two separated pillars with different crowns; you must place two distinct shots.
    private static void Level03_TwinPillars(List<ModelBlockSpec> b)
    {
        Column(b, -1.08f, 3); Soldier(b, -1.08f, 3);
        Column(b, 1.08f, 3); Cannister(b, 1.08f, 3);
    }

    // ---- 4-6 : structure ----------------------------------------------------------------------

    // A gateway: two towers carry a long lintel, an objective shelters under the arch, and a
    // second objective stands on the bridge. First appearance of the lintel piece.
    private static void Level04_TheGate(List<ModelBlockSpec> b)
    {
        Column(b, -1.08f, 2);
        Column(b, 1.08f, 2);
        Lintel(b, 0f, 2, 2.88f);            // spans both towers, bottom at y = 1.44
        Soldier(b, 0f, 0);                  // sheltered under the arch
        At(b, 0f, 1.80f, 4, 0.72f, 0.72f);  // stands on top of the bridge (lintel top = 1.80)
    }

    // The classic 5-3-1 pyramid crowned by an objective. Stable, so it rewards a low hard shot.
    private static void Level05_Pyramid(List<ModelBlockSpec> b)
    {
        Box(b, -1.44f, 0, true); Box(b, -0.72f, 0, true); Box(b, 0f, 0, true); Box(b, 0.72f, 0, true); Box(b, 1.44f, 0, true);
        Box(b, -0.72f, 1, false); Box(b, 0f, 1, true); Box(b, 0.72f, 1, false);
        Box(b, 0f, 2, true);
        Soldier(b, 0f, 3);
    }

    // A wide, low bunker of heavy crates finished with one topper per column (turret, barrel,
    // objective, barrel, turret). Heavy and stable - teaches that mass resists a glancing hit.
    private static void Level06_HeavyBunker(List<ModelBlockSpec> b)
    {
        float[] xs = { -1.44f, -0.72f, 0f, 0.72f, 1.44f };
        for (int i = 0; i < xs.Length; i++) { Box(b, xs[i], 0, true); Box(b, xs[i], 1, false); }
        Turret(b, -1.44f, 2); Cannister(b, -0.72f, 2); Soldier(b, 0f, 2); Cannister(b, 0.72f, 2); Turret(b, 1.44f, 2);
    }

    // ---- 7-9 : bombs & chains -----------------------------------------------------------------

    // A 3x3 block with a bomb at its heart. One shot into the core chains to its neighbours.
    private static void Level07_BombCore(List<ModelBlockSpec> b)
    {
        Box(b, -0.72f, 0, true); Box(b, 0f, 0, true); Box(b, 0.72f, 0, true);
        Box(b, -0.72f, 1, false); Bomb(b, 0f, 1); Box(b, 0.72f, 1, false);
        Box(b, -0.72f, 2, false); Box(b, 0f, 2, true); Box(b, 0.72f, 2, false);
        Soldier(b, 0f, 3);
    }

    // Barrel-topped columns flank a central stack whose base is a bomb - detonating it drops the
    // middle and rocks the barrels off their perches.
    private static void Level08_BarrelDepot(List<ModelBlockSpec> b)
    {
        Column(b, -1.44f, 3); Cannister(b, -1.44f, 3);
        Column(b, 1.44f, 3); Cannister(b, 1.44f, 3);
        Bomb(b, 0f, 0); Box(b, 0f, 1, false); Box(b, 0f, 2, true); Cannister(b, 0f, 3);
    }

    // A two-storey gatehouse: two decks bridged by lintels, objectives on each floor.
    private static void Level09_DoubleBridge(List<ModelBlockSpec> b)
    {
        Column(b, -1.08f, 4);
        Column(b, 1.08f, 4);
        Lintel(b, 0f, 2, 2.88f);            // lower deck, bottom y = 1.44
        Lintel(b, 0f, 4, 2.88f);            // upper deck, bottom y = 2.88
        At(b, 0f, 1.80f, 4, 0.72f, 0.72f);  // objective on the lower deck
        At(b, -0.72f, 3.24f, 5, 0.56f, 0.72f); // barrel on the upper deck
        At(b, 0f, 3.24f, 4, 0.72f, 0.72f);     // objective on the upper deck
        At(b, 0.72f, 3.24f, 5, 0.56f, 0.72f);  // barrel on the upper deck
    }

    // ---- 10-12 : fortress ideas ---------------------------------------------------------------

    // A keep: two corner towers with turrets, a two-high curtain wall between them, an objective
    // on the wall and a bomb buried in the wall base.
    private static void Level10_TheKeep(List<ModelBlockSpec> b)
    {
        Column(b, -1.44f, 3); Turret(b, -1.44f, 3);
        Column(b, 1.44f, 3); Turret(b, 1.44f, 3);
        Box(b, -0.72f, 0, true); Bomb(b, 0f, 0); Box(b, 0.72f, 0, true);
        Box(b, -0.72f, 1, false); Box(b, 0f, 1, false); Box(b, 0.72f, 1, false);
        Soldier(b, 0f, 2);
    }

    // An ascending staircase 1-2-3-4-5 with an objective on the low step and a turret at the top.
    private static void Level11_Staircase(List<ModelBlockSpec> b)
    {
        Column(b, -1.44f, 1);
        Column(b, -0.72f, 2);
        Column(b, 0f, 3);
        Column(b, 0.72f, 4);
        Column(b, 1.44f, 5);
        Soldier(b, -1.44f, 1);
        Turret(b, 1.44f, 5);
    }

    // Two tall towers, far apart, each standing on a bomb; a small turret-topped guard sits in
    // the gap. Detonating a base bomb fells an entire tower.
    private static void Level12_SplitTowers(List<ModelBlockSpec> b)
    {
        Bomb(b, -1.44f, 0); Box(b, -1.44f, 1, false); Box(b, -1.44f, 2, true); Box(b, -1.44f, 3, false); Box(b, -1.44f, 4, true);
        Soldier(b, -1.44f, 5);
        Bomb(b, 1.44f, 0); Box(b, 1.44f, 1, false); Box(b, 1.44f, 2, true); Box(b, 1.44f, 3, false); Box(b, 1.44f, 4, true);
        Cannister(b, 1.44f, 5);
        Box(b, 0f, 0, true); Box(b, 0f, 1, false); Turret(b, 0f, 2);
    }

    // ---- 13-15 : set pieces -------------------------------------------------------------------

    // A long crenellated fortress wall: seven-wide heavy base, an upper course with two bombs
    // built into it, and a crown of turret / barrel / objective merlons.
    private static void Level13_FortressWall(List<ModelBlockSpec> b)
    {
        float[] xs = { -2.16f, -1.44f, -0.72f, 0f, 0.72f, 1.44f, 2.16f };
        for (int i = 0; i < xs.Length; i++) Box(b, xs[i], 0, true);
        Box(b, -2.16f, 1, false); Bomb(b, -1.44f, 1); Box(b, -0.72f, 1, false); Box(b, 0f, 1, false);
        Box(b, 0.72f, 1, false); Bomb(b, 1.44f, 1); Box(b, 2.16f, 1, false);
        Turret(b, -2.16f, 2); Cannister(b, -0.72f, 2); Soldier(b, 0f, 2); Cannister(b, 0.72f, 2); Turret(b, 2.16f, 2);
    }

    // A grand arch: tall outer pillars, shorter inner shoulders, a lintel across the opening, a
    // bomb keystone on the crown, barrels on the parapets and an objective beneath.
    private static void Level14_TheArch(List<ModelBlockSpec> b)
    {
        Column(b, -1.44f, 4);
        Column(b, -0.72f, 3);
        Column(b, 0.72f, 3);
        Column(b, 1.44f, 4);
        Lintel(b, 0f, 3, 2.16f);            // spans the inner shoulders, top y = 2.52
        Soldier(b, 0f, 0);                  // objective under the arch
        At(b, 0f, 2.52f, 6, 0.56f, 0.72f);  // bomb keystone on the crown
        Cannister(b, -1.44f, 4); Cannister(b, 1.44f, 4);
    }

    // An "M" of twin peaks with a bomb-laced valley between them and turret-crowned summits.
    private static void Level15_TwinPeaks(List<ModelBlockSpec> b)
    {
        Box(b, -2.16f, 0, true); Box(b, -2.16f, 1, false);
        Column(b, -1.44f, 4); Turret(b, -1.44f, 4);
        Bomb(b, -0.72f, 0); Box(b, -0.72f, 1, false); Box(b, -0.72f, 2, true);
        Bomb(b, 0f, 0); Soldier(b, 0f, 1);
        Bomb(b, 0.72f, 0); Box(b, 0.72f, 1, false); Box(b, 0.72f, 2, true);
        Column(b, 1.44f, 4); Turret(b, 1.44f, 4);
        Box(b, 2.16f, 0, true); Box(b, 2.16f, 1, false);
    }

    // ---- 16-20 : the big stuff ----------------------------------------------------------------

    // A layered bastion: a crowned front wall with a bomb at its centre, backed by two taller
    // rear towers that rise into view as a skyline (first use of the rear depth layer).
    private static void Level16_LayeredBastion(List<ModelBlockSpec> b)
    {
        float[] xs = { -1.44f, -0.72f, 0f, 0.72f, 1.44f };
        for (int i = 0; i < xs.Length; i++) Box(b, xs[i], 1, false);
        Box(b, -1.44f, 0, true); Box(b, -0.72f, 0, true); Bomb(b, 0f, 0); Box(b, 0.72f, 0, true); Box(b, 1.44f, 0, true);
        Turret(b, -1.44f, 2); Cannister(b, -0.72f, 2); Soldier(b, 0f, 2); Cannister(b, 0.72f, 2); Turret(b, 1.44f, 2);
        RearColumn(b, -0.72f, 4); RearTurret(b, -0.72f, 4);
        RearColumn(b, 0.72f, 4); RearTurret(b, 0.72f, 4);
    }

    // A stepped citadel: a five-tower keep that climbs to a central spire, with three bombs
    // seated in the tower bases and objectives / turrets / barrels crowning the profile.
    private static void Level17_TheCitadel(List<ModelBlockSpec> b)
    {
        Box(b, -2.16f, 0, true); Box(b, -2.16f, 1, false); Box(b, -2.16f, 2, true); Turret(b, -2.16f, 3);
        Box(b, -1.44f, 0, true); Box(b, -1.44f, 1, false); Cannister(b, -1.44f, 2);
        Bomb(b, -0.72f, 0); Box(b, -0.72f, 1, false); Box(b, -0.72f, 2, true); Box(b, -0.72f, 3, false);
        Bomb(b, 0f, 0); Box(b, 0f, 1, false); Box(b, 0f, 2, true); Box(b, 0f, 3, false); Box(b, 0f, 4, true); Soldier(b, 0f, 5);
        Bomb(b, 0.72f, 0); Box(b, 0.72f, 1, false); Box(b, 0.72f, 2, true); Box(b, 0.72f, 3, false);
        Box(b, 1.44f, 0, true); Box(b, 1.44f, 1, false); Cannister(b, 1.44f, 2);
        Box(b, 2.16f, 0, true); Box(b, 2.16f, 1, false); Box(b, 2.16f, 2, true); Turret(b, 2.16f, 3);
    }

    // A suspended deck: two towers plus a fragile bomb-cored central pillar carry a wide platform
    // loaded with objectives and barrels. Knock out a support and the whole deck comes down.
    private static void Level18_SuspendedDeck(List<ModelBlockSpec> b)
    {
        Column(b, -1.44f, 4);
        Column(b, 1.44f, 4);
        Box(b, 0f, 0, true); Bomb(b, 0f, 1); Box(b, 0f, 2, false); Box(b, 0f, 3, true); // central support
        Lintel(b, 0f, 4, 3.6f);             // the platform, top y = 3.24
        At(b, -1.62f, 3.24f, 2, 0.36f, 0.72f);  // edge turret
        At(b, -1.08f, 3.24f, 5, 0.56f, 0.72f);  // barrel
        At(b, -0.36f, 3.24f, 4, 0.72f, 0.72f);  // objective
        At(b, 0.36f, 3.24f, 4, 0.72f, 0.72f);   // objective
        At(b, 1.08f, 3.24f, 5, 0.56f, 0.72f);   // barrel
        At(b, 1.62f, 3.24f, 2, 0.36f, 0.72f);   // edge turret
    }

    // The colossus: a broad seven-wide wall with three bombs, backed by a single towering keep.
    private static void Level19_TheColossus(List<ModelBlockSpec> b)
    {
        float[] xs = { -2.16f, -1.44f, -0.72f, 0f, 0.72f, 1.44f, 2.16f };
        for (int i = 0; i < xs.Length; i++) Box(b, xs[i], 0, true);
        Box(b, -2.16f, 1, false); Bomb(b, -1.44f, 1); Box(b, -0.72f, 1, false); Bomb(b, 0f, 1);
        Box(b, 0.72f, 1, false); Bomb(b, 1.44f, 1); Box(b, 2.16f, 1, false);
        Turret(b, -2.16f, 2); Cannister(b, -1.44f, 2); Soldier(b, -0.72f, 2); Soldier(b, 0f, 2);
        Soldier(b, 0.72f, 2); Cannister(b, 1.44f, 2); Turret(b, 2.16f, 2);
        RearColumn(b, 0f, 6); RearTurret(b, 0f, 6);   // the central keep towers over the wall
    }

    // The grand bastion finale: a central gate under a lintel, two corner keeps standing on bombs,
    // a bomb under the arch and a keystone bomb, all backed by a three-tower rear skyline.
    private static void Level20_WarfestBastion(List<ModelBlockSpec> b)
    {
        // corner keeps on bomb bases
        Bomb(b, -1.44f, 0); Box(b, -1.44f, 1, false); Box(b, -1.44f, 2, true); Box(b, -1.44f, 3, false); Turret(b, -1.44f, 4);
        Bomb(b, 1.44f, 0); Box(b, 1.44f, 1, false); Box(b, 1.44f, 2, true); Box(b, 1.44f, 3, false); Turret(b, 1.44f, 4);
        // gate pillars
        Column(b, -0.72f, 3);
        Column(b, 0.72f, 3);
        Lintel(b, 0f, 3, 2.16f);            // gate bridge, top y = 2.52
        // the defended objective under the arch, with a bomb stacked above it
        Soldier(b, 0f, 0); Bomb(b, 0f, 1);
        At(b, 0f, 2.52f, 6, 0.56f, 0.72f);  // keystone bomb
        At(b, -0.72f, 2.52f, 5, 0.56f, 0.72f); // barrels on the bridge
        At(b, 0.72f, 2.52f, 5, 0.56f, 0.72f);
        // rear skyline
        RearColumn(b, -1.44f, 4);
        RearColumn(b, 0f, 5);
        RearColumn(b, 1.44f, 4);
    }

    // #endregion  --  AuthoredCampaign

    // Right-half column heights (index 0 = centre column, index half = outermost column).
    // The ten motifs cycle with (level % 10); scale comes from half and peak, which grow with level.
    private static int[] BuildProfile(int motif, int half, int peak)
    {
        int[] h = new int[half + 1];
        int mid = half / 2;
        int peakCol = Mathf.CeilToInt(half / 2f);

        for (int c = 0; c <= half; c++)
        {
            int v;
            switch (motif)
            {
                case 0: v = peak; break;                                        // solid wall
                case 1: v = peak - c; break;                                    // pyramid
                case 2: v = peak - (half - c); break;                           // gate (tall edges, low centre)
                case 3: v = peak - (c % 2); break;                              // crenellated wall
                case 4: v = peak - c / 2; break;                                // stepped ziggurat
                case 5: v = c == 0 ? peak - 3 : peak; break;                    // arched gateway
                case 6: v = peak - Mathf.Abs(c - peakCol); break;               // twin peaks (M)
                case 7: v = peak - (mid - Mathf.Abs(c - mid)); break;           // split valley (W)
                case 8: v = (c == 0 || c == half) ? peak : peak - 1; break;     // keep + corner towers
                default: v = Mathf.Max(peak - c, c == half ? peak - 1 : 1); break; // citadel
            }
            h[c] = Mathf.Clamp(v, 1, MaxProfileHeight);
        }
        return h;
    }

    private static void BuildFrontStructure(List<ModelBlockSpec> b, int motif, int[] profile)
    {
        int half = profile.Length - 1;
        for (int c = 0; c <= half; c++)
        {
            PlaceColumn(b, c, 1, profile[c]);
            if (c > 0) PlaceColumn(b, c, -1, profile[c]);
        }

        AddToppers(b, profile);
        AddLintels(b, motif, profile);
    }

    // One mirror-symmetric column of alternating light / heavy bricks.
    private static void PlaceColumn(List<ModelBlockSpec> b, int c, int sign, int height)
    {
        float x = sign * ColX(c);
        for (int r = 0; r < height; r++)
        {
            int variant = ((r + c) % 2 == 0) ? 0 : 1;
            AddModel(b, x, RowY(r), variant, 0.72f, 0.72f);
        }
    }

    // Each column is finished with exactly one topper, so every asset earns a natural role:
    // a soldier crowns the centre, slim box3 turrets cap the corners, cannister barrels and
    // long_box capstones alternate across the rest.
    private static void AddToppers(List<ModelBlockSpec> b, int[] profile)
    {
        int half = profile.Length - 1;
        for (int c = 0; c <= half; c++)
        {
            float topY = RowY(profile[c]);
            int variant;
            float width;
            if (c == 0) { variant = 4; width = 0.72f; }          // soldier objective on the centre
            else if (c == half) { variant = 2; width = 0.36f; }  // slim box3 turret on each corner
            else if (c % 2 == 1) { variant = 5; width = 0.56f; } // cannister barrel
            else { variant = 3; width = 0.72f; }                 // long_box capstone

            // Sandbags use the full grid-cell height while preserving their narrow corner
            // footprint and the existing support placement.
            float height = variant == 3 ? 0.36f : 0.72f;
            AddModel(b, ColX(c), topY, variant, width, height);
            if (c > 0) AddModel(b, -ColX(c), topY, variant, width, height);
        }
    }

    // Gate and arch motifs get a long_box lintel bridging the central opening.
private static void AddLintels(List<ModelBlockSpec> b, int motif, int[] profile)
    {
        int half = profile.Length - 1;
        if (motif == 5 && half >= 1)
        {
            float y = RowY(profile[1]);                          // rest on the columns flanking the door
            AddModel(b, 0f, y, 3, ColX(2) + 0.72f, 0.36f);
        }
        else if (motif == 2 && half >= 2)
        {
            // The central bridge must span the two columns it rests on. As the gate grows wider,
            // use the real column separation rather than a fixed short capstone, avoiding a
            // floating lintel in the wider campaign layouts.
            float y = RowY(profile[half - 1]);
            float bridgeWidth = 2f * ColX(half - 1) + ModelColPitch;
            AddModel(b, 0f, y, 3, bridgeWidth, 0.36f);
        }
    }

    // Taller towers set directly behind the front structure so they rise into view above the
    // front silhouette, giving advanced levels a layered, three-dimensional skyline.
    private static void BuildRearSkyline(List<ModelBlockSpec> b, int tier, int[] profile)
    {
        int half = profile.Length - 1;
        List<int> columns = new List<int> { half };
        if (tier >= 3) columns.Add(0);
        if (tier >= 4) columns.Add(Mathf.Max(1, half - 2));

        foreach (int c in columns)
        {
            int rearHeight = Mathf.Min(MaxProfileHeight, profile[c] + 2);
            if (rearHeight <= profile[c]) rearHeight = Mathf.Min(MaxProfileHeight, profile[c] + 1);
            PlaceRearColumn(b, c, 1, rearHeight);
            if (c > 0) PlaceRearColumn(b, c, -1, rearHeight);
        }
    }

    private static void PlaceRearColumn(List<ModelBlockSpec> b, int c, int sign, int height)
    {
        float x = sign * ColX(c);
        for (int r = 0; r < height; r++)
        {
            int variant = ((r + c) % 2 == 0) ? 1 : 0;
            AddModel(b, x, RowY(r), variant, 0.72f, 0.72f, 1);
        }
    }

    private static void InjectBombs(int zeroBasedLevel, List<ModelBlockSpec> b)
    {
        if (b.Count == 0) return;
        int bombCount = Mathf.Clamp(1 + zeroBasedLevel / 13, 1, 4);
        float maxY = 0f;
        for (int i = 0; i < b.Count; i++) maxY = Mathf.Max(maxY, b[i].yOffset);

        float[] targetX;
        float[] targetHeight;
        switch (bombCount)
        {
            case 1:
                targetX = new[] { 0f }; targetHeight = new[] { 0.42f }; break;
            case 2:
                targetX = new[] { -0.78f, 0.78f }; targetHeight = new[] { 0.38f, 0.38f }; break;
            case 3:
                targetX = new[] { -1.08f, 0f, 1.08f }; targetHeight = new[] { 0.32f, 0.58f, 0.32f }; break;
            default:
                targetX = new[] { -1.12f, -0.38f, 0.38f, 1.12f }; targetHeight = new[] { 0.30f, 0.60f, 0.60f, 0.30f }; break;
        }

        HashSet<int> used = new HashSet<int>();
        for (int bomb = 0; bomb < bombCount; bomb++)
        {
            int bestIndex = -1;
            float bestScore = float.MaxValue;
            float desiredY = maxY * targetHeight[bomb];
            for (int i = 0; i < b.Count; i++)
            {
                if (used.Contains(i)) continue;
                ModelBlockSpec candidate = b[i];
                if (candidate.variant == 3 || candidate.variant == 4 || candidate.variant == 6 || candidate.width < 0.5f) continue;
                float score = Mathf.Abs(candidate.x - targetX[bomb]) + Mathf.Abs(candidate.yOffset - desiredY) * 0.75f;
                if (candidate.depthLayer > 0) score += 0.35f;
                if (score >= bestScore) continue;
                bestScore = score;
                bestIndex = i;
            }

            if (bestIndex < 0) continue;
            ModelBlockSpec selected = b[bestIndex];
            selected.variant = 6;
            selected.width = 0.56f;
            selected.height = 0.72f;
            b[bestIndex] = selected;
            used.Add(bestIndex);
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
