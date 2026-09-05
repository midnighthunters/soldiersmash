using System;
using System.Collections.Generic;
using UnityEngine;

public static partial class WarfestLevelCatalog
{
    // Modular component dimensions
    public const float C_CUBE = 0.55f;
    public const float C_BEAM_W = 0.95f;
    public const float C_BEAM_H = 0.30f;
    public const float C_PLANK_W = 1.10f;
    public const float C_PLANK_H = 0.22f;
    public const float C_TURRET_W = 0.40f;
    public const float C_TURRET_H = 0.50f;
    public const float C_SOLDIER_W = 0.40f;
    public const float C_SOLDIER_H = 0.55f;
    public const float C_CAN_W = 0.40f;
    public const float C_CAN_H = 0.50f;
    public const float C_BOMB_W = 0.45f;
    public const float C_BOMB_H = 0.50f;
    public const float C_KING_W = 0.70f;
    public const float C_KING_H = 0.85f;

    private static void AddB(List<ModelBlockSpec> b, float x, float y, int variant, float w, float h, int layer = 0, int table = 0, float rot = 0f)
    {
        b.Add(new ModelBlockSpec(x, y, variant, w, h, layer, table, rot));
    }

    private static void AddCube(List<ModelBlockSpec> b, float x, float y, bool heavy = false, int table = 0, int layer = 0)
    {
        AddB(b, x, y, heavy ? BOX2 : BOX, C_CUBE, C_CUBE, layer, table);
    }

    private static void AddBeam(List<ModelBlockSpec> b, float x, float y, int table = 0, int layer = 0, float rot = 0f)
    {
        AddB(b, x, y, LONG_BOX, C_BEAM_W, C_BEAM_H, layer, table, rot);
    }

    private static void AddPlank(List<ModelBlockSpec> b, float x, float y, int table = 0, int layer = 0, float rot = 0f)
    {
        AddB(b, x, y, LONG_BOX2, C_PLANK_W, C_PLANK_H, layer, table, rot);
    }

    private static void AddTurret(List<ModelBlockSpec> b, float x, float y, int table = 0, int layer = 0, float rot = 0f)
    {
        AddB(b, x, y, BOX3, C_TURRET_W, C_TURRET_H, layer, table, rot);
    }

    private static void AddSoldier(List<ModelBlockSpec> b, float x, float y, int table = 0, int layer = 0)
    {
        AddB(b, x, y, SOLDIER, C_SOLDIER_W, C_SOLDIER_H, layer, table);
    }

    private static void AddCan(List<ModelBlockSpec> b, float x, float y, int table = 0, int layer = 0)
    {
        AddB(b, x, y, CANNISTER, C_CAN_W, C_CAN_H, layer, table);
    }

    private static void AddBomb(List<ModelBlockSpec> b, float x, float y, int table = 0, int layer = 0)
    {
        AddB(b, x, y, BOMB, C_BOMB_W, C_BOMB_H, layer, table);
    }

    private static void AddKing(List<ModelBlockSpec> b, float x, float y, int table = 0, int layer = 0)
    {
        AddB(b, x, y, KING, C_KING_W, C_KING_H, layer, table);
    }

    private static void AddMirroredCube(List<ModelBlockSpec> b, float x, float y, bool heavy = false, int table = 0, int layer = 0)
    {
        AddCube(b, -x, y, heavy, table, layer);
        AddCube(b, x, y, heavy, table, layer);
    }

    private static void AddMirroredSoldier(List<ModelBlockSpec> b, float x, float y, int table = 0, int layer = 0)
    {
        AddSoldier(b, -x, y, table, layer);
        AddSoldier(b, x, y, table, layer);
    }

    private static void AddMirroredBomb(List<ModelBlockSpec> b, float x, float y, int table = 0, int layer = 0)
    {
        AddBomb(b, -x, y, table, layer);
        AddBomb(b, x, y, table, layer);
    }

    private static void AddMirroredCan(List<ModelBlockSpec> b, float x, float y, int table = 0, int layer = 0)
    {
        AddCan(b, -x, y, table, layer);
        AddCan(b, x, y, table, layer);
    }

