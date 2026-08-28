using System.Collections.Generic;
using UnityEngine;

// =================================================================================================
// SOLDIER SMASH -- authored 100-level campaign.
//
// This partial extends WarfestLevelCatalog with the campaign defined by the master design brief:
// 100 hand-specified military set pieces built from the full gameplay palette, with exact per-level
// composition budgets and a 1 -> 2 -> 3 -> 4 -> 5 table progression.
//
//   Levels  1-20 : 1 table      21-42 : 2 tables      43-70 : 3 tables
//   Levels 71-99 : 4 tables     100   : 5 tables
//
// Each level's exact block budget is stored in CampaignComposition. A deterministic builder turns
// that inventory into a stable, readable fort silhouette on each table, consuming EVERY block so the
// placed count always equals the budget. The game freezes all blocks (kinematic) until the first
// target breaks, so structures never collapse before the player's first shot.
// =================================================================================================
public static partial class WarfestLevelCatalog
{
    // ---- Canonical gameplay variants (index order MUST match WarfestGameController prefab load) --
    public const int BOX = 0;        // light structural brick
    public const int BOX2 = 1;       // heavy structural brick (stable base)
    public const int BOX3 = 2;       // slim corner turret / wedge
    public const int LONG_BOX = 3;   // chunky beam / lintel
    public const int LONG_BOX2 = 4;  // flat plank / roof cap
    public const int SOLDIER = 5;    // objective topper
    public const int CANNISTER = 6;  // barrel prop (NOT explosive)
    public const int BOMB = 7;       // explosive chain target
    public const int KING = 8;       // royal milestone crown piece

    public static bool IsBombVariant(int variant) => variant == BOMB;

    // Layout tuning ------------------------------------------------------------------------------
    private const float TierGap = 0.55f;      // vertical clearance between stacked table tiers
    private const float TableMargin = 0.55f;  // pedestal breathing room around a structure footprint

