using System.Collections.Generic;
using UnityEngine;

// Hand-authored Levels 4-14, translated from the supplied carnival block references into
// Warfest's military crate palette. This partial keeps the reference layouts isolated from the
// remaining procedural campaign and gives every piece an explicit fitted width and height.
public static partial class WarfestLevelCatalog
{
    private const float ReferenceCell = 0.48f;
    private const float ReferenceSquare = 0.47f;

    private static bool TryBuildReferenceLayout(
        int zeroBasedLevel,
        List<ModelBlockSpec> blocks,
        List<ModelTableSpec> tables)
    {
        if (zeroBasedLevel < 3 || zeroBasedLevel > 13) return false;

        if (zeroBasedLevel == 3)
        {
            // Level 4: Twin outpost watchtowers with stationed sentries on separate pedestals.
            tables.Add(new ModelTableSpec(-1.20f, 2.22f, -0.44f, FrontLayerZ, 3f));
            tables.Add(new ModelTableSpec(1.20f, 2.22f, -0.44f, FrontLayerZ, -3f));
            BuildReferencePillarDrill(blocks);
            return true;
        }

        tables.Add(new ModelTableSpec(0f, TargetTableWidth, -0.35f, FrontLayerZ, 0f));
        switch (zeroBasedLevel)
        {
            case 4: BuildReferenceCheckerboard(blocks); break;       // Level 5 / reference 2
            case 5: BuildReferenceCrossWall(blocks); break;          // Level 6 / reference 3
            case 6: BuildReferenceCrenellatedFort(blocks); break;    // Level 7 / reference 4
            case 7: BuildReferenceTowerBridge(blocks); break;        // Level 8 / reference 5
            case 8: BuildReferenceSolidWall(blocks); break;          // Level 9 / reference 6
            case 9: BuildReferenceTwinCanisterTowers(blocks); break; // Level 10 / reference 7
            case 10: BuildReferenceColonnade(blocks); break;         // Level 11 / reference 8
            case 11: BuildReferenceNestedRings(blocks); break;       // Level 12 / reference 9
            case 12: BuildReferenceThroneFort(blocks, false); break; // Level 13 / reference 10
            case 13: BuildReferenceThroneFort(blocks, true); break;  // Level 14 / grand finale
        }
        return true;
    }

    private static int ReferenceLayoutBlockCount(int zeroBasedLevel)
    {
        switch (zeroBasedLevel)
        {
            case 3: return 24;
            case 4: return 49;
            case 5: return 36;
            case 6: return 30;
            case 7: return 31;
            case 8: return 25;
            case 9: return 21;
            case 10: return 38;
            case 11: return 49;
            case 12: return 32;
            case 13: return 49;
            default: return -1;
        }
    }

    private static void AddReferencePiece(
        List<ModelBlockSpec> blocks,
        float x,
        float y,
        int variant,
        float width = ReferenceSquare,
        float height = ReferenceSquare,
        int tableIndex = 0,
        float rotation = 0f)
    {
        AddModel(blocks, x, y, variant, width, height, 0, tableIndex, rotation);
    }