    private static void AddMirroredTurret(List<ModelBlockSpec> b, float x, float y, int table = 0, int layer = 0)
    {
        AddTurret(b, -x, y, table, layer);
        AddTurret(b, x, y, table, layer);
    }

    private static void AddMirroredBeam(List<ModelBlockSpec> b, float x, float y, int table = 0, int layer = 0)
    {
        AddBeam(b, -x, y, table, layer);
        AddBeam(b, x, y, table, layer);
    }

    private static void AddMirroredPlank(List<ModelBlockSpec> b, float x, float y, int table = 0, int layer = 0)
    {
        AddPlank(b, -x, y, table, layer);
        AddPlank(b, x, y, table, layer);
    }

    private static void AddBaseRow(List<ModelBlockSpec> b, int halfCount, float y, bool heavy = true, float pitch = 0.55f, int table = 0, int layer = 0)
    {
        AddCube(b, 0f, y, heavy, table, layer);
        for (int i = 1; i <= halfCount; i++)
        {
            AddMirroredCube(b, i * pitch, y, heavy, table, layer);
        }
    }

    private static void AddMirroredTower(List<ModelBlockSpec> b, float x, float baseY, int height, bool heavyBase = false, int table = 0, int layer = 0)
    {
        for (int r = 0; r < height; r++)
        {
            AddMirroredCube(b, x, baseY + r * C_CUBE, (r == 0 && heavyBase), table, layer);
        }
    }

    // 100 Authored Level Names for Levels 101 to 200
    private static readonly string[] CampaignNames101to200 =
    {
        // 101-110: Iconic Shapes
        "Imperial Crown", "Aegis Shield", "Giant X-Fortress", "Arrowhead Citadel", "Royal Diamond",
        "Tactical Hourglass", "Lightning Fortress", "Star Bastion Redoubt", "Double Tower Bridge", "Colossal Gate",

        // 111-120: Fortifications
        "Castle Gatehouse", "Bastion Redoubt", "Curtain Defense Wall", "Grand Watchtower", "Hardened Army Bunker",
        "Perimeter Barricade", "Outpost Guard Tower", "Citadel Barbican", "Double Trench Fort", "Star Bastion Citadel",

        // 121-130: Military Machines
        "Heavy Battle Tank", "Delta Jet Interceptor", "War Battleship", "Combat Helicopter", "Attack Submarine",
        "ICBM Launch Complex", "Heavy Siege Battery", "Radar Warning Station", "Rocket Artillery MLRS", "Super Aircraft Carrier",

        // 131-140: Architectural Structures
        "Classical Temple", "Five-Tier Pagoda", "Royal Imperial Palace", "Step Pyramid Ziggurat", "Triumphal Monument Arch",
        "Coastal Lighthouse", "Cable Suspension Bridge", "Obelisk War Memorial", "Gothic Twin Cathedrals", "Mountain Cliff Fortress",

        // 141-150: Geometric Art & Level 150 Milestone
        "Fibonacci Spiral Mandala", "Nested Diamond Matrix", "Interlocking Stairways", "Colossal Chevron Meander", "Courtyard Diamond Palace",
        "Double Chevron Fortress", "Layered Triforce Citadel", "Checkerboard Fortress", "Templar Cross Stronghold", "Grand Military Stronghold",

        // 151-160: Fragile Engineering
        "Monolith on a Pin", "Stilt Walkway Citadel", "Knife-Edge Truss Bridge", "Inverted Sky Citadel", "Balanced Cantilever Wings",
        "Stepped Overhang Citadel", "Hourglass Battle-Station", "Top-Heavy Warhead Tower", "Split-Support A-Frame", "Symmetrical Domino Array",

        // 161-170: Multi-Structure Scenes
        "Symmetrical Twin Forts", "Armored Convoy & Gates", "Roman Aqueduct Span", "Imperial Keep & Pillboxes", "Naval Fleet & Battery",
        "Radar Array & Command Post", "Three Symmetrical Castles", "Imperial Spire & Redoubts", "Dual Checkpoint Bastions", "Heavy Mortar Redoubts",

        // 171-180: Advanced Asymmetry
        "Symmetrical Ruined Fortress", "Twin Sheared Spire", "Counterbalanced Leaning Towers", "Symmetrical Pylon Forts", "Symmetrical Stepped Escarpment",
        "Crashed Gunship Perimeter", "Fractured Suspension Bridge", "Symmetrical Monolith Temples", "Stepped Terraced Ziggurat", "Symmetrical Fault Line Forts",

        // 181-190: Advanced Structural Puzzles
        "The Keystone Vault", "Pendulum Counterweight Frame", "Symmetrical 4-Stage Fuse", "Shielded King Sanctum", "Domino Hair-Trigger Bastion",
        "Symmetrical Dual Chambers", "Symmetrical Guillotine Beam", "Triple Interlocking Arches", "Twin Blast Chimneys", "Symmetrical Jenga Bastion",

        // 191-200: Master Levels & Finale
        "Dreadnought Super-Battleship", "Royal Grand Castle", "Orbital Rocket Complex", "Iron Mountain Citadel", "Fleet Aircraft Carrier",
        "Triumphal Imperial Monument", "Land Dreadnought Crawler", "Triple Imperial Stronghold", "High Command War Headquarters", "Grand Finale Stronghold"
    };