    // [box, box2, box3, long_box, long_box2, soldier, cannister, bomb, king] for levels 1..100.
    // Transcribed verbatim from the master brief; every row's sum is the level's block budget.
    private static readonly int[][] CampaignComposition =
    {
        new[]{10,4,0,3,0,3,0,0,0}, // 01 Training Gatehouse
        new[]{13,3,0,2,0,3,0,0,0}, // 02 Twin Sentries
        new[]{14,3,0,2,0,3,0,0,0}, // 03 Supply Arch
        new[]{11,3,2,3,0,4,0,0,0}, // 04 Pillar Drill
        new[]{11,4,2,2,0,3,2,0,0}, // 05 Barricade Stack
        new[]{13,3,2,2,0,3,2,0,0}, // 06 Double Arch
        new[]{11,3,3,3,1,3,2,0,0}, // 07 Beam Nest
        new[]{12,3,2,2,1,4,3,0,0}, // 08 Barracks Stack
        new[]{12,4,2,2,1,3,2,2,0}, // 09 Fuse Lesson
        new[]{12,3,2,3,2,3,2,1,1}, // 10 Royal Outpost
        new[]{12,3,2,2,1,3,2,1,0}, // 11 Watchtower Row
        new[]{9,3,3,2,1,4,3,2,0},  // 12 Hollow Bunker
        new[]{12,4,2,3,1,3,2,1,0}, // 13 H-Bridge
        new[]{15,3,2,2,1,3,2,1,0}, // 14 Offset Keep
        new[]{14,3,2,2,2,3,2,2,0}, // 15 Ammunition Alley
        new[]{14,3,2,3,1,4,3,1,0}, // 16 Guarded Vault
        new[]{16,4,3,2,1,3,2,1,0}, // 17 Zigzag Wall
        new[]{11,3,2,2,1,3,2,2,0}, // 18 Layered Balcony
        new[]{12,3,2,3,1,3,2,1,0}, // 19 Bomb Pocket
        new[]{10,3,2,2,2,4,3,1,1}, // 20 Royal Fortress
        new[]{9,5,2,3,2,4,2,3,0},  // 21 Split Checkpoint
        new[]{10,4,3,4,2,4,2,2,0}, // 22 Twin Depots
        new[]{13,4,2,3,2,4,2,2,0}, // 23 Two Bridges
        new[]{11,4,2,3,2,5,3,3,0}, // 24 Crossfire Posts
        new[]{12,5,2,4,3,4,2,2,0}, // 25 Canyon Gate
        new[]{16,4,2,3,2,4,2,2,0}, // 26 Split Bunkers
        new[]{15,4,3,3,2,4,2,3,0}, // 27 Relay Towers
        new[]{8,4,2,4,2,5,3,2,0},  // 28 Offset Barracks
        new[]{11,5,2,3,2,4,2,2,0}, // 29 Chain-Reaction Twins
        new[]{10,4,2,3,3,4,2,3,1}, // 30 Royal Twin Keep
        new[]{10,4,3,4,2,4,3,2,0}, // 31 High-Low Towers
        new[]{9,4,4,3,2,5,4,2,0},  // 32 Center Gap
        new[]{11,5,3,3,2,4,3,3,0}, // 33 X-Brace Fort
        new[]{13,4,3,4,2,4,3,2,0}, // 34 Double Staircase
        new[]{14,4,3,3,3,4,3,2,0}, // 35 Balanced Barracks
        new[]{13,4,3,3,2,5,4,3,0}, // 36 Canister Vaults
        new[]{14,5,4,4,2,4,3,2,0}, // 37 Bomb Wells
        new[]{18,4,3,3,2,4,3,2,0}, // 38 Guard Galleries
        new[]{10,4,3,3,2,4,3,3,0}, // 39 Funnel Forts
        new[]{7,4,3,4,3,5,4,2,1},  // 40 Royal Bridge
        new[]{8,6,3,4,3,5,3,3,0},  // 41 Fortress Wings
        new[]{8,5,4,4,3,5,3,4,0},  // 42 Double Citadel
        new[]{10,5,3,5,3,5,3,3,0}, // 43 Three-Post Trial
        new[]{10,5,3,4,3,6,4,3,0}, // 44 Staggered Supply Line
        new[]{10,6,3,4,4,5,3,4,0}, // 45 Tri-Tower Camp
        new[]{13,5,3,5,3,5,3,3,0}, // 46 Bomb Triangle
        new[]{14,5,4,4,3,5,3,3,0}, // 47 Long-Beam Citadel
        new[]{13,5,3,4,3,6,4,4,0}, // 48 Soldier Gallery
        new[]{7,6,3,5,3,5,3,3,0},  // 49 Triple Gate
        new[]{8,5,3,4,4,5,3,3,1},  // 50 Royal Command Camp
        new[]{11,5,3,4,3,5,3,4,0}, // 51 Frontline Arc
        new[]{9,5,4,5,3,6,4,3,0},  // 52 Three Bunkers
        new[]{13,6,3,4,3,5,3,3,0}, // 53 Central Bomb Spine
        new[]{14,5,3,4,3,5,3,4,0}, // 54 Bridge and Wings
        new[]{14,5,3,5,4,5,3,3,0}, // 55 Canister Columns
        new[]{14,5,3,4,3,6,4,3,1}, // 56 Stepped Defense
        new[]{15,6,4,4,3,5,3,4,0}, // 57 Ring of Guards
        new[]{18,5,3,5,3,5,3,3,0}, // 58 Tower Canyon
        new[]{12,5,3,4,3,5,3,3,0}, // 59 Cascade Wall
        new[]{8,5,3,4,4,6,4,4,1},  // 60 Royal Triangle
        new[]{6,6,4,6,4,6,4,4,0},  // 61 Heavy Fort Line
        new[]{7,6,5,5,4,6,4,4,0},  // 62 Bomb Funnel
        new[]{8,6,4,5,4,6,4,5,0},  // 63 Split Roof Camp
        new[]{7,6,4,6,4,7,5,4,0},  // 64 Double Bomb Chambers
        new[]{9,7,4,5,5,6,4,4,0},  // 65 Layer-Cake Fort
        new[]{11,6,4,5,4,6,4,5,0}, // 66 Soldier Balcony
        new[]{11,6,5,6,4,6,4,4,0}, // 67 Alternating Pillars
        new[]{12,6,4,5,4,7,5,4,0}, // 68 Canister Gauntlet
        new[]{13,7,4,5,4,6,4,5,0}, // 69 Chain-Reaction Castle
        new[]{6,5,3,6,5,6,4,4,1},  // 70 Royal Siege Line
        new[]{9,6,4,5,4,6,4,4,0},  // 71 Four Corners
        new[]{6,6,5,5,4,7,5,5,0},  // 72 Diamond Camp
        new[]{8,7,4,6,4,6,4,4,1},  // 73 Zigzag Batteries
        new[]{12,6,4,5,4,6,4,4,0}, // 74 Four Watchtowers
        new[]{11,6,4,5,5,6,4,5,0}, // 75 Cross Formation
        new[]{11,6,4,6,4,7,5,4,0}, // 76 Twin Front Twin Rear
        new[]{13,7,5,5,4,6,4,4,0}, // 77 Outer Ring
        new[]{15,6,4,5,4,6,4,5,0}, // 78 Bomb Chessboard
        new[]{16,6,4,6,4,6,4,4,0}, // 79 Tall-Short Rhythm
        new[]{6,5,4,5,5,7,5,4,1},  // 80 Royal Square
        new[]{6,7,4,6,5,7,4,5,0},  // 81 Fortress Quadrants
        new[]{6,7,5,6,5,7,4,5,0},  // 82 Four-Bridge Run
        new[]{8,7,4,6,5,7,4,5,0},  // 83 Command Diamond
        new[]{6,7,4,6,5,8,5,5,1},  // 84 Bomb Moat
        new[]{10,7,4,6,5,7,4,5,0}, // 85 Canister Compass
        new[]{11,7,4,6,5,7,4,5,0}, // 86 High-Low Diamond
        new[]{11,7,5,6,5,7,4,5,0}, // 87 Four Bunkers
        new[]{11,7,4,6,5,8,5,5,0}, // 88 Spiral Rise
        new[]{14,7,4,6,5,7,4,5,0}, // 89 Chain-Reaction Quadrants
        new[]{14,7,4,6,5,7,4,5,1}, // 90 Royal Citadel
        new[]{7,7,5,6,5,7,5,5,1},  // 91 Grand Parade
        new[]{6,7,6,6,5,8,6,5,0},  // 92 Twin Front Bastions
        new[]{10,7,5,6,5,7,5,5,0}, // 93 Command Staircase
        new[]{10,7,5,6,5,7,5,5,1}, // 94 Crown Labyrinth
        new[]{12,7,5,6,5,7,5,5,0}, // 95 Explosive Stairway
        new[]{11,7,5,6,5,8,6,5,0}, // 96 Command Grid
        new[]{12,7,6,6,5,7,5,5,1}, // 97 Royal Gauntlet
        new[]{14,7,5,6,5,7,5,5,1}, // 98 Last Barricade
        new[]{15,7,5,6,5,7,5,5,1}, // 99 Siege Cathedral
        new[]{14,7,5,6,5,8,6,5,1}, // 100 Grand King's Fortress
    };