    private static void AddReferenceGrid(
        List<ModelBlockSpec> blocks,
        float centerX,
        int columns,
        int rows,
        float baseY,
        int evenVariant,
        int oddVariant,
        int tableIndex = 0)
    {
        float left = centerX - (columns - 1) * ReferenceCell * 0.5f;
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                int variant = ((row + col) & 1) == 0 ? evenVariant : oddVariant;
                AddReferencePiece(
                    blocks,
                    left + col * ReferenceCell,
                    baseY + row * ReferenceCell,
                    variant,
                    ReferenceSquare,
                    ReferenceSquare,
                    tableIndex);
            }
        }
    }

    private static void BuildReferencePillarDrill(List<ModelBlockSpec> blocks)
    {
        // Level 4: Twin outpost watchtowers (12 blocks per table, 24 blocks total).
        float[] tableCenters = { -1.20f, 1.20f };
        for (int t = 0; t < 2; t++)
        {
            float cx = tableCenters[t];
            float y = 0f;
            // 2 pillar columns
            float colOffset = 0.28f;
            for (int r = 0; r < 3; r++)
            {
                int variant = r == 0 ? BOX2 : BOX;
                AddReferencePiece(blocks, cx - colOffset, y, variant, 0.54f, 0.54f, t);
                AddReferencePiece(blocks, cx + colOffset, y, variant, 0.54f, 0.54f, t);
                y += 0.54f;
            }
            // Lintel bridge
            AddReferencePiece(blocks, cx, y, LONG_BOX2, 1.24f, 0.20f, t);
            y += 0.20f;
            // Parapet sandbags flanking, cannister in center
            AddReferencePiece(blocks, cx - 0.32f, y, SANDBAG, 0.50f, 0.45f, t);
            AddReferencePiece(blocks, cx + 0.32f, y, SANDBAG, 0.50f, 0.45f, t);
            AddReferencePiece(blocks, cx, y, CANNISTER, 0.44f, 0.56f, t);
            y += 0.56f;
            // Watchtower roof platform
            AddReferencePiece(blocks, cx, y, LONG_BOX, 0.90f, 0.25f, t);
            y += 0.25f;
            // Stationed sentry soldier
            AddReferencePiece(blocks, cx, y, SOLDIER, 0.572f, 0.806f, t);
        }
    }

    private static void BuildReferenceCheckerboard(List<ModelBlockSpec> blocks)
    {
        // Level 5: Stepped military barricade fortress (49 blocks, 3 soldiers, 3 bombs).
        float[] cx = { -1.30f, -0.65f, 0f, 0.65f, 1.30f };
        // Row 0: Heavy foundation (5)
        for (int i = 0; i < 5; i++) AddReferencePiece(blocks, cx[i], 0f, BOX2, 0.54f, 0.54f);
        // Row 1: Structural crates (5)
        for (int i = 0; i < 5; i++) AddReferencePiece(blocks, cx[i], 0.54f, BOX, 0.54f, 0.54f);
        // Row 2: Defense tier (5)
        for (int i = 0; i < 5; i++) AddReferencePiece(blocks, cx[i], 1.08f, (i % 2 == 0) ? BOX2 : BOX, 0.54f, 0.54f);
        // Row 3: Bomb vaults (3 bombs at cols 0, 2, 4; 2 pillars at cols 1, 3) (5)
        AddReferencePiece(blocks, cx[0], 1.62f, BOMB, 0.50f, 0.56f);
        AddReferencePiece(blocks, cx[1], 1.62f, BOX2, 0.54f, 0.54f);
        AddReferencePiece(blocks, cx[2], 1.62f, BOMB, 0.50f, 0.56f);
        AddReferencePiece(blocks, cx[3], 1.62f, BOX2, 0.54f, 0.54f);
        AddReferencePiece(blocks, cx[4], 1.62f, BOMB, 0.50f, 0.56f);
        // Row 4: Vault roof lintels (2)
        AddReferencePiece(blocks, -0.65f, 2.18f, LONG_BOX, 1.45f, 0.28f);
        AddReferencePiece(blocks, 0.65f, 2.18f, LONG_BOX, 1.45f, 0.28f);
        // Row 5: Mid-tier parapet (5)
        for (int i = 0; i < 5; i++) AddReferencePiece(blocks, cx[i], 2.46f, BOX, 0.54f, 0.54f);
        // Row 6: Inner towers (cols 1, 2, 3) (6)
        AddReferencePiece(blocks, cx[1], 3.00f, BOX, 0.54f, 0.54f);
        AddReferencePiece(blocks, cx[2], 3.00f, BOX, 0.54f, 0.54f);
        AddReferencePiece(blocks, cx[3], 3.00f, BOX, 0.54f, 0.54f);
        AddReferencePiece(blocks, cx[1], 3.54f, SANDBAG, 0.52f, 0.45f);
        AddReferencePiece(blocks, cx[2], 3.54f, SANDBAG, 0.52f, 0.45f);
        AddReferencePiece(blocks, cx[3], 3.54f, SANDBAG, 0.52f, 0.45f);
        // Outer watchtowers (cols 0, 4) (6)
        AddReferencePiece(blocks, cx[0], 3.00f, SANDBAG, 0.52f, 0.45f);
        AddReferencePiece(blocks, cx[4], 3.00f, SANDBAG, 0.52f, 0.45f);
        AddReferencePiece(blocks, cx[0], 3.45f, CANNISTER, 0.44f, 0.56f);
        AddReferencePiece(blocks, cx[4], 3.45f, CANNISTER, 0.44f, 0.56f);
        AddReferencePiece(blocks, cx[0], 4.01f, SOLDIER, 0.572f, 0.806f);
        AddReferencePiece(blocks, cx[4], 4.01f, SOLDIER, 0.572f, 0.806f);
        // Center command tower (col 2) (4)
        AddReferencePiece(blocks, cx[2] - 0.24f, 3.99f, CANNISTER, 0.44f, 0.56f);
        AddReferencePiece(blocks, cx[2] + 0.24f, 3.99f, CANNISTER, 0.44f, 0.56f);
        AddReferencePiece(blocks, cx[2], 4.55f, LONG_BOX2, 1.10f, 0.20f);
        AddReferencePiece(blocks, cx[2], 4.75f, SOLDIER, 0.572f, 0.806f);
        // Additional defensive ramparts & ammo caches (6)
        AddReferencePiece(blocks, cx[1] - 0.22f, 3.99f, SANDBAG, 0.48f, 0.45f);
        AddReferencePiece(blocks, cx[3] + 0.22f, 3.99f, SANDBAG, 0.48f, 0.45f);
        AddReferencePiece(blocks, cx[1], 4.44f, SANDBAG, 0.48f, 0.45f);
        AddReferencePiece(blocks, cx[3], 4.44f, SANDBAG, 0.48f, 0.45f);
        AddReferencePiece(blocks, cx[1], 4.89f, CANNISTER, 0.44f, 0.56f);
        AddReferencePiece(blocks, cx[3], 4.89f, CANNISTER, 0.44f, 0.56f);
    }

    private static void BuildReferenceCrossWall(List<ModelBlockSpec> blocks)
    {
        // Level 6: Twin-arch military aqueduct bridge (36 blocks, 4 soldiers, 2 bombs).
        float[] pillarX = { -1.15f, 0f, 1.15f };
        float pWidth = 0.28f;
        // 3 pillars, each 2 columns wide x 3 rows high = 18 blocks
        for (int p = 0; p < 3; p++)
        {
            float px = pillarX[p];
            for (int r = 0; r < 3; r++)
            {
                int variant = r == 0 ? BOX2 : BOX;
                AddReferencePiece(blocks, px - pWidth, r * 0.54f, variant, 0.52f, 0.54f);
                AddReferencePiece(blocks, px + pWidth, r * 0.54f, variant, 0.52f, 0.54f);
            }
        }
        // 2 Bomb caches inside the arches = 2 blocks
        AddReferencePiece(blocks, -0.575f, 0f, BOMB, 0.50f, 0.56f);
        AddReferencePiece(blocks, 0.575f, 0f, BOMB, 0.50f, 0.56f);
        // 2 Arch bridge spans = 2 blocks
        AddReferencePiece(blocks, -0.575f, 1.62f, LONG_BOX2, 1.25f, 0.20f);
        AddReferencePiece(blocks, 0.575f, 1.62f, LONG_BOX2, 1.25f, 0.20f);
        // Bridge deck roadway = 6 blocks
        float[] deckX = { -1.35f, -0.85f, -0.35f, 0.35f, 0.85f, 1.35f };
        for (int i = 0; i < 6; i++) AddReferencePiece(blocks, deckX[i], 1.82f, BOX, 0.48f, 0.50f);
        // Battlements: 4 sandbags = 4 blocks
        AddReferencePiece(blocks, -1.35f, 2.32f, SANDBAG, 0.48f, 0.45f);
        AddReferencePiece(blocks, -0.35f, 2.32f, SANDBAG, 0.48f, 0.45f);
        AddReferencePiece(blocks, 0.35f, 2.32f, SANDBAG, 0.48f, 0.45f);
        AddReferencePiece(blocks, 1.35f, 2.32f, SANDBAG, 0.48f, 0.45f);
        // Stationed bridge guards: 4 soldiers = 4 blocks
        AddReferencePiece(blocks, -0.85f, 2.32f, SOLDIER, 0.572f, 0.806f);
        AddReferencePiece(blocks, 0.85f, 2.32f, SOLDIER, 0.572f, 0.806f);
        AddReferencePiece(blocks, -1.35f, 2.77f, SOLDIER, 0.572f, 0.806f);
        AddReferencePiece(blocks, 1.35f, 2.77f, SOLDIER, 0.572f, 0.806f);
    }

    private static void BuildReferenceCrenellatedFort(List<ModelBlockSpec> blocks)
    {
        // Level 7: Armored Turtle Bastion (30 blocks, 1 turtle, 2 soldiers, 2 bombs).
        float[] colX = { -1.30f, -0.78f, -0.26f, 0.26f, 0.78f, 1.30f };
        // Row 0: Heavy foundation (6)
        for (int i = 0; i < 6; i++) AddReferencePiece(blocks, colX[i], 0f, BOX2, 0.50f, 0.54f);
        // Row 1: Wall tier (6)
        for (int i = 0; i < 6; i++) AddReferencePiece(blocks, colX[i], 0.54f, BOX, 0.50f, 0.54f);
        // Center bunker: 2 bombs inside + 2 sandbags flanking (4)
        AddReferencePiece(blocks, -0.26f, 1.08f, BOMB, 0.50f, 0.56f);
        AddReferencePiece(blocks, 0.26f, 1.08f, BOMB, 0.50f, 0.56f);
        AddReferencePiece(blocks, -0.78f, 1.08f, SANDBAG, 0.50f, 0.45f);
        AddReferencePiece(blocks, 0.78f, 1.08f, SANDBAG, 0.50f, 0.45f);
        // Left tower (x=-1.30): 2 boxes (2)
        AddReferencePiece(blocks, -1.30f, 1.08f, BOX, 0.50f, 0.54f);
        AddReferencePiece(blocks, -1.30f, 1.62f, BOX, 0.50f, 0.54f);
        // Right tower (x=1.30): 2 boxes (2)
        AddReferencePiece(blocks, 1.30f, 1.08f, BOX, 0.50f, 0.54f);
        AddReferencePiece(blocks, 1.30f, 1.62f, BOX, 0.50f, 0.54f);
        // Central bridge lintels over bunker (2)
        AddReferencePiece(blocks, 0f, 1.64f, LONG_BOX, 1.45f, 0.28f);
        AddReferencePiece(blocks, 0f, 1.92f, LONG_BOX2, 1.45f, 0.20f);
        // Armored Turtle on the central bridge = 1 block!
        AddReferencePiece(blocks, 0f, 2.12f, TURTLE, 0.84f, 0.675f);
        // Tower parapets: 2 sandbags on outer towers (2)
        AddReferencePiece(blocks, -1.30f, 2.16f, SANDBAG, 0.50f, 0.45f);
        AddReferencePiece(blocks, 1.30f, 2.16f, SANDBAG, 0.50f, 0.45f);
        // Sentries: 2 soldiers atop outer towers (2)
        AddReferencePiece(blocks, -1.30f, 2.61f, SOLDIER, 0.572f, 0.806f);
        AddReferencePiece(blocks, 1.30f, 2.61f, SOLDIER, 0.572f, 0.806f);
        // Ammo reserves: 2 cannisters on mid ramparts (2)
        AddReferencePiece(blocks, -0.78f, 1.53f, CANNISTER, 0.44f, 0.56f);
        AddReferencePiece(blocks, 0.78f, 1.53f, CANNISTER, 0.44f, 0.56f);
        // Center support block under the bridge (1)
        AddReferencePiece(blocks, 0f, 1.08f, BOX, 0.50f, 0.54f);
    }

    private static void BuildReferenceTowerBridge(List<ModelBlockSpec> blocks)
    {
        // Level 8: Twin Barracks Towers & Skybridge (31 blocks, 3 soldiers, 1 bomb).
        float[] leftCols = { -1.40f, -0.92f };
        float[] rightCols = { 0.92f, 1.40f };
        // Left tower: 2 cols x 4 rows = 8 blocks
        for (int r = 0; r < 4; r++)
        {
            int variant = r < 2 ? BOX2 : BOX;
            AddReferencePiece(blocks, leftCols[0], r * 0.54f, variant, 0.46f, 0.54f);
            AddReferencePiece(blocks, leftCols[1], r * 0.54f, variant, 0.46f, 0.54f);
        }
        // Right tower: 2 cols x 4 rows = 8 blocks
        for (int r = 0; r < 4; r++)
        {
            int variant = r < 2 ? BOX2 : BOX;
            AddReferencePiece(blocks, rightCols[0], r * 0.54f, variant, 0.46f, 0.54f);
            AddReferencePiece(blocks, rightCols[1], r * 0.54f, variant, 0.46f, 0.54f);
        }
        // Center ground gate: 2 sandbags + 1 bomb in middle = 3 blocks
        AddReferencePiece(blocks, -0.45f, 0f, SANDBAG, 0.48f, 0.45f);
        AddReferencePiece(blocks, 0.45f, 0f, SANDBAG, 0.48f, 0.45f);
        AddReferencePiece(blocks, 0f, 0f, BOMB, 0.50f, 0.56f);
        // Skybridge lintels spanning towers = 2 blocks
        AddReferencePiece(blocks, 0f, 1.80f, LONG_BOX, 1.60f, 0.28f);
        AddReferencePiece(blocks, 0f, 2.08f, LONG_BOX2, 1.60f, 0.20f);
        // Skybridge sentry post: 2 sandbags + 1 soldier = 3 blocks
        AddReferencePiece(blocks, -0.45f, 2.28f, SANDBAG, 0.46f, 0.45f);
        AddReferencePiece(blocks, 0.45f, 2.28f, SANDBAG, 0.46f, 0.45f);
        AddReferencePiece(blocks, 0f, 2.28f, SOLDIER, 0.572f, 0.806f);
        // Left tower crown: 1 sandbag + 1 cannister + 1 soldier = 3 blocks
        AddReferencePiece(blocks, -1.40f, 2.16f, SANDBAG, 0.46f, 0.45f);
        AddReferencePiece(blocks, -0.92f, 2.16f, CANNISTER, 0.44f, 0.56f);
        AddReferencePiece(blocks, -1.16f, 2.72f, SOLDIER, 0.572f, 0.806f);
        // Right tower crown: 1 sandbag + 1 cannister + 1 soldier = 3 blocks
        AddReferencePiece(blocks, 1.40f, 2.16f, SANDBAG, 0.46f, 0.45f);
        AddReferencePiece(blocks, 0.92f, 2.16f, CANNISTER, 0.44f, 0.56f);
        AddReferencePiece(blocks, 1.16f, 2.72f, SOLDIER, 0.572f, 0.806f);
        // Center crown capstone = 1 block
        AddReferencePiece(blocks, 0f, 3.086f, BOX, 0.48f, 0.48f);
    }

    private static void BuildReferenceSolidWall(List<ModelBlockSpec> blocks)
    {
        // Level 9: Explosive Ordnance Depot (25 blocks, 2 soldiers, 4 bombs).
        float[] cx = { -1.10f, -0.55f, 0f, 0.55f, 1.10f };
        // Row 0: Heavy foundation = 5 blocks
        for (int i = 0; i < 5; i++) AddReferencePiece(blocks, cx[i], 0f, BOX2, 0.52f, 0.54f);
        // Lower munitions vault: 2 bombs + side/center pillars = 5 blocks
        AddReferencePiece(blocks, -0.55f, 0.54f, BOMB, 0.50f, 0.56f);
        AddReferencePiece(blocks, 0.55f, 0.54f, BOMB, 0.50f, 0.56f);
        AddReferencePiece(blocks, -1.10f, 0.54f, BOX2, 0.52f, 0.54f);
        AddReferencePiece(blocks, 1.10f, 0.54f, BOX2, 0.52f, 0.54f);
        AddReferencePiece(blocks, 0f, 0.54f, BOX, 0.52f, 0.54f);
        // Intermediate lintel beam over lower vault = 1 block
        AddReferencePiece(blocks, 0f, 1.10f, LONG_BOX, 2.50f, 0.28f);
        // Upper munitions shelf: 2 bombs + side pillars = 4 blocks
        AddReferencePiece(blocks, -0.55f, 1.38f, BOMB, 0.50f, 0.56f);
        AddReferencePiece(blocks, 0.55f, 1.38f, BOMB, 0.50f, 0.56f);
        AddReferencePiece(blocks, -1.10f, 1.38f, BOX, 0.52f, 0.54f);
        AddReferencePiece(blocks, 1.10f, 1.38f, BOX, 0.52f, 0.54f);
        // Upper roof lintel = 1 block
        AddReferencePiece(blocks, 0f, 1.94f, LONG_BOX2, 2.50f, 0.20f);
        // Parapet sandbags = 4 blocks
        AddReferencePiece(blocks, -1.10f, 2.14f, SANDBAG, 0.50f, 0.45f);
        AddReferencePiece(blocks, -0.40f, 2.14f, SANDBAG, 0.50f, 0.45f);
        AddReferencePiece(blocks, 0.40f, 2.14f, SANDBAG, 0.50f, 0.45f);
        AddReferencePiece(blocks, 1.10f, 2.14f, SANDBAG, 0.50f, 0.45f);
        // Sentry soldiers on parapet = 2 blocks
        AddReferencePiece(blocks, -0.80f, 2.59f, SOLDIER, 0.572f, 0.806f);
        AddReferencePiece(blocks, 0.80f, 2.59f, SOLDIER, 0.572f, 0.806f);
        // Ammo reserves & capstones = 3 blocks
        AddReferencePiece(blocks, 0f, 2.14f, CANNISTER, 0.44f, 0.56f);
        AddReferencePiece(blocks, 0f, 2.70f, BOX, 0.48f, 0.48f);
        AddReferencePiece(blocks, 0f, 3.18f, BOX, 0.48f, 0.48f);
    }

    private static void BuildReferenceTwinCanisterTowers(List<ModelBlockSpec> blocks)
    {
        // Level 10: Royal Outpost (21 blocks, 1 king, 2 soldiers).
        // Ground table beam = 1 block
        AddReferencePiece(blocks, 0f, 0f, LONG_BOX2, 3.20f, 0.20f);
        // Left watchtower: 6 cannisters + 1 sandbag + 1 soldier = 8 blocks
        float lx = -1.15f;
        for (int r = 0; r < 3; r++)
        {
            AddReferencePiece(blocks, lx - 0.24f, 0.20f + r * 0.56f, CANNISTER, 0.44f, 0.56f);
            AddReferencePiece(blocks, lx + 0.24f, 0.20f + r * 0.56f, CANNISTER, 0.44f, 0.56f);
        }
        AddReferencePiece(blocks, lx, 0.20f + 3 * 0.56f, SANDBAG, 0.70f, 0.45f);
        AddReferencePiece(blocks, lx, 0.20f + 3 * 0.56f + 0.45f, SOLDIER, 0.572f, 0.806f);

        // Right watchtower: 6 cannisters + 1 sandbag + 1 soldier = 8 blocks
        float rx = 1.15f;
        for (int r = 0; r < 3; r++)
        {
            AddReferencePiece(blocks, rx - 0.24f, 0.20f + r * 0.56f, CANNISTER, 0.44f, 0.56f);
            AddReferencePiece(blocks, rx + 0.24f, 0.20f + r * 0.56f, CANNISTER, 0.44f, 0.56f);
        }
        AddReferencePiece(blocks, rx, 0.20f + 3 * 0.56f, SANDBAG, 0.70f, 0.45f);
        AddReferencePiece(blocks, rx, 0.20f + 3 * 0.56f + 0.45f, SOLDIER, 0.572f, 0.806f);

        // Center throne dais: 2 crates + 1 King + 1 canopy = 4 blocks
        AddReferencePiece(blocks, -0.28f, 0.20f, BOX2, 0.54f, 0.54f);
        AddReferencePiece(blocks, 0.28f, 0.20f, BOX2, 0.54f, 0.54f);
        AddReferencePiece(blocks, 0f, 0.74f, KING, 0.78f, 0.96f);
        AddReferencePiece(blocks, 0f, 1.70f, LONG_BOX2, 1.20f, 0.20f);
    }

    private static void BuildReferenceColonnade(List<ModelBlockSpec> blocks)
    {
        // Level 11: Fortified Imperial Colonnade (38 blocks, 1 turtle, 4 soldiers).
        float[] cx = { -1.36f, -0.68f, 0f, 0.68f, 1.36f };
        // 5 columns x 3 rows = 15 blocks
        for (int i = 0; i < 5; i++)
        {
            AddReferencePiece(blocks, cx[i], 0f, BOX2, 0.50f, 0.54f);
            AddReferencePiece(blocks, cx[i], 0.54f, BOX, 0.50f, 0.54f);
            AddReferencePiece(blocks, cx[i], 1.08f, BOX, 0.50f, 0.54f);
        }
        // 3 Connecting lintels = 3 blocks
        AddReferencePiece(blocks, -1.02f, 1.62f, LONG_BOX2, 1.25f, 0.20f);
        AddReferencePiece(blocks, 0f, 1.62f, LONG_BOX, 1.45f, 0.28f);
        AddReferencePiece(blocks, 1.02f, 1.62f, LONG_BOX2, 1.25f, 0.20f);
        // Upper row of crates on pillars = 5 blocks
        for (int i = 0; i < 5; i++)
        {
            float y = i == 2 ? 1.90f : 1.82f;
            AddReferencePiece(blocks, cx[i], y, BOX, 0.50f, 0.54f);
        }
        // Parapet sandbags = 6 blocks
        AddReferencePiece(blocks, cx[0], 2.36f, SANDBAG, 0.50f, 0.45f);
        AddReferencePiece(blocks, cx[1], 2.36f, SANDBAG, 0.50f, 0.45f);
        AddReferencePiece(blocks, cx[3], 2.36f, SANDBAG, 0.50f, 0.45f);
        AddReferencePiece(blocks, cx[4], 2.36f, SANDBAG, 0.50f, 0.45f);
        AddReferencePiece(blocks, -0.34f, 2.44f, SANDBAG, 0.46f, 0.45f);
        AddReferencePiece(blocks, 0.34f, 2.44f, SANDBAG, 0.46f, 0.45f);
        // Ammo reserves = 4 cannisters
        AddReferencePiece(blocks, cx[0], 2.81f, CANNISTER, 0.44f, 0.56f);
        AddReferencePiece(blocks, cx[4], 2.81f, CANNISTER, 0.44f, 0.56f);
        AddReferencePiece(blocks, -0.34f, 2.89f, CANNISTER, 0.44f, 0.56f);
        AddReferencePiece(blocks, 0.34f, 2.89f, CANNISTER, 0.44f, 0.56f);
        // Center champion: 1 Turtle = 1 block!
        AddReferencePiece(blocks, 0f, 2.44f, TURTLE, 0.84f, 0.675f);
        // Outer sentries: 4 soldiers = 4 blocks!
        AddReferencePiece(blocks, cx[0], 3.37f, SOLDIER, 0.572f, 0.806f);
        AddReferencePiece(blocks, cx[1], 2.81f, SOLDIER, 0.572f, 0.806f);
        AddReferencePiece(blocks, cx[3], 2.81f, SOLDIER, 0.572f, 0.806f);
        AddReferencePiece(blocks, cx[4], 3.37f, SOLDIER, 0.572f, 0.806f);
    }

    private static void BuildReferenceNestedRings(List<ModelBlockSpec> blocks)
    {
        // Level 12: Grand Hollow Citadel (49 blocks, 1 king, 4 soldiers, 4 bombs).
        float[] cx = { -1.35f, -0.90f, -0.45f, 0f, 0.45f, 0.90f, 1.35f };
        // Foundation row = 7 blocks
        for (int i = 0; i < 7; i++) AddReferencePiece(blocks, cx[i], 0f, BOX2, 0.44f, 0.50f);
        // Outer bastions (cols 0, 1, 5, 6) rows 1..3 = 12 blocks
        for (int r = 1; r <= 3; r++)
        {
            AddReferencePiece(blocks, cx[0], r * 0.50f, BOX2, 0.44f, 0.50f);
            AddReferencePiece(blocks, cx[1], r * 0.50f, BOX, 0.44f, 0.50f);
            AddReferencePiece(blocks, cx[5], r * 0.50f, BOX, 0.44f, 0.50f);
            AddReferencePiece(blocks, cx[6], r * 0.50f, BOX2, 0.44f, 0.50f);
        }
        // Inner foundation & moat (cols 2, 3, 4) row 1 = 3 blocks
        AddReferencePiece(blocks, cx[2], 0.50f, BOX, 0.44f, 0.50f);
        AddReferencePiece(blocks, cx[3], 0.50f, BOX2, 0.44f, 0.50f);
        AddReferencePiece(blocks, cx[4], 0.50f, BOX, 0.44f, 0.50f);
        // Central royal dais: 2 crates + King = 3 blocks
        AddReferencePiece(blocks, -0.22f, 1.00f, BOX2, 0.44f, 0.50f);
        AddReferencePiece(blocks, 0.22f, 1.00f, BOX2, 0.44f, 0.50f);
        AddReferencePiece(blocks, 0f, 1.50f, KING, 0.82f, 1.02f);
        // 4 Bombs in dedicated open vaults = 4 blocks
        AddReferencePiece(blocks, cx[2], 1.00f, BOMB, 0.44f, 0.50f);
        AddReferencePiece(blocks, cx[4], 1.00f, BOMB, 0.44f, 0.50f);
        AddReferencePiece(blocks, -1.125f, 2.75f, BOMB, 0.44f, 0.50f);
        AddReferencePiece(blocks, 1.125f, 2.75f, BOMB, 0.44f, 0.50f);
        // Outer tower roofs: 2 lintels over bastions = 2 blocks
        AddReferencePiece(blocks, -1.125f, 2.00f, LONG_BOX, 1.00f, 0.25f);
        AddReferencePiece(blocks, 1.125f, 2.00f, LONG_BOX, 1.00f, 0.25f);
        // Outer parapets: 6 sandbags = 6 blocks
        AddReferencePiece(blocks, cx[0], 2.25f, SANDBAG, 0.44f, 0.45f);
        AddReferencePiece(blocks, cx[1], 2.25f, SANDBAG, 0.44f, 0.45f);
        AddReferencePiece(blocks, cx[5], 2.25f, SANDBAG, 0.44f, 0.45f);
        AddReferencePiece(blocks, cx[6], 2.25f, SANDBAG, 0.44f, 0.45f);
        AddReferencePiece(blocks, -0.40f, 2.00f, SANDBAG, 0.40f, 0.45f);
        AddReferencePiece(blocks, 0.40f, 2.00f, SANDBAG, 0.40f, 0.45f);
        // Throne canopy lintel & crown capstone = 2 blocks
        AddReferencePiece(blocks, 0f, 2.52f, LONG_BOX2, 1.30f, 0.20f);
        AddReferencePiece(blocks, 0f, 2.72f, BOX, 0.44f, 0.44f);
        // Additional fortress blocks: 6 blocks (2 sandbags + 4 crates)
        AddReferencePiece(blocks, -0.90f, 2.45f, SANDBAG, 0.44f, 0.45f);
        AddReferencePiece(blocks, 0.90f, 2.45f, SANDBAG, 0.44f, 0.45f);
        AddReferencePiece(blocks, -1.125f, 2.25f, BOX, 0.48f, 0.48f);
        AddReferencePiece(blocks, 1.125f, 2.25f, BOX, 0.48f, 0.48f);
        AddReferencePiece(blocks, -0.55f, 2.45f, BOX, 0.48f, 0.48f);
        AddReferencePiece(blocks, 0.55f, 2.45f, BOX, 0.48f, 0.48f);
        // Sentries: 4 Soldiers guarding the bastions = 4 blocks!
        AddReferencePiece(blocks, cx[0], 2.70f, SOLDIER, 0.572f, 0.806f);
        AddReferencePiece(blocks, cx[6], 2.70f, SOLDIER, 0.572f, 0.806f);
        AddReferencePiece(blocks, -0.90f, 2.90f, SOLDIER, 0.572f, 0.806f);
        AddReferencePiece(blocks, 0.90f, 2.90f, SOLDIER, 0.572f, 0.806f);
    }

