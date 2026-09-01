using System.Collections.Generic;
using UnityEngine;

// =================================================================================================
// SOLDIER SMASH -- authored 100-level campaign.
//
// This partial extends WarfestLevelCatalog with the campaign defined by the master design brief:
// 100 hand-specified military set pieces built from the full gameplay palette, with exact per-level
// composition budgets. Levels 20-50 use a mirrored two-table presentation; every other level keeps
// the original single presentation table.
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
    private const float CampaignCell = 0.60f; // one consistent block module across the full campaign
    private const float TableMargin = 0.36f;  // pedestal breathing room around a structure footprint
    private const float TableGap = 0.10f;     // visible seam between tables sharing the same stage row
    private const float StageStep = 0.16f;    // small tabletop lift used to draw gates, crowns, and stairs
    private const float BaseTableTop = -0.35f;
    private const float TwinTableWidth = 3.25f;
    private const float TwinTableCenterOffset = 0.62f;
    private const float TwinFrontTop = -0.82f;
    private const float TwinRearTop = 0.48f;
    private const float TwinFrontYaw = 30f;
    private const float TwinRearYaw = -30f;

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
        return UsesTwinTableFormat(zeroBasedLevel) ? 2 : 1;
    }

    private static bool UsesTwinTableFormat(int zeroBasedLevel)
    {
        int levelNumber = Mathf.Clamp(zeroBasedLevel, 0, 99) + 1;
        return levelNumber >= 20 && levelNumber <= 50;
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
        int referenceCount = ReferenceLayoutBlockCount(zeroBasedLevel);
        if (referenceCount >= 0) return referenceCount;

        int[] c = CampaignCompositionFor(zeroBasedLevel);
        int sum = 0;
        for (int i = 0; i < c.Length; i++) sum += c[i];
        return sum;
    }