    public static int CampaignTableCount(int zeroBasedLevel)
    {
        int lv = Mathf.Clamp(zeroBasedLevel, 0, 99) + 1;
        if (lv <= 20) return 1;
        if (lv <= 42) return 2;
        if (lv <= 70) return 3;
        if (lv <= 99) return 4;
        return 5;
    }

    // Authored military-toy names for each level (from the master brief's design column). Used by
    // the HUD so every level shows its intended identity.
    private static readonly string[] CampaignNames =
    {
        "Training Gatehouse", "Twin Sentries", "Supply Arch", "Pillar Drill", "Barricade Stack",
        "Double Arch", "Beam Nest", "Barracks Stack", "Fuse Lesson", "Royal Outpost",
        "Watchtower Row", "Hollow Bunker", "H-Bridge", "Offset Keep", "Ammunition Alley",
        "Guarded Vault", "Zigzag Wall", "Layered Balcony", "Bomb Pocket", "Royal Fortress",
        "Split Checkpoint", "Twin Depots", "Two Bridges", "Crossfire Posts", "Canyon Gate",
        "Split Bunkers", "Relay Towers", "Offset Barracks", "Chain-Reaction Twins", "Royal Twin Keep",
        "High-Low Towers", "Center Gap", "X-Brace Fort", "Double Staircase", "Balanced Barracks",
        "Canister Vaults", "Bomb Wells", "Guard Galleries", "Funnel Forts", "Royal Bridge",
        "Fortress Wings", "Double Citadel", "Three-Post Trial", "Staggered Supply Line", "Tri-Tower Camp",
        "Bomb Triangle", "Long-Beam Citadel", "Soldier Gallery", "Triple Gate", "Royal Command Camp",
        "Frontline Arc", "Three Bunkers", "Central Bomb Spine", "Bridge and Wings", "Canister Columns",
        "Stepped Defense", "Ring of Guards", "Tower Canyon", "Cascade Wall", "Royal Triangle",
        "Heavy Fort Line", "Bomb Funnel", "Split Roof Camp", "Double Bomb Chambers", "Layer-Cake Fort",
        "Soldier Balcony", "Alternating Pillars", "Canister Gauntlet", "Chain-Reaction Castle", "Royal Siege Line",
        "Four Corners", "Diamond Camp", "Zigzag Batteries", "Four Watchtowers", "Cross Formation",
        "Twin Front Twin Rear", "Outer Ring", "Bomb Chessboard", "Tall-Short Rhythm", "Royal Square",
        "Fortress Quadrants", "Four-Bridge Run", "Command Diamond", "Bomb Moat", "Canister Compass",
        "High-Low Diamond", "Four Bunkers", "Spiral Rise", "Chain-Reaction Quadrants", "Royal Citadel",
        "Grand Parade", "Twin Front Bastions", "Command Staircase", "Crown Labyrinth", "Explosive Stairway",
        "Command Grid", "Royal Gauntlet", "Last Barricade", "Siege Cathedral", "Grand King's Fortress",
    };