    public static string CampaignName101to200(int zeroBasedLevel)
    {
        int idx = Mathf.Clamp(zeroBasedLevel - 100, 0, 99);
        return CampaignNames101to200[idx];
    }

    public static int CampaignTableCount101to200(int zeroBasedLevel)
    {
        int lv = zeroBasedLevel + 1; // 101..200
        // 3-Table levels
        if (lv == 150 || lv == 164 || lv == 167 || lv == 168 || lv == 198 || lv == 199 || lv == 200)
            return 3;

        // 2-Table levels
        if (lv == 109 || lv == 118 || lv == 119 || lv == 137 || lv == 139 || lv == 140 ||
            lv == 161 || lv == 162 || lv == 163 || lv == 165 || lv == 166 || lv == 169 || lv == 170 ||
            lv == 174 || lv == 177 || lv == 178 || lv == 180 || lv == 193 || lv == 194)
            return 2;

        return 1;
    }

    private static readonly Dictionary<int, int> AuthoredBlockCounts101to200 = new Dictionary<int, int>();

    public static int CampaignBlockCount101to200(int zeroBasedLevel)
    {
        if (AuthoredBlockCounts101to200.TryGetValue(zeroBasedLevel, out int count))
            return count;

        var tempBlocks = new List<ModelBlockSpec>();
        var tempTables = new List<ModelTableSpec>();
        BuildCampaignLevel101To200(zeroBasedLevel, tempBlocks, tempTables);
        AuthoredBlockCounts101to200[zeroBasedLevel] = tempBlocks.Count;
        return tempBlocks.Count;
    }

    public static void BuildCampaignLevel101To200(int zeroBasedLevel, List<ModelBlockSpec> blocks, List<ModelTableSpec> tables)
    {
        int lv = zeroBasedLevel + 1; // 101..200
        int tableCount = CampaignTableCount101to200(zeroBasedLevel);

        if (tableCount == 3)
        {
            // Stepped 3-table staging: Center raised, Left & Right lower forward bastions
            tables.Add(new ModelTableSpec(0f, 2.30f, 0.35f, RearLayerZ, 0f));
            tables.Add(new ModelTableSpec(-1.22f, 2.10f, -0.65f, FrontLayerZ, 14f));
            tables.Add(new ModelTableSpec(1.22f, 2.10f, -0.65f, FrontLayerZ, -14f));
        }
        else if (tableCount == 2)
        {
            // Twin table staging: Left front, Right rear
            tables.Add(new ModelTableSpec(-TwinTableCenterOffset, TwinTableWidth, TwinFrontTop, FrontLayerZ, TwinFrontYaw));
            tables.Add(new ModelTableSpec(TwinTableCenterOffset, TwinTableWidth, TwinRearTop, RearLayerZ, TwinRearYaw));
        }
        else
        {
            // Single pedestal table
            tables.Add(new ModelTableSpec(0f, TargetTableWidth, BaseTableTop, FrontLayerZ, 0f));
        }

        if (lv <= 150)
        {
            BuildCampaign101to150(lv, blocks);
        }
        else
        {
            BuildCampaign151to200(lv, blocks);
        }
    }
}
