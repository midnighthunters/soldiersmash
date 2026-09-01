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
            // Level 4 / reference 1: two compact, identical block banks on separate pedestals.
            tables.Add(new ModelTableSpec(-1.20f, 2.22f, -0.44f, FrontLayerZ, 3f));
            tables.Add(new ModelTableSpec(1.20f, 2.22f, -0.44f, FrontLayerZ, -3f));
            AddReferenceGrid(blocks, -1.20f, 3, 4, 0f, BOX2, BOX, 0);
            AddReferenceGrid(blocks, 1.20f, 3, 4, 0f, BOX2, BOX, 1);
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

    private static void BuildReferenceCheckerboard(List<ModelBlockSpec> blocks)
    {
        // Seven-by-seven alternating field. Bombs at the central cross make the dense wall playable.
        const int side = 7;
        float left = -(side - 1) * ReferenceCell * 0.5f;
        for (int row = 0; row < side; row++)
        {
            for (int col = 0; col < side; col++)
            {
                bool centralFuse = (row == 3 && (col == 1 || col == 3 || col == 5));
                int variant = centralFuse ? BOMB : (((row + col) & 1) == 0 ? BOX2 : BOX);
                AddReferencePiece(blocks, left + col * ReferenceCell, row * ReferenceCell, variant);
            }
        }
    }

    private static void BuildReferenceCrossWall(List<ModelBlockSpec> blocks)
    {
        // Six-by-six star-crate corners wrapped around a contrasting metallic cross.
        const int side = 6;
        float left = -(side - 1) * ReferenceCell * 0.5f;
        for (int row = 0; row < side; row++)
        {
            for (int col = 0; col < side; col++)
            {
                bool cornerQuadrant =
                    ((col < 2 || col > 3) && (row < 2 || row > 3)) ||
                    (col >= 2 && col <= 3 && row >= 2 && row <= 3);
                int variant = cornerQuadrant ? BOX : BOX2;
                AddReferencePiece(blocks, left + col * ReferenceCell, row * ReferenceCell, variant);
            }
        }
    }

    private static void BuildReferenceCrenellatedFort(List<ModelBlockSpec> blocks)
    {
        const int columns = 6;
        float left = -(columns - 1) * ReferenceCell * 0.5f;
        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                int variant = row < 2 ? BOX : BOX2;
                AddReferencePiece(blocks, left + col * ReferenceCell, row * ReferenceCell, variant);
            }
        }

        float spireY = 4f * ReferenceCell;
        for (int col = 0; col < columns; col++)
        {
            AddReferencePiece(blocks, left + col * ReferenceCell, spireY, BOX3, 0.34f, 0.58f);
        }
    }

    private static void BuildReferenceTowerBridge(List<ModelBlockSpec> blocks)
    {
        float[] outerColumns = { -1.50f, -1.02f, 1.02f, 1.50f };
        for (int row = 0; row < 4; row++)
        {
            for (int i = 0; i < outerColumns.Length; i++)
                AddReferencePiece(blocks, outerColumns[i], row * ReferenceCell, row < 2 ? BOX2 : BOX);
        }

        float[] centerColumns = { -0.24f, 0.24f };
        for (int row = 0; row < 2; row++)
        {
            for (int i = 0; i < centerColumns.Length; i++)
                AddReferencePiece(blocks, centerColumns[i], row * ReferenceCell, BOX2);
        }
        for (int row = 3; row < 5; row++)
        {
            for (int i = 0; i < centerColumns.Length; i++)
                AddReferencePiece(blocks, centerColumns[i], row * ReferenceCell, BOX);
        }

        AddReferencePiece(blocks, -1.26f, 4f * ReferenceCell, LONG_BOX2, 1.04f, 0.20f);
        AddReferencePiece(blocks, 1.26f, 4f * ReferenceCell, LONG_BOX2, 1.04f, 0.20f);

        float[] spireX = { -1.50f, -1.02f, 0f, 1.02f, 1.50f };
        for (int i = 0; i < spireX.Length; i++)
        {
            float y = i == 2 ? 5f * ReferenceCell : 4f * ReferenceCell + 0.20f;
            AddReferencePiece(blocks, spireX[i], y, BOX3, 0.34f, 0.58f);
        }
    }

    private static void BuildReferenceSolidWall(List<ModelBlockSpec> blocks)
    {
        AddReferenceGrid(blocks, 0f, 5, 5, 0f, BOX, BOX, 0);
    }

    private static void BuildReferenceTwinCanisterTowers(List<ModelBlockSpec> blocks)
    {
        // A single low beam supports two narrow five-storey canister towers.
        AddReferencePiece(blocks, 0f, 0f, LONG_BOX2, 2.82f, 0.18f);
        float[] columns = { -0.96f, -0.48f, 0.48f, 0.96f };
        for (int row = 0; row < 5; row++)
        {
            for (int col = 0; col < columns.Length; col++)
                AddReferencePiece(blocks, columns[col], 0.18f + row * ReferenceCell, CANNISTER);
        }
    }

private static void BuildReferenceColonnade(List<ModelBlockSpec> blocks)
    {
        float[] columns = { -1.44f, -0.72f, 0f, 0.72f, 1.44f };
        float railY = 3f * ReferenceCell;
        const float railHeight = 0.20f;
        for (int i = 0; i < columns.Length; i++)
        {
            for (int row = 0; row < 3; row++)
                AddReferencePiece(blocks, columns[i], row * ReferenceCell, BOX);

            for (int row = 0; row < 3; row++)
                AddReferencePiece(blocks, columns[i], railY + railHeight + row * ReferenceCell, BOX2);

            AddReferencePiece(
                blocks,
                columns[i],
                railY + railHeight + 3f * ReferenceCell,
                BOX3,
                0.34f,
                0.58f);
        }

        // Three broad signs join the five posts, matching the reference's linked middle rail.
        AddReferencePiece(blocks, -1.08f, railY, LONG_BOX2, 1.04f, railHeight);
        AddReferencePiece(blocks, 0f, railY, LONG_BOX2, 1.04f, railHeight);
        AddReferencePiece(blocks, 1.08f, railY, LONG_BOX2, 1.04f, railHeight);
    }

    private static void BuildReferenceNestedRings(List<ModelBlockSpec> blocks)
    {
        const int side = 7;
        float left = -(side - 1) * ReferenceCell * 0.5f;
        for (int row = 0; row < side; row++)
        {
            for (int col = 0; col < side; col++)
            {
                int distance = Mathf.Min(Mathf.Min(row, col), Mathf.Min(side - 1 - row, side - 1 - col));
                int variant = distance == 0 ? BOX2 : (distance == 1 ? BOX : BOX2);
                if (row == 3 && col == 3) variant = KING;
                else if (distance == 2 && ((row + col) & 1) == 0) variant = BOMB;
                AddReferencePiece(blocks, left + col * ReferenceCell, row * ReferenceCell, variant);
            }
        }
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