    public static string CampaignName(int zeroBasedLevel)
    {
        return CampaignNames[Mathf.Clamp(zeroBasedLevel, 0, 99)];
    }

    public static int CampaignBlockCount(int zeroBasedLevel)
    {
        int[] c = CampaignComposition[Mathf.Clamp(zeroBasedLevel, 0, 99)];
        int sum = 0;
        for (int i = 0; i < c.Length; i++) sum += c[i];
        return sum;
    }

    public static int[] CampaignCompositionFor(int zeroBasedLevel)
    {
        return CampaignComposition[Mathf.Clamp(zeroBasedLevel, 0, 99)];
    }

    // ---- Table arrangement -----------------------------------------------------------------------
    // A table slot is a world-X position, a depth tier (0 = front/low, higher = raised rear), the
    // maximum column count the slot's width allows, and whether it is the command (tallest / royal)
    // structure. Tiers are stacked vertically because the portrait camera is tall but narrow, so
    // "rear" tables are raised rather than pushed back (an orthographic view ignores depth anyway).
    private struct TableSlot
    {
        public float x;
        public int tier;
        public int maxCols;
        public bool command;
    }

    private static TableSlot Slot(float x, int tier, int maxCols, bool command)
        => new TableSlot { x = x, tier = tier, maxCols = maxCols, command = command };

    private static List<TableSlot> GetArrangement(int zeroBasedLevel)
    {
        int n = CampaignTableCount(zeroBasedLevel);
        int[] comp = CampaignComposition[Mathf.Clamp(zeroBasedLevel, 0, 99)];
        bool hasKing = comp[KING] > 0;
        int lv = zeroBasedLevel;
        var s = new List<TableSlot>();

        switch (n)
        {
            case 1:
                s.Add(Slot(0f, 0, 6, true));
                break;

            case 2:
            {
                int p = hasKing ? 1 : lv % 3;
                if (p == 0) { s.Add(Slot(-1.30f, 0, 3, false)); s.Add(Slot(1.30f, 0, 3, true)); }
                else if (p == 1) { s.Add(Slot(-1.30f, 0, 3, false)); s.Add(Slot(1.30f, 1, 3, true)); }
                else { s.Add(Slot(-1.30f, 1, 3, true)); s.Add(Slot(1.30f, 0, 3, false)); }
                break;
            }

            case 3:
            {
                // All 3-table arrangements are tiered so tables never need tight horizontal gaps.
                int p = hasKing ? 0 : lv % 3;
                if (p == 0)
                {
                    // two front wings + raised rear command (king camp).
                    s.Add(Slot(-1.25f, 0, 2, false));
                    s.Add(Slot(1.25f, 0, 2, false));
                    s.Add(Slot(0f, 1, 3, true));
                }
                else if (p == 1)
                {
                    // one front command post + two raised rear wings.
                    s.Add(Slot(0f, 0, 3, true));
                    s.Add(Slot(-1.30f, 1, 2, false));
                    s.Add(Slot(1.30f, 1, 2, false));
                }
                else
                {
                    // shallow arc: side tables forward (low), centre raised.
                    s.Add(Slot(-1.35f, 0, 2, false));
                    s.Add(Slot(1.35f, 0, 2, false));
                    s.Add(Slot(0f, 1, 3, true));
                }
                break;
            }

            case 4:
            {
                int p = hasKing ? 1 : lv % 3;
                if (p == 0)
                {
                    // 2x2 quadrant / fortress square.
                    s.Add(Slot(-1.25f, 0, 3, false));
                    s.Add(Slot(1.25f, 0, 3, false));
                    s.Add(Slot(-1.25f, 1, 3, false));
                    s.Add(Slot(1.25f, 1, 3, true));
                }
                else if (p == 1)
                {
                    // diamond: front, left, right, raised rear command.
                    s.Add(Slot(0f, 0, 3, false));
                    s.Add(Slot(-1.45f, 1, 2, false));
                    s.Add(Slot(1.45f, 1, 2, false));
                    s.Add(Slot(0f, 2, 3, true));
                }
                else
                {
                    // stepped: two front, one mid-rear, one high-rear command.
                    s.Add(Slot(-1.25f, 0, 3, false));
                    s.Add(Slot(1.25f, 0, 3, false));
                    s.Add(Slot(-1.25f, 1, 3, false));
                    s.Add(Slot(1.25f, 2, 3, true));
                }
                break;
            }

            default: // 5 tables (level 100 finale)
                s.Add(Slot(-1.75f, 0, 2, false)); // front-left wing
                s.Add(Slot(1.75f, 0, 2, false));  // front-right wing
                s.Add(Slot(-1.25f, 1, 2, false)); // rear-left command wing
                s.Add(Slot(1.25f, 1, 2, false));  // rear-right command wing
                s.Add(Slot(0f, 2, 3, true));      // high rear-centre royal tower
                break;
        }

        return s;
    }

