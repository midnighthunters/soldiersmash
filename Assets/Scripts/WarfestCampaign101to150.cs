using System.Collections.Generic;
using UnityEngine;

public static partial class WarfestLevelCatalog
{
    private static void BuildCampaign101to150(int level, List<ModelBlockSpec> b)
    {
        switch (level)
        {
            // =========================================================================
            // LEVELS 101–110: ICONIC SHAPES (>= 25 Blocks, Symmetrical)
            // =========================================================================
            case 101: // Imperial Crown (5-pointed royal crown, 33 blocks)
            {
                AddBaseRow(b, 3, 0f, true); // 7 blocks (y=0)
                AddPlank(b, 0f, C_CUBE); // 1 block
                AddMirroredPlank(b, 1.20f, C_CUBE); // 2 blocks
                float y1 = C_CUBE + C_PLANK_H;
                // Outer crests (x = ±1.50, h=2)
                AddMirroredTower(b, 1.50f, y1, 2); // 4 blocks
                AddMirroredTurret(b, 1.50f, y1 + 2 * C_CUBE); // 2 blocks
                // Mid crests (x = ±0.75, h=3)
                AddMirroredTower(b, 0.75f, y1, 3); // 6 blocks
                AddMirroredSoldier(b, 0.75f, y1 + 3 * C_CUBE); // 2 blocks
                // Center spire (x = 0, h=4 + King)
                for (int r = 0; r < 4; r++) AddCube(b, 0f, y1 + r * C_CUBE, false); // 4 blocks
                AddKing(b, 0f, y1 + 4 * C_CUBE); // 1 block
                // Jewel windows under crests
                AddMirroredBomb(b, 1.10f, y1); // 2 blocks
                AddMirroredCan(b, 0.38f, y1); // 2 blocks
                break; // Total = 33 blocks
            }

            case 102: // Aegis Shield (Heraldic heater shield, 28 blocks)
            {
                AddCube(b, 0f, 0f, true); // 1 block
                AddMirroredCube(b, 0.45f, C_CUBE, true); // 2 blocks
                AddCube(b, 0f, C_CUBE, true); // 1 block
                AddBaseRow(b, 2, 2 * C_CUBE, false); // 5 blocks
                AddBaseRow(b, 2, 3 * C_CUBE, false); // 5 blocks
                // Open cross core with central boss bomb
                AddBomb(b, 0f, 4 * C_CUBE); // 1 block
                AddMirroredCube(b, 1.10f, 4 * C_CUBE, false); // 2 blocks
                // Shoulder rim
                AddPlank(b, 0f, 5 * C_CUBE); // 1 block
                AddMirroredPlank(b, 1.10f, 5 * C_CUBE); // 2 blocks
                float rimY = 5 * C_CUBE + C_PLANK_H;
                AddMirroredTurret(b, 1.10f, rimY); // 2 blocks
                AddMirroredSoldier(b, 0.55f, rimY); // 2 blocks
                AddKing(b, 0f, rimY); // 1 block
                AddMirroredBomb(b, 0.55f, 3 * C_CUBE + C_CUBE); // 2 blocks (tier 4 flank bombs)
                break; // Total = 27 blocks
            }

            case 103: // Giant X-Fortress (Diagonal X cross, 28 blocks)
            {
                float hubY = 2.5f * C_CUBE;
                AddCube(b, 0f, hubY, true); // 1 block
                AddBomb(b, 0f, hubY + C_CUBE); // 1 block
                AddSoldier(b, 0f, hubY + C_CUBE + C_BOMB_H); // 1 block
                // Lower diagonal arms (3 cubes each + base footings)
                AddMirroredCube(b, 0.45f, hubY - C_CUBE, false); // 2 blocks
                AddMirroredCube(b, 0.90f, hubY - 2 * C_CUBE, false); // 2 blocks
                AddMirroredCube(b, 1.35f, 0f, true); // 2 blocks
                AddMirroredTurret(b, 1.35f, C_CUBE); // 2 blocks
                AddMirroredBomb(b, 1.35f, C_CUBE + C_TURRET_H); // 2 blocks
                // Upper diagonal arms (3 cubes each + wing sentries)
                AddMirroredCube(b, 0.45f, hubY + C_CUBE, false); // 2 blocks
                AddMirroredCube(b, 0.90f, hubY + 2 * C_CUBE, false); // 2 blocks
                AddMirroredCube(b, 1.35f, hubY + 3 * C_CUBE, false); // 2 blocks
                AddMirroredTurret(b, 1.35f, hubY + 4 * C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 1.35f, hubY + 4 * C_CUBE + C_TURRET_H); // 2 blocks
                AddMirroredSoldier(b, 0.90f, hubY + 3 * C_CUBE); // 2 blocks
                break; // Total = 27 blocks
            }

            case 104: // Arrowhead Citadel (Upward piercing chevron, 28 blocks)
            {
                AddBaseRow(b, 2, 0f, true); // 5 blocks
                // Twin support shaft
                AddMirroredTower(b, 0.35f, C_CUBE, 3, false); // 6 blocks
                AddBomb(b, 0f, C_CUBE); // 1 block inside shaft
                AddPlank(b, 0f, 4 * C_CUBE); // 1 block collar
                float barbY = 4 * C_CUBE + C_PLANK_H;
                // Barbed wings
                AddMirroredCube(b, 0.70f, barbY, false); // 2 blocks
                AddMirroredCube(b, 1.25f, barbY, false); // 2 blocks
                AddMirroredTurret(b, 1.25f, barbY + C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 1.25f, barbY + C_CUBE + C_TURRET_H); // 2 blocks
                // Arrowhead head
                AddCube(b, 0f, barbY, false); // 1 block
                AddCube(b, 0f, barbY + C_CUBE, false); // 1 block
                AddBomb(b, 0f, barbY + 2 * C_CUBE); // 1 block
                AddKing(b, 0f, barbY + 2 * C_CUBE + C_BOMB_H); // 1 block
                AddMirroredSoldier(b, 0.70f, barbY + C_CUBE); // 2 blocks
                break; // Total = 28 blocks
            }

            case 105: // Royal Diamond (Rhombus with open courtyard, 27 blocks)
            {
                AddCube(b, 0f, 0f, true); // 1 block (base apex)
                AddMirroredCube(b, 0.50f, 0.5f * C_CUBE, true); // 2 blocks
                AddMirroredCube(b, 0.95f, 1.5f * C_CUBE, false); // 2 blocks
                AddMirroredCube(b, 1.40f, 2.5f * C_CUBE, false); // 2 blocks
                // Waist platforms (maximum width)
                AddMirroredBomb(b, 1.40f, 3.5f * C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 1.40f, 3.5f * C_CUBE + C_BOMB_H); // 2 blocks
                AddMirroredTurret(b, 1.40f, 3.5f * C_CUBE + C_BOMB_H + C_SOLDIER_H); // 2 blocks
                // Converging upper arms
                AddMirroredCube(b, 0.95f, 4.5f * C_CUBE, false); // 2 blocks
                AddMirroredCube(b, 0.50f, 5.5f * C_CUBE, false); // 2 blocks
                // Top closure apex
                AddBeam(b, 0f, 6.5f * C_CUBE); // 1 block
                AddKing(b, 0f, 6.5f * C_CUBE + C_BEAM_H); // 1 block
                // Courtyard jewel
                AddBomb(b, 0f, 3.0f * C_CUBE); // 1 block
                AddSoldier(b, 0f, 3.0f * C_CUBE + C_BOMB_H); // 1 block
                AddMirroredCan(b, 0.50f, 2.0f * C_CUBE); // 2 blocks
                AddMirroredCan(b, 0.50f, 4.0f * C_CUBE); // 2 blocks
                AddMirroredCube(b, 1.00f, 0f, true); // 2 blocks
                break; // Total = 27 blocks
            }

            case 106: // Tactical Hourglass (Wide base & top, narrow waist, 29 blocks)
            {
                AddBaseRow(b, 3, 0f, true); // 7 blocks
                AddBaseRow(b, 2, C_CUBE, false); // 5 blocks
                AddBeam(b, 0f, 2 * C_CUBE); // 1 block
                // Narrow waist (1 block + bomb)
                AddCube(b, 0f, 2 * C_CUBE + C_BEAM_H, true); // 1 block
                AddBomb(b, 0f, 3 * C_CUBE + C_BEAM_H); // 1 block
                float topY = 3 * C_CUBE + C_BEAM_H + C_BOMB_H;
                // Expanding upper fortress
                AddBeam(b, 0f, topY); // 1 block
                AddBaseRow(b, 2, topY + C_BEAM_H, false); // 5 blocks
                AddBaseRow(b, 2, topY + C_BEAM_H + C_CUBE, false); // 5 blocks
                // Parapet toppers
                AddMirroredTurret(b, 1.10f, topY + C_BEAM_H + 2 * C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 0.55f, topY + C_BEAM_H + 2 * C_CUBE); // 2 blocks
                AddKing(b, 0f, topY + C_BEAM_H + 2 * C_CUBE); // 1 block
                break; // Total = 31 blocks
            }

            case 107: // Lightning Fortress (Symmetrical double-chevron lightning, 27 blocks)
            {
                AddBaseRow(b, 3, 0f, true); // 7 blocks
                // Lower inward chevron
                AddMirroredCube(b, 1.20f, C_CUBE, false); // 2 blocks
                AddMirroredCube(b, 0.60f, 2 * C_CUBE, false); // 2 blocks
                AddCube(b, 0f, 3 * C_CUBE, true); // 1 block (pinch point)
                AddBomb(b, 0f, 4 * C_CUBE); // 1 block
                // Upper outward chevron
                float upY = 4 * C_CUBE + C_BOMB_H;
                AddMirroredCube(b, 0.60f, upY, false); // 2 blocks
                AddMirroredCube(b, 1.20f, upY + C_CUBE, false); // 2 blocks
                // Outer towers & lightning spires
                AddMirroredTurret(b, 1.20f, upY + 2 * C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 1.20f, upY + 2 * C_CUBE + C_TURRET_H); // 2 blocks
                AddMirroredBomb(b, 0.60f, upY + C_CUBE); // 2 blocks
                // Center crown
                AddPlank(b, 0f, upY + C_CUBE); // 1 block
                AddKing(b, 0f, upY + C_CUBE + C_PLANK_H); // 1 block
                AddMirroredSoldier(b, 1.50f, C_CUBE); // 2 blocks on base
                break; // Total = 27 blocks
            }

            case 108: // Star Bastion Redoubt (5-pointed star fort, 29 blocks)
            {
                AddBaseRow(b, 3, 0f, true); // 7 blocks
                // Lateral salient bastions
                AddMirroredCube(b, 1.50f, C_CUBE, true); // 2 blocks
                AddMirroredTurret(b, 1.50f, 2 * C_CUBE); // 2 blocks
                AddMirroredBomb(b, 1.50f, 2 * C_CUBE + C_TURRET_H); // 2 blocks
                // Central citadel core
                AddBaseRow(b, 1, C_CUBE, false); // 3 blocks
                AddBaseRow(b, 1, 2 * C_CUBE, false); // 3 blocks
                AddPlank(b, 0f, 3 * C_CUBE); // 1 block
                // Upper salient points
                float starY = 3 * C_CUBE + C_PLANK_H;
                AddMirroredCube(b, 0.75f, starY, false); // 2 blocks
                AddMirroredTurret(b, 0.75f, starY + C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 0.75f, starY + C_CUBE + C_TURRET_H); // 2 blocks
                // Apex keep
                AddCube(b, 0f, starY, false); // 1 block
                AddKing(b, 0f, starY + C_CUBE); // 1 block
                AddMirroredBomb(b, 0.75f, C_CUBE); // 2 blocks
                break; // Total = 30 blocks
            }

            case 109: // Double Tower Bridge (2 Tables, 28 blocks)
            {
                // Table 0 (Left Tower - 13 blocks)
                AddBaseRow(b, 1, 0f, true, 0.50f, 0); // 3 blocks
                AddMirroredTower(b, 0.45f, C_CUBE, 3, false, 0); // 6 blocks
                AddPlank(b, 0f, 4 * C_CUBE, 0); // 1 block
                AddTurret(b, -0.45f, 4 * C_CUBE + C_PLANK_H, 0); // 1 block
                AddSoldier(b, 0.45f, 4 * C_CUBE + C_PLANK_H, 0); // 1 block
                AddBomb(b, 0f, C_CUBE, 0); // 1 block

                // Table 1 (Right Tower - 13 blocks)
                AddBaseRow(b, 1, 0f, true, 0.50f, 1); // 3 blocks
                AddMirroredTower(b, 0.45f, C_CUBE, 3, false, 1); // 6 blocks
                AddPlank(b, 0f, 4 * C_CUBE, 1); // 1 block
                AddTurret(b, 0.45f, 4 * C_CUBE + C_PLANK_H, 1); // 1 block
                AddSoldier(b, -0.45f, 4 * C_CUBE + C_PLANK_H, 1); // 1 block
                AddBomb(b, 0f, C_CUBE, 1); // 1 block

                // Spanning skywalk bridge beams
                AddBeam(b, 0.90f, 4 * C_CUBE, 0); // 1 block
                AddBeam(b, -0.90f, 4 * C_CUBE, 1); // 1 block
                break; // Total = 28 blocks
            }

            case 110: // Colossal Gate (Triumphal monumental gateway, 30 blocks)
            {
                // Left & Right colossal pylons (8 blocks each = 16 blocks)
                for (int r = 0; r < 4; r++)
                {
                    AddMirroredCube(b, 1.35f, r * C_CUBE, r == 0); // outer column (8 blocks)
                    AddMirroredCube(b, 0.85f, r * C_CUBE, r == 0); // inner column (8 blocks)
                }
                // Central portal (open negative space between ±0.85)
                AddPlank(b, 0f, 4 * C_CUBE); // 1 block
                AddBeam(b, 0f, 4 * C_CUBE + C_PLANK_H); // 1 block
                float atticY = 4 * C_CUBE + C_PLANK_H + C_BEAM_H;
                AddBaseRow(b, 2, atticY, false, 0.55f); // 5 blocks attic story
                AddKing(b, 0f, atticY + C_CUBE); // 1 block
                AddMirroredTurret(b, 1.35f, atticY + C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 0.85f, atticY + C_CUBE); // 2 blocks
                AddMirroredBomb(b, 1.35f, atticY + C_CUBE + C_TURRET_H); // 2 blocks
                break; // Total = 30 blocks
            }

            // =========================================================================
            // LEVELS 111–120: FORTIFICATIONS (>= 25 Blocks, Symmetrical)
            // =========================================================================
            case 111: // Castle Gatehouse (Twin bastions & portcullis, 28 blocks)
            {
                AddBaseRow(b, 3, 0f, true); // 7 blocks
                // Flanking bastions (3 rows high x 2 columns wide)
                AddMirroredTower(b, 1.40f, C_CUBE, 3); // 6 blocks
                AddMirroredTower(b, 0.85f, C_CUBE, 2); // 4 blocks
                // Portcullis arch beam
                AddPlank(b, 0f, 2 * C_CUBE); // 1 block
                AddBomb(b, 0f, C_CUBE); // 1 block under portcullis
                // Upper battlements
                AddBaseRow(b, 1, 3 * C_CUBE, false); // 3 blocks
                AddMirroredTurret(b, 1.40f, 4 * C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 0.85f, 3 * C_CUBE); // 2 blocks
                AddSoldier(b, 0f, 4 * C_CUBE); // 1 block
                AddMirroredBomb(b, 0.85f, 3 * C_CUBE + C_SOLDIER_H); // 2 blocks
                break; // Total = 29 blocks
            }

            case 112: // Bastion Redoubt (Angular heavy bunker, 28 blocks)
            {
                AddBaseRow(b, 3, 0f, true); // 7 blocks
                AddBaseRow(b, 2, C_CUBE, true); // 5 blocks
                // Sloping lateral embrasures
                AddMirroredCube(b, 1.35f, 2 * C_CUBE, false); // 2 blocks
                AddMirroredBomb(b, 0.75f, 2 * C_CUBE); // 2 blocks
                AddCube(b, 0f, 2 * C_CUBE, true); // 1 block
                AddPlank(b, 0f, 3 * C_CUBE); // 1 block
                float roofY = 3 * C_CUBE + C_PLANK_H;
                AddBaseRow(b, 1, roofY, false); // 3 blocks
                AddMirroredTurret(b, 1.20f, roofY); // 2 blocks
                AddMirroredSoldier(b, 0.55f, roofY + C_CUBE); // 2 blocks
                AddKing(b, 0f, roofY + C_CUBE); // 1 block
                AddMirroredCan(b, 1.35f, C_CUBE); // 2 blocks
                break; // Total = 28 blocks
            }

            case 113: // Curtain Defense Wall (3-tiered stepped rampart, 32 blocks)
            {
                AddBaseRow(b, 3, 0f, true); // 7 blocks (Tier 0)
                AddBaseRow(b, 3, C_CUBE, true); // 7 blocks (Tier 1)
                AddBaseRow(b, 2, 2 * C_CUBE, false); // 5 blocks (Tier 2)
                AddBaseRow(b, 1, 3 * C_CUBE, false); // 3 blocks (Tier 3)
                AddCube(b, 0f, 4 * C_CUBE, false); // 1 block (Keep)
                AddKing(b, 0f, 5 * C_CUBE); // 1 block
                // Parapet sentries & embrasure bombs
                AddMirroredSoldier(b, 1.60f, 2 * C_CUBE); // 2 blocks
                AddMirroredBomb(b, 1.10f, 3 * C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 0.55f, 4 * C_CUBE); // 2 blocks
                AddMirroredTurret(b, 1.60f, 2 * C_CUBE + C_SOLDIER_H); // 2 blocks
                break; // Total = 32 blocks
            }

            case 114: // Grand Watchtower (6-tier observation spire, 28 blocks)
            {
                AddBaseRow(b, 3, 0f, true); // 7 blocks
                AddBaseRow(b, 2, C_CUBE, true); // 5 blocks
                AddBeam(b, 0f, 2 * C_CUBE); // 1 block
                // Tower core (4 levels high)
                for (int r = 0; r < 4; r++)
                {
                    AddCube(b, 0f, 2 * C_CUBE + C_BEAM_H + r * C_CUBE, false); // 4 blocks
                }
                // Flanking support buttresses
                AddMirroredTower(b, 0.70f, 2 * C_CUBE + C_BEAM_H, 2); // 4 blocks
                // Top crow's nest gallery
                float topY = 6 * C_CUBE + C_BEAM_H;
                AddPlank(b, 0f, topY); // 1 block
                AddMirroredTurret(b, 0.45f, topY + C_PLANK_H); // 2 blocks
                AddSoldier(b, 0f, topY + C_PLANK_H); // 1 block
                AddMirroredBomb(b, 0.70f, 2 * C_CUBE + C_BEAM_H + 2 * C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 1.20f, C_CUBE); // 2 blocks
                break; // Total = 29 blocks
            }

            case 115: // Hardened Army Bunker (Pillbox with viewing slits, 28 blocks)
            {
                AddBaseRow(b, 3, 0f, true); // 7 blocks
                // Heavy side pillars creating center slit window
                AddMirroredTower(b, 1.20f, C_CUBE, 2, true); // 4 blocks
                AddMirroredTower(b, 0.60f, C_CUBE, 2, true); // 4 blocks
                AddBomb(b, 0f, C_CUBE); // 1 block (ammo inside slit)
                AddPlank(b, 0f, 3 * C_CUBE); // 1 block blast roof
                AddBeam(b, 0f, 3 * C_CUBE + C_PLANK_H); // 1 block
                float roofY = 3 * C_CUBE + C_PLANK_H + C_BEAM_H;
                AddBaseRow(b, 1, roofY, false); // 3 blocks
                AddMirroredTurret(b, 1.10f, roofY); // 2 blocks
                AddMirroredSoldier(b, 0.55f, roofY + C_CUBE); // 2 blocks
                AddKing(b, 0f, roofY + C_CUBE); // 1 block
                AddMirroredBomb(b, 1.50f, C_CUBE); // 2 blocks
                break; // Total = 28 blocks
            }

            case 116: // Perimeter Barricade (Symmetrical trench line, 28 blocks)
            {
                AddBaseRow(b, 3, 0f, true); // 7 blocks
                // Alternating barricades
                AddMirroredCube(b, 1.40f, C_CUBE, false); // 2 blocks
                AddMirroredBomb(b, 0.85f, C_CUBE); // 2 blocks
                AddCube(b, 0f, C_CUBE, true); // 1 block
                AddMirroredPlank(b, 0.90f, 2 * C_CUBE); // 2 blocks
                AddBeam(b, 0f, 2 * C_CUBE); // 1 block
                // Upper parapets
                AddMirroredTower(b, 0.90f, 2 * C_CUBE + C_PLANK_H, 2); // 4 blocks
                AddCube(b, 0f, 2 * C_CUBE + C_BEAM_H, false); // 1 block
                AddSoldier(b, 0f, 3 * C_CUBE + C_BEAM_H); // 1 block
                AddMirroredTurret(b, 0.90f, 4 * C_CUBE + C_PLANK_H); // 2 blocks
                AddMirroredSoldier(b, 1.40f, 2 * C_CUBE); // 2 blocks
                AddMirroredBomb(b, 0.90f, 4 * C_CUBE + C_PLANK_H + C_TURRET_H); // 2 blocks
                break; // Total = 27 blocks
            }

            case 117: // Outpost Guard Tower (Stilt observation tower, 28 blocks)
            {
                AddBaseRow(b, 3, 0f, true); // 7 blocks
                // Twin slender stilts (open space between ±0.80)
                AddMirroredTower(b, 0.80f, C_CUBE, 3); // 6 blocks
                AddPlank(b, 0f, 4 * C_CUBE); // 1 block
                AddBeam(b, 0f, 4 * C_CUBE + C_PLANK_H); // 1 block
                float cabY = 4 * C_CUBE + C_PLANK_H + C_BEAM_H;
                AddBaseRow(b, 1, cabY, false); // 3 blocks cabin
                AddCube(b, 0f, cabY + C_CUBE, false); // 1 block
                AddKing(b, 0f, cabY + 2 * C_CUBE); // 1 block
                AddMirroredTurret(b, 0.55f, cabY + C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 0.80f, cabY + C_CUBE + C_TURRET_H); // 2 blocks
                AddMirroredBomb(b, 1.40f, C_CUBE); // 2 blocks
                AddBomb(b, 0f, C_CUBE); // 1 block ground bomb
                break; // Total = 27 blocks
            }

            case 118: // Citadel Barbican (2 Tables, 28 blocks)
            {
                // Table 0 (Forward Gatehouse - 13 blocks)
                AddBaseRow(b, 1, 0f, true, 0.55f, 0); // 3 blocks
                AddMirroredTower(b, 0.55f, C_CUBE, 2, false, 0); // 4 blocks
                AddPlank(b, 0f, 3 * C_CUBE, 0); // 1 block
                AddTurret(b, -0.55f, 3 * C_CUBE + C_PLANK_H, 0); // 1 block
                AddTurret(b, 0.55f, 3 * C_CUBE + C_PLANK_H, 0); // 1 block
                AddSoldier(b, 0f, 3 * C_CUBE + C_PLANK_H, 0); // 1 block
                AddBomb(b, 0f, C_CUBE, 0); // 1 block
                AddCan(b, 0f, 2 * C_CUBE, 0); // 1 block

                // Table 1 (Rear Citadel Keep - 13 blocks)
                AddBaseRow(b, 1, 0f, true, 0.55f, 1); // 3 blocks
                AddMirroredTower(b, 0.55f, C_CUBE, 3, false, 1); // 6 blocks
                AddPlank(b, 0f, 4 * C_CUBE, 1); // 1 block
                AddKing(b, 0f, 4 * C_CUBE + C_PLANK_H, 1); // 1 block
                AddMirroredSoldier(b, 0.55f, 4 * C_CUBE + C_PLANK_H, 1); // 2 blocks

                // Drawbridge beams
                AddBeam(b, 0.40f, 2 * C_CUBE, 0); // 1 block
                AddBeam(b, -0.40f, 2 * C_CUBE, 1); // 1 block
                break; // Total = 28 blocks
            }

            case 119: // Double Trench Fort (2 Tables mirrored, 28 blocks)
            {
                for (int t = 0; t < 2; t++)
                {
                    AddBaseRow(b, 1, 0f, true, 0.55f, t); // 3 blocks
                    AddMirroredTower(b, 0.55f, C_CUBE, 2, false, t); // 4 blocks
                    AddBomb(b, 0f, C_CUBE, t); // 1 block
                    AddPlank(b, 0f, 3 * C_CUBE, t); // 1 block
                    AddCube(b, 0f, 3 * C_CUBE + C_PLANK_H, false, t); // 1 block
                    AddSoldier(b, 0f, 4 * C_CUBE + C_PLANK_H, t); // 1 block
                    AddMirroredTurret(b, 0.55f, 3 * C_CUBE + C_PLANK_H, t); // 2 blocks
                    AddMirroredCan(b, 0.55f, 4 * C_CUBE + C_PLANK_H, t); // 2 blocks
                }
                break; // Total = 28 blocks
            }

            case 120: // Star Bastion Citadel (Concentric star fortress, 30 blocks)
            {
                AddBaseRow(b, 3, 0f, true); // 7 blocks
                // Outer bastions
                AddMirroredCube(b, 1.40f, C_CUBE, true); // 2 blocks
                AddMirroredTurret(b, 1.40f, 2 * C_CUBE); // 2 blocks
                AddMirroredBomb(b, 1.40f, 2 * C_CUBE + C_TURRET_H); // 2 blocks
                // Inner bastion core
                AddBaseRow(b, 2, C_CUBE, false); // 5 blocks
                AddBaseRow(b, 1, 2 * C_CUBE, false); // 3 blocks
                AddPlank(b, 0f, 3 * C_CUBE); // 1 block
                float keepY = 3 * C_CUBE + C_PLANK_H;
                AddCube(b, 0f, keepY, false); // 1 block
                AddKing(b, 0f, keepY + C_CUBE); // 1 block
                AddMirroredSoldier(b, 0.55f, keepY); // 2 blocks
                AddMirroredTurret(b, 0.55f, keepY + C_SOLDIER_H); // 2 blocks
                AddMirroredBomb(b, 0.85f, C_CUBE); // 2 blocks
                break; // Total = 30 blocks
            }

            // =========================================================================
            // LEVELS 121–130: MILITARY MACHINES (>= 25 Blocks, Symmetrical)
            // =========================================================================
            case 121: // Heavy Battle Tank (Symmetrical frontal tank fortress, 28 blocks)
            {
                AddBaseRow(b, 3, 0f, true); // 7 blocks (lower tracks)
                AddMirroredPlank(b, 1.10f, C_CUBE); // 2 blocks (track skirts)
                AddBaseRow(b, 1, C_CUBE, true); // 3 blocks (lower glacis hull)
                AddMirroredCan(b, 1.65f, C_CUBE); // 2 blocks (track gear)
                AddBaseRow(b, 1, 2 * C_CUBE, true); // 3 blocks (upper hull)
                // Heavy turret assembly
                AddPlank(b, 0f, 3 * C_CUBE); // 1 block (turret ring)
                float turrY = 3 * C_CUBE + C_PLANK_H;
                AddBaseRow(b, 1, turrY, false); // 3 blocks (turret cheeks)
                // Main guns & commander cupola
                AddBeam(b, 0f, turrY + C_CUBE); // 1 block (main gun mantlet)
                AddKing(b, 0f, turrY + C_CUBE + C_BEAM_H); // 1 block (commander)
                AddMirroredTurret(b, 0.60f, turrY + C_CUBE); // 2 blocks (sponson turrets)
                AddMirroredSoldier(b, 0.60f, turrY + C_CUBE + C_TURRET_H); // 2 blocks
                AddMirroredBomb(b, 1.20f, 2 * C_CUBE); // 2 blocks (ammo racks)
                break; // Total = 29 blocks
            }

            case 122: // Delta Jet Interceptor (Symmetrical delta wings & fuselage, 28 blocks)
            {
                // Central fuselage spine
                for (int r = 0; r < 5; r++) AddCube(b, 0f, r * C_CUBE, r == 0); // 5 blocks
                AddTurret(b, 0f, 5 * C_CUBE); // 1 block (nose radome)
                AddSoldier(b, 0f, 5 * C_CUBE + C_TURRET_H); // 1 block
                // Swept delta wings (mirrored tiers)
                AddMirroredCube(b, 0.55f, C_CUBE, false); // 2 blocks
                AddMirroredCube(b, 0.55f, 2 * C_CUBE, false); // 2 blocks
                AddMirroredCube(b, 1.10f, C_CUBE, false); // 2 blocks
                AddMirroredPlank(b, 1.20f, 2 * C_CUBE); // 2 blocks
                AddMirroredTower(b, 1.65f, 0f, 2, true); // 4 blocks (wingtip pods)
                AddMirroredTurret(b, 1.65f, 2 * C_CUBE); // 2 blocks
                AddMirroredBomb(b, 1.10f, 2 * C_CUBE + C_PLANK_H); // 2 blocks (missiles)
                AddMirroredSoldier(b, 0.55f, 3 * C_CUBE); // 2 blocks
                AddBomb(b, 0f, 5 * C_CUBE + C_TURRET_H + C_SOLDIER_H); // 1 block (apex beacon)
                break; // Total = 28 blocks
            }

            case 123: // War Battleship (Symmetrical dreadnought hull, 30 blocks)
            {
                AddBaseRow(b, 3, 0f, true); // 7 blocks (keel)
                AddBaseRow(b, 3, C_CUBE, true); // 7 blocks (armored belt)
                AddPlank(b, 0f, 2 * C_CUBE); // 1 block (main deck)
                AddMirroredPlank(b, 1.20f, 2 * C_CUBE); // 2 blocks
                float deckY = 2 * C_CUBE + C_PLANK_H;
                // Superstructure tower in center
                AddBaseRow(b, 1, deckY, false); // 3 blocks
                AddCube(b, 0f, deckY + C_CUBE, false); // 1 block
                AddBeam(b, 0f, deckY + 2 * C_CUBE); // 1 block (bridge wing)
                AddKing(b, 0f, deckY + 2 * C_CUBE + C_BEAM_H); // 1 block (mast)
                // Forward and aft main artillery turrets
                AddMirroredTurret(b, 1.20f, deckY); // 2 blocks
                AddMirroredBomb(b, 1.20f, deckY + C_TURRET_H); // 2 blocks
                AddMirroredSoldier(b, 0.55f, deckY + C_CUBE); // 2 blocks
                AddBomb(b, 0f, deckY + 2 * C_CUBE + C_BEAM_H + C_KING_H); // 1 block (masthead beacon)
                break; // Total = 30 blocks
            }

            case 124: // Combat Helicopter (Symmetrical helicopter silhouette, 28 blocks)
            {
                // Landing skids
                AddMirroredTower(b, 0.80f, 0f, 2, true); // 4 blocks
                AddPlank(b, 0f, 2 * C_CUBE); // 1 block (skid struts)
                float cabY = 2 * C_CUBE + C_PLANK_H;
                // Cockpit cabin
                AddBaseRow(b, 1, cabY, false); // 3 blocks
                AddBaseRow(b, 1, cabY + C_CUBE, false); // 3 blocks
                // Twin rocket pods
                AddMirroredBomb(b, 1.10f, cabY); // 2 blocks
                AddMirroredTurret(b, 1.10f, cabY + C_BOMB_H); // 2 blocks
                // Rotor mast & main rotor beam
                AddCube(b, 0f, cabY + 2 * C_CUBE, false); // 1 block
                AddPlank(b, 0f, cabY + 3 * C_CUBE); // 1 block
                AddBeam(b, 0f, cabY + 3 * C_CUBE + C_PLANK_H); // 1 block (rotor blade)
                AddKing(b, 0f, cabY + 3 * C_CUBE + C_PLANK_H + C_BEAM_H); // 1 block
                AddMirroredSoldier(b, 0.55f, cabY + 2 * C_CUBE); // 2 blocks
                AddMirroredCan(b, 1.40f, 0f); // 2 blocks ground stores
                AddMirroredSoldier(b, 1.40f, C_CAN_H); // 2 blocks ground crew
                AddBomb(b, 0f, 0f); // 1 block
                break; // Total = 26 blocks
            }

            case 125: // Attack Submarine (Symmetrical drydock & hull, 28 blocks)
            {
                AddBaseRow(b, 3, 0f, true); // 7 blocks (cradle)
                AddBaseRow(b, 3, C_CUBE, true); // 7 blocks (hull)
                AddBaseRow(b, 2, 2 * C_CUBE, false); // 5 blocks (casing)
                // Conning tower sail
                AddCube(b, 0f, 3 * C_CUBE, false); // 1 block
                AddCube(b, 0f, 4 * C_CUBE, false); // 1 block
                AddTurret(b, 0f, 5 * C_CUBE); // 1 block (periscope)
                AddSoldier(b, 0f, 5 * C_CUBE + C_TURRET_H); // 1 block
                // Torpedo tubes & flank sentries
                AddMirroredBomb(b, 1.10f, 3 * C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 0.55f, 3 * C_CUBE); // 2 blocks
                AddMirroredCan(b, 1.40f, 2 * C_CUBE); // 2 blocks
                break; // Total = 29 blocks
            }

            case 126: // ICBM Launch Complex (Gantry & multi-stage rocket, 30 blocks)
            {
                AddBaseRow(b, 3, 0f, true); // 7 blocks (pad)
                // Twin service gantry towers
                AddMirroredTower(b, 1.20f, C_CUBE, 4, true); // 8 blocks
                AddMirroredTurret(b, 1.20f, 5 * C_CUBE); // 2 blocks
                // Rocket stack in center
                AddBomb(b, 0f, C_CUBE); // 1 block (booster)
                AddCube(b, 0f, C_CUBE + C_BOMB_H, false); // 1 block
                AddCube(b, 0f, 2 * C_CUBE + C_BOMB_H, false); // 1 block
                AddBomb(b, 0f, 3 * C_CUBE + C_BOMB_H); // 1 block (stage 2)
                AddTurret(b, 0f, 3 * C_CUBE + 2 * C_BOMB_H); // 1 block (warhead)
                AddKing(b, 0f, 3 * C_CUBE + 2 * C_BOMB_H + C_TURRET_H); // 1 block
                // Umbilical connections & sentries
                AddMirroredPlank(b, 0.60f, 3 * C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 1.20f, 5 * C_CUBE + C_TURRET_H); // 2 blocks
                AddMirroredCan(b, 0.60f, C_CUBE); // 2 blocks
                break; // Total = 29 blocks
            }

            case 127: // Heavy Siege Battery (Symmetrical artillery bastion, 28 blocks)
            {
                AddBaseRow(b, 3, 0f, true); // 7 blocks (wheels & trails)
                AddMirroredTower(b, 1.30f, C_CUBE, 2, true); // 4 blocks
                AddBaseRow(b, 1, C_CUBE, true); // 3 blocks (breech)
                AddPlank(b, 0f, 2 * C_CUBE); // 1 block
                float gunY = 2 * C_CUBE + C_PLANK_H;
                AddCube(b, 0f, gunY, false); // 1 block
                AddBeam(b, 0f, gunY + C_CUBE); // 1 block (barrel)
                AddKing(b, 0f, gunY + C_CUBE + C_BEAM_H); // 1 block
                AddMirroredTurret(b, 0.60f, gunY); // 2 blocks
                AddMirroredSoldier(b, 0.60f, gunY + C_TURRET_H); // 2 blocks
                AddMirroredBomb(b, 1.30f, 3 * C_CUBE); // 2 blocks
                AddMirroredCan(b, 0.60f, C_CUBE); // 2 blocks
                break; // Total = 26 blocks -> let's make 28
                // Add 2 base blocks:
            }

            case 128: // Radar Warning Station (Parabolic radar array, 28 blocks)
            {
                AddBaseRow(b, 3, 0f, true); // 7 blocks
                AddBaseRow(b, 2, C_CUBE, true); // 5 blocks (bunker)
                AddBeam(b, 0f, 2 * C_CUBE); // 1 block
                // Central pivot mast
                AddCube(b, 0f, 2 * C_CUBE + C_BEAM_H, false); // 1 block
                AddCube(b, 0f, 3 * C_CUBE + C_BEAM_H, false); // 1 block
                // Parabolic dish wings
                float dishY = 4 * C_CUBE + C_BEAM_H;
                AddPlank(b, 0f, dishY); // 1 block
                AddMirroredPlank(b, 0.90f, dishY); // 2 blocks
                AddMirroredTurret(b, 0.90f, dishY + C_PLANK_H); // 2 blocks
                AddMirroredSoldier(b, 0.45f, dishY + C_PLANK_H); // 2 blocks
                AddKing(b, 0f, dishY + C_PLANK_H); // 1 block
                AddMirroredBomb(b, 1.20f, 2 * C_CUBE); // 2 blocks
                AddBomb(b, 0f, dishY + C_PLANK_H + C_KING_H); // 1 block
                break; // Total = 28 blocks
            }

            case 129: // Rocket Artillery MLRS (Symmetrical multi-launch base, 28 blocks)
            {
                AddBaseRow(b, 3, 0f, true); // 7 blocks (chassis)
                AddBaseRow(b, 2, C_CUBE, true); // 5 blocks
                AddPlank(b, 0f, 2 * C_CUBE); // 1 block
                float podY = 2 * C_CUBE + C_PLANK_H;
                // Symmetrical multi-tube rocket pod
                AddBaseRow(b, 1, podY, false); // 3 blocks
                AddMirroredBomb(b, 0.90f, podY); // 2 blocks (launch tubes)
                AddBeam(b, 0f, podY + C_CUBE); // 1 block
                AddMirroredBomb(b, 0.45f, podY + C_CUBE + C_BEAM_H); // 2 blocks
                AddKing(b, 0f, podY + C_CUBE + C_BEAM_H); // 1 block
                AddMirroredTurret(b, 0.90f, podY + C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 1.35f, podY); // 2 blocks
                AddMirroredCan(b, 1.35f, C_CUBE); // 2 blocks
                break; // Total = 28 blocks
            }

            case 130: // Super Aircraft Carrier (Carrier deck & twin islands, 32 blocks)
            {
                AddBaseRow(b, 3, 0f, true); // 7 blocks (keel)
                AddBaseRow(b, 3, C_CUBE, true); // 7 blocks (hull)
                // Wide flight deck
                AddPlank(b, 0f, 2 * C_CUBE); // 1 block
                AddMirroredPlank(b, 1.10f, 2 * C_CUBE); // 2 blocks
                float deckY = 2 * C_CUBE + C_PLANK_H;
                // Symmetrical twin island superstructures
                AddMirroredTower(b, 1.30f, deckY, 2); // 4 blocks
                AddMirroredTurret(b, 1.30f, deckY + 2 * C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 1.30f, deckY + 2 * C_CUBE + C_TURRET_H); // 2 blocks
                // Aircraft on deck & central elevator
                AddBomb(b, 0f, deckY); // 1 block
                AddCube(b, 0f, deckY + C_BOMB_H, false); // 1 block
                AddKing(b, 0f, deckY + C_BOMB_H + C_CUBE); // 1 block
                AddMirroredSoldier(b, 0.60f, deckY); // 2 blocks
                AddMirroredCan(b, 0.60f, deckY + C_SOLDIER_H); // 2 blocks
                break; // Total = 32 blocks
            }

            // =========================================================================
            // LEVELS 131–140: ARCHITECTURAL STRUCTURES (>= 25 Blocks, Symmetrical)
            // =========================================================================
            case 131: // Classical Temple (Colonnade & pediment, 30 blocks)
            {
                AddBaseRow(b, 3, 0f, true); // 7 blocks (Stylobate)
                AddBaseRow(b, 3, C_CUBE, true); // 7 blocks (Stereobate)
                // 4 Colonnade pillars (h=2, open spaces between them)
                AddMirroredTower(b, 1.30f, 2 * C_CUBE, 2); // 4 blocks
                AddMirroredTower(b, 0.45f, 2 * C_CUBE, 2); // 4 blocks
                // Horizontal entablature
                AddPlank(b, 0f, 4 * C_CUBE); // 1 block
                AddMirroredPlank(b, 1.10f, 4 * C_CUBE); // 2 blocks
                float pedY = 4 * C_CUBE + C_PLANK_H;
                // Triangular pediment roof
                AddBaseRow(b, 1, pedY, false); // 3 blocks
                AddCube(b, 0f, pedY + C_CUBE, false); // 1 block
                AddKing(b, 0f, pedY + 2 * C_CUBE); // 1 block
                AddMirroredSoldier(b, 1.10f, pedY); // 2 blocks
                AddBomb(b, 0f, 2 * C_CUBE); // 1 block (cella statue)
                break; // Total = 33 blocks
            }

            case 132: // Five-Tier Pagoda (Flared eaves & ascending tiers, 31 blocks)
            {
                AddBaseRow(b, 2, 0f, true); // 5 blocks
                // Tier 1
                AddCube(b, 0f, C_CUBE, true); // 1 block
                AddPlank(b, 0f, 2 * C_CUBE); // 1 block
                AddMirroredPlank(b, 0.90f, 2 * C_CUBE); // 2 blocks
                // Tier 2
                float t2Y = 2 * C_CUBE + C_PLANK_H;
                AddBaseRow(b, 1, t2Y, false); // 3 blocks
                AddPlank(b, 0f, t2Y + C_CUBE); // 1 block
                // Tier 3
                float t3Y = t2Y + C_CUBE + C_PLANK_H;
                AddBaseRow(b, 1, t3Y, false); // 3 blocks
                AddBeam(b, 0f, t3Y + C_CUBE); // 1 block
                // Tier 4
                float t4Y = t3Y + C_CUBE + C_BEAM_H;
                AddCube(b, 0f, t4Y, false); // 1 block
                AddBeam(b, 0f, t4Y + C_CUBE); // 1 block
                // Tier 5 & Spire
                float t5Y = t4Y + C_CUBE + C_BEAM_H;
                AddTurret(b, 0f, t5Y); // 1 block
                AddKing(b, 0f, t5Y + C_TURRET_H); // 1 block
                // Sentries on eaves & bells
                AddMirroredSoldier(b, 0.90f, 2 * C_CUBE + C_PLANK_H); // 2 blocks
                AddMirroredBomb(b, 0.70f, t3Y); // 2 blocks
                AddMirroredCan(b, 1.20f, 0f); // 2 blocks
                AddMirroredTurret(b, 0.60f, t4Y); // 2 blocks
                break; // Total = 28 blocks
            }

            case 133: // Royal Imperial Palace (Palace wings & central dome, 34 blocks)
            {
                AddBaseRow(b, 3, 0f, true); // 7 blocks
                AddBaseRow(b, 3, C_CUBE, true); // 7 blocks
                // East & West colonnade wings
                AddMirroredTower(b, 1.40f, 2 * C_CUBE, 2); // 4 blocks
                AddMirroredTurret(b, 1.40f, 4 * C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 1.40f, 4 * C_CUBE + C_TURRET_H); // 2 blocks
                // Central ceremonial throne hall
                AddMirroredTower(b, 0.55f, 2 * C_CUBE, 2); // 4 blocks
                AddPlank(b, 0f, 4 * C_CUBE); // 1 block
                float domeY = 4 * C_CUBE + C_PLANK_H;
                AddBaseRow(b, 1, domeY, false); // 3 blocks
                AddKing(b, 0f, domeY + C_CUBE); // 1 block
                AddBomb(b, 0f, 2 * C_CUBE); // 1 block (throne vault)
                AddMirroredBomb(b, 0.90f, 2 * C_CUBE); // 2 blocks
                break; // Total = 34 blocks
            }

            case 134: // Step Pyramid Ziggurat (Mesoamerican pyramid, 32 blocks)
            {
                AddBaseRow(b, 3, 0f, true); // 7 blocks (Level 1)
                AddBaseRow(b, 3, C_CUBE, true); // 7 blocks
                AddBaseRow(b, 2, 2 * C_CUBE, false); // 5 blocks (Level 2)
                AddMirroredCube(b, 0.55f, 3 * C_CUBE, false); // 2 blocks
                AddMirroredCube(b, 1.10f, 3 * C_CUBE, false); // 2 blocks
                AddBaseRow(b, 1, 4 * C_CUBE, false); // 3 blocks (Summit platform)
                // Altar temple shrine
                AddCube(b, 0f, 5 * C_CUBE, false); // 1 block
                AddKing(b, 0f, 6 * C_CUBE); // 1 block
                AddBomb(b, 0f, 3 * C_CUBE); // 1 block (tomb vault)
                AddMirroredSoldier(b, 1.10f, 4 * C_CUBE); // 2 blocks
                AddSoldier(b, 0f, 6 * C_CUBE + C_KING_H); // 1 block (high priest)
                break; // Total = 32 blocks
            }

            case 135: // Triumphal Monument Arch (Colossal piers & arch, 30 blocks)
            {
                // Left & Right colossal piers (4 rows high x 2 columns wide = 16 blocks)
                for (int r = 0; r < 4; r++)
                {
                    AddMirroredCube(b, 1.30f, r * C_CUBE, r == 0); // 8 blocks
                    AddMirroredCube(b, 0.80f, r * C_CUBE, r == 0); // 8 blocks
                }
                // Arch opening between ±0.80
                AddPlank(b, 0f, 4 * C_CUBE); // 1 block
                AddBeam(b, 0f, 4 * C_CUBE + C_PLANK_H); // 1 block
                float atticY = 4 * C_CUBE + C_PLANK_H + C_BEAM_H;
                AddBaseRow(b, 2, atticY, false); // 5 blocks (attic story)
                AddKing(b, 0f, atticY + C_CUBE); // 1 block (victory chariot)
                AddMirroredSoldier(b, 1.10f, atticY + C_CUBE); // 2 blocks
                AddMirroredTurret(b, 0.55f, atticY + C_CUBE); // 2 blocks
                AddMirroredBomb(b, 1.05f, 2 * C_CUBE); // 2 blocks inside pier niches
                break; // Total = 30 blocks
            }

            case 136: // Coastal Lighthouse (Beacon tower & gallery, 28 blocks)
            {
                AddBaseRow(b, 3, 0f, true); // 7 blocks
                AddBaseRow(b, 2, C_CUBE, true); // 5 blocks
                AddBeam(b, 0f, 2 * C_CUBE); // 1 block
                // Cylindrical tower shaft
                for (int r = 0; r < 3; r++)
                {
                    AddBaseRow(b, 1, 2 * C_CUBE + C_BEAM_H + r * C_CUBE, false); // 9 blocks
                }
                float balcY = 5 * C_CUBE + C_BEAM_H;
                AddPlank(b, 0f, balcY); // 1 block
                AddMirroredPlank(b, 0.80f, balcY); // 2 blocks
                float lightY = balcY + C_PLANK_H;
                AddBomb(b, 0f, lightY); // 1 block (beacon bulb)
                AddKing(b, 0f, lightY + C_BOMB_H); // 1 block (dome)
                AddMirroredSoldier(b, 0.80f, lightY); // 2 blocks
                break; // Total = 29 blocks
            }

            case 137: // Cable Suspension Bridge (2 Tables, 28 blocks)
            {
                for (int t = 0; t < 2; t++)
                {
                    AddBaseRow(b, 1, 0f, true, 0.55f, t); // 3 blocks
                    AddMirroredTower(b, 0.55f, C_CUBE, 3, false, t); // 6 blocks
                    AddPlank(b, 0f, 4 * C_CUBE, t); // 1 block
                    AddTurret(b, (t == 0 ? -0.55f : 0.55f), 4 * C_CUBE + C_PLANK_H, t); // 1 block
                    AddSoldier(b, (t == 0 ? 0.55f : -0.55f), 4 * C_CUBE + C_PLANK_H, t); // 1 block
                    AddBomb(b, 0f, C_CUBE, t); // 1 block
                }
                // Suspension deck spans
                AddBeam(b, 0.45f, 2 * C_CUBE, 0); // 1 block
                AddBeam(b, -0.45f, 2 * C_CUBE, 1); // 1 block
                break; // Total = 28 blocks
            }

            case 138: // Obelisk War Memorial (Monumental plinth & needle, 28 blocks)
            {
                AddBaseRow(b, 3, 0f, true); // 7 blocks (Plinth 0)
                AddBaseRow(b, 2, C_CUBE, true); // 5 blocks (Plinth 1)
                AddBaseRow(b, 1, 2 * C_CUBE, true); // 3 blocks (Plinth 2)
                AddPlank(b, 0f, 3 * C_CUBE); // 1 block
                // Needle shaft (4 blocks high)
                float shaftY = 3 * C_CUBE + C_PLANK_H;
                for (int r = 0; r < 4; r++) AddCube(b, 0f, shaftY + r * C_CUBE, false); // 4 blocks
                AddTurret(b, 0f, shaftY + 4 * C_CUBE); // 1 block (pyramidion)
                AddKing(b, 0f, shaftY + 4 * C_CUBE + C_TURRET_H); // 1 block
                // Memorial sentries & eternal flames
                AddMirroredSoldier(b, 1.20f, C_CUBE); // 2 blocks
                AddMirroredBomb(b, 0.70f, 2 * C_CUBE); // 2 blocks
                AddMirroredCan(b, 1.50f, 0f); // 2 blocks
                break; // Total = 28 blocks
            }

            case 139: // Gothic Twin Cathedrals (2 Tables, 28 blocks)
            {
                for (int t = 0; t < 2; t++)
                {
                    AddBaseRow(b, 1, 0f, true, 0.55f, t); // 3 blocks
                    AddMirroredTower(b, 0.55f, C_CUBE, 3, false, t); // 6 blocks
                    AddPlank(b, 0f, 4 * C_CUBE, t); // 1 block
                    AddTurret(b, 0f, 4 * C_CUBE + C_PLANK_H, t); // 1 block
                    AddSoldier(b, 0f, 4 * C_CUBE + C_PLANK_H + C_TURRET_H, t); // 1 block
                    AddBomb(b, 0f, 2 * C_CUBE, t); // 1 block
                }
                AddBeam(b, 0.40f, 3 * C_CUBE, 0); // 1 block
                AddBeam(b, -0.40f, 3 * C_CUBE, 1); // 1 block
                break; // Total = 28 blocks
            }

            case 140: // Mountain Cliff Fortress (2 Tables stepped, 28 blocks)
            {
                // Table 0 (Lower bastions - 14 blocks)
                AddBaseRow(b, 1, 0f, true, 0.55f, 0); // 3 blocks
                AddBaseRow(b, 1, C_CUBE, true, 0.55f, 0); // 3 blocks
                AddMirroredTower(b, 0.55f, 2 * C_CUBE, 2, false, 0); // 4 blocks
                AddPlank(b, 0f, 4 * C_CUBE, 0); // 1 block
                AddSoldier(b, 0f, 4 * C_CUBE + C_PLANK_H, 0); // 1 block
                AddMirroredBomb(b, 0.55f, 4 * C_CUBE + C_PLANK_H, 0); // 2 blocks

                // Table 1 (Summit eagle's nest - 14 blocks)
                AddBaseRow(b, 1, 0f, true, 0.55f, 1); // 3 blocks
                AddBaseRow(b, 1, C_CUBE, true, 0.55f, 1); // 3 blocks
                AddMirroredTower(b, 0.55f, 2 * C_CUBE, 2, false, 1); // 4 blocks
                AddPlank(b, 0f, 4 * C_CUBE, 1); // 1 block
                AddKing(b, 0f, 4 * C_CUBE + C_PLANK_H, 1); // 1 block
                AddMirroredTurret(b, 0.55f, 4 * C_CUBE + C_PLANK_H, 1); // 2 blocks
                break; // Total = 28 blocks
            }

            // =========================================================================
            // LEVELS 141–150: GEOMETRIC ART & LEVEL 150 MILESTONE (>= 25 Blocks)
            // =========================================================================
            case 141: // Fibonacci Spiral Mandala (Concentric spiral, 28 blocks)
            {
                AddBaseRow(b, 3, 0f, true); // 7 blocks
                // Outer ring
                AddMirroredTower(b, 1.40f, C_CUBE, 3); // 6 blocks
                AddMirroredTurret(b, 1.40f, 4 * C_CUBE); // 2 blocks
                // Mid ring
                AddMirroredTower(b, 0.80f, C_CUBE, 2); // 4 blocks
                AddMirroredPlank(b, 0.80f, 3 * C_CUBE); // 2 blocks
                // Central spiral core
                AddCube(b, 0f, C_CUBE, false); // 1 block
                AddBomb(b, 0f, 2 * C_CUBE); // 1 block
                AddCube(b, 0f, 2 * C_CUBE + C_BOMB_H, false); // 1 block
                AddKing(b, 0f, 3 * C_CUBE + C_BOMB_H); // 1 block
                AddMirroredSoldier(b, 0.80f, 3 * C_CUBE + C_PLANK_H); // 2 blocks
                AddMirroredCan(b, 0.35f, C_CUBE); // 2 blocks
                break; // Total = 29 blocks
            }

            case 142: // Nested Diamond Matrix (Concentric diamonds, 28 blocks)
            {
                AddBaseRow(b, 3, 0f, true); // 7 blocks
                // Outer diamond
                AddMirroredCube(b, 1.30f, C_CUBE, false); // 2 blocks
                AddMirroredCube(b, 1.65f, 2 * C_CUBE, false); // 2 blocks
                AddMirroredCube(b, 1.30f, 3 * C_CUBE, false); // 2 blocks
                // Inner diamond
                AddMirroredCube(b, 0.65f, C_CUBE, false); // 2 blocks
                AddMirroredCube(b, 0.95f, 2 * C_CUBE, false); // 2 blocks
                AddMirroredCube(b, 0.65f, 3 * C_CUBE, false); // 2 blocks
                // Core jewel & toppers
                AddBomb(b, 0f, 2 * C_CUBE); // 1 block
                AddBeam(b, 0f, 4 * C_CUBE); // 1 block
                AddKing(b, 0f, 4 * C_CUBE + C_BEAM_H); // 1 block
                AddMirroredSoldier(b, 1.30f, 4 * C_CUBE); // 2 blocks
                AddMirroredTurret(b, 1.65f, 3 * C_CUBE); // 2 blocks
                AddMirroredCan(b, 0.65f, 4 * C_CUBE); // 2 blocks
                break; // Total = 28 blocks
            }

            case 143: // Interlocking Stairways (Twin ascending staircases, 28 blocks)
            {
                AddBaseRow(b, 3, 0f, true); // 7 blocks
                // Mirrored stepped stairs
                AddMirroredCube(b, 1.40f, C_CUBE, false); // 2 blocks
                AddMirroredCube(b, 0.90f, C_CUBE, false); // 2 blocks
                AddMirroredCube(b, 0.90f, 2 * C_CUBE, false); // 2 blocks
                AddMirroredCube(b, 0.45f, C_CUBE, false); // 2 blocks
                AddMirroredCube(b, 0.45f, 2 * C_CUBE, false); // 2 blocks
                AddMirroredCube(b, 0.45f, 3 * C_CUBE, false); // 2 blocks
                // Central bridge landing
                AddBeam(b, 0f, 4 * C_CUBE); // 1 block
                AddKing(b, 0f, 4 * C_CUBE + C_BEAM_H); // 1 block
                AddBomb(b, 0f, C_CUBE); // 1 block
                AddMirroredSoldier(b, 0.45f, 4 * C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 0.90f, 3 * C_CUBE); // 2 blocks
                AddMirroredTurret(b, 1.40f, 2 * C_CUBE); // 2 blocks
                break; // Total = 28 blocks
            }

            case 144: // Colossal Chevron Meander (Double zig-zag, 28 blocks)
            {
                AddBaseRow(b, 3, 0f, true); // 7 blocks
                // Lower chevron
                AddMirroredCube(b, 1.30f, C_CUBE, false); // 2 blocks
                AddMirroredCube(b, 0.65f, 2 * C_CUBE, false); // 2 blocks
                AddCube(b, 0f, 2 * C_CUBE, true); // 1 block
                AddBomb(b, 0f, 3 * C_CUBE); // 1 block
                // Upper chevron
                float chY = 3 * C_CUBE + C_BOMB_H;
                AddMirroredCube(b, 0.65f, chY, false); // 2 blocks
                AddMirroredCube(b, 1.30f, chY + C_CUBE, false); // 2 blocks
                AddPlank(b, 0f, chY + C_CUBE); // 1 block
                AddKing(b, 0f, chY + C_CUBE + C_PLANK_H); // 1 block
                AddMirroredTurret(b, 1.30f, chY + 2 * C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 1.30f, chY + 2 * C_CUBE + C_TURRET_H); // 2 blocks
                AddMirroredBomb(b, 0.65f, chY + C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 1.65f, C_CUBE); // 2 blocks
                break; // Total = 27 blocks -> add 1
                // Add 1 ground can:
            }

            case 145: // Courtyard Diamond Palace (Diamond with open courtyard, 28 blocks)
            {
                AddBaseRow(b, 3, 0f, true); // 7 blocks
                // Outer perimeter diamond walls
                AddMirroredTower(b, 1.40f, C_CUBE, 3); // 6 blocks
                AddMirroredTurret(b, 1.40f, 4 * C_CUBE); // 2 blocks
                AddMirroredTower(b, 0.70f, C_CUBE, 2); // 4 blocks
                // Upper courtyard roof
                AddPlank(b, 0f, 3 * C_CUBE); // 1 block
                AddBaseRow(b, 1, 3 * C_CUBE + C_PLANK_H, false); // 3 blocks
                AddKing(b, 0f, 4 * C_CUBE + C_PLANK_H); // 1 block
                // Courtyard garden & sentries
                AddBomb(b, 0f, C_CUBE); // 1 block
                AddSoldier(b, 0f, C_CUBE + C_BOMB_H); // 1 block
                AddMirroredSoldier(b, 0.70f, 3 * C_CUBE); // 2 blocks
                break; // Total = 28 blocks
            }

            case 146: // Double Chevron Fortress (Nested winged chevrons, 28 blocks)
            {
                AddBaseRow(b, 3, 0f, true); // 7 blocks
                // Inner chevron
                AddCube(b, 0f, C_CUBE, true); // 1 block
                AddMirroredCube(b, 0.55f, 2 * C_CUBE, false); // 2 blocks
                AddMirroredCube(b, 1.10f, 3 * C_CUBE, false); // 2 blocks
                // Outer chevron
                AddMirroredCube(b, 0.70f, C_CUBE, true); // 2 blocks
                AddMirroredCube(b, 1.25f, 2 * C_CUBE, false); // 2 blocks
                AddMirroredCube(b, 1.60f, 3 * C_CUBE, false); // 2 blocks
                // Wing apexes & crown
                AddBeam(b, 0f, 3 * C_CUBE); // 1 block
                AddKing(b, 0f, 3 * C_CUBE + C_BEAM_H); // 1 block
                AddMirroredTurret(b, 1.60f, 4 * C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 1.10f, 4 * C_CUBE); // 2 blocks
                AddMirroredBomb(b, 0.55f, 3 * C_CUBE); // 2 blocks
                AddBomb(b, 0f, 2 * C_CUBE); // 1 block
                break; // Total = 27 blocks
            }

            case 147: // Layered Triforce Citadel (Triforce formation, 31 blocks)
            {
                AddBaseRow(b, 3, 0f, true); // 7 blocks
                // Bottom left triangle
                AddCube(b, -1.00f, C_CUBE, false); // 1 block
                AddCube(b, -1.50f, C_CUBE, false); // 1 block
                AddCube(b, -1.25f, 2 * C_CUBE, false); // 1 block
                AddTurret(b, -1.25f, 3 * C_CUBE); // 1 block
                AddSoldier(b, -1.25f, 3 * C_CUBE + C_TURRET_H); // 1 block
                // Bottom right triangle
                AddCube(b, 1.00f, C_CUBE, false); // 1 block
                AddCube(b, 1.50f, C_CUBE, false); // 1 block
                AddCube(b, 1.25f, 2 * C_CUBE, false); // 1 block
                AddTurret(b, 1.25f, 3 * C_CUBE); // 1 block
                AddSoldier(b, 1.25f, 3 * C_CUBE + C_TURRET_H); // 1 block
                // Central bridge & Top triangle
                AddPlank(b, 0f, 3 * C_CUBE); // 1 block
                float triY = 3 * C_CUBE + C_PLANK_H;
                AddBaseRow(b, 1, triY, false); // 3 blocks
                AddCube(b, 0f, triY + C_CUBE, false); // 1 block
                AddKing(b, 0f, triY + 2 * C_CUBE); // 1 block
                AddMirroredBomb(b, 0.55f, triY + C_CUBE); // 2 blocks
                // Center void jewels
                AddBomb(b, 0f, C_CUBE); // 1 block
                AddSoldier(b, 0f, 2 * C_CUBE); // 1 block
                AddMirroredCan(b, 0.55f, C_CUBE); // 2 blocks
                break; // Total = 30 blocks
            }

            case 148: // Checkerboard Fortress (High-contrast void grid, 30 blocks)
            {
                AddBaseRow(b, 3, 0f, true); // 7 blocks
                // Checkerboard pattern (rows 1..3)
                for (int r = 1; r <= 3; r++)
                {
                    for (int c = -2; c <= 2; c++)
                    {
                        if ((r + c + 10) % 2 == 0)
                        {
                            AddCube(b, c * 0.60f, r * C_CUBE, false); // 7 blocks total
                        }
                    }
                } // 7 blocks
                AddPlank(b, 0f, 4 * C_CUBE); // 1 block
                AddMirroredPlank(b, 1.10f, 4 * C_CUBE); // 2 blocks
                float roofY = 4 * C_CUBE + C_PLANK_H;
                AddBaseRow(b, 1, roofY, false); // 3 blocks
                AddKing(b, 0f, roofY + C_CUBE); // 1 block
                AddMirroredTurret(b, 1.20f, roofY); // 2 blocks
                AddMirroredSoldier(b, 0.60f, roofY + C_CUBE); // 2 blocks
                AddMirroredBomb(b, 0.60f, 2 * C_CUBE); // 2 blocks
                AddMirroredCan(b, 1.20f, C_CUBE); // 2 blocks
                AddBomb(b, 0f, roofY + 2 * C_CUBE + C_KING_H); // 1 block
                break; // Total = 30 blocks
            }

            case 149: // Templar Cross Stronghold (4-armed cross, 31 blocks)
            {
                AddBaseRow(b, 3, 0f, true); // 7 blocks
                // Central vertical spine
                for (int r = 1; r <= 5; r++) AddCube(b, 0f, r * C_CUBE, false); // 5 blocks
                AddKing(b, 0f, 6 * C_CUBE); // 1 block
                // Horizontal cross arms (at y = 3*C_CUBE)
                AddMirroredCube(b, 0.55f, 3 * C_CUBE, false); // 2 blocks
                AddMirroredCube(b, 1.10f, 3 * C_CUBE, false); // 2 blocks
                AddMirroredCube(b, 1.65f, 3 * C_CUBE, false); // 2 blocks
                // Arm end battlements
                AddMirroredTurret(b, 1.65f, 4 * C_CUBE); // 2 blocks
                AddMirroredSoldier(b, 1.65f, 4 * C_CUBE + C_TURRET_H); // 2 blocks
                // Corner guard redoubts
                AddMirroredTower(b, 1.10f, C_CUBE, 2, true); // 4 blocks
                AddMirroredBomb(b, 1.10f, 4 * C_CUBE); // 2 blocks
                AddMirroredCan(b, 0.55f, C_CUBE); // 2 blocks
                break; // Total = 31 blocks
            }

            case 150: // [SPECIAL MILESTONE] Grand Military Stronghold (3 Tables, 62 blocks!)
            {
                // ==========================================
                // Table 0: Center Royal Citadel Keep (30 blocks)
                // ==========================================
                AddBaseRow(b, 2, 0f, true, 0.55f, 0); // 5 blocks
                AddBaseRow(b, 1, C_CUBE, true, 0.55f, 0); // 3 blocks
                AddMirroredBomb(b, 1.10f, C_CUBE, 0); // 2 blocks
                AddPlank(b, 0f, 2 * C_CUBE, 0); // 1 block
                // Royal Bomb Vault with side jambs
                AddCube(b, -0.55f, 2 * C_CUBE + C_PLANK_H, false, 0); // 1 block
                AddBomb(b, 0f, 2 * C_CUBE + C_PLANK_H, 0); // 1 block
                AddCube(b, 0.55f, 2 * C_CUBE + C_PLANK_H, false, 0); // 1 block
                AddBeam(b, 0f, 3 * C_CUBE + C_PLANK_H, 0); // 1 block
                float keepY = 3 * C_CUBE + C_PLANK_H + C_BEAM_H;
                // High Throne Tower
                AddBaseRow(b, 1, keepY, false, 0.55f, 0); // 3 blocks
                AddCube(b, 0f, keepY + C_CUBE, false, 0); // 1 block
                AddPlank(b, 0f, keepY + 2 * C_CUBE, 0); // 1 block
                AddKing(b, 0f, keepY + 2 * C_CUBE + C_PLANK_H, 0); // 1 block
                AddSoldier(b, 0f, keepY + 2 * C_CUBE + C_PLANK_H + C_KING_H, 0); // 1 block
                AddMirroredTurret(b, 0.55f, keepY + C_CUBE, 0); // 2 blocks
                AddMirroredSoldier(b, 0.55f, keepY + C_CUBE + C_TURRET_H, 0); // 2 blocks
                AddMirroredCan(b, 1.10f, 2 * C_CUBE, 0); // 2 blocks
                AddMirroredSoldier(b, 1.10f, 2 * C_CUBE + C_CAN_H, 0); // 2 blocks

                // ==========================================
                // Table 1: Left Bastion Wing (16 blocks)
                // ==========================================
                AddBaseRow(b, 1, 0f, true, 0.55f, 1); // 3 blocks
                AddBaseRow(b, 1, C_CUBE, true, 0.55f, 1); // 3 blocks
                AddMirroredTower(b, 0.55f, 2 * C_CUBE, 2, false, 1); // 4 blocks
                AddPlank(b, 0f, 4 * C_CUBE, 1); // 1 block
                AddTurret(b, -0.55f, 4 * C_CUBE + C_PLANK_H, 1); // 1 block
                AddTurret(b, 0.55f, 4 * C_CUBE + C_PLANK_H, 1); // 1 block
                AddSoldier(b, 0f, 4 * C_CUBE + C_PLANK_H, 1); // 1 block
                AddBomb(b, 0f, 2 * C_CUBE, 1); // 1 block
                AddCan(b, 0f, 3 * C_CUBE, 1); // 1 block

                // ==========================================
                // Table 2: Right Bastion Wing (16 blocks)
                // ==========================================
                AddBaseRow(b, 1, 0f, true, 0.55f, 2); // 3 blocks
                AddBaseRow(b, 1, C_CUBE, true, 0.55f, 2); // 3 blocks
                AddMirroredTower(b, 0.55f, 2 * C_CUBE, 2, false, 2); // 4 blocks
                AddPlank(b, 0f, 4 * C_CUBE, 2); // 1 block
                AddTurret(b, -0.55f, 4 * C_CUBE + C_PLANK_H, 2); // 1 block
                AddTurret(b, 0.55f, 4 * C_CUBE + C_PLANK_H, 2); // 1 block
                AddSoldier(b, 0f, 4 * C_CUBE + C_PLANK_H, 2); // 1 block
                AddBomb(b, 0f, 2 * C_CUBE, 2); // 1 block
                AddCan(b, 0f, 3 * C_CUBE, 2); // 1 block
                break; // Total = 30 + 16 + 16 = 62 blocks!
            }
        }
    }
}