private static void BuildReferenceThroneFort(List<ModelBlockSpec> blocks, bool grand)
    {
        float cell = grand ? 0.44f : ReferenceCell;
        float square = grand ? 0.43f : ReferenceSquare;

        if (!grand)
        {
            // Compact throne façade: stepped base, twin side towers, suspended bombs and a king.
            for (int col = -3; col <= 3; col++)
                AddReferencePiece(blocks, col * cell, 0f, (col & 1) == 0 ? BOX2 : BOX, square, square);
            for (int col = -2; col <= 2; col++)
                AddReferencePiece(blocks, col * cell, cell, BOX, square, square);
            for (int row = 1; row <= 5; row++)
            {
                AddReferencePiece(blocks, -3f * cell, row * cell, row < 3 ? BOX : BOX2, square, square);
                AddReferencePiece(blocks, 3f * cell, row * cell, row < 3 ? BOX : BOX2, square, square);
            }
            for (int row = 2; row <= 4; row++)
            {
                AddReferencePiece(blocks, -2f * cell, row * cell, BOX2, square, square);
                AddReferencePiece(blocks, 2f * cell, row * cell, BOX2, square, square);
            }
            AddReferencePiece(blocks, 0f, 6f * cell, LONG_BOX2, 3.32f, 0.24f);
            AddReferencePiece(blocks, -cell, 4.50f * cell, BOMB, 0.44f, 0.52f);
            AddReferencePiece(blocks, cell, 4.50f * cell, BOMB, 0.44f, 0.52f);
            AddReferencePiece(blocks, 0f, 2.15f * cell, KING, 0.82f, 1.02f);
            return;
        }

        // Grand Level 14: a denser nine-wide throne fortress with enlarged royal centerpiece.
        for (int col = -4; col <= 4; col++)
            AddReferencePiece(blocks, col * cell, 0f, (col & 1) == 0 ? BOX2 : BOX, square, square);

        int[] outer = { -4, -3, 3, 4 };
        for (int row = 1; row <= 5; row++)
        {
            for (int i = 0; i < outer.Length; i++)
                AddReferencePiece(blocks, outer[i] * cell, row * cell, row < 3 ? BOX : BOX2, square, square);
        }
        for (int row = 1; row <= 2; row++)
        {
            AddReferencePiece(blocks, -2f * cell, row * cell, BOX2, square, square);
            AddReferencePiece(blocks, 2f * cell, row * cell, BOX2, square, square);
        }
        for (int row = 1; row <= 2; row++)
        {
            for (int col = -1; col <= 1; col++)
                AddReferencePiece(blocks, col * cell, row * cell, BOX, square, square);
        }

        float roofY = 6f * cell;
        AddReferencePiece(blocks, 0f, roofY, LONG_BOX2, 3.72f, 0.24f);
        AddReferencePiece(blocks, -2f * cell, 3f * cell, BOMB, square, square);
        AddReferencePiece(blocks, 2f * cell, 3f * cell, BOMB, square, square);
        AddReferencePiece(blocks, 0f, 3f * cell, KING, 1.02f, 1.24f);

        for (int i = 0; i < outer.Length; i++)
            AddReferencePiece(blocks, outer[i] * cell, roofY + 0.24f, BOX3, 0.32f, 0.56f);
        AddReferencePiece(blocks, -cell, roofY + 0.24f, SOLDIER, 0.32f, 0.56f);
        AddReferencePiece(blocks, cell, roofY + 0.24f, SOLDIER, 0.32f, 0.56f);
    }
}