    // Splits a level's exact inventory across its tables. Every type is spread as evenly as
    // possible, the remainder favours the command table (making it the tallest), and the king is
    // always placed on the command table so the royal piece crowns the special structure.
    private static int[][] AllocateInventory(int[] comp, List<TableSlot> slots)
    {
        int n = slots.Count;
        int cmd = 0;
        for (int i = 0; i < n; i++) if (slots[i].command) cmd = i;

        int[][] inv = new int[n][];
        for (int i = 0; i < n; i++) inv[i] = new int[comp.Length];

        for (int t = 0; t < comp.Length; t++)
        {
            int total = comp[t];
            if (t == KING) { inv[cmd][KING] = total; continue; }
            int baseCount = total / n;
            int rem = total % n;
            for (int i = 0; i < n; i++) inv[i][t] = baseCount;
            for (int r = 0; r < rem; r++) inv[(cmd + r) % n][t]++;
        }
        return inv;
    }

    // ---- Entry points used by FillModelLayout / FillModelTables ---------------------------------
    // The two entry points are called back-to-back for every world build, so one deterministic
    // compute (blocks + tables, mutually consistent and camera-fitted) is cached and shared.
    private const float VSafeTop = 7.7f;    // world-Y ceiling for the tallest piece
    private const float HSafeHalf = 2.72f;  // world-X half-extent for the widest piece

    private static int _cacheLevel = -1;
    private static List<ModelBlockSpec> _cacheBlocks;
    private static List<ModelTableSpec> _cacheTables;

    private static void EnsureCampaign(int level)
    {
        if (_cacheLevel == level && _cacheBlocks != null && _cacheTables != null) return;
        _cacheLevel = level;
        _cacheBlocks = new List<ModelBlockSpec>();
        _cacheTables = new List<ModelTableSpec>();
        ComputeCampaign(level, _cacheBlocks, _cacheTables);
    }

    private static void BuildCampaignLayout(int zeroBasedLevel, List<ModelBlockSpec> blocks)
    {
        EnsureCampaign(Mathf.Clamp(zeroBasedLevel, 0, 99));
        blocks.AddRange(_cacheBlocks);
    }

    private static void BuildCampaignTables(int zeroBasedLevel, List<ModelTableSpec> tables)
    {
        EnsureCampaign(Mathf.Clamp(zeroBasedLevel, 0, 99));
        tables.AddRange(_cacheTables);
    }

