using System.Collections.Generic;
using UnityEngine;

public static partial class WarfestLevelCatalog
{
    private static void BuildCampaign151to200(int level, List<ModelBlockSpec> b)
    {
        switch (level)
        {
            // =========================================================================
            // LEVELS 151–160: FRAGILE ENGINEERING (>= 25 Blocks, Bilaterally Symmetrical)
            // =========================================================================
            case 151: // Monolith on a Pin (29 blocks)
            {
                AddCube(b, 0f, 0f, true); // 1 block (base pin)
                AddCube(b, 0f, C_CUBE, true); // 1 block (narrow waist)
                AddMirroredCube(b, 1.25f, 0f, true); // 2 blocks (ground stabilizer outriggers)
                AddMirroredSoldier(b, 1.25f, C_CUBE); // 2 blocks
                AddMirroredCan(b, 0.70f, 0f); // 2 blocks
                // Wide cantilever deck balanced on top of pin
                AddPlank(b, 0f, 2 * C_CUBE); // 1 block
                AddMirroredPlank(b, 1.0f, 2 * C_CUBE); // 2 blocks
                float dY = 2 * C_CUBE + C_PLANK_H;
                // Massive temple structure
                AddBaseRow(b, 2, dY, false); // 5 blocks
                AddBaseRow(b, 2, dY + C_CUBE, false); // 5 blocks
                AddMirroredBomb(b, 1.10f, 2 * C_CUBE); // 2 blocks hanging below wing tips
                // Apex crown
                AddBeam(b, 0f, dY + 2 * C_CUBE); // 1 block
                AddKing(b, 0f, dY + 2 * C_CUBE + C_BEAM_H); // 1 block
                AddMirroredTurret(b, 1.10f, dY + 2 * C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 0.55f, dY + 2 * C_CUBE); // 2 blocks
                break; // Total = 29 blocks
            }

            case 152: // Stilt Walkway Citadel (30 blocks)
            {
                // Slender columnar stilts
                AddMirroredTower(b, 1.10f, 0f, 3, true); // 6 blocks
                AddMirroredTower(b, 0.55f, 0f, 3, false); // 6 blocks
                // Ground center vault
                AddBomb(b, 0f, 0f); // 1 block
                AddSoldier(b, 0f, C_BOMB_H); // 1 block
                // Sky walkway deck
                AddPlank(b, 0f, 3 * C_CUBE); // 1 block
                AddMirroredPlank(b, 1.0f, 3 * C_CUBE); // 2 blocks
                float dY = 3 * C_CUBE + C_PLANK_H;
                // Citadel floor
                AddBaseRow(b, 2, dY, false); // 5 blocks
                // Battlements
                AddMirroredTurret(b, 1.25f, dY + C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 0.70f, dY + C_CUBE); // 2 blocks
                // Center keep
                AddCube(b, 0f, dY + C_CUBE, false); // 1 block
                AddKing(b, 0f, dY + 2 * C_CUBE); // 1 block
                AddMirroredCan(b, 1.45f, 0f); // 2 blocks
                break; // Total = 30 blocks
            }

            case 153: // Knife-Edge Truss Bridge (27 blocks)
            {
                // Knife-edge abutments
                AddMirroredTower(b, 1.35f, 0f, 4, true); // 8 blocks
                AddMirroredCan(b, 0.85f, 0f); // 2 blocks
                // Lower arch chords
                AddMirroredCube(b, 0.90f, C_CUBE); // 2 blocks
                AddMirroredCube(b, 0.45f, 2 * C_CUBE); // 2 blocks
                // Keystone trigger
                AddBomb(b, 0f, 2 * C_CUBE); // 1 block
                AddCube(b, 0f, 2 * C_CUBE + C_BOMB_H); // 1 block
                // Spanning deck
                float deckY = 3 * C_CUBE + C_BOMB_H;
                AddPlank(b, 0f, deckY); // 1 block
                AddMirroredPlank(b, 0.95f, deckY); // 2 blocks
                float trafficY = deckY + C_PLANK_H;
                AddMirroredTurret(b, 1.35f, trafficY); // 2 blocks
                AddMirroredSoldier(b, 0.85f, trafficY); // 2 blocks
                AddMirroredCan(b, 0.40f, trafficY); // 2 blocks
                AddKing(b, 0f, trafficY); // 1 block
                AddBomb(b, 0f, trafficY + C_KING_H); // 1 block
                break; // Total = 27 blocks
            }

            case 154: // Inverted Sky Citadel (28 blocks)
            {
                // Outer suspension pillars
                AddMirroredTower(b, 1.30f, 0f, 5, true); // 10 blocks
                AddMirroredTurret(b, 1.30f, 5 * C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 1.30f, 5 * C_CUBE + C_TURRET_H); // 2 blocks
                // Suspension deck
                AddPlank(b, 0f, 4 * C_CUBE); // 1 block
                AddMirroredBeam(b, 0.70f, 4 * C_CUBE); // 2 blocks
                AddKing(b, 0f, 4 * C_CUBE + C_PLANK_H); // 1 block
                // Inverted pyramid hanging below deck
                AddBaseRow(b, 1, 3 * C_CUBE, false); // 3 blocks
                AddCube(b, 0f, 2 * C_CUBE, false); // 1 block
                AddMirroredBomb(b, 0.55f, 2 * C_CUBE); // 2 blocks
                AddCan(b, 0f, C_CUBE); // 1 block
                AddBomb(b, 0f, 0f); // 1 block
                AddMirroredSoldier(b, 0.60f, 0f); // 2 blocks
                break; // Total = 28 blocks
            }

            case 155: // Balanced Cantilever Wings (28 blocks)
            {
                // Central spine tower
                for (int r = 0; r < 4; r++) AddCube(b, 0f, r * C_CUBE, r == 0); // 4 blocks
                // Base anchors
                AddMirroredCube(b, 0.55f, 0f, true); // 2 blocks
                AddMirroredCan(b, 1.10f, 0f); // 2 blocks
                AddMirroredSoldier(b, 1.10f, C_CAN_H); // 2 blocks
                // Lower struts & braces
                AddMirroredPlank(b, 0.65f, 2 * C_CUBE); // 2 blocks
                AddMirroredBeam(b, 0.40f, 3 * C_CUBE); // 2 blocks
                // Main cantilever deck
                AddPlank(b, 0f, 4 * C_CUBE); // 1 block
                AddMirroredPlank(b, 1.0f, 4 * C_CUBE); // 2 blocks
                float wingY = 4 * C_CUBE + C_PLANK_H;
                // Wing payload pods
                AddMirroredTurret(b, 1.35f, wingY); // 2 blocks
                AddMirroredBomb(b, 1.35f, wingY + C_TURRET_H); // 2 blocks
                AddMirroredCube(b, 0.70f, wingY); // 2 blocks
                AddMirroredSoldier(b, 0.70f, wingY + C_CUBE); // 2 blocks
                // Spire apex
                AddCube(b, 0f, wingY); // 1 block
                AddKing(b, 0f, wingY + C_CUBE); // 1 block
                AddBomb(b, 0f, wingY + C_CUBE + C_KING_H); // 1 block
                break; // Total = 28 blocks
            }

            case 156: // Stepped Overhang Citadel (29 blocks)
            {
                // Base footing
                AddBaseRow(b, 1, 0f, true); // 3 blocks
                // Tier 2 (stepped outward)
                AddBaseRow(b, 2, C_CUBE, false); // 5 blocks
                // Corbel cantilever planks
                AddMirroredPlank(b, 0.70f, 2 * C_CUBE); // 2 blocks
                AddBeam(b, 0f, 2 * C_CUBE); // 1 block
                // Tier 3 wide deck
                float deckY = 2 * C_CUBE + C_PLANK_H;
                AddBaseRow(b, 2, deckY, false); // 5 blocks
                // Flying wing sentries
                AddMirroredTurret(b, 1.35f, deckY); // 2 blocks
                AddMirroredSoldier(b, 1.35f, deckY + C_TURRET_H); // 2 blocks
                // Central keep
                AddMirroredCube(b, 0.55f, deckY + C_CUBE); // 2 blocks
                AddBomb(b, 0f, deckY + C_CUBE); // 1 block
                AddPlank(b, 0f, deckY + 2 * C_CUBE); // 1 block
                AddKing(b, 0f, deckY + 2 * C_CUBE + C_PLANK_H); // 1 block
                // Ground flank outposts
                AddMirroredCan(b, 1.25f, 0f); // 2 blocks
                AddMirroredSoldier(b, 1.25f, C_CAN_H); // 2 blocks
                break; // Total = 29 blocks
            }

            case 157: // Hourglass Battle-Station (27 blocks)
            {
                // Base foundation
                AddBaseRow(b, 2, 0f, true); // 5 blocks
                AddBaseRow(b, 1, C_CUBE, false); // 3 blocks
                AddMirroredCube(b, 0.30f, 2 * C_CUBE, false); // 2 blocks
                // Waist pin & collar
                AddCube(b, 0f, 3 * C_CUBE, true); // 1 block
                AddBeam(b, 0f, 4 * C_CUBE); // 1 block
                float platY = 4 * C_CUBE + C_BEAM_H;
                // Upper platform floor
                AddBaseRow(b, 1, platY, false); // 3 blocks
                AddMirroredPlank(b, 0.80f, platY + C_CUBE); // 2 blocks
                float upperY = platY + C_CUBE + C_PLANK_H;
                // Upper towers
                AddMirroredTower(b, 1.10f, upperY, 2); // 4 blocks
                AddMirroredTurret(b, 1.10f, upperY + 2 * C_CUBE); // 2 blocks
                AddMirroredBomb(b, 0.55f, upperY); // 2 blocks
                // Center throne
                AddKing(b, 0f, upperY); // 1 block
                AddSoldier(b, 0f, upperY + C_KING_H); // 1 block
                break; // Total = 27 blocks
            }

            case 158: // Top-Heavy Warhead Tower (29 blocks)
            {
                // Slender trunk
                for (int r = 0; r < 4; r++) AddCube(b, 0f, r * C_CUBE, r == 0); // 4 blocks
                // Guy-line anchors
                AddMirroredTower(b, 1.30f, 0f, 2, true); // 4 blocks
                AddMirroredSoldier(b, 1.30f, 2 * C_CUBE); // 2 blocks
                AddMirroredBomb(b, 0.60f, 0f); // 2 blocks
                // Mushroom head collar
                AddPlank(b, 0f, 4 * C_CUBE); // 1 block
                AddMirroredPlank(b, 0.85f, 4 * C_CUBE); // 2 blocks
                float headY = 4 * C_CUBE + C_PLANK_H;
                AddBaseRow(b, 2, headY, false); // 5 blocks
                AddBaseRow(b, 1, headY + C_CUBE, false); // 3 blocks
                // Warhead battery
                AddMirroredTurret(b, 1.20f, headY + C_CUBE); // 2 blocks
                AddMirroredBomb(b, 0.60f, headY + 2 * C_CUBE); // 2 blocks
                AddKing(b, 0f, headY + 2 * C_CUBE); // 1 block
                AddSoldier(b, 0f, headY + 2 * C_CUBE + C_KING_H); // 1 block
                break; // Total = 29 blocks
            }

            case 159: // Split-Support A-Frame (28 blocks)
            {
                // Outer footings
                AddMirroredCube(b, 1.30f, 0f, true); // 2 blocks
                AddMirroredCan(b, 1.30f, C_CUBE); // 2 blocks
                // Inward stepping legs
                AddMirroredCube(b, 0.95f, 0.6f * C_CUBE); // 2 blocks
                AddMirroredCube(b, 0.65f, 1.6f * C_CUBE); // 2 blocks
                AddMirroredCube(b, 0.35f, 2.6f * C_CUBE); // 2 blocks
                // Arch interior vault
                AddBomb(b, 0f, 0f); // 1 block
                AddCube(b, 0f, C_BOMB_H); // 1 block
                AddSoldier(b, 0f, C_BOMB_H + C_CUBE); // 1 block
                // Keystone apex
                float apexY = 3.6f * C_CUBE;
                AddBeam(b, 0f, apexY); // 1 block
                AddPlank(b, 0f, apexY + C_BEAM_H); // 1 block
                float crownY = apexY + C_BEAM_H + C_PLANK_H;
                AddKing(b, 0f, crownY); // 1 block
                AddMirroredTurret(b, 0.55f, crownY); // 2 blocks
                // Wing outriggers
                AddMirroredBeam(b, 0.90f, 2.0f * C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 0.90f, 2.0f * C_CUBE + C_BEAM_H); // 2 blocks
                AddMirroredBomb(b, 1.30f, 2.0f * C_CUBE + C_BEAM_H); // 2 blocks
                AddMirroredCan(b, 0.50f, 0f); // 2 blocks
                AddMirroredSoldier(b, 0.50f, C_CAN_H); // 2 blocks
                break; // Total = 28 blocks
            }

            case 160: // Symmetrical Domino Array (28 blocks)
            {
                // 5 Domino pillars
                AddMirroredTower(b, 1.20f, 0f, 2, true); // 4 blocks
                AddMirroredTower(b, 0.60f, 0f, 2, false); // 4 blocks
                for (int r = 0; r < 3; r++) AddCube(b, 0f, r * C_CUBE, r == 0); // 3 blocks
                // Inter-pillar bombs
                AddMirroredBomb(b, 0.90f, 0f); // 2 blocks
                AddMirroredBomb(b, 0.30f, 0f); // 2 blocks
                // Linked planks
                AddMirroredPlank(b, 0.60f, 2 * C_CUBE); // 2 blocks
                // Pillar caps
                AddMirroredTurret(b, 1.20f, 2 * C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 1.20f, 2 * C_CUBE + C_TURRET_H); // 2 blocks
                AddMirroredSoldier(b, 0.60f, 2 * C_CUBE + C_PLANK_H); // 2 blocks
                // Center crown
                AddPlank(b, 0f, 3 * C_CUBE); // 1 block
                AddKing(b, 0f, 3 * C_CUBE + C_PLANK_H); // 1 block
                AddBomb(b, 0f, 3 * C_CUBE + C_PLANK_H + C_KING_H); // 1 block
                AddMirroredCan(b, 1.55f, 0f); // 2 blocks
                break; // Total = 28 blocks
            }

            // =========================================================================
            // LEVELS 161–170: MULTI-STRUCTURE SCENES (2-3 Tables, Bilaterally Symmetrical)
            // =========================================================================
            case 161: // Symmetrical Twin Forts (2 Tables, 28 blocks)
            {
                // Symmetrical mirrored forts on Table 0 (Left) and Table 1 (Right)
                for (int t = 0; t < 2; t++)
                {
                    AddCube(b, -0.55f, 0f, true, t); // 1 block
                    AddCube(b, 0f, 0f, true, t); // 1 block
                    AddCube(b, 0.55f, 0f, true, t); // 1 block
                    AddCube(b, -0.55f, C_CUBE, false, t); // 1 block
                    AddCube(b, 0.55f, C_CUBE, false, t); // 1 block
                    AddBeam(b, 0f, 2 * C_CUBE, t); // 1 block
                    float topY = 2 * C_CUBE + C_BEAM_H;
                    AddCube(b, 0f, topY, false, t); // 1 block
                    AddTurret(b, -0.50f, topY, t); // 1 block
                    AddTurret(b, 0.50f, topY, t); // 1 block
                    AddSoldier(b, -0.50f, topY + C_TURRET_H, t); // 1 block
                    if (t == 1) AddKing(b, 0.50f, topY + C_TURRET_H, 1); // 1 block (commander)
                    else AddSoldier(b, 0.50f, topY + C_TURRET_H, 0); // 1 block
                    AddBomb(b, 0f, C_CUBE, t); // 1 block
                    AddCan(b, -0.85f, 0f, t); // 1 block
                    AddCan(b, 0.85f, 0f, t); // 1 block
                }
                break; // Total = 28 blocks
            }

            case 162: // Armored Convoy & Gates (2 Tables, 28 blocks)
            {
                // Table 0 (Left): Armored Combat Tank & Escort (14 blocks)
                AddPlank(b, 0f, 0f, 0); // 1 block
                AddCube(b, -0.45f, C_PLANK_H, true, 0); // 1 block
                AddCube(b, 0.45f, C_PLANK_H, true, 0); // 1 block
                AddCube(b, 0f, C_PLANK_H, true, 0); // 1 block
                AddBeam(b, 0f, C_PLANK_H + C_CUBE, 0); // 1 block
                float tY = C_PLANK_H + C_CUBE + C_BEAM_H;
                AddCube(b, 0f, tY, false, 0); // 1 block
                AddBeam(b, 0.40f, tY, 0); // 1 block (cannon)
                AddKing(b, -0.25f, tY + C_CUBE, 0); // 1 block
                AddCan(b, -0.80f, 0f, 0); // 1 block
                AddSoldier(b, -0.80f, C_CAN_H, 0); // 1 block
                AddBomb(b, 0.80f, 0f, 0); // 1 block
                AddSoldier(b, 0.80f, C_BOMB_H, 0); // 1 block
                AddCan(b, -1.05f, 0f, 0); // 1 block
                AddCan(b, 1.05f, 0f, 0); // 1 block

                // Table 1 (Right): Fortified Gatehouse Barrier (14 blocks)
                AddCube(b, -0.55f, 0f, true, 1); // 1 block
                AddCube(b, 0f, 0f, true, 1); // 1 block
                AddCube(b, 0.55f, 0f, true, 1); // 1 block
                AddCube(b, -0.55f, C_CUBE, false, 1); // 1 block
                AddCube(b, 0.55f, C_CUBE, false, 1); // 1 block
                AddCube(b, -0.55f, 2 * C_CUBE, false, 1); // 1 block
                AddCube(b, 0.55f, 2 * C_CUBE, false, 1); // 1 block
                AddBeam(b, 0f, 3 * C_CUBE, 1); // 1 block
                float gY = 3 * C_CUBE + C_BEAM_H;
                AddTurret(b, -0.55f, gY, 1); // 1 block
                AddTurret(b, 0.55f, gY, 1); // 1 block
                AddSoldier(b, -0.55f, gY + C_TURRET_H, 1); // 1 block
                AddSoldier(b, 0.55f, gY + C_TURRET_H, 1); // 1 block
                AddBomb(b, 0f, C_CUBE, 1); // 1 block
                AddBomb(b, 0f, 2 * C_CUBE, 1); // 1 block
                break; // Total = 28 blocks
            }

            case 163: // Roman Aqueduct Span (2 Tables, 28 blocks)
            {
                // Symmetrical aqueduct spans on Table 0 and Table 1
                for (int t = 0; t < 2; t++)
                {
                    AddCube(b, -0.55f, 0f, true, t); // 1 block
                    AddCube(b, 0f, 0f, true, t); // 1 block
                    AddCube(b, 0.55f, 0f, true, t); // 1 block
                    AddCube(b, -0.55f, C_CUBE, false, t); // 1 block
                    AddCube(b, 0f, C_CUBE, false, t); // 1 block
                    AddCube(b, 0.55f, C_CUBE, false, t); // 1 block
                    AddPlank(b, 0f, 2 * C_CUBE, t); // 1 block
                    float aY = 2 * C_CUBE + C_PLANK_H;
                    AddCube(b, -0.55f, aY, false, t); // 1 block
                    AddCube(b, 0.55f, aY, false, t); // 1 block
                    AddBeam(b, 0f, aY + C_CUBE, t); // 1 block
                    float bY = aY + C_CUBE + C_BEAM_H;
                    AddTurret(b, -0.55f, bY, t); // 1 block
                    AddTurret(b, 0.55f, bY, t); // 1 block
                    AddSoldier(b, -0.55f, bY + C_TURRET_H, t); // 1 block
                    if (t == 0) AddKing(b, 0.55f, bY + C_TURRET_H, 0); // 1 block
                    else AddSoldier(b, 0.55f, bY + C_TURRET_H, 1); // 1 block
                }
                break; // Total = 28 blocks
            }

            case 164: // Imperial Keep & Pillboxes (3 Tables, 28 blocks)
            {
                // Center Table 0 (Raised High Keep, 14 blocks)
                AddBaseRow(b, 1, 0f, true, 0.55f, 0); // 3 blocks
                AddCube(b, -0.55f, C_CUBE, false, 0); // 1 block
                AddCube(b, 0.55f, C_CUBE, false, 0); // 1 block
                AddBeam(b, 0f, 2 * C_CUBE, 0); // 1 block
                float kY = 2 * C_CUBE + C_BEAM_H;
                AddCube(b, -0.55f, kY, false, 0); // 1 block
                AddCube(b, 0.55f, kY, false, 0); // 1 block
                AddPlank(b, 0f, kY + C_CUBE, 0); // 1 block
                float rY = kY + C_CUBE + C_PLANK_H;
                AddTurret(b, -0.55f, rY, 0); // 1 block
                AddTurret(b, 0.55f, rY, 0); // 1 block
                AddSoldier(b, -0.55f, rY + C_TURRET_H, 0); // 1 block
                AddSoldier(b, 0.55f, rY + C_TURRET_H, 0); // 1 block
                AddKing(b, 0f, rY, 0); // 1 block

                // Left Table 1 & Right Table 2 (Lower Pillbox Bastions, 7 blocks each)
                for (int t = 1; t <= 2; t++)
                {
                    AddCube(b, -0.30f, 0f, true, t); // 1 block
                    AddCube(b, 0.30f, 0f, true, t); // 1 block
                    AddPlank(b, 0f, C_CUBE, t); // 1 block
                    float pY = C_CUBE + C_PLANK_H;
                    AddTurret(b, t == 1 ? -0.30f : 0.30f, pY, t); // 1 block
                    AddSoldier(b, t == 1 ? 0.30f : -0.30f, pY, t); // 1 block
                    AddBomb(b, 0f, pY, t); // 1 block
                    AddCan(b, t == 1 ? -0.70f : 0.70f, 0f, t); // 1 block
                }
                break; // Total = 14 + 7 + 7 = 28 blocks
            }

            case 165: // Naval Fleet & Battery (2 Tables, 28 blocks)
            {
                // Table 0 (Left): Symmetrical Cruiser Warship (14 blocks)
                AddCube(b, -0.55f, 0f, true, 0); // 1 block
                AddCube(b, 0f, 0f, true, 0); // 1 block
                AddCube(b, 0.55f, 0f, true, 0); // 1 block
                AddPlank(b, 0f, C_CUBE, 0); // 1 block
                float wY = C_CUBE + C_PLANK_H;
                AddCube(b, -0.30f, wY, false, 0); // 1 block
                AddCube(b, 0.30f, wY, false, 0); // 1 block
                AddBeam(b, 0f, wY + C_CUBE, 0); // 1 block
                float mY = wY + C_CUBE + C_BEAM_H;
                AddTurret(b, -0.55f, mY, 0); // 1 block
                AddTurret(b, 0.55f, mY, 0); // 1 block
                AddCube(b, 0f, mY, false, 0); // 1 block
                AddKing(b, 0f, mY + C_CUBE, 0); // 1 block
                AddBomb(b, -0.85f, 0f, 0); // 1 block
                AddBomb(b, 0.85f, 0f, 0); // 1 block
                AddCan(b, -1.15f, 0f, 0); // 1 block

                // Table 1 (Right): Coastal Artillery Fortress (14 blocks)
                AddCube(b, -0.55f, 0f, true, 1); // 1 block
                AddCube(b, 0f, 0f, true, 1); // 1 block
                AddCube(b, 0.55f, 0f, true, 1); // 1 block
                AddPlank(b, 0f, C_CUBE, 1); // 1 block
                float fY = C_CUBE + C_PLANK_H;
                AddCube(b, -0.40f, fY, false, 1); // 1 block
                AddCube(b, 0.40f, fY, false, 1); // 1 block
                AddBeam(b, 0f, fY + C_CUBE, 1); // 1 block
                float cY = fY + C_CUBE + C_BEAM_H;
                AddTurret(b, -0.55f, cY, 1); // 1 block
                AddTurret(b, 0.55f, cY, 1); // 1 block
                AddSoldier(b, -0.55f, cY + C_TURRET_H, 1); // 1 block
                AddSoldier(b, 0.55f, cY + C_TURRET_H, 1); // 1 block
                AddBomb(b, 0f, fY, 1); // 1 block
                AddCan(b, -0.85f, 0f, 1); // 1 block
                AddCan(b, 0.85f, 0f, 1); // 1 block
                break; // Total = 28 blocks
            }

            case 166: // Radar Array & Command Post (2 Tables, 26 blocks)
            {
                // Table 0 (Left): Radar Array Tower (13 blocks)
                AddBaseRow(b, 1, 0f, true, 0.55f, 0); // 3 blocks
                AddCube(b, -0.55f, C_CUBE, false, 0); // 1 block
                AddCube(b, 0.55f, C_CUBE, false, 0); // 1 block
                AddPlank(b, 0f, 2 * C_CUBE, 0); // 1 block
                float rY = 2 * C_CUBE + C_PLANK_H;
                AddCube(b, -0.30f, rY, false, 0); // 1 block
                AddCube(b, 0.30f, rY, false, 0); // 1 block
                AddTurret(b, -0.55f, rY + C_CUBE, 0); // 1 block
                AddTurret(b, 0.55f, rY + C_CUBE, 0); // 1 block
                AddBomb(b, 0f, C_CUBE, 0); // 1 block
                AddCan(b, -0.85f, 0f, 0); // 1 block
                AddCan(b, 0.85f, 0f, 0); // 1 block

                // Table 1 (Right): Bunkered Command Post (13 blocks)
                AddBaseRow(b, 1, 0f, true, 0.55f, 1); // 3 blocks
                AddCube(b, -0.55f, C_CUBE, false, 1); // 1 block
                AddCube(b, 0.55f, C_CUBE, false, 1); // 1 block
                AddBeam(b, 0f, 2 * C_CUBE, 1); // 1 block
                float pY = 2 * C_CUBE + C_BEAM_H;
                AddKing(b, 0f, pY, 1); // 1 block
                AddTurret(b, -0.55f, pY, 1); // 1 block
                AddTurret(b, 0.55f, pY, 1); // 1 block
                AddSoldier(b, -0.55f, pY + C_TURRET_H, 1); // 1 block
                AddSoldier(b, 0.55f, pY + C_TURRET_H, 1); // 1 block
                AddCan(b, -0.85f, 0f, 1); // 1 block
                AddCan(b, 0.85f, 0f, 1); // 1 block
                break; // Total = 26 blocks
            }

            case 167: // Three Symmetrical Castles (3 Tables, 30 blocks)
            {
                // Center Table 0 (Raised High Castle, 14 blocks)
                AddBaseRow(b, 1, 0f, true, 0.55f, 0); // 3 blocks
                AddCube(b, -0.55f, C_CUBE, false, 0); // 1 block
                AddCube(b, 0.55f, C_CUBE, false, 0); // 1 block
                AddBeam(b, 0f, 2 * C_CUBE, 0); // 1 block
                float cY = 2 * C_CUBE + C_BEAM_H;
                AddCube(b, -0.55f, cY, false, 0); // 1 block
                AddCube(b, 0.55f, cY, false, 0); // 1 block
                AddPlank(b, 0f, cY + C_CUBE, 0); // 1 block
                float rY = cY + C_CUBE + C_PLANK_H;
                AddKing(b, 0f, rY, 0); // 1 block
                AddTurret(b, -0.55f, rY, 0); // 1 block
                AddTurret(b, 0.55f, rY, 0); // 1 block
                AddSoldier(b, -0.55f, rY + C_TURRET_H, 0); // 1 block
                AddSoldier(b, 0.55f, rY + C_TURRET_H, 0); // 1 block

                // Left Table 1 & Right Table 2 (Lower Barbican Castles, 8 blocks each)
                for (int t = 1; t <= 2; t++)
                {
                    AddCube(b, -0.30f, 0f, true, t); // 1 block
                    AddCube(b, 0.30f, 0f, true, t); // 1 block
                    AddPlank(b, 0f, C_CUBE, t); // 1 block
                    float bY = C_CUBE + C_PLANK_H;
                    AddCube(b, -0.30f, bY, false, t); // 1 block
                    AddCube(b, 0.30f, bY, false, t); // 1 block
                    AddTurret(b, t == 1 ? -0.30f : 0.30f, bY + C_CUBE, t); // 1 block
                    AddSoldier(b, t == 1 ? 0.30f : -0.30f, bY + C_CUBE, t); // 1 block
                    AddBomb(b, 0f, bY, t); // 1 block
                }
                break; // Total = 14 + 8 + 8 = 30 blocks
            }

            case 168: // Imperial Spire & Redoubts (3 Tables, 28 blocks)
            {
                // Center Table 0 (Imperial Spire, 14 blocks)
                AddBaseRow(b, 1, 0f, true, 0.55f, 0); // 3 blocks
                for (int r = 1; r <= 3; r++) AddCube(b, 0f, r * C_CUBE, false, 0); // 3 blocks
                AddPlank(b, 0f, 4 * C_CUBE, 0); // 1 block
                float sY = 4 * C_CUBE + C_PLANK_H;
                AddCube(b, -0.40f, sY, false, 0); // 1 block
                AddCube(b, 0.40f, sY, false, 0); // 1 block
                AddKing(b, 0f, sY); // 1 block
                AddSoldier(b, -0.40f, sY + C_CUBE, 0); // 1 block
                AddSoldier(b, 0.40f, sY + C_CUBE, 0); // 1 block
                AddBomb(b, -0.55f, C_CUBE, 0); // 1 block
                AddBomb(b, 0.55f, C_CUBE, 0); // 1 block

                // Left Table 1 & Right Table 2 (Redoubts, 7 blocks each)
                for (int t = 1; t <= 2; t++)
                {
                    AddCube(b, -0.30f, 0f, true, t); // 1 block
                    AddCube(b, 0.30f, 0f, true, t); // 1 block
                    AddPlank(b, 0f, C_CUBE, t); // 1 block
                    float rY = C_CUBE + C_PLANK_H;
                    AddTurret(b, t == 1 ? -0.30f : 0.30f, rY, t); // 1 block
                    AddSoldier(b, t == 1 ? 0.30f : -0.30f, rY, t); // 1 block
                    AddBomb(b, 0f, rY, t); // 1 block
                    AddCan(b, t == 1 ? -0.65f : 0.65f, 0f, t); // 1 block
                }
                break; // Total = 14 + 7 + 7 = 28 blocks
            }

            case 169: // Dual Checkpoint Bastions (2 Tables, 28 blocks)
            {
                // Symmetrical Checkpoint Bastions on Table 0 and Table 1
                for (int t = 0; t < 2; t++)
                {
                    AddCube(b, -0.55f, 0f, true, t); // 1 block
                    AddCube(b, 0f, 0f, true, t); // 1 block
                    AddCube(b, 0.55f, 0f, true, t); // 1 block
                    AddCube(b, -0.55f, C_CUBE, false, t); // 1 block
                    AddCube(b, 0.55f, C_CUBE, false, t); // 1 block
                    AddBeam(b, 0f, 2 * C_CUBE, t); // 1 block
                    float topY = 2 * C_CUBE + C_BEAM_H;
                    AddCube(b, -0.55f, topY, false, t); // 1 block
                    AddCube(b, 0.55f, topY, false, t); // 1 block
                    AddTurret(b, -0.55f, topY + C_CUBE, t); // 1 block
                    AddTurret(b, 0.55f, topY + C_CUBE, t); // 1 block
                    AddSoldier(b, -0.55f, topY + C_CUBE + C_TURRET_H, t); // 1 block
                    if (t == 1) AddKing(b, 0.55f, topY + C_CUBE + C_TURRET_H, 1); // 1 block (commander)
                    else AddSoldier(b, 0.55f, topY + C_CUBE + C_TURRET_H, 0); // 1 block
                    AddBomb(b, 0f, C_CUBE, t); // 1 block
                    AddCan(b, t == 0 ? -0.85f : 0.85f, 0f, t); // 1 block
                }
                break; // Total = 28 blocks
            }

            case 170: // Heavy Mortar Redoubts (2 Tables, 28 blocks)
            {
                // Mirrored Mortar Redoubts on Table 0 and Table 1
                for (int t = 0; t < 2; t++)
                {
                    AddCube(b, -0.55f, 0f, true, t); // 1 block
                    AddCube(b, 0f, 0f, true, t); // 1 block
                    AddCube(b, 0.55f, 0f, true, t); // 1 block
                    AddPlank(b, 0f, C_CUBE, t); // 1 block
                    float pY = C_CUBE + C_PLANK_H;
                    AddCube(b, -0.40f, pY, false, t); // 1 block
                    AddCube(b, 0.40f, pY, false, t); // 1 block
                    AddBeam(b, 0f, pY + C_CUBE, t); // 1 block
                    float mY = pY + C_CUBE + C_BEAM_H;
                    AddTurret(b, -0.40f, mY, t); // 1 block
                    AddTurret(b, 0.40f, mY, t); // 1 block
                    AddSoldier(b, -0.40f, mY + C_TURRET_H, t); // 1 block
                    if (t == 0) AddKing(b, 0.40f, mY + C_TURRET_H, 0); // 1 block
                    else AddSoldier(b, 0.40f, mY + C_TURRET_H, 1); // 1 block
                    AddBomb(b, -0.85f, 0f, t); // 1 block
                    AddBomb(b, 0.85f, 0f, t); // 1 block
                    AddCan(b, 0f, pY, t); // 1 block
                }
                break; // Total = 28 blocks
            }

            // =========================================================================
            // LEVELS 171–180: ADVANCED STRUCTURAL BALANCE (>= 25 Blocks, Bilaterally Symmetrical)
            // =========================================================================
            case 171: // Symmetrical Ruined Fortress (28 blocks)
            {
                AddBaseRow(b, 2, 0f, true); // 5 blocks
                AddMirroredCube(b, 1.10f, C_CUBE); // 2 blocks
                AddMirroredCube(b, 0.55f, C_CUBE); // 2 blocks
                AddMirroredBeam(b, 0.60f, 2 * C_CUBE); // 2 blocks
                float bY = 2 * C_CUBE + C_BEAM_H;
                AddMirroredTower(b, 1.10f, bY, 2); // 4 blocks
                AddMirroredTurret(b, 1.10f, bY + 2 * C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 1.10f, bY + 2 * C_CUBE + C_TURRET_H); // 2 blocks
                // Core shrine
                AddBomb(b, 0f, C_CUBE); // 1 block
                AddCube(b, 0f, C_CUBE + C_BOMB_H); // 1 block
                AddKing(b, 0f, 2 * C_CUBE + C_BOMB_H); // 1 block
                AddSoldier(b, 0f, 2 * C_CUBE + C_BOMB_H + C_KING_H); // 1 block
                // Flank munitions & rubble
                AddMirroredCan(b, 1.45f, 0f); // 2 blocks
                AddMirroredBomb(b, 0.55f, bY); // 2 blocks
                break; // Total = 28 blocks
            }

            case 172: // Twin Sheared Spire (28 blocks)
            {
                AddBaseRow(b, 2, 0f, true); // 5 blocks
                AddMirroredTower(b, 0.85f, C_CUBE, 4); // 8 blocks
                AddMirroredTurret(b, 0.85f, 5 * C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 0.85f, 5 * C_CUBE + C_TURRET_H); // 2 blocks
                // Central canyon vault
                AddBomb(b, 0f, C_CUBE); // 1 block
                AddCan(b, 0f, C_CUBE + C_BOMB_H); // 1 block
                AddKing(b, 0f, C_CUBE + C_BOMB_H + C_CAN_H); // 1 block
                // High bridge
                AddBeam(b, 0f, 4 * C_CUBE); // 1 block
                AddSoldier(b, 0f, 4 * C_CUBE + C_BEAM_H); // 1 block
                // Outer buttresses
                AddMirroredCube(b, 1.40f, 0f, true); // 2 blocks
                AddMirroredBomb(b, 1.40f, C_CUBE); // 2 blocks
                AddMirroredCan(b, 1.40f, 2 * C_CUBE); // 2 blocks
                break; // Total = 28 blocks
            }

            case 173: // Counterbalanced Leaning Towers (27 blocks)
            {
                // Symmetrical stepping inward towers
                AddMirroredTower(b, 1.25f, 0f, 2, true); // 4 blocks
                AddMirroredCube(b, 0.90f, 2 * C_CUBE); // 2 blocks
                AddMirroredCube(b, 0.55f, 3 * C_CUBE); // 2 blocks
                AddMirroredCube(b, 0.25f, 4 * C_CUBE); // 2 blocks
                // Keystone bridge
                AddBeam(b, 0f, 5 * C_CUBE); // 1 block
                AddKing(b, 0f, 5 * C_CUBE + C_BEAM_H); // 1 block
                // Center ground sanctum
                AddBaseRow(b, 1, 0f, true); // 3 blocks
                AddBomb(b, 0f, C_CUBE); // 1 block
                AddSoldier(b, 0f, C_CUBE + C_BOMB_H); // 1 block
                // Outrigger counterweights
                AddMirroredPlank(b, 1.0f, 2 * C_CUBE); // 2 blocks
                float pY = 2 * C_CUBE + C_PLANK_H;
                AddMirroredTurret(b, 1.25f, pY); // 2 blocks
                AddMirroredBomb(b, 1.25f, pY + C_TURRET_H); // 2 blocks
                AddMirroredSoldier(b, 0.65f, 4 * C_CUBE); // 2 blocks
                break; // Total = 27 blocks
            }

            case 174: // Symmetrical Pylon Forts (2 Tables, 28 blocks)
            {
                for (int t = 0; t < 2; t++)
                {
                    AddCube(b, -0.55f, 0f, true, t); // 1 block
                    AddCube(b, 0f, 0f, true, t); // 1 block
                    AddCube(b, 0.55f, 0f, true, t); // 1 block
                    AddCube(b, -0.40f, C_CUBE, false, t); // 1 block
                    AddCube(b, 0.40f, C_CUBE, false, t); // 1 block
                    AddPlank(b, 0f, 2 * C_CUBE, t); // 1 block
                    float pY = 2 * C_CUBE + C_PLANK_H;
                    AddCube(b, -0.30f, pY, false, t); // 1 block
                    AddCube(b, 0.30f, pY, false, t); // 1 block
                    AddBeam(b, 0f, pY + C_CUBE, t); // 1 block
                    float bY = pY + C_CUBE + C_BEAM_H;
                    AddTurret(b, -0.50f, bY, t); // 1 block
                    AddTurret(b, 0.50f, bY, t); // 1 block
                    AddSoldier(b, -0.50f, bY + C_TURRET_H, t); // 1 block
                    if (t == 1) AddKing(b, 0.50f, bY + C_TURRET_H, 1); // 1 block
                    else AddSoldier(b, 0.50f, bY + C_TURRET_H, 0); // 1 block
                    AddBomb(b, 0f, pY, t); // 1 block
                }
                break; // Total = 28 blocks
            }

            case 175: // Symmetrical Stepped Escarpment (30 blocks)
            {
                AddBaseRow(b, 3, 0f, true); // 7 blocks
                AddBaseRow(b, 2, C_CUBE, false); // 5 blocks
                AddBaseRow(b, 1, 2 * C_CUBE, false); // 3 blocks
                AddCube(b, 0f, 3 * C_CUBE, false); // 1 block
                AddMirroredTurret(b, 0.55f, 3 * C_CUBE); // 2 blocks
                AddKing(b, 0f, 4 * C_CUBE); // 1 block
                AddSoldier(b, 0f, 4 * C_CUBE + C_KING_H); // 1 block
                // Terraced sentries & munitions
                AddMirroredBomb(b, 1.10f, 2 * C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 1.65f, C_CUBE); // 2 blocks
                AddMirroredTurret(b, 1.10f, 2 * C_CUBE + C_BOMB_H); // 2 blocks
                AddMirroredCan(b, 0.55f, 3 * C_CUBE + C_TURRET_H); // 2 blocks
                AddMirroredSoldier(b, 0.55f, 3 * C_CUBE + C_TURRET_H + C_CAN_H); // 2 blocks
                break; // Total = 30 blocks
            }

            case 176: // Crashed Gunship Perimeter (28 blocks)
            {
                // Symmetrical fuselage & wings
                AddCube(b, 0f, 0f, true); // 1 block
                AddCube(b, 0f, C_CUBE, true); // 1 block
                AddCube(b, 0f, 2 * C_CUBE, false); // 1 block
                AddMirroredPlank(b, 0.75f, C_CUBE); // 2 blocks
                AddMirroredBeam(b, 1.10f, C_CUBE + C_PLANK_H); // 2 blocks
                AddKing(b, 0f, 3 * C_CUBE); // 1 block
                AddMirroredBomb(b, 0.60f, 2 * C_CUBE); // 2 blocks
                AddMirroredTurret(b, 1.35f, C_CUBE + C_PLANK_H); // 2 blocks
                AddMirroredSoldier(b, 1.35f, C_CUBE + C_PLANK_H + C_TURRET_H); // 2 blocks
                // Perimeter sandbag redoubts
                AddMirroredCube(b, 1.45f, 0f, true); // 2 blocks
                AddMirroredCube(b, 0.75f, 0f, true); // 2 blocks
                AddMirroredCan(b, 1.45f, C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 0.75f, C_CUBE + C_PLANK_H); // 2 blocks
                AddMirroredBomb(b, 0.75f, C_CUBE + C_PLANK_H + C_SOLDIER_H); // 2 blocks
                AddMirroredTurret(b, 1.45f, C_CUBE + C_CAN_H); // 2 blocks
                AddSoldier(b, 0f, 3 * C_CUBE + C_KING_H); // 1 block
                break; // Total = 28 blocks
            }

            case 177: // Fractured Suspension Bridge (2 Tables, 28 blocks)
            {
                for (int t = 0; t < 2; t++)
                {
                    AddCube(b, -0.55f, 0f, true, t); // 1 block
                    AddCube(b, 0f, 0f, true, t); // 1 block
                    AddCube(b, 0.55f, 0f, true, t); // 1 block
                    // Pier tower
                    float towerX = t == 0 ? -0.40f : 0.40f;
                    AddCube(b, towerX, C_CUBE, false, t); // 1 block
                    AddCube(b, towerX, 2 * C_CUBE, false, t); // 1 block
                    AddCube(b, towerX, 3 * C_CUBE, false, t); // 1 block
                    AddPlank(b, 0f, 4 * C_CUBE, t); // 1 block
                    float topY = 4 * C_CUBE + C_PLANK_H;
                    AddTurret(b, towerX, topY, t); // 1 block
                    AddSoldier(b, towerX, topY + C_TURRET_H, t); // 1 block
                    // Broken deck
                    float deckX = t == 0 ? 0.40f : -0.40f;
                    AddBeam(b, deckX, C_CUBE, t); // 1 block
                    AddBomb(b, deckX, C_CUBE + C_BEAM_H, t); // 1 block
                    if (t == 1) AddKing(b, deckX, C_CUBE + C_BEAM_H + C_BOMB_H, 1); // 1 block
                    else AddSoldier(b, deckX, C_CUBE + C_BEAM_H + C_BOMB_H, 0); // 1 block
                    AddCan(b, t == 0 ? -0.85f : 0.85f, 0f, t); // 1 block
                    AddCan(b, t == 0 ? 0.85f : -0.85f, 0f, t); // 1 block
                }
                break; // Total = 28 blocks
            }

            case 178: // Symmetrical Monolith Temples (2 Tables, 28 blocks)
            {
                for (int t = 0; t < 2; t++)
                {
                    AddCube(b, -0.55f, 0f, true, t); // 1 block
                    AddCube(b, 0f, 0f, true, t); // 1 block
                    AddCube(b, 0.55f, 0f, true, t); // 1 block
                    // Colonnade pillars
                    AddCube(b, -0.55f, C_CUBE, false, t); // 1 block
                    AddCube(b, 0.55f, C_CUBE, false, t); // 1 block
                    AddCube(b, -0.55f, 2 * C_CUBE, false, t); // 1 block
                    AddCube(b, 0.55f, 2 * C_CUBE, false, t); // 1 block
                    // Pediment architrave
                    AddBeam(b, 0f, 3 * C_CUBE, t); // 1 block
                    float pedY = 3 * C_CUBE + C_BEAM_H;
                    AddTurret(b, -0.55f, pedY, t); // 1 block
                    AddTurret(b, 0.55f, pedY, t); // 1 block
                    AddSoldier(b, -0.55f, pedY + C_TURRET_H, t); // 1 block
                    if (t == 1) AddKing(b, 0.55f, pedY + C_TURRET_H, 1); // 1 block
                    else AddSoldier(b, 0.55f, pedY + C_TURRET_H, 0); // 1 block
                    // Inner sanctum
                    AddBomb(b, 0f, C_CUBE, t); // 1 block
                    AddCan(b, 0f, C_CUBE + C_BOMB_H, t); // 1 block
                }
                break; // Total = 28 blocks
            }

            case 179: // Stepped Terraced Ziggurat (30 blocks)
            {
                AddBaseRow(b, 3, 0f, true); // 7 blocks
                AddBaseRow(b, 2, C_CUBE, false); // 5 blocks
                AddBaseRow(b, 1, 2 * C_CUBE, false); // 3 blocks
                AddCube(b, 0f, 3 * C_CUBE, false); // 1 block
                AddBeam(b, 0f, 4 * C_CUBE); // 1 block
                AddKing(b, 0f, 4 * C_CUBE + C_BEAM_H); // 1 block
                // Terraced guards & offerings
                AddMirroredTurret(b, 1.40f, C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 1.40f, C_CUBE + C_TURRET_H); // 2 blocks
                AddMirroredBomb(b, 0.90f, 2 * C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 0.90f, 2 * C_CUBE + C_BOMB_H); // 2 blocks
                AddMirroredCan(b, 0.45f, 3 * C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 0.45f, 3 * C_CUBE + C_CAN_H); // 2 blocks
                break; // Total = 30 blocks
            }

            case 180: // Symmetrical Fault Line Forts (2 Tables, 28 blocks)
            {
                for (int t = 0; t < 2; t++)
                {
                    AddCube(b, -0.55f, 0f, true, t); // 1 block
                    AddCube(b, 0f, 0f, true, t); // 1 block
                    AddCube(b, 0.55f, 0f, true, t); // 1 block
                    AddCube(b, -0.45f, C_CUBE, false, t); // 1 block
                    AddCube(b, 0.45f, C_CUBE, false, t); // 1 block
                    AddPlank(b, 0f, 2 * C_CUBE, t); // 1 block
                    float fY = 2 * C_CUBE + C_PLANK_H;
                    AddCube(b, -0.35f, fY, false, t); // 1 block
                    AddCube(b, 0.35f, fY, false, t); // 1 block
                    AddBeam(b, 0f, fY + C_CUBE, t); // 1 block
                    float topY = fY + C_CUBE + C_BEAM_H;
                    AddTurret(b, -0.45f, topY, t); // 1 block
                    AddTurret(b, 0.45f, topY, t); // 1 block
                    AddSoldier(b, -0.45f, topY + C_TURRET_H, t); // 1 block
                    if (t == 0) AddKing(b, 0.45f, topY + C_TURRET_H, 0); // 1 block
                    else AddSoldier(b, 0.45f, topY + C_TURRET_H, 1); // 1 block
                    AddBomb(b, 0f, fY, t); // 1 block
                }
                break; // Total = 28 blocks
            }

            // =========================================================================
            // LEVELS 181–190: ADVANCED STRUCTURAL PUZZLES (>= 25 Blocks, Bilaterally Symmetrical)
            // =========================================================================
            case 181: // The Keystone Vault (29 blocks)
            {
                // Twin abutment towers
                AddMirroredTower(b, 1.20f, 0f, 4, true); // 8 blocks
                AddMirroredTurret(b, 1.20f, 4 * C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 1.20f, 4 * C_CUBE + C_TURRET_H); // 2 blocks
                // Spanning arch springers
                AddMirroredCube(b, 0.65f, 3 * C_CUBE); // 2 blocks
                AddMirroredCube(b, 0.65f, 4 * C_CUBE); // 2 blocks
                // Keystone lintel & trigger
                AddBeam(b, 0f, 5 * C_CUBE); // 1 block
                AddCube(b, 0f, 5 * C_CUBE + C_BEAM_H, true); // 1 block
                AddKing(b, 0f, 6 * C_CUBE + C_BEAM_H); // 1 block
                // Vault interior chamber
                AddBaseRow(b, 1, 0f, true); // 3 blocks
                AddBomb(b, 0f, C_CUBE); // 1 block
                AddCan(b, 0f, C_CUBE + C_BOMB_H); // 1 block
                AddSoldier(b, 0f, C_CUBE + C_BOMB_H + C_CAN_H); // 1 block
                AddMirroredSoldier(b, 0.55f, C_CUBE); // 2 blocks
                AddMirroredCan(b, 1.55f, 0f); // 2 blocks
                break; // Total = 29 blocks
            }

            case 182: // Pendulum Counterweight Frame (28 blocks)
            {
                AddBaseRow(b, 2, 0f, true); // 5 blocks
                for (int r = 1; r < 4; r++) AddCube(b, 0f, r * C_CUBE, false); // 3 blocks
                AddPlank(b, 0f, 4 * C_CUBE); // 1 block
                AddMirroredPlank(b, 0.95f, 4 * C_CUBE); // 2 blocks
                float beamY = 4 * C_CUBE + C_PLANK_H;
                // Suspended counterweights
                AddMirroredCube(b, 1.30f, 3 * C_CUBE); // 2 blocks
                AddMirroredBomb(b, 1.30f, 2 * C_CUBE); // 2 blocks
                AddMirroredTurret(b, 1.30f, C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 1.30f, C_CUBE + C_TURRET_H); // 2 blocks
                // Crown
                AddKing(b, 0f, beamY); // 1 block
                AddBomb(b, 0f, beamY + C_KING_H); // 1 block
                // Inner braces
                AddMirroredBeam(b, 0.55f, 2 * C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 0.55f, 2 * C_CUBE + C_BEAM_H); // 2 blocks
                AddMirroredCan(b, 0.75f, 0f); // 2 blocks
                AddSoldier(b, 0f, beamY + C_KING_H + C_BOMB_H); // 1 block
                break; // Total = 28 blocks
            }

            case 183: // Symmetrical 4-Stage Fuse (29 blocks)
            {
                // Stage 1 (Base)
                AddBaseRow(b, 1, 0f, true); // 3 blocks
                AddMirroredBomb(b, 1.0f, 0f); // 2 blocks
                AddPlank(b, 0f, C_CUBE); // 1 block
                AddMirroredPlank(b, 1.0f, C_CUBE); // 2 blocks
                float s2Y = C_CUBE + C_PLANK_H;
                // Stage 2
                AddCube(b, 0f, s2Y, true); // 1 block
                AddMirroredBomb(b, 0.70f, s2Y); // 2 blocks
                AddMirroredCube(b, 1.30f, s2Y, false); // 2 blocks
                AddPlank(b, 0f, s2Y + C_CUBE); // 1 block
                float s3Y = s2Y + C_CUBE + C_PLANK_H;
                // Stage 3
                AddBomb(b, 0f, s3Y); // 1 block
                AddMirroredSoldier(b, 0.70f, s3Y); // 2 blocks
                AddBeam(b, 0f, s3Y + C_BOMB_H); // 1 block
                float s4Y = s3Y + C_BOMB_H + C_BEAM_H;
                // Stage 4 (Apex)
                AddKing(b, 0f, s4Y); // 1 block
                AddMirroredTurret(b, 0.50f, s4Y); // 2 blocks
                // Outer stabilizer towers
                AddMirroredTower(b, 1.45f, 0f, 2, true); // 4 blocks
                AddMirroredSoldier(b, 1.45f, 2 * C_CUBE); // 2 blocks
                AddMirroredCan(b, 1.45f, 2 * C_CUBE + C_SOLDIER_H); // 2 blocks
                break; // Total = 29 blocks
            }

            case 184: // Shielded King Sanctum (29 blocks)
            {
                AddBaseRow(b, 2, 0f, true); // 5 blocks
                AddMirroredTower(b, 1.25f, C_CUBE, 3, true); // 6 blocks
                AddMirroredTurret(b, 1.25f, 4 * C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 1.25f, 4 * C_CUBE + C_TURRET_H); // 2 blocks
                // Inner sanctum
                AddBaseRow(b, 1, C_CUBE, false); // 3 blocks
                AddMirroredCube(b, 0.55f, 2 * C_CUBE, true); // 2 blocks
                AddKing(b, 0f, 2 * C_CUBE); // 1 block
                AddBeam(b, 0f, 3 * C_CUBE); // 1 block
                AddPlank(b, 0f, 3 * C_CUBE + C_BEAM_H); // 1 block
                float rY = 3 * C_CUBE + C_BEAM_H + C_PLANK_H;
                AddBomb(b, 0f, rY); // 1 block
                AddSoldier(b, 0f, rY + C_BOMB_H); // 1 block
                AddMirroredBomb(b, 0.65f, rY); // 2 blocks
                AddMirroredCan(b, 1.55f, 0f); // 2 blocks
                break; // Total = 29 blocks
            }

            case 185: // Domino Hair-Trigger Bastion (28 blocks)
            {
                AddBaseRow(b, 2, 0f, true); // 5 blocks
                AddMirroredCube(b, 0.70f, C_CUBE, true); // 2 blocks
                AddMirroredPlank(b, 0.70f, 2 * C_CUBE); // 2 blocks
                float pY = 2 * C_CUBE + C_PLANK_H;
                AddMirroredBomb(b, 1.10f, pY); // 2 blocks
                AddMirroredTurret(b, 0.30f, pY); // 2 blocks
                // Center tower
                for (int r = 1; r <= 3; r++) AddCube(b, 0f, r * C_CUBE, false); // 3 blocks
                AddKing(b, 0f, 4 * C_CUBE); // 1 block
                AddBomb(b, 0f, 4 * C_CUBE + C_KING_H); // 1 block
                // Outer kicked towers
                AddMirroredTower(b, 1.40f, 0f, 2, false); // 4 blocks
                AddMirroredSoldier(b, 1.40f, 2 * C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 0.55f, 3 * C_CUBE); // 2 blocks
                AddMirroredCan(b, 1.40f, 2 * C_CUBE + C_SOLDIER_H); // 2 blocks
                break; // Total = 28 blocks
            }

            case 186: // Symmetrical Dual Chambers (30 blocks)
            {
                AddBaseRow(b, 3, 0f, true); // 7 blocks
                AddMirroredTower(b, 1.35f, C_CUBE, 3); // 6 blocks
                AddMirroredTower(b, 0.55f, C_CUBE, 3); // 6 blocks
                // Twin explosive chambers
                AddMirroredBomb(b, 0.95f, C_CUBE); // 2 blocks
                AddMirroredCan(b, 0.95f, C_CUBE + C_BOMB_H); // 2 blocks
                // Center corridor
                AddKing(b, 0f, C_CUBE); // 1 block
                AddSoldier(b, 0f, C_CUBE + C_KING_H); // 1 block
                // Roof vault
                AddPlank(b, 0f, 4 * C_CUBE); // 1 block
                AddMirroredBeam(b, 0.95f, 4 * C_CUBE); // 2 blocks
                AddMirroredTurret(b, 1.35f, 4 * C_CUBE + C_PLANK_H); // 2 blocks
                break; // Total = 30 blocks
            }

            case 187: // Symmetrical Guillotine Beam (28 blocks)
            {
                AddBaseRow(b, 2, 0f, true); // 5 blocks
                AddMirroredTower(b, 0.80f, C_CUBE, 3, true); // 6 blocks
                AddMirroredTurret(b, 0.80f, 4 * C_CUBE); // 2 blocks
                // Suspended heavy guillotine beam
                float bY = 4 * C_CUBE + C_TURRET_H;
                AddPlank(b, 0f, bY); // 1 block
                AddBeam(b, 0f, bY + C_PLANK_H); // 1 block
                float dropY = bY + C_PLANK_H + C_BEAM_H;
                AddCube(b, 0f, dropY, true); // 1 block
                AddBomb(b, 0f, dropY + C_CUBE); // 1 block
                AddKing(b, 0f, dropY + C_CUBE + C_BOMB_H); // 1 block
                // Below blade
                AddBomb(b, 0f, C_CUBE); // 1 block
                AddSoldier(b, 0f, C_CUBE + C_BOMB_H); // 1 block
                // Outer safety bastions
                AddMirroredTower(b, 1.40f, 0f, 2, false); // 4 blocks
                AddMirroredSoldier(b, 1.40f, 2 * C_CUBE); // 2 blocks
                AddMirroredCan(b, 1.40f, 2 * C_CUBE + C_SOLDIER_H); // 2 blocks
                break; // Total = 28 blocks
            }

            case 188: // Triple Interlocking Arches (31 blocks)
            {
                AddMirroredTower(b, 1.25f, 0f, 3, true); // 6 blocks
                AddMirroredTower(b, 0.45f, 0f, 3, true); // 6 blocks
                AddPlank(b, 0f, 3 * C_CUBE); // 1 block
                AddMirroredBeam(b, 0.85f, 3 * C_CUBE); // 2 blocks
                // Arch bays
                AddMirroredBomb(b, 0.85f, 0f); // 2 blocks
                AddMirroredSoldier(b, 0.85f, C_BOMB_H); // 2 blocks
                AddBomb(b, 0f, 0f); // 1 block
                AddCan(b, 0f, C_BOMB_H); // 1 block
                AddSoldier(b, 0f, C_BOMB_H + C_CAN_H); // 1 block
                // Upper arcade
                float dY = 3 * C_CUBE + C_PLANK_H;
                AddBaseRow(b, 2, dY, false); // 5 blocks
                AddMirroredTurret(b, 1.25f, dY + C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 0.65f, dY + C_CUBE); // 2 blocks
                AddKing(b, 0f, dY + C_CUBE); // 1 block
                break; // Total = 31 blocks
            }

            case 189: // Twin Blast Chimneys (28 blocks)
            {
                AddBaseRow(b, 2, 0f, true); // 5 blocks
                AddMirroredTower(b, 1.20f, C_CUBE, 3); // 6 blocks
                AddMirroredTower(b, 0.50f, C_CUBE, 3); // 6 blocks
                // Chimney interiors
                AddMirroredBomb(b, 0.85f, C_CUBE); // 2 blocks
                AddMirroredCan(b, 0.85f, C_CUBE + C_BOMB_H); // 2 blocks
                AddMirroredTurret(b, 0.85f, 4 * C_CUBE); // 2 blocks
                // Center corridor
                AddKing(b, 0f, C_CUBE); // 1 block
                AddBomb(b, 0f, C_CUBE + C_KING_H); // 1 block
                AddSoldier(b, 0f, C_CUBE + C_KING_H + C_BOMB_H); // 1 block
                AddMirroredCan(b, 1.55f, 0f); // 2 blocks
                break; // Total = 28 blocks
            }

            case 190: // Symmetrical Jenga Bastion (30 blocks)
            {
                AddBaseRow(b, 2, 0f, true); // 5 blocks
                AddBeam(b, 0f, C_CUBE); // 1 block
                AddMirroredBeam(b, 0.90f, C_CUBE); // 2 blocks
                float l2Y = C_CUBE + C_BEAM_H;
                AddCube(b, 0f, l2Y, false); // 1 block
                AddMirroredCube(b, 0.70f, l2Y, false); // 2 blocks
                AddPlank(b, 0f, l2Y + C_CUBE); // 1 block
                AddMirroredPlank(b, 0.95f, l2Y + C_CUBE); // 2 blocks
                float l4Y = l2Y + C_CUBE + C_PLANK_H;
                AddCube(b, 0f, l4Y, false); // 1 block
                AddMirroredCube(b, 0.70f, l4Y, false); // 2 blocks
                AddBeam(b, 0f, l4Y + C_CUBE); // 1 block
                float topY = l4Y + C_CUBE + C_BEAM_H;
                AddKing(b, 0f, topY); // 1 block
                AddMirroredTurret(b, 0.80f, topY); // 2 blocks
                AddMirroredSoldier(b, 0.80f, topY + C_TURRET_H); // 2 blocks
                AddMirroredCan(b, 1.45f, 0f); // 2 blocks
                AddMirroredSoldier(b, 1.45f, C_CAN_H); // 2 blocks
                AddBomb(b, 0f, topY + C_KING_H); // 1 block
                break; // Total = 30 blocks
            }

            // =========================================================================
            // LEVELS 191–200: MASTER LEVELS & GRAND FINALE (>= 25 Blocks, Bilaterally Symmetrical)
            // =========================================================================
            case 191: // Dreadnought Super-Battleship (42 blocks)
            {
                // Armored hull
                AddBaseRow(b, 3, 0f, true); // 7 blocks
                AddBaseRow(b, 3, C_CUBE, true); // 7 blocks
                AddPlank(b, 0f, 2 * C_CUBE); // 1 block
                AddMirroredPlank(b, 1.10f, 2 * C_CUBE); // 2 blocks
                float deckY = 2 * C_CUBE + C_PLANK_H;
                // Forward & Aft main gun batteries
                AddMirroredCube(b, 1.25f, deckY, false); // 2 blocks
                AddMirroredTurret(b, 1.25f, deckY + C_CUBE); // 2 blocks
                AddMirroredBomb(b, 1.25f, deckY + C_CUBE + C_TURRET_H); // 2 blocks
                // Secondary guns & crew
                AddMirroredTurret(b, 0.65f, deckY); // 2 blocks
                AddMirroredSoldier(b, 0.65f, deckY + C_TURRET_H); // 2 blocks
                // Superstructure citadel
                AddCube(b, 0f, deckY, false); // 1 block
                AddCube(b, 0f, deckY + C_CUBE, false); // 1 block
                AddBeam(b, 0f, deckY + 2 * C_CUBE); // 1 block
                float bridgeY = deckY + 2 * C_CUBE + C_BEAM_H;
                AddKing(b, 0f, bridgeY); // 1 block
                AddMirroredTurret(b, 0.40f, bridgeY); // 2 blocks
                // Depth charges & ammo
                AddMirroredBomb(b, 1.60f, 0f); // 2 blocks
                AddMirroredCan(b, 1.60f, C_BOMB_H); // 2 blocks
                AddMirroredSoldier(b, 1.60f, C_BOMB_H + C_CAN_H); // 2 blocks
                AddBomb(b, 0f, bridgeY + C_KING_H); // 1 block
                break; // Total = 42 blocks
            }

            case 192: // Royal Grand Castle (46 blocks)
            {
                // Base foundation
                AddBaseRow(b, 3, 0f, true); // 7 blocks
                // Outer curtain towers
                AddMirroredTower(b, 1.45f, C_CUBE, 4, true); // 8 blocks
                AddMirroredTurret(b, 1.45f, 5 * C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 1.45f, 5 * C_CUBE + C_TURRET_H); // 2 blocks
                AddMirroredBomb(b, 1.45f, 5 * C_CUBE + C_TURRET_H + C_SOLDIER_H); // 2 blocks
                // Mid towers
                AddMirroredTower(b, 0.75f, C_CUBE, 3, false); // 6 blocks
                AddMirroredTurret(b, 0.75f, 4 * C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 0.75f, 4 * C_CUBE + C_TURRET_H); // 2 blocks
                // Central throne keep
                for (int r = 1; r <= 3; r++) AddCube(b, 0f, r * C_CUBE, false); // 3 blocks
                AddBeam(b, 0f, 4 * C_CUBE); // 1 block
                float kY = 4 * C_CUBE + C_BEAM_H;
                AddCube(b, 0f, kY, false); // 1 block
                AddPlank(b, 0f, kY + C_CUBE); // 1 block
                AddKing(b, 0f, kY + C_CUBE + C_PLANK_H); // 1 block
                // Courtyard vault & munitions
                AddBomb(b, 0f, kY + C_CUBE + C_PLANK_H + C_KING_H); // 1 block (apex beacon)
                AddMirroredCan(b, 0.38f, C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 0.38f, C_CUBE + C_CAN_H); // 2 blocks
                AddMirroredBomb(b, 0.38f, C_CUBE + C_CAN_H + C_SOLDIER_H); // 2 blocks
                break; // Total = 46 blocks
            }

            case 193: // Orbital Rocket Complex (2 Tables, 42 blocks)
            {
                // Table 0 (Left): Rocket Launch Gantry & Multi-Stage Rocket (21 blocks)
                AddBaseRow(b, 1, 0f, true, 0.55f, 0); // 3 blocks
                // Twin gantry towers
                AddCube(b, -0.65f, C_CUBE, false, 0); // 1 block
                AddCube(b, -0.65f, 2 * C_CUBE, false, 0); // 1 block
                AddCube(b, -0.65f, 3 * C_CUBE, false, 0); // 1 block
                AddCube(b, -0.65f, 4 * C_CUBE, false, 0); // 1 block
                AddCube(b, 0.65f, C_CUBE, false, 0); // 1 block
                AddCube(b, 0.65f, 2 * C_CUBE, false, 0); // 1 block
                AddCube(b, 0.65f, 3 * C_CUBE, false, 0); // 1 block
                AddCube(b, 0.65f, 4 * C_CUBE, false, 0); // 1 block
                // Catwalk beam
                AddBeam(b, 0f, 3 * C_CUBE, 0); // 1 block
                // Rocket stack in center
                AddBomb(b, 0f, C_CUBE, 0); // 1 block (booster)
                AddCube(b, 0f, C_CUBE + C_BOMB_H, false, 0); // 1 block
                AddCube(b, 0f, 2 * C_CUBE + C_BOMB_H, false, 0); // 1 block
                AddBomb(b, 0f, 3 * C_CUBE + C_BOMB_H, 0); // 1 block
                AddTurret(b, 0f, 3 * C_CUBE + 2 * C_BOMB_H, 0); // 1 block (capsule)
                AddKing(b, 0f, 3 * C_CUBE + 2 * C_BOMB_H + C_TURRET_H, 0); // 1 block
                AddTurret(b, -0.65f, 5 * C_CUBE, 0); // 1 block
                AddTurret(b, 0.65f, 5 * C_CUBE, 0); // 1 block
                AddSoldier(b, -0.65f, 5 * C_CUBE + C_TURRET_H, 0); // 1 block
                AddSoldier(b, 0.65f, 5 * C_CUBE + C_TURRET_H, 0); // 1 block

                // Table 1 (Right): Mission Control & Radar Tracking Dome (21 blocks)
                AddBaseRow(b, 1, 0f, true, 0.55f, 1); // 3 blocks
                AddCube(b, -0.55f, C_CUBE, false, 1); // 1 block
                AddCube(b, 0f, C_CUBE, false, 1); // 1 block
                AddCube(b, 0.55f, C_CUBE, false, 1); // 1 block
                AddPlank(b, 0f, 2 * C_CUBE, 1); // 1 block
                float mcY = 2 * C_CUBE + C_PLANK_H;
                AddCube(b, -0.40f, mcY, false, 1); // 1 block
                AddCube(b, 0.40f, mcY, false, 1); // 1 block
                AddBeam(b, 0f, mcY + C_CUBE, 1); // 1 block
                float rdY = mcY + C_CUBE + C_BEAM_H;
                AddTurret(b, -0.50f, rdY, 1); // 1 block
                AddTurret(b, 0.50f, rdY, 1); // 1 block
                AddSoldier(b, -0.50f, rdY + C_TURRET_H, 1); // 1 block
                AddSoldier(b, 0.50f, rdY + C_TURRET_H, 1); // 1 block
                AddKing(b, 0f, rdY, 1); // 1 block
                AddBomb(b, -0.85f, 0f, 1); // 1 block
                AddBomb(b, 0.85f, 0f, 1); // 1 block
                AddCan(b, -0.85f, C_BOMB_H, 1); // 1 block
                AddCan(b, 0.85f, C_BOMB_H, 1); // 1 block
                AddSoldier(b, 0f, rdY + C_KING_H, 1); // 1 block
                AddBomb(b, 0f, mcY, 1); // 1 block
                break; // Total = 21 + 21 = 42 blocks
            }

            case 194: // Iron Mountain Citadel (2 Tables, 44 blocks)
            {
                // Symmetrical mountain bastion complexes on Table 0 (Left) and Table 1 (Right)
                for (int t = 0; t < 2; t++)
                {
                    AddCube(b, -0.75f, 0f, true, t); // 1 block
                    AddCube(b, -0.25f, 0f, true, t); // 1 block
                    AddCube(b, 0.25f, 0f, true, t); // 1 block
                    AddCube(b, 0.75f, 0f, true, t); // 1 block
                    AddCube(b, -0.50f, C_CUBE, false, t); // 1 block
                    AddCube(b, 0f, C_CUBE, false, t); // 1 block
                    AddCube(b, 0.50f, C_CUBE, false, t); // 1 block
                    AddPlank(b, 0f, 2 * C_CUBE, t); // 1 block
                    float pY = 2 * C_CUBE + C_PLANK_H;
                    AddCube(b, -0.35f, pY, false, t); // 1 block
                    AddCube(b, 0.35f, pY, false, t); // 1 block
                    AddBeam(b, 0f, pY + C_CUBE, t); // 1 block
                    float bY = pY + C_CUBE + C_BEAM_H;
                    AddTurret(b, -0.50f, bY, t); // 1 block
                    AddTurret(b, 0.50f, bY, t); // 1 block
                    AddSoldier(b, -0.50f, bY + C_TURRET_H, t); // 1 block
                    AddSoldier(b, 0.50f, bY + C_TURRET_H, t); // 1 block
                    if (t == 1) AddKing(b, 0f, bY, 1); // 1 block (commander on table 1)
                    else AddTurret(b, 0f, bY, 0); // 1 block
                    AddBomb(b, -0.85f, 0f, t); // 1 block
                    AddBomb(b, 0.85f, 0f, t); // 1 block
                    AddCan(b, -0.85f, C_BOMB_H, t); // 1 block
                    AddCan(b, 0.85f, C_BOMB_H, t); // 1 block
                    AddSoldier(b, 0f, pY, t); // 1 block
                }
                break; // Total = 22 + 22 = 44 blocks
            }

            case 195: // Fleet Aircraft Carrier (44 blocks)
            {
                // Symmetrical carrier hull
                AddBaseRow(b, 3, 0f, true); // 7 blocks
                AddBaseRow(b, 3, C_CUBE, true); // 7 blocks
                AddPlank(b, 0f, 2 * C_CUBE); // 1 block
                AddMirroredPlank(b, 1.05f, 2 * C_CUBE); // 2 blocks
                float deckY = 2 * C_CUBE + C_PLANK_H;
                // Island superstructure (center)
                AddCube(b, 0f, deckY, false); // 1 block
                AddCube(b, 0f, deckY + C_CUBE, false); // 1 block
                AddBeam(b, 0f, deckY + 2 * C_CUBE); // 1 block
                float islandY = deckY + 2 * C_CUBE + C_BEAM_H;
                AddKing(b, 0f, islandY); // 1 block
                AddMirroredTurret(b, 0.40f, islandY); // 2 blocks
                AddMirroredSoldier(b, 0.40f, islandY + C_TURRET_H); // 2 blocks
                // Parked delta fighter jets on deck
                AddMirroredTurret(b, 1.25f, deckY); // 2 blocks
                AddMirroredBomb(b, 1.25f, deckY + C_TURRET_H); // 2 blocks
                AddMirroredSoldier(b, 1.25f, deckY + C_TURRET_H + C_BOMB_H); // 2 blocks
                AddMirroredTurret(b, 0.70f, deckY); // 2 blocks
                AddMirroredBomb(b, 0.70f, deckY + C_TURRET_H); // 2 blocks
                AddMirroredSoldier(b, 0.70f, deckY + C_TURRET_H + C_BOMB_H); // 2 blocks
                // Deck crew & ammo stores
                AddMirroredCan(b, 1.55f, 0f); // 2 blocks
                AddMirroredSoldier(b, 1.55f, C_CAN_H); // 2 blocks
                AddMirroredBomb(b, 1.55f, C_CAN_H + C_SOLDIER_H); // 2 blocks
                break; // Total = 44 blocks
            }

            case 196: // Triumphal Imperial Monument (44 blocks)
            {
                // Base foundation
                AddBaseRow(b, 3, 0f, true); // 7 blocks
                // Paired triumphal columns
                AddMirroredTower(b, 1.35f, C_CUBE, 4, true); // 8 blocks
                AddMirroredTower(b, 0.65f, C_CUBE, 4, true); // 8 blocks
                // Monument arch lintel
                AddPlank(b, 0f, 5 * C_CUBE); // 1 block
                AddMirroredBeam(b, 1.0f, 5 * C_CUBE); // 2 blocks
                float atticY = 5 * C_CUBE + C_PLANK_H;
                // Attic frieze
                AddBaseRow(b, 2, atticY, false); // 5 blocks
                // Victory crown
                AddKing(b, 0f, atticY + C_CUBE); // 1 block
                AddMirroredTurret(b, 0.55f, atticY + C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 0.55f, atticY + C_CUBE + C_TURRET_H); // 2 blocks
                AddMirroredBomb(b, 1.10f, atticY + C_CUBE); // 2 blocks
                // Base statues & sentries
                AddMirroredTurret(b, 1.65f, C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 1.65f, C_CUBE + C_TURRET_H); // 2 blocks
                AddBomb(b, 0f, C_CUBE); // 1 block inside arch
                AddSoldier(b, 0f, C_CUBE + C_BOMB_H); // 1 block
                break; // Total = 44 blocks
            }

            case 197: // Land Dreadnought Crawler (46 blocks)
            {
                // Massive armored caterpillar treads
                AddBaseRow(b, 3, 0f, true); // 7 blocks
                AddBaseRow(b, 3, C_CUBE, true); // 7 blocks
                AddPlank(b, 0f, 2 * C_CUBE); // 1 block
                AddMirroredPlank(b, 1.10f, 2 * C_CUBE); // 2 blocks
                float hullY = 2 * C_CUBE + C_PLANK_H;
                // Armored hull sponsons
                AddMirroredTurret(b, 1.35f, hullY); // 2 blocks
                AddMirroredBomb(b, 1.35f, hullY + C_TURRET_H); // 2 blocks
                AddMirroredSoldier(b, 1.35f, hullY + C_TURRET_H + C_BOMB_H); // 2 blocks
                AddMirroredTurret(b, 0.70f, hullY); // 2 blocks
                AddMirroredBomb(b, 0.70f, hullY + C_TURRET_H); // 2 blocks
                // Central bridge citadel
                AddBaseRow(b, 1, hullY, false); // 3 blocks
                AddCube(b, 0f, hullY + C_CUBE, false); // 1 block
                AddBeam(b, 0f, hullY + 2 * C_CUBE); // 1 block
                float bridgeY = hullY + 2 * C_CUBE + C_BEAM_H;
                AddKing(b, 0f, bridgeY); // 1 block
                AddMirroredTurret(b, 0.45f, bridgeY); // 2 blocks
                AddMirroredSoldier(b, 0.45f, bridgeY + C_TURRET_H); // 2 blocks
                // Rear munitions & escorts
                AddMirroredCan(b, 1.65f, 2 * C_CUBE + C_PLANK_H); // 2 blocks
                AddMirroredSoldier(b, 1.65f, 2 * C_CUBE + C_PLANK_H + C_CAN_H); // 2 blocks
                AddMirroredBomb(b, 1.65f, 2 * C_CUBE + C_PLANK_H + C_CAN_H + C_SOLDIER_H); // 2 blocks
                AddBomb(b, 0f, bridgeY + C_KING_H); // 1 block
                AddSoldier(b, 0f, bridgeY + C_KING_H + C_BOMB_H); // 1 block
                break; // Total = 46 blocks
            }

            case 198: // Triple Imperial Stronghold (3 Tables, 54 blocks)
            {
                // Center Table 0 (Raised Throne Keep, 26 blocks)
                AddBaseRow(b, 2, 0f, true, 0.55f, 0); // 5 blocks
                AddCube(b, -0.65f, C_CUBE, false, 0); // 1 block
                AddCube(b, 0.65f, C_CUBE, false, 0); // 1 block
                AddCube(b, -0.65f, 2 * C_CUBE, false, 0); // 1 block
                AddCube(b, 0.65f, 2 * C_CUBE, false, 0); // 1 block
                AddBeam(b, 0f, 3 * C_CUBE, 0); // 1 block
                float kY = 3 * C_CUBE + C_BEAM_H;
                AddCube(b, -0.55f, kY, false, 0); // 1 block
                AddCube(b, 0.55f, kY, false, 0); // 1 block
                AddCube(b, 0f, kY, false, 0); // 1 block
                AddPlank(b, 0f, kY + C_CUBE, 0); // 1 block
                float rY = kY + C_CUBE + C_PLANK_H;
                AddKing(b, 0f, rY, 0); // 1 block
                AddTurret(b, -0.55f, rY, 0); // 1 block
                AddTurret(b, 0.55f, rY, 0); // 1 block
                AddSoldier(b, -0.55f, rY + C_TURRET_H, 0); // 1 block
                AddSoldier(b, 0.55f, rY + C_TURRET_H, 0); // 1 block
                // Vault interior
                AddBomb(b, 0f, C_CUBE, 0); // 1 block
                AddBomb(b, 0f, 2 * C_CUBE, 0); // 1 block
                AddSoldier(b, 0f, 2 * C_CUBE + C_BOMB_H, 0); // 1 block
                AddCan(b, -0.95f, 0f, 0); // 1 block
                AddCan(b, 0.95f, 0f, 0); // 1 block
                AddSoldier(b, -0.95f, C_CAN_H, 0); // 1 block
                AddSoldier(b, 0.95f, C_CAN_H, 0); // 1 block

                // Left Table 1 & Right Table 2 (Artillery Bastions, 14 blocks each)
                for (int t = 1; t <= 2; t++)
                {
                    AddCube(b, -0.55f, 0f, true, t); // 1 block
                    AddCube(b, 0f, 0f, true, t); // 1 block
                    AddCube(b, 0.55f, 0f, true, t); // 1 block
                    AddPlank(b, 0f, C_CUBE, t); // 1 block
                    float bY = C_CUBE + C_PLANK_H;
                    AddCube(b, -0.35f, bY, false, t); // 1 block
                    AddCube(b, 0.35f, bY, false, t); // 1 block
                    AddBeam(b, 0f, bY + C_CUBE, t); // 1 block
                    float aY = bY + C_CUBE + C_BEAM_H;
                    AddTurret(b, -0.45f, aY, t); // 1 block
                    AddTurret(b, 0.45f, aY, t); // 1 block
                    AddSoldier(b, -0.45f, aY + C_TURRET_H, t); // 1 block
                    AddSoldier(b, 0.45f, aY + C_TURRET_H, t); // 1 block
                    AddBomb(b, t == 1 ? -0.85f : 0.85f, 0f, t); // 1 block
                    AddBomb(b, t == 1 ? 0.85f : -0.85f, 0f, t); // 1 block
                    AddCan(b, 0f, bY, t); // 1 block
                }
                break; // Total = 26 + 14 + 14 = 54 blocks
            }

            case 199: // High Command War Headquarters (3 Tables, 58 blocks)
            {
                // Center Table 0 (Raised High Command Bunker, 28 blocks)
                AddBaseRow(b, 2, 0f, true, 0.55f, 0); // 5 blocks
                AddCube(b, -0.65f, C_CUBE, false, 0); // 1 block
                AddCube(b, 0.65f, C_CUBE, false, 0); // 1 block
                AddPlank(b, 0f, 2 * C_CUBE, 0); // 1 block
                float d1Y = 2 * C_CUBE + C_PLANK_H;
                AddBaseRow(b, 1, d1Y, false, 0.55f, 0); // 3 blocks
                AddBeam(b, 0f, d1Y + C_CUBE, 0); // 1 block
                float d2Y = d1Y + C_CUBE + C_BEAM_H;
                AddCube(b, -0.40f, d2Y, false, 0); // 1 block
                AddCube(b, 0.40f, d2Y, false, 0); // 1 block
                AddKing(b, 0f, d2Y, 0); // 1 block
                AddTurret(b, -0.45f, d2Y + C_CUBE, 0); // 1 block
                AddTurret(b, 0.45f, d2Y + C_CUBE, 0); // 1 block
                AddSoldier(b, -0.45f, d2Y + C_CUBE + C_TURRET_H, 0); // 1 block
                AddSoldier(b, 0.45f, d2Y + C_CUBE + C_TURRET_H, 0); // 1 block
                // Vault interior & perimeter
                AddBomb(b, 0f, C_CUBE, 0); // 1 block
                AddBomb(b, 0f, d2Y + C_KING_H, 0); // 1 block
                AddSoldier(b, 0f, d2Y + C_KING_H + C_BOMB_H, 0); // 1 block
                AddCan(b, -0.95f, 0f, 0); // 1 block
                AddCan(b, 0.95f, 0f, 0); // 1 block
                AddSoldier(b, -0.95f, C_CAN_H, 0); // 1 block
                AddSoldier(b, 0.95f, C_CAN_H, 0); // 1 block
                AddBomb(b, -0.95f, C_CAN_H + C_SOLDIER_H, 0); // 1 block
                AddBomb(b, 0.95f, C_CAN_H + C_SOLDIER_H, 0); // 1 block

                // Left Table 1 (Missile Silo Battery, 15 blocks)
                AddCube(b, -0.55f, 0f, true, 1); // 1 block
                AddCube(b, 0f, 0f, true, 1); // 1 block
                AddCube(b, 0.55f, 0f, true, 1); // 1 block
                AddPlank(b, 0f, C_CUBE, 1); // 1 block
                float sY = C_CUBE + C_PLANK_H;
                AddCube(b, -0.35f, sY, false, 1); // 1 block
                AddCube(b, 0.35f, sY, false, 1); // 1 block
                AddBomb(b, -0.35f, sY + C_CUBE, 1); // 1 block
                AddBomb(b, 0.35f, sY + C_CUBE, 1); // 1 block
                AddTurret(b, -0.35f, sY + C_CUBE + C_BOMB_H, 1); // 1 block
                AddTurret(b, 0.35f, sY + C_CUBE + C_BOMB_H, 1); // 1 block
                AddSoldier(b, 0f, sY, 1); // 1 block
                AddSoldier(b, -0.75f, 0f, 1); // 1 block
                AddSoldier(b, 0.75f, 0f, 1); // 1 block
                AddCan(b, -0.75f, C_SOLDIER_H, 1); // 1 block
                AddCan(b, 0.75f, C_SOLDIER_H, 1); // 1 block

                // Right Table 2 (Radar Array Station, 15 blocks - mirrored)
                AddCube(b, -0.55f, 0f, true, 2); // 1 block
                AddCube(b, 0f, 0f, true, 2); // 1 block
                AddCube(b, 0.55f, 0f, true, 2); // 1 block
                AddPlank(b, 0f, C_CUBE, 2); // 1 block
                float rY2 = C_CUBE + C_PLANK_H;
                AddCube(b, -0.35f, rY2, false, 2); // 1 block
                AddCube(b, 0.35f, rY2, false, 2); // 1 block
                AddTurret(b, -0.35f, rY2 + C_CUBE, 2); // 1 block
                AddTurret(b, 0.35f, rY2 + C_CUBE, 2); // 1 block
                AddSoldier(b, -0.35f, rY2 + C_CUBE + C_TURRET_H, 2); // 1 block
                AddSoldier(b, 0.35f, rY2 + C_CUBE + C_TURRET_H, 2); // 1 block
                AddSoldier(b, 0f, rY2, 2); // 1 block
                AddBomb(b, -0.75f, 0f, 2); // 1 block
                AddBomb(b, 0.75f, 0f, 2); // 1 block
                AddCan(b, -0.75f, C_BOMB_H, 2); // 1 block
                AddCan(b, 0.75f, C_BOMB_H, 2); // 1 block
                break; // Total = 28 + 15 + 15 = 58 blocks
            }

            case 200: // GRAND FINALE STRONGHOLD (3 Tables, 82 blocks!)
            {
                // =====================================================================
                // CENTER TABLE 0: Raised Imperial High Citadel (44 blocks)
                // =====================================================================
                AddBaseRow(b, 3, 0f, true, 0.55f, 0); // 7 blocks (foundation)
                AddBaseRow(b, 2, C_CUBE, false, 0.55f, 0); // 5 blocks (lower barbican)
                // Double arched gates
                AddBeam(b, -0.60f, 2 * C_CUBE, 0); // 1 block
                AddBeam(b, 0.60f, 2 * C_CUBE, 0); // 1 block
                float midY = 2 * C_CUBE + C_BEAM_H;
                // Middle court & vault
                AddCube(b, -0.60f, midY, false, 0); // 1 block
                AddCube(b, 0f, midY, false, 0); // 1 block
                AddCube(b, 0.60f, midY, false, 0); // 1 block
                AddBomb(b, -0.30f, midY, 0); // 1 block
                AddBomb(b, 0.30f, midY, 0); // 1 block
                // Mid terrace deck
                AddPlank(b, 0f, midY + C_CUBE, 0); // 1 block
                AddPlank(b, -0.90f, midY + C_CUBE, 0); // 1 block
                AddPlank(b, 0.90f, midY + C_CUBE, 0); // 1 block
                float upY = midY + C_CUBE + C_PLANK_H;
                // Upper Keep & Spires
                AddCube(b, -0.55f, upY, false, 0); // 1 block
                AddCube(b, 0f, upY, false, 0); // 1 block
                AddCube(b, 0.55f, upY, false, 0); // 1 block
                AddCube(b, -0.55f, upY + C_CUBE, false, 0); // 1 block
                AddCube(b, 0.55f, upY + C_CUBE, false, 0); // 1 block
                AddBeam(b, 0f, upY + 2 * C_CUBE, 0); // 1 block
                float throneY = upY + 2 * C_CUBE + C_BEAM_H;
                // Imperial Throne & Guard
                AddKing(b, 0f, throneY, 0); // 1 block (EMPEROR)
                AddTurret(b, -0.55f, throneY, 0); // 1 block
                AddTurret(b, 0.55f, throneY, 0); // 1 block
                AddSoldier(b, -0.55f, throneY + C_TURRET_H, 0); // 1 block
                AddSoldier(b, 0.55f, throneY + C_TURRET_H, 0); // 1 block
                AddBomb(b, 0f, throneY + C_KING_H, 0); // 1 block (apex warhead)
                // Mid terrace sentinels & artillery
                AddTurret(b, -1.10f, upY, 0); // 1 block
                AddTurret(b, 1.10f, upY, 0); // 1 block
                AddSoldier(b, -1.10f, upY + C_TURRET_H, 0); // 1 block
                AddSoldier(b, 1.10f, upY + C_TURRET_H, 0); // 1 block
                AddBomb(b, -1.10f, upY + C_TURRET_H + C_SOLDIER_H, 0); // 1 block
                AddBomb(b, 1.10f, upY + C_TURRET_H + C_SOLDIER_H, 0); // 1 block
                // Ground treasury & honor guard
                AddCan(b, -1.45f, 0f, 0); // 1 block
                AddCan(b, 1.45f, 0f, 0); // 1 block
                AddSoldier(b, -1.45f, C_CAN_H, 0); // 1 block
                AddSoldier(b, 1.45f, C_CAN_H, 0); // 1 block

                // =====================================================================
                // LEFT TABLE 1 & RIGHT TABLE 2: Flanking Fortress Bastions (19 blocks each)
                // =====================================================================
                for (int t = 1; t <= 2; t++)
                {
                    // Foundation
                    AddCube(b, -0.55f, 0f, true, t); // 1 block
                    AddCube(b, 0f, 0f, true, t); // 1 block
                    AddCube(b, 0.55f, 0f, true, t); // 1 block
                    AddPlank(b, 0f, C_CUBE, t); // 1 block
                    float b1Y = C_CUBE + C_PLANK_H;
                    // Mid bunker
                    AddCube(b, -0.45f, b1Y, false, t); // 1 block
                    AddCube(b, 0f, b1Y, false, t); // 1 block
                    AddCube(b, 0.45f, b1Y, false, t); // 1 block
                    AddBeam(b, 0f, b1Y + C_CUBE, t); // 1 block
                    float b2Y = b1Y + C_CUBE + C_BEAM_H;
                    // Artillery battery
                    AddTurret(b, -0.45f, b2Y, t); // 1 block
                    AddTurret(b, 0.45f, b2Y, t); // 1 block
                    AddSoldier(b, -0.45f, b2Y + C_TURRET_H, t); // 1 block
                    AddSoldier(b, 0.45f, b2Y + C_TURRET_H, t); // 1 block
                    AddTurret(b, 0f, b2Y, t); // 1 block
                    // Munitions & perimeter
                    AddBomb(b, t == 1 ? -0.85f : 0.85f, 0f, t); // 1 block
                    AddBomb(b, t == 1 ? 0.85f : -0.85f, 0f, t); // 1 block
                    AddCan(b, t == 1 ? -0.85f : 0.85f, C_BOMB_H, t); // 1 block
                    AddCan(b, t == 1 ? 0.85f : -0.85f, C_BOMB_H, t); // 1 block
                    AddSoldier(b, t == 1 ? -0.85f : 0.85f, C_BOMB_H + C_CAN_H, t); // 1 block
                    AddSoldier(b, t == 1 ? 0.85f : -0.85f, C_BOMB_H + C_CAN_H, t); // 1 block
                }
                break; // Total = 44 + 19 + 19 = 82 blocks!
            }
        }
    }
}
