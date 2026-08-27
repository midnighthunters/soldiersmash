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

        int tier = Mathf.Clamp(zeroBasedLevel / 10, 0, 4);
        int half = HalfWidth(zeroBasedLevel);

        // Wide enough that the whole mirrored structure - including its corner turrets - rests
        // fully on the surface with a little margin to spare.
        float width = (2 * half + 1) * ModelColPitch + 1.0f;
        // Lower the table as the campaign gets taller so tall stacks stay clear of the frame top.
        float topY = -0.351f - Mathf.Max(0, tier - 1) * 0.22f;
        tables.Add(new ModelTableSpec(0f, width, topY));
    }

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