    private static void ComputeCampaign(int level, List<ModelBlockSpec> blocks, List<ModelTableSpec> tables)
    {
        var slots = GetArrangement(level);
        int n = slots.Count;
        int[] comp = CampaignComposition[level];
        int[][] inv = AllocateInventory(comp, slots);

        for (int ti = 0; ti < n; ti++)
            BuildTable(blocks, ti, slots[ti], inv[ti], level + ti * 5);

        // Force every table's fort to be a perfect mirror image about its own centre axis. The
        // exact-inventory builder can leave odd pieces on one side; this reflection removes any
        // such lopsidedness so every level reads as a clean, symmetric design.
        SymmetrizeTables(blocks, slots);

        // Make every stacked column physically touch (per column, per depth layer, per table).
        CompactStacks(blocks);

        // Horizontal fit. Single-table levels use the fixed pedestal framing; multi-table
        // battlefields are squeezed inward if the widest piece would leave the portrait frame.
        if (n <= 1)
        {
            NormalizeLayoutToTable(blocks);
        }
        else
        {
            float maxEdge = 0.0001f;
            for (int i = 0; i < blocks.Count; i++)
                maxEdge = Mathf.Max(maxEdge, Mathf.Abs(blocks[i].x) + blocks[i].width * 0.5f);
            if (maxEdge > HSafeHalf)
            {
                float g = HSafeHalf / maxEdge;
                for (int i = 0; i < blocks.Count; i++)
                {
                    var s = blocks[i];
                    s.x *= g; s.width *= g;
                    blocks[i] = s;
                }
            }
        }

        // Per-table footprint (x) and local structure top (height above that table's own surface).
        float[] minX = new float[n], maxX = new float[n], localTop = new float[n];
        bool[] any = new bool[n];
        for (int i = 0; i < n; i++) { minX[i] = float.MaxValue; maxX[i] = float.MinValue; }
        for (int i = 0; i < blocks.Count; i++)
        {
            var s = blocks[i];
            int t = Mathf.Clamp(s.tableIndex, 0, n - 1);
            any[t] = true;
            float rad = s.rotation * Mathf.Deg2Rad;
            float hw = 0.5f * (Mathf.Abs(s.width * Mathf.Cos(rad)) + Mathf.Abs(s.height * Mathf.Sin(rad)));
            minX[t] = Mathf.Min(minX[t], s.x - hw);
            maxX[t] = Mathf.Max(maxX[t], s.x + hw);
            localTop[t] = Mathf.Max(localTop[t], s.yOffset + s.height);
        }

        // Raise each higher tier so it clears the tallest structure on the tier below it.
        int maxTier = 0; for (int i = 0; i < n; i++) maxTier = Mathf.Max(maxTier, slots[i].tier);
        float[] tierTop = new float[maxTier + 1];
        for (int i = 0; i < n; i++) tierTop[slots[i].tier] = Mathf.Max(tierTop[slots[i].tier], localTop[i]);
        float[] tierRaise = new float[maxTier + 1];
        for (int t = 1; t <= maxTier; t++) tierRaise[t] = tierRaise[t - 1] + tierTop[t - 1] + TierGap;

        // Vertical fit: uniformly shrink the composition (block heights + tier raises) so the
        // highest piece sits below the camera's top edge with margin.
        float worldTop = 0.0001f;
        for (int i = 0; i < n; i++) worldTop = Mathf.Max(worldTop, tierRaise[slots[i].tier] + localTop[i]);
        float f = worldTop > VSafeTop ? VSafeTop / worldTop : 1f;
        if (f < 1f)
        {
            for (int i = 0; i < blocks.Count; i++)
            {
                var s = blocks[i];
                s.yOffset *= f; s.height *= f;
                blocks[i] = s;
            }
            for (int t = 0; t <= maxTier; t++) tierRaise[t] *= f;
        }

        // Emit pedestal specs in table-index order (CreateModelTable stores their surfaces in order).
        for (int i = 0; i < n; i++)
        {
            float center, width;
            if (!any[i]) { center = slots[i].x; width = 1.5f; }
            else { center = (minX[i] + maxX[i]) * 0.5f; width = (maxX[i] - minX[i]) + TableMargin; }
            if (n == 1) width = Mathf.Clamp((maxX[i] - minX[i]) + 0.9f, 3.4f, TargetTableWidth);
            tables.Add(new ModelTableSpec(center, width, tierRaise[slots[i].tier], FrontLayerZ));
        }
    }

    // Guarantees every table's structure is perfectly mirror-symmetric about its own centre axis.
    // For each table we keep the denser half (plus any pieces sitting on the axis) and reflect it
    // across the axis to rebuild the opposite side, negating rotation so tilted pieces mirror too.
    // The later steps preserve this: CompactStacks restacks each column independently (symmetric
    // columns stay identical) and the horizontal fit is a uniform scale about the origin (which maps
    // a pair mirrored about x=a to a pair mirrored about x=a*scale). So the symmetry survives to render.
    private static void SymmetrizeTables(List<ModelBlockSpec> blocks, List<TableSlot> slots)
    {
        if (blocks.Count == 0 || slots.Count == 0) return;
        const float eps = 0.02f; // separates centre-column pieces (on the axis) from side pieces

        var result = new List<ModelBlockSpec>(blocks.Count);
        for (int t = 0; t < slots.Count; t++)
        {
            float axis = slots[t].x;

            int rightCount = 0, leftCount = 0;
            for (int i = 0; i < blocks.Count; i++)
            {
                if (blocks[i].tableIndex != t) continue;
                float d = blocks[i].x - axis;
                if (d > eps) rightCount++;
                else if (d < -eps) leftCount++;
            }
            bool useRight = rightCount >= leftCount;

            for (int i = 0; i < blocks.Count; i++)
            {
                ModelBlockSpec s = blocks[i];
                if (s.tableIndex != t) continue;
                float d = s.x - axis;

                if (Mathf.Abs(d) <= eps)
                {
                    // A piece on the centre axis is already self-symmetric: snap it exactly on and keep it once.
                    s.x = axis;
                    s.rotation = 0f;
                    result.Add(s);
                    continue;
                }

                bool isSource = useRight ? d > eps : d < -eps;
                if (!isSource) continue; // discard the thinner half; it is recreated by the reflection below

                result.Add(s);
                ModelBlockSpec mirror = s;
                mirror.x = axis - d; // reflect across the axis (2*axis - s.x)
                mirror.rotation = -s.rotation;
                result.Add(mirror);
            }
        }

        blocks.Clear();
        blocks.AddRange(result);
    }