public static int[] CampaignCompositionFor(int zeroBasedLevel)
    {
        int level = Mathf.Clamp(zeroBasedLevel, 0, 99);

        // Levels 1-3 are deliberately small, reference-driven tutorial layouts. Their inventories
        // are exact rather than expanded so the construction never receives unrelated pieces.
        if (level == 0) return new[] { 0, 0, 0, 0, 0, 1, 18, 0, 0 };
        if (level == 1) return new[] { 0, 0, 0, 0, 0, 1, 20, 0, 0 };
        if (level == 2) return new[] { 9, 0, 0, 0, 9, 0, 0, 0, 0 };

        int[] expanded = (int[])CampaignComposition[level].Clone();
        int lv = level + 1;

        // Every level gains a little more material, while the later acts gain enough structural
        // pieces to form genuinely deeper multi-row fortifications instead of thin tall stacks.
        if (lv <= 10)
        {
            expanded[LONG_BOX2] += 2; // mirrored extra roof course for the symmetric tutorial act
        }
        else if (lv <= 25)
        {
            expanded[BOX] += 2;
            expanded[LONG_BOX2] += 2;
        }
        else if (lv <= 50)
        {
            expanded[BOX] += 2;
            expanded[BOX2] += 2;
            expanded[LONG_BOX] += 2;
        }
        else if (lv <= 75)
        {
            expanded[BOX] += 4;
            expanded[BOX2] += 2;
            expanded[LONG_BOX] += 2;
            expanded[SOLDIER] += 2;
        }
        else
        {
            expanded[BOX] += 4;
            expanded[BOX2] += 4;
            expanded[LONG_BOX] += 2;
            expanded[SOLDIER] += 2;
            expanded[CANNISTER] += 2;
        }

        if (UsesTwinTableFormat(level))
        {
            // The paired forts are deliberately denser than the surrounding single-table levels.
            // Even counts let both tables receive identical inventories, so the complete formation
            // remains a true mirror pair instead of merely looking approximately balanced.
            expanded[BOX] += 4;
            expanded[BOX2] += 2;
            expanded[LONG_BOX2] += 2;
            expanded[SOLDIER] += 2;
            for (int i = 0; i < expanded.Length; i++)
            {
                if ((expanded[i] & 1) != 0) expanded[i]++;
            }
        }
        return expanded;
    }

    // ---- Table arrangement -----------------------------------------------------------------------
    // A table slot describes an intentional stage position. `x` is a lane hint used to order tables
    // inside a row; exact centres are solved from the finished structure footprints so tables cannot
    // overlap. Tiers form readable terraces, like the linked gates and pyramids in the references.
    private struct TableSlot
    {
        public float x;
        public int tier;
        public int maxCols;
        public bool command;
        public int design;
        public float width;
        public float visibleTopY;
        public float depth;
        public float yawDegrees;
    }

    private static TableSlot Slot(float lane, int tier, int maxCols, bool command, int design,
        float width, float visibleTopY, float depth, float yawDegrees)
        => new TableSlot
        {
            x = lane,
            tier = tier,
            maxCols = maxCols,
            command = command,
            design = design,
            width = width,
            visibleTopY = visibleTopY,
            depth = depth,
            yawDegrees = yawDegrees
        };

    private static List<TableSlot> GetArrangement(int zeroBasedLevel)
    {
        int level = Mathf.Clamp(zeroBasedLevel, 0, 99);
        if (UsesTwinTableFormat(level))
        {
            // The left table sits forward and lower; the right table is just behind it and raised
            // enough that both play surfaces remain readable. Opposing yaw angles reproduce the
            // open V-shaped arrangement in the supplied reference while blocks remain upright.
            return new List<TableSlot>
            {
                Slot(-TwinTableCenterOffset, 0, 4, false, level % 20,
                    TwinTableWidth, TwinFrontTop, FrontLayerZ, TwinFrontYaw),
                Slot(TwinTableCenterOffset, 1, 4, true, (level + 1) % 20,
                    TwinTableWidth, TwinRearTop, RearLayerZ, TwinRearYaw)
            };
        }

        // Seven fixed cells fit inside the full 4.8-unit table and give every level a stable,
        // arcade-like presentation board. The profile and ornament order vary per level.
        return new List<TableSlot>
        {
            Slot(0f, 0, 7, true, level % 20,
                TargetTableWidth, BaseTableTop, FrontLayerZ, 0f)
        };
    }

    // Splits a level's exact inventory across its tables. Every type is spread as evenly as
    // possible, the remainder favours the command table (making it the tallest), and the king is
    // always placed on the command table so the royal piece crowns the special structure.
    private static int[][] AllocateInventory(int[] comp, List<TableSlot> slots, int level)
    {
        int[][] inventory = new int[slots.Count][];
        for (int i = 0; i < inventory.Length; i++) inventory[i] = new int[comp.Length];

        int command = 0;
        for (int i = 0; i < slots.Count; i++) if (slots[i].command) command = i;
        for (int variant = 0; variant < comp.Length; variant++)
        {
            int perTable = comp[variant] / slots.Count;
            int remainder = comp[variant] % slots.Count;
            for (int i = 0; i < slots.Count; i++) inventory[i][variant] = perTable;
            for (int i = 0; i < remainder; i++) inventory[(command + i) % slots.Count][variant]++;
        }
        return inventory;
    }

    private static float EstimatedPieceHeight(int variant)
    {
        switch (variant)
        {
            case LONG_BOX: return 0.34f;
            case LONG_BOX2: return 0.24f;
            case SOLDIER: return 0.62f;
            case BOX3: return 0.58f;
            case KING: return 0.90f;
            case CANNISTER:
            case BOMB: return 0.56f;
            default: return CampaignCell;
        }
    }

    // ---- Entry points used by FillModelLayout / FillModelTables ---------------------------------
    // The two entry points are called back-to-back for every world build, so one deterministic
    // compute (blocks + tables, mutually consistent and camera-fitted) is cached and shared.
    private const float VSafeTop = 7.8f;    // world-Y ceiling for the tallest piece (camera top is 8.2)
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