    // ---- Per-table silhouette builder -----------------------------------------------------------
    // Builds one readable fort on a table, consuming EXACTLY the supplied inventory. Body columns
    // come from box/box2 (heavy box2 anchors the base); box3 caps as turrets; long_box/long_box2
    // form lintels and roof caps (alternating the dominant beam); cannisters/bombs tuck into the
    // structure; soldiers crown the roofline; the king (if any) sits at the very apex.
    private static void BuildTable(List<ModelBlockSpec> b, int tableIndex, TableSlot slot, int[] inv, int styleSeed)
    {
        const float CELL = ModelColPitch; // 0.72
        float ox = slot.x;

        int box = inv[BOX], box2 = inv[BOX2], box3 = inv[BOX3];
        int lbox = inv[LONG_BOX], lbox2 = inv[LONG_BOX2];
        int sol = inv[SOLDIER], can = inv[CANNISTER], bomb = inv[BOMB], king = inv[KING];

        int bodyCubes = box + box2;
        int maxCols = Mathf.Max(2, slot.maxCols);
        int C = Mathf.Clamp(Mathf.CeilToInt(bodyCubes / 2f), 2, maxCols);
        if (bodyCubes < C) C = Mathf.Max(1, bodyCubes);

        int style = slot.command ? 1 : (styleSeed % 4); // 0 wall, 1 peak, 2 staircase, 3 bunker
        int[] heights = BuildProfile(bodyCubes, C, style);

        float[] cx = new float[C];
        for (int i = 0; i < C; i++) cx[i] = ox + (i - (C - 1) * 0.5f) * CELL;

        // Body: box2 fills the lowest rows across all columns, box the rest. This consumes the
        // full box+box2 budget and keeps the heaviest bricks on the ground for a stable base.
        int box2Left = box2, boxLeft = box;
        int maxH = 0; for (int i = 0; i < C; i++) if (heights[i] > maxH) maxH = heights[i];
        for (int row = 0; row < maxH; row++)
        {
            for (int i = 0; i < C; i++)
            {
                if (heights[i] <= row) continue;
                int variant;
                if (box2Left > 0) { variant = BOX2; box2Left--; }
                else { variant = BOX; boxLeft--; }
                AddModel(b, cx[i], RowY(row), variant, CELL, CELL, 0, tableIndex);
            }
        }

        // Per-column stacking cursor for toppers (CompactStacks later tightens the contact).
        float[] cursor = new float[C];
        for (int i = 0; i < C; i++) cursor[i] = heights[i] * CELL;

        int[] outer = OrderOuterFirst(C);
        int[] centre = OrderCenterFirst(C);
        int[] interior = InteriorColumns(C);

        // box3 turrets on the outer shoulders.
        for (int k = 0; k < box3; k++)
        {
            int i = outer[k % C];
            AddModel(b, cx[i], cursor[i], BOX3, 0.50f, 0.66f, 0, tableIndex); cursor[i] += 0.66f;
        }
        // cannisters tucked onto the structure (barrels), spread from the centre.
        for (int k = 0; k < can; k++)
        {
            int i = centre[k % C];
            AddModel(b, cx[i], cursor[i], CANNISTER, 0.50f, 0.66f, 0, tableIndex); cursor[i] += 0.66f;
        }
        // bombs in separated interior pockets (different columns) so shot order matters.
        for (int k = 0; k < bomb; k++)
        {
            int i = interior[k % interior.Length];
            AddModel(b, cx[i], cursor[i], BOMB, 0.58f, 0.62f, 0, tableIndex); cursor[i] += 0.62f;
        }
        // soldiers crown the roofline, spread from the centre so they read clearly on top.
        for (int k = 0; k < sol; k++)
        {
            int i = centre[k % C];
            AddModel(b, cx[i], cursor[i], SOLDIER, 0.50f, 0.72f, 0, tableIndex); cursor[i] += 0.72f;
        }
        // king at the central apex, sitting above everything else. It is placed exactly on the
        // table's symmetry axis (ox) so the mirror pass keeps it a single, centred royal piece
        // instead of reflecting an off-centre king into a duplicate pair.
        if (king > 0)
        {
            float topY = 0f;
            for (int i = 0; i < C; i++) topY = Mathf.Max(topY, cursor[i]);
            AddModel(b, ox, topY, KING, 0.85f, 1.05f, 0, tableIndex);
        }

        // Beams: lintels bridging adjacent column tops, alternating long_box / long_box2 so the
        // dominant beam language changes level to level. Extra beams stack as a second roof course.
        int beamTotal = lbox + lbox2;
        if (beamTotal > 0)
        {
            int lbLeft = lbox, lb2Left = lbox2;
            bool longBoxFirst = (styleSeed % 2 == 0);
            if (C >= 2)
            {
                int mids = C - 1;
                float[] midCursor = new float[mids];
                for (int m = 0; m < mids; m++)
                    midCursor[m] = Mathf.Min(heights[m], heights[m + 1]) * CELL;
                for (int k = 0; k < beamTotal; k++)
                {
                    int m = k % mids;
                    bool pickLong = longBoxFirst ? (k % 2 == 0) : (k % 2 == 1);
                    int variant;
                    if (pickLong && lbLeft > 0) { variant = LONG_BOX; lbLeft--; }
                    else if (!pickLong && lb2Left > 0) { variant = LONG_BOX2; lb2Left--; }
                    else if (lbLeft > 0) { variant = LONG_BOX; lbLeft--; }
                    else { variant = LONG_BOX2; lb2Left--; }
                    float w = variant == LONG_BOX ? 1.55f * CELL : 1.70f * CELL;
                    float h = variant == LONG_BOX ? 0.42f : 0.30f;
                    float mx = (cx[m] + cx[m + 1]) * 0.5f;
                    AddModel(b, mx, midCursor[m], variant, w, h, 0, tableIndex);
                    midCursor[m] += h;
                }
            }
            else
            {
                for (int k = 0; k < beamTotal; k++)
                {
                    bool pickLong = longBoxFirst ? (k % 2 == 0) : (k % 2 == 1);
                    int variant;
                    if (pickLong && lbLeft > 0) { variant = LONG_BOX; lbLeft--; }
                    else if (!pickLong && lb2Left > 0) { variant = LONG_BOX2; lb2Left--; }
                    else if (lbLeft > 0) { variant = LONG_BOX; lbLeft--; }
                    else { variant = LONG_BOX2; lb2Left--; }
                    float w = variant == LONG_BOX ? 1.05f * CELL : 1.15f * CELL;
                    float h = variant == LONG_BOX ? 0.42f : 0.30f;
                    AddModel(b, cx[0], cursor[0], variant, w, h, 0, tableIndex); cursor[0] += h;
                }
            }
        }
    }

    // Column height profile summing to `total`, shaped by style. Uses the largest-remainder method
    // so the sum is always exact and every used column has at least one brick.
    private static int[] BuildProfile(int total, int C, int style)
    {
        int[] h = new int[C];
        if (C <= 0) return h;
        if (total <= C)
        {
            // Centre the few bricks so short structures still read as a compact block.
            int[] order = OrderCenterFirst(C);
            for (int i = 0; i < total; i++) h[order[i]] = 1;
            return h;
        }

        for (int i = 0; i < C; i++) h[i] = 1;
        int left = total - C;
        int mid = C / 2;
        float[] w = new float[C];
        for (int i = 0; i < C; i++)
        {
            switch (style)
            {
                case 1: w[i] = C - Mathf.Abs(i - mid); break;      // peak (centre tall)
                case 2: w[i] = i + 1; break;                       // staircase
                case 3: w[i] = 1 + Mathf.Abs(i - mid); break;      // bunker (U-shape, tall ends)
                default: w[i] = 1; break;                          // wall (even)
            }
            if (w[i] < 0.01f) w[i] = 0.01f;
        }
        float wsum = 0f; for (int i = 0; i < C; i++) wsum += w[i];
        float[] frac = new float[C]; int used = 0;
        for (int i = 0; i < C; i++)
        {
            float ideal = w[i] / wsum * left;
            int fl = Mathf.FloorToInt(ideal);
            h[i] += fl; used += fl; frac[i] = ideal - fl;
        }
        int rem = left - used;
        for (int r = 0; r < rem; r++)
        {
            int bi = 0; for (int i = 1; i < C; i++) if (frac[i] > frac[bi]) bi = i;
            h[bi]++; frac[bi] = -1f;
        }
        return h;
    }

    private static int[] OrderOuterFirst(int C)
    {
        int[] r = new int[C];
        int lo = 0, hi = C - 1, idx = 0;
        while (lo <= hi)
        {
            r[idx++] = lo;
            if (lo != hi) r[idx++] = hi;
            lo++; hi--;
        }
        return r;
    }

    private static int[] OrderCenterFirst(int C)
    {
        int[] r = new int[C];
        int mid = C / 2, idx = 0;
        r[idx++] = mid;
        for (int d = 1; idx < C; d++)
        {
            if (mid - d >= 0) r[idx++] = mid - d;
            if (idx < C && mid + d < C) r[idx++] = mid + d;
        }
        return r;
    }

    private static int[] InteriorColumns(int C)
    {
        if (C <= 2)
        {
            int[] a = new int[C];
            for (int i = 0; i < C; i++) a[i] = i;
            return a;
        }
        List<int> list = new List<int>();
        for (int i = 1; i < C - 1; i++) list.Add(i);
        return list.ToArray();
    }
}