private static void BuildRequestedOpeningLayout(int level, List<ModelBlockSpec> blocks)
    {
        const float CannisterWidth = 0.44f;
        const float CannisterHeight = 0.56f;

        if (level == 0)
        {
            // Level 1: two identical 3 x 3 cannister walls, front and rear, with one soldier above.
            const float ColumnPitch = 0.52f;
            for (int depth = 0; depth < 2; depth++)
            {
                for (int row = 0; row < 3; row++)
                {
                    for (int column = -1; column <= 1; column++)
                    {
                        AddModel(blocks, column * ColumnPitch, row * CannisterHeight,
                            CANNISTER, CannisterWidth, CannisterHeight, depth, 0);
                    }
                }
            }
            AddModel(blocks, 0f, 3f * CannisterHeight, SOLDIER, 0.44f, 0.62f, 0, 0);
            return;
        }

        if (level == 1)
        {
            // Level 2: 6, 5, 4, 3, 2 cannisters; the soldier is the only apex piece.
            const float ColumnPitch = 0.50f;
            for (int row = 0; row < 5; row++)
            {
                int count = 6 - row;
                for (int column = 0; column < count; column++)
                {
                    float x = (column - (count - 1) * 0.5f) * ColumnPitch;
                    AddModel(blocks, x, row * CannisterHeight,
                        CANNISTER, CannisterWidth, CannisterHeight, 0, 0);
                }
            }
            AddModel(blocks, 0f, 5f * CannisterHeight, SOLDIER, 0.44f, 0.62f, 0, 0);
            return;
        }

        // Level 3: a three-course green-box lattice. Every horizontal rail is long_box2,
        // matching the attached reference while retaining a small, readable starter footprint.
        const float GreenBoxSize = 0.60f;
        const float RailWidth = 1.08f;
        const float RailHeight = 0.24f;
        const float PanelPitch = 1.12f;
        const float CoursePitch = 0.84f;
        for (int course = 0; course < 3; course++)
        {
            float railY = course * CoursePitch;
            for (int panel = -1; panel <= 1; panel++)
            {
                float x = panel * PanelPitch;
                AddModel(blocks, x, railY, LONG_BOX2, RailWidth, RailHeight, 0, 0);
                AddModel(blocks, x, railY + RailHeight, BOX, GreenBoxSize, GreenBoxSize, 0, 0);
            }
        }
    }


private static void ComputeCampaign(int level, List<ModelBlockSpec> blocks, List<ModelTableSpec> tables)
    {
        var slots = GetArrangement(level);

        // Preserve the explicitly requested opening layouts before routing Levels 4+ through
        // the newer reference-layout collection.
        if (level <= 2)
        {
            BuildRequestedOpeningLayout(level, blocks);
            TableSlot openingSlot = slots[0];
            tables.Add(new ModelTableSpec(
                openingSlot.x, openingSlot.width, openingSlot.visibleTopY,
                openingSlot.depth, openingSlot.yawDegrees));

            if (blocks.Count != CampaignBlockCount(level))
                Debug.LogError("Opening layout changed inventory at level " + (level + 1));
            return;
        }

        if (TryBuildReferenceLayout(level, blocks, tables)) return;

        int[] comp = CampaignCompositionFor(level);
        int[][] inv = AllocateInventory(comp, slots, level);

        if (slots.Count == 2)
        {
            // Build one compact fort, move it onto the front table, then reflect the exact same
            // pieces onto the rear table. This keeps variant counts, silhouettes, and rotations
            // perfectly symmetric while the table heights/depths create the front/back staging.
            BuildTable(blocks, 0, slots[0], inv[0], level);
            int sourceCount = blocks.Count;
            for (int i = 0; i < sourceCount; i++)
            {
                ModelBlockSpec source = blocks[i];
                source.x += slots[0].x;
                source.tableIndex = 0;
                source.depthLayer = 0;
                blocks[i] = source;
            }
            for (int i = 0; i < sourceCount; i++)
            {
                ModelBlockSpec mirror = blocks[i];
                mirror.x = -mirror.x;
                mirror.rotation = -mirror.rotation;
                mirror.tableIndex = 1;
                mirror.depthLayer = 1;
                blocks.Add(mirror);
            }
        }
        else if (level < 10)
        {
            BuildSymmetricEarlyTable(blocks, 0, inv[0], level);
        }
        else
        {
            BuildTable(blocks, 0, slots[0], inv[0], level);
        }

        for (int i = 0; i < slots.Count; i++)
        {
            TableSlot slot = slots[i];
            tables.Add(new ModelTableSpec(
                slot.x, slot.width, slot.visibleTopY, slot.depth, slot.yawDegrees));
        }

        if (blocks.Count != CampaignBlockCount(level))
            Debug.LogError("Campaign layout changed inventory at level " + (level + 1));
    }

    private static void PlaceTableRows(List<TableSlot> slots, float[] widths, float[] centers)
    {
        var row = new List<int>();
        for (int i = 0; i < slots.Count; i++) row.Add(i);
        row.Sort((a, b) => slots[a].x.CompareTo(slots[b].x));

        float total = TableGap * Mathf.Max(0, row.Count - 1);
        for (int i = 0; i < row.Count; i++) total += widths[row[i]];

        float cursor = -total * 0.5f;
        for (int i = 0; i < row.Count; i++)
        {
            int index = row[i];
            centers[index] = cursor + widths[index] * 0.5f;
            cursor += widths[index] + TableGap;
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
    // The opening act is explicitly art-directed. Each profile is [inner, middle, outer] mirrored
    // column height, producing ten distinct silhouettes while every variant is itself mirrored.
    private static readonly int[][] EarlySymmetricProfiles =
    {
        new[]{1,2,3}, // 01 wide gatehouse: tall outer towers
        new[]{3,4},   // 02 compact twin sentries
        new[]{2,5},   // 03 narrow high supply arch
        new[]{1,2,3}, // 04 wide disciplined pillars
        new[]{4,2},   // 05 compact central barricade
        new[]{2,2,3}, // 06 wide double arch
        new[]{4,2},   // 07 compact nested beam pyramid
        new[]{3,3},   // 08 compact barracks wall
        new[]{3,4},   // 09 protected fuse towers
        new[]{1,2,3}, // 10 wide royal outpost
    };

    private static void BuildSymmetricEarlyTable(List<ModelBlockSpec> b, int tableIndex, int[] inv, int level)
    {
        const float CELL = CampaignCell;
        int[] pairHeights = EarlySymmetricProfiles[level];
        float[] pairCursor = new float[pairHeights.Length];

        int box = inv[BOX], box2 = inv[BOX2];
        int centreBox = (box & 1) == 1 ? 1 : 2;
        if (level == 4 && box >= 3) centreBox = 3;
        int centreBox2 = (box2 & 1) == 1 ? 1 : 0;

        float centreCursor = 0f;
        for (int i = 0; i < centreBox2; i++)
        {
            AddModel(b, 0f, centreCursor, BOX2, CELL, CELL, 0, tableIndex);
            centreCursor += CELL;
        }
        for (int i = 0; i < centreBox; i++)
        {
            AddModel(b, 0f, centreCursor, BOX, CELL, CELL, 0, tableIndex);
            centreCursor += CELL;
        }

        int heavyPairs = (box2 - centreBox2) / 2;
        int lightPairs = (box - centreBox) / 2;
        int maxRows = 0;
        for (int i = 0; i < pairHeights.Length; i++) maxRows = Mathf.Max(maxRows, pairHeights[i]);
        for (int row = 0; row < maxRows; row++)
        {
            for (int pair = 0; pair < pairHeights.Length; pair++)
            {
                if (pairHeights[pair] <= row) continue;
                int variant = heavyPairs > 0 ? BOX2 : BOX;
                if (heavyPairs > 0) heavyPairs--; else lightPairs--;
                float x = (pair + 1) * CELL;
                AddModel(b, -x, row * CELL, variant, CELL, CELL, 0, tableIndex);
                AddModel(b, x, row * CELL, variant, CELL, CELL, 0, tableIndex);
                pairCursor[pair] = (row + 1) * CELL;
            }
        }

        // Lintels establish a clean common roof before objectives are added. Odd beam counts use a
        // centred lintel plus a mirrored pair; even counts use a mirrored pair around the axis.
        PlaceSymmetricBeamRow(b, tableIndex, LONG_BOX, inv[LONG_BOX], ref centreCursor, pairCursor);
        PlaceSymmetricBeamRow(b, tableIndex, LONG_BOX2, inv[LONG_BOX2], ref centreCursor, pairCursor);

        PlaceSymmetricTopPieces(b, tableIndex, BOX3, inv[BOX3], 0.42f, 0.58f,
            level, 1, ref centreCursor, pairCursor);
        PlaceSymmetricTopPieces(b, tableIndex, CANNISTER, inv[CANNISTER], 0.44f, 0.56f,
            level, 2, ref centreCursor, pairCursor);
        PlaceSymmetricTopPieces(b, tableIndex, BOMB, inv[BOMB], 0.50f, 0.56f,
            level, 3, ref centreCursor, pairCursor);
        PlaceSymmetricTopPieces(b, tableIndex, SOLDIER, inv[SOLDIER], 0.44f, 0.62f,
            level, 4, ref centreCursor, pairCursor);
        PlaceSymmetricTopPieces(b, tableIndex, KING, inv[KING], 0.74f, 0.90f,
            level, 5, ref centreCursor, pairCursor);

        if (heavyPairs != 0 || lightPairs != 0)
            Debug.LogError("Opening level profile did not consume its structural pairs at level " + (level + 1));
    }

    private static void PlaceSymmetricBeamRow(List<ModelBlockSpec> b, int tableIndex, int variant,
        int count, ref float centreCursor, float[] pairCursor)
    {
        if (count <= 0) return;
        float roof = centreCursor;
        for (int i = 0; i < pairCursor.Length; i++) roof = Mathf.Max(roof, pairCursor[i]);
        float width = variant == LONG_BOX ? 1.55f * CampaignCell : 1.70f * CampaignCell;
        float height = variant == LONG_BOX ? 0.34f : 0.24f;

        if ((count & 1) == 1) AddModel(b, 0f, roof, variant, width, height, 0, tableIndex);
        int pairs = count / 2;
        for (int i = 0; i < pairs; i++)
        {
            float x = ((count & 1) == 1 ? 2f : 1.5f) * CampaignCell + i * CampaignCell;
            float rowY = roof + i * height;
            AddModel(b, -x, rowY, variant, width, height, 0, tableIndex);
            AddModel(b, x, rowY, variant, width, height, 0, tableIndex);
        }

        float finalRoof = roof + Mathf.Max(1, pairs) * height;
        centreCursor = finalRoof;
        for (int i = 0; i < pairCursor.Length; i++) pairCursor[i] = finalRoof;
    }

    private static void PlaceSymmetricTopPieces(List<ModelBlockSpec> b, int tableIndex, int variant,
        int count, float width, float height, int style, int phase,
        ref float centreCursor, float[] pairCursor)
    {
        if ((count & 1) == 1)
        {
            AddModel(b, 0f, centreCursor, variant, width, height, 0, tableIndex);
            centreCursor += height;
        }

        int[] order = OrderForStyle(pairCursor.Length, style, phase);
        for (int p = 0; p < count / 2; p++)
        {
            int best = order[0];
            for (int oi = 1; oi < order.Length; oi++)
            {
                int candidate = order[oi];
                if (pairCursor[candidate] < pairCursor[best] - 0.0001f) best = candidate;
            }
            float x = (best + 1) * CampaignCell;
            AddModel(b, -x, pairCursor[best], variant, width, height, 0, tableIndex);
            AddModel(b, x, pairCursor[best], variant, width, height, 0, tableIndex);
            pairCursor[best] += height;
        }
    }

    // Builds one readable fort on a table, consuming EXACTLY the supplied inventory. Body columns
    // come from box/box2 (heavy box2 anchors the base); box3 caps as turrets; long_box/long_box2
    // form lintels and roof caps (alternating the dominant beam); cannisters/bombs tuck into the
    // structure; soldiers crown the roofline; the king (if any) sits at the very apex.
    private static readonly int[] CampaignColumnPattern =
    {
        5,4,6,3,5,6,4,5,3,7, 6,4,7,5,3,6,5,4,3,7,
    };

    private static int DesignColumnCount(int bodyCubes, int totalPieces, int level, int maxColumns)
    {
        int band = level / 20;
        int authored = CampaignColumnPattern[level % CampaignColumnPattern.Length];
        authored += band % 3 - 1;
        int minimumForHeight = Mathf.CeilToInt(bodyCubes / 5f);
        int minimumForDensity = Mathf.CeilToInt(totalPieces / 8f);
        int upper = Mathf.Clamp(maxColumns, 3, 7);
        return Mathf.Clamp(Mathf.Max(authored, minimumForHeight, minimumForDensity), 3, upper);
    }

    private static float[] DesignColumnCenters(int count, int level, float tableWidth)
    {
        float[] centres = new float[count];
        int spacingMode = (level * 7 + level / 10) % 5;
        float desiredPitch = 0.60f + spacingMode * 0.055f;
        float maximumPitch = count <= 1 ? 0f : (tableWidth - CampaignCell - 0.20f) / (count - 1);
        float pitch = count <= 1 ? 0f : Mathf.Min(desiredPitch, maximumPitch);
        for (int i = 0; i < count; i++) centres[i] = (i - (count - 1) * 0.5f) * pitch;
        return centres;
    }

    private static void BuildTable(List<ModelBlockSpec> b, int tableIndex, TableSlot slot, int[] inv, int styleSeed)
    {
        const float CELL = CampaignCell;

        int box = inv[BOX], box2 = inv[BOX2], box3 = inv[BOX3];
        int lbox = inv[LONG_BOX], lbox2 = inv[LONG_BOX2];
        int sol = inv[SOLDIER], can = inv[CANNISTER], bomb = inv[BOMB], king = inv[KING];

        int bodyCubes = box + box2;
        int totalPieces = 0;
        for (int i = 0; i < inv.Length; i++) totalPieces += inv[i];
        bool denseStage = totalPieces >= 42;
        int C = DesignColumnCount(bodyCubes, totalPieces, styleSeed, slot.maxCols);
        int style = (styleSeed * 7 + (styleSeed / 20) * 3) % 20;
        int[] heights = BuildProfile(bodyCubes, C, style);
        float[] cx = DesignColumnCenters(C, styleSeed, slot.width);

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
                AddModel(b, cx[i], row * CELL, variant, CELL, CELL, 0, tableIndex);
            }
        }

        // Per-column stacking cursor for decorations and lintels. It is the single source of
        // truth for vertical placement, so pieces never overlap and never need a scale-down pass.
        float[] cursor = new float[C];
        for (int i = 0; i < C; i++) cursor[i] = heights[i] * CELL;

        int[] turretOrder = OrderForStyle(C, style, 1);
        int[] canisterOrder = OrderForStyle(C, style, 3);
        int[] bombOrder = InteriorOrder(C, style);
        int[] soldierOrder = OrderForStyle(C, style, 5);

        // Beam strategy alternates between valley bridges, authored bridge sequences, and tower
        // crowns. That prevents every level from settling into the same flat roof course.
        int beamTotal = lbox + lbox2;
        int lbLeft = lbox, lb2Left = lbox2;
        int[] bridgeOrder = AdjacentOrder(C, style);
        for (int k = 0; k < beamTotal; k++)
        {
            int beamMode = denseStage ? 0 : (styleSeed + style) % 3;
            int bridge = beamMode == 1 ? bridgeOrder[k % bridgeOrder.Length] : bridgeOrder[0];
            float bestTop = Mathf.Max(cursor[bridge], cursor[bridge + 1]);
            for (int oi = beamMode == 1 ? bridgeOrder.Length : 1; oi < bridgeOrder.Length; oi++)
            {
                int candidate = bridgeOrder[oi];
                float top = Mathf.Max(cursor[candidate], cursor[candidate + 1]);
                bool preferred = beamMode == 0
                    ? top < bestTop - 0.0001f
                    : top > bestTop + 0.0001f;
                if (preferred)
                {
                    bridge = candidate;
                    bestTop = top;
                }
            }
            int variant = TakeBeamVariant(ref lbLeft, ref lb2Left, k, styleSeed);
            float width = variant == LONG_BOX ? 1.55f * CELL : 1.70f * CELL;
            float height = variant == LONG_BOX ? 0.34f : 0.24f;
            AddModel(b, (cx[bridge] + cx[bridge + 1]) * 0.5f, bestTop,
                variant, width, height, 0, tableIndex);
            cursor[bridge] = bestTop + height;
            cursor[bridge + 1] = bestTop + height;
        }

        // Decoration strategies rotate between balanced, ordered, alternating, and clustered
        // placement. Counts still come from the authored level inventory.
        for (int k = 0; k < box3; k++)
        {
            int i = denseStage ? LowestColumn(cursor, turretOrder) : DesignedColumn(cursor, turretOrder, styleSeed, k, 1);
            AddModel(b, cx[i], cursor[i], BOX3, 0.42f, 0.58f, 0, tableIndex); cursor[i] += 0.58f;
        }
        for (int k = 0; k < can; k++)
        {
            int i = denseStage ? LowestColumn(cursor, canisterOrder) : DesignedColumn(cursor, canisterOrder, styleSeed, k, 2);
            AddModel(b, cx[i], cursor[i], CANNISTER, 0.44f, 0.56f, 0, tableIndex); cursor[i] += 0.56f;
        }
        for (int k = 0; k < bomb; k++)
        {
            int i = denseStage ? LowestColumn(cursor, bombOrder) : DesignedColumn(cursor, bombOrder, styleSeed, k, 3);
            AddModel(b, cx[i], cursor[i], BOMB, 0.50f, 0.56f, 0, tableIndex); cursor[i] += 0.56f;
        }
        for (int k = 0; k < sol; k++)
        {
            int i = denseStage ? LowestColumn(cursor, soldierOrder) : DesignedColumn(cursor, soldierOrder, styleSeed, k, 4);
            AddModel(b, cx[i], cursor[i], SOLDIER, 0.44f, 0.62f, 0, tableIndex); cursor[i] += 0.62f;
        }
        for (int k = 0; k < king; k++)
        {
            int i = HighestColumn(cursor, cx, style + k);
            AddModel(b, cx[i], cursor[i], KING, 0.74f, 0.90f, 0, tableIndex);
            cursor[i] += 0.90f;
        }
    }

    private static int DesignedColumn(float[] cursor, int[] order, int design, int serial, int phase)
    {
        int mode = (design + phase * 3) % 4;
        if (mode == 0) return LowestColumn(cursor, order);
        if (mode == 1) return order[serial % order.Length];
        if (mode == 2) return order[(serial * 2 + design / 10) % order.Length];

        int a = order[serial % order.Length];
        int b = order[(serial + 1) % order.Length];
        return cursor[a] <= cursor[b] ? a : b;
    }

    private static int LowestColumn(float[] cursor, int[] order)
    {
        int best = order[0];
        for (int oi = 1; oi < order.Length; oi++)
        {
            int candidate = order[oi];
            if (cursor[candidate] < cursor[best] - 0.0001f) best = candidate;
        }
        return best;
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
        float[] w = new float[C];
        for (int i = 0; i < C; i++)
        {
            float p = C <= 1 ? 0.5f : i / (float)(C - 1);
            float centre = 1f - Mathf.Abs(p * 2f - 1f);
            float edge = 1f - centre;
            switch (style)
            {
                case 0: w[i] = 0.80f + ((i & 1) == 0 ? 0.70f : 0.15f); break; // battlement
                case 1: w[i] = 0.40f + 1.90f * centre; break;               // pyramid
                case 2: w[i] = 0.45f + 1.75f * edge; break;                 // gate / U
                case 3: w[i] = 0.45f + 1.80f * p; break;                    // rising stairs
                case 4: w[i] = 0.45f + 1.80f * (1f - p); break;             // falling stairs
                case 5:
                {
                    float twin = 1f - Mathf.Min(Mathf.Abs(p - 0.25f), Mathf.Abs(p - 0.75f)) / 0.25f;
                    w[i] = 0.45f + 1.75f * Mathf.Clamp01(twin); break;       // twin peaks
                }
                case 6: w[i] = 0.55f + 1.55f * edge + 0.30f * (i & 1); break; // split bunker
                case 7: w[i] = 0.75f + ((i + 1) % 3 == 0 ? 1.25f : 0.25f); break; // watchtowers
                case 8: w[i] = 0.40f + 2.10f * (1f - p) * (1f - p); break;  // left keep
                case 9: w[i] = 0.40f + 2.10f * p * p; break;                // right keep
                case 10: w[i] = 0.65f + 1.20f * Mathf.Abs(Mathf.Cos(p * Mathf.PI * 2f)); break;
                case 11: w[i] = 0.55f + 1.40f * Mathf.Abs(Mathf.Cos(p * Mathf.PI * 3f)); break;
                case 12: w[i] = 0.45f + 1.25f * p + ((i & 1) == 0 ? 0.55f : 0f); break;
                case 13: w[i] = 0.45f + 1.25f * (1f - p) + ((i & 1) == 1 ? 0.55f : 0f); break;
                case 14: w[i] = 0.55f + (p >= 0.20f && p <= 0.80f ? 1.45f : 0.20f); break; // mesa
                case 15: w[i] = 0.40f + 2.35f * centre * centre * centre; break; // needle
                case 16:
                {
                    float split = 1f - Mathf.Min(Mathf.Abs(p - 0.18f), Mathf.Abs(p - 0.82f)) / 0.32f;
                    w[i] = 0.50f + 1.65f * Mathf.Clamp01(split); break;
                }
                case 17: w[i] = 0.70f + 1.10f * (0.5f + 0.5f * Mathf.Sin(p * Mathf.PI * 2f)); break;
                case 18: w[i] = 0.55f + 0.55f * Mathf.Floor(p * 3.99f); break; // ziggurat steps
                default: w[i] = 0.55f + ((i * 7 + C * 3) % 5) * 0.34f; break; // city skyline
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
        int[] tieOrder = OrderForStyle(C, style, 2);
        for (int r = 0; r < rem; r++)
        {
            int bi = tieOrder[0];
            for (int oi = 1; oi < C; oi++)
            {
                int candidate = tieOrder[oi];
                if (frac[candidate] > frac[bi] + 0.0001f) bi = candidate;
            }
            h[bi]++; frac[bi] = -1f;
        }
        return h;
    }

    private static int TakeBeamVariant(ref int longLeft, ref int flatLeft, int index, int styleSeed)
    {
        bool takeLong = ((index + styleSeed) & 1) == 0;
        if (takeLong && longLeft > 0) { longLeft--; return LONG_BOX; }
        if (!takeLong && flatLeft > 0) { flatLeft--; return LONG_BOX2; }
        if (longLeft > 0) { longLeft--; return LONG_BOX; }
        flatLeft--; return LONG_BOX2;
    }

    private static int HighestColumn(float[] cursor, float[] x, int style)
    {
        int best = 0;
        for (int i = 1; i < cursor.Length; i++)
        {
            bool higher = cursor[i] > cursor[best] + 0.0001f;
            bool tied = Mathf.Abs(cursor[i] - cursor[best]) <= 0.0001f;
            bool preferred = (style & 1) == 0 ? Mathf.Abs(x[i]) < Mathf.Abs(x[best]) : x[i] > x[best];
            if (higher || (tied && preferred)) best = i;
        }
        return best;
    }

    private static int[] AdjacentOrder(int C, int style)
    {
        int count = Mathf.Max(1, C - 1);
        int[] result = new int[count];
        int[] columns = OrderForStyle(count, style, 4);
        for (int i = 0; i < count; i++) result[i] = columns[i];
        return result;
    }

    private static int[] InteriorOrder(int C, int style)
    {
        int[] candidates = InteriorColumns(C);
        int[] order = OrderForStyle(candidates.Length, style, 6);
        int[] result = new int[candidates.Length];
        for (int i = 0; i < result.Length; i++) result[i] = candidates[order[i]];
        return result;
    }

    private static int[] OrderForStyle(int C, int style, int phase)
    {
        if (C <= 0) return new int[0];
        int mode = (style + phase) % 6;
        if (mode == 0) return OrderCenterFirst(C);
        if (mode == 1) return OrderOuterFirst(C);

        int[] result = new int[C];
        if (mode == 2)
        {
            for (int i = 0; i < C; i++) result[i] = i;
        }
        else if (mode == 3)
        {
            for (int i = 0; i < C; i++) result[i] = C - 1 - i;
        }
        else if (mode == 4)
        {
            int at = 0;
            for (int i = 0; i < C; i += 2) result[at++] = i;
            for (int i = 1; i < C; i += 2) result[at++] = i;
        }
        else
        {
            int at = 0;
            for (int i = C - 1 - ((C - 1) & 1); i >= 0; i -= 2) result[at++] = i;
            for (int i = C - 2 + ((C - 1) & 1); i >= 0; i -= 2) result[at++] = i;
        }
        return result;
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
