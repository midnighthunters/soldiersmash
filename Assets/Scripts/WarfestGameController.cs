using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class WarfestGameController : MonoBehaviour
{
    private static readonly Color LightBackground = new Color(0.93f, 0.96f, 0.98f, 1f);
    private static readonly Color Ink = new Color(0.08f, 0.14f, 0.22f, 1f);
    private static readonly Color LightPanel = new Color(1f, 1f, 1f, 0.95f);
    private static readonly Color DisabledBall = new Color(0.70f, 0.76f, 0.82f, 1f);

    private readonly List<GameObject> blocks = new List<GameObject>();
    private readonly List<Image> ballSlots = new List<Image>();
    [SerializeField] private Font headingFont;
    [SerializeField] private Font bodyFont;
    private Font font;
    private Camera gameplayCamera;
    private Transform worldRoot;
    private Transform pistolPivot;
    private Transform pistolVisual;
    private Transform muzzle;
    private Text ballsText;
    private Text targetText;
    private WarfestLevelCatalog.LevelDefinition level;
    private Sprite pistolSprite;
    private Sprite pistolBaseSprite;

    private Sprite backgroundSprite;
    private Sprite ballsPanelSprite;
    private Sprite levelPanelSprite;
    private Sprite blueLabelSprite;
    private Sprite settingsPanelSprite;
    private Sprite explosionSprite;
    private Sprite tableSprite;
    private Sprite cannonBaseSprite;
    private GameObject[] boxModelPrefabs;
    private Texture2D[] boxModelTextures;
    private Material[] boxModelMaterials;
    private GameObject tableModelPrefab;
    private Material tableModelMaterial;
    private GameObject ballModelPrefab;
    private Material ballModelMaterial;
    private readonly List<float> modelTableTopYs = new List<float>();
    private readonly List<int> blockDepthLayers = new List<int>();
    private Sprite[] blockSprites;
    private int ballCapacity;
    private int remainingBalls;
    private int targetsRemaining;


    private bool isAiming;
    private bool gestureBlocked;
    private LineRenderer aimLine;
    private bool hasAimPoint;
    private Vector2 aimWorldPosition;
    private bool modelPhysicsReleased;
    private bool levelEnded;
    private Vector3 pistolRestLocalPosition;
    private Canvas hudCanvas;
    private RectTransform safeAreaRoot;
    private CanvasScaler hudScaler;
    private Vector2 appliedReferenceResolution;
    private Rect appliedSafeArea;

    private const float TableColliderWidth = 16.10f;
    private const float TableColliderHeight = 0.52f;
    private const float TableColliderOffsetY = 1.64f;
    private const float ShotRadius = 0.16f;
    private const float ShotSpeed = 17f;
    private const float ShotLifetime = 4f;
    private float recoilTime;

private void Start()
    {
        font = bodyFont != null ? bodyFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (headingFont == null) headingFont = font;
        if (bodyFont == null) bodyFont = font;
        LoadOriginalSprites();
        EnsureEventSystem();
        EnsureCamera();
        level = WarfestLevelCatalog.Get(WarfestSession.SelectedLevel);
        ballCapacity = WarfestSession.GetBallAllowance(level.number - 1);
        remainingBalls = ballCapacity;
        BuildWorld();
        BuildHud();
        RefreshHud();
    }

private void LoadOriginalSprites()
    {
        Sprite[] pistolSprites = Resources.LoadAll<Sprite>("pistol");
        Sprite[] tables = Resources.LoadAll<Sprite>("table");

        Texture2D backgroundTexture = Resources.Load<Texture2D>("background");
        if (backgroundTexture != null)
        {
            backgroundSprite = Sprite.Create(
                backgroundTexture,
                new Rect(0f, 0f, backgroundTexture.width, backgroundTexture.height),
                new Vector2(0.5f, 0.5f),
                100f);
            backgroundSprite.name = "Gameplay Background";
        }

        Texture2D panelTexture = Resources.Load<Texture2D>("panel");
        if (panelTexture != null && panelTexture.width >= 1500 && panelTexture.height >= 1000)
        {
            // Pixel rectangles are authored from the transparent panel spritesheet. Sprite.Create
            // uses a bottom-left origin, while the source artwork was measured from the top-left.
            // Scale the crop coordinates if Unity applies NPOT resizing on another platform.
            float panelScaleX = panelTexture.width / 1536f;
            float panelScaleY = panelTexture.height / 1024f;
            ballsPanelSprite = CreateSheetSprite(panelTexture, ScaleSheetRect(new Rect(35f, 426f, 480f, 431f), panelScaleX, panelScaleY), "Balls Panel");
            levelPanelSprite = CreateSheetSprite(panelTexture, ScaleSheetRect(new Rect(571f, 423f, 931f, 471f), panelScaleX, panelScaleY), "Level Panel");
            blueLabelSprite = CreateSheetSprite(panelTexture, ScaleSheetRect(new Rect(222f, 95f, 494f, 201f), panelScaleX, panelScaleY), "Blue Label");
            settingsPanelSprite = CreateSheetSprite(panelTexture, ScaleSheetRect(new Rect(930f, 0f, 410f, 404f), panelScaleX, panelScaleY), "Settings Button");
        }
        blockSprites = Resources.LoadAll<Sprite>("blocks");

        for (int i = 0; i < pistolSprites.Length; i++)
        {
            if (pistolSprites[i].name == "pistol_0") pistolSprite = pistolSprites[i];
            if (pistolSprites[i].name == "pistol_1") pistolBaseSprite = pistolSprites[i];
        }
        if (pistolSprite == null && pistolSprites.Length > 0) pistolSprite = pistolSprites[0];
        if (pistolBaseSprite == null && pistolSprites.Length > 1) pistolBaseSprite = pistolSprites[1];

        boxModelPrefabs = new[]
        {
            Resources.Load<GameObject>("box/base_basic_shaded"),
            Resources.Load<GameObject>("box2/base_basic_shaded"),
            Resources.Load<GameObject>("box3/base_basic_shaded"),
            Resources.Load<GameObject>("long_box/base_basic_shaded"),
            Resources.Load<GameObject>("soldier/base_basic_shaded"),
            Resources.Load<GameObject>("cannister/base_basic_shaded"),
            Resources.Load<GameObject>("bomb/base_basic_shaded"),
        };
        boxModelTextures = new[]
        {
            Resources.Load<Texture2D>("box/shaded"),
            Resources.Load<Texture2D>("box2/shaded"),
            Resources.Load<Texture2D>("box3/shaded"),
            Resources.Load<Texture2D>("long_box/shaded"),
            Resources.Load<Texture2D>("soldier/shaded"),
            Resources.Load<Texture2D>("cannister/shaded"),
            Resources.Load<Texture2D>("bomb/shaded"),
        };
        boxModelMaterials = new Material[boxModelTextures.Length];
        for (int i = 0; i < boxModelTextures.Length; i++)
        {
            boxModelMaterials[i] = CreateBrightModelMaterial(boxModelTextures[i], "Warfest Block " + (i + 1));
        }

        tableModelPrefab = Resources.Load<GameObject>("table/base_basic_shaded");
        tableModelMaterial = CreateBrightModelMaterial(Resources.Load<Texture2D>("table/shaded"), "Warfest Table");
        ballModelPrefab = Resources.Load<GameObject>("ball/base_basic_shaded");
        ballModelMaterial = CreateBrightModelMaterial(Resources.Load<Texture2D>("ball/shaded"), "Warfest Projectile Ball");
        tableSprite = tables.Length > 0 ? tables[0] : null;

        if (pistolSprite == null || pistolBaseSprite == null || tableSprite == null || blockSprites == null || blockSprites.Length == 0)
        {
            Debug.LogError("Warfest requires both pistol sprites plus the original table and blocks sprites.");
        }
        if (!HasAnyBoxPrefab())
        {
            Debug.LogError("The 3D crate levels require Resources/box, box2, box3, long_box, or soldier models.");
        }
        if (tableModelPrefab == null)
        {
            Debug.LogError("The 3D crate levels require Resources/table/base_basic_shaded.fbx.");
        }
    }

    private bool HasAnyBoxPrefab()
    {
        if (boxModelPrefabs == null) return false;
        for (int i = 0; i < boxModelPrefabs.Length; i++)
        {
            if (boxModelPrefabs[i] != null) return true;
        }
        return false;
    }

    private static Sprite CreateSheetSprite(Texture2D sheet, Rect rect, string spriteName)
    {
        Sprite sprite = Sprite.Create(sheet, rect, new Vector2(0.5f, 0.5f), 100f, 0u, SpriteMeshType.FullRect);
        sprite.name = spriteName;
        return sprite;
    }

    private static Rect ScaleSheetRect(Rect rect, float scaleX, float scaleY)
    {
        return new Rect(rect.x * scaleX, rect.y * scaleY, rect.width * scaleX, rect.height * scaleY);
    }

    // Returns the requested crate skin, falling back to the first available model.
    private GameObject GetBoxPrefab(int variant)
    {
        if (boxModelPrefabs == null || boxModelPrefabs.Length == 0) return null;
        if (variant >= 0 && variant < boxModelPrefabs.Length && boxModelPrefabs[variant] != null)
        {
            return boxModelPrefabs[variant];
        }
        for (int i = 0; i < boxModelPrefabs.Length; i++)
        {
            if (boxModelPrefabs[i] != null) return boxModelPrefabs[i];
        }
        return null;
    }

private Material CreateBrightModelMaterial(Texture2D texture, string materialName)
    {
        if (texture == null) return null;
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        if (shader == null) return null;

        Material material = new Material(shader);
        material.name = materialName;
        material.mainTexture = texture;
        if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
        Color exposure = new Color(1.45f, 1.45f, 1.45f, 1f);
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", exposure);
        material.color = exposure;
        material.enableInstancing = true;
        return material;
    }

    private Material GetBoxMaterial(int variant)
    {
        if (boxModelMaterials == null || boxModelMaterials.Length == 0) return null;
        if (variant >= 0 && variant < boxModelMaterials.Length && boxModelMaterials[variant] != null)
        {
            return boxModelMaterials[variant];
        }
        for (int i = 0; i < boxModelMaterials.Length; i++)
        {
            if (boxModelMaterials[i] != null) return boxModelMaterials[i];
        }
        return null;
    }

    // Explicit gameplay weights for the authored 3D pieces. These values are intentionally
    // independent of visual size so long_box and the sandbag block can have matching weight.
    private static float GetModelMass(int variant)
    {
        switch (variant)
        {
            case 0: return 0.24f; // box: very light
            case 1: return 1.60f; // box1 / heavy crate
            case 2: return 0.62f; // box2 / sandbag: moderately light
            case 3: return 0.62f; // long_box: equal to sandbag
            case 4: return 0.20f; // soldier: very light
            case 5: return 0.16f; // cannister: lightest
            case 6: return 0.32f; // bomb: light enough to move, heavy enough to aim at
            default: return 0.55f;
        }
    }

    private static void ApplyModelMaterial(GameObject model, Material material)
    {
        if (model == null || material == null) return;
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].sharedMaterial = material;
        }
    }


private void Update()
    {
        ApplyCanvasScale();
        ApplySafeArea();
        UpdateRecoil();

        if (levelEnded || gameplayCamera == null || muzzle == null) return;

        // A shot must begin with a new press inside this gameplay scene. This prevents the
        // release of the menu-selection click/tap from becoming an unintended first shot.
        if (!TryGetPointer(out Vector2 position, out bool held, out bool pressedThisFrame))
        {
            CancelAim();
            return;
        }

        if (pressedThisFrame)
        {
            isAiming = true;
            hasAimPoint = false;
            gestureBlocked = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            if (!gestureBlocked)
            {
                AimAt(position);
                UpdateAimLine();
            }
            return;
        }

        if (held)
        {
            if (isAiming && !gestureBlocked)
            {
                AimAt(position);
                UpdateAimLine();
            }
            return;
        }

        // Only a press that armed aiming above may fire on release.
        if (isAiming)
        {
            bool shouldFire = !gestureBlocked && hasAimPoint;
            isAiming = false;
            gestureBlocked = false;
            HideAimLine();
            if (shouldFire) Fire();
        }
    }

    private void CancelAim()
    {
        isAiming = false;
        gestureBlocked = false;
        HideAimLine();
    }

    // Draws the aim preview from the muzzle to the first block the shot would actually strike.
    // The ball ignores the table layer, so the raycast does too - the line ends exactly where
    // the projectile will land, making "aim where you shoot" unambiguous.
    private void UpdateAimLine()
    {
        if (aimLine == null || muzzle == null) return;

        Vector2 origin = muzzle.position;
        Vector2 dir = aimWorldPosition - origin;
        if (dir.sqrMagnitude < 0.0001f)
        {
            aimLine.enabled = false;
            return;
        }
        dir.Normalize();

        int mask = ~0;
        int tableLayer = LayerMask.NameToLayer("WarfestTable");
        int shotLayer = LayerMask.NameToLayer("WarfestShot");
        if (tableLayer >= 0) mask &= ~(1 << tableLayer);
        if (shotLayer >= 0) mask &= ~(1 << shotLayer);

        const float maxLength = 14f;
        RaycastHit2D hit = Physics2D.Raycast(origin, dir, maxLength, mask);
        Vector2 end = hit.collider != null ? hit.point : origin + dir * maxLength;

        aimLine.enabled = true;
        aimLine.SetPosition(0, new Vector3(origin.x, origin.y, -0.3f));
        aimLine.SetPosition(1, new Vector3(end.x, end.y, -0.3f));
    }

    private void HideAimLine()
    {
        if (aimLine != null) aimLine.enabled = false;
    }

    private void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;
        new GameObject("EventSystem", typeof(EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
    }

private void EnsureCamera()
    {
        gameplayCamera = Camera.main;
        if (gameplayCamera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            gameplayCamera = cameraObject.GetComponent<Camera>();
        }
        gameplayCamera.transform.position = new Vector3(0f, 1.85f, -10f);
        gameplayCamera.orthographic = true;
        gameplayCamera.orthographicSize = 6.35f;
        gameplayCamera.clearFlags = CameraClearFlags.SolidColor;
        gameplayCamera.backgroundColor = LightBackground;
    }

private void BuildWorld()
    {

        worldRoot = new GameObject("Runtime Level // " + level.number.ToString("00")).transform;
        CreateBackground();
        bool isModelLevel = level.number >= 1 && level.number <= WarfestLevelCatalog.AuthoredLevelCount;
        bool canBuildModels = isModelLevel && tableModelPrefab != null && HasAnyBoxPrefab();

        if (canBuildModels)
        {
            List<WarfestLevelCatalog.ModelTableSpec> tableSpecs = new List<WarfestLevelCatalog.ModelTableSpec>();
            WarfestLevelCatalog.FillModelTables(level.number - 1, tableSpecs);
            modelTableTopYs.Clear();
            for (int i = 0; i < tableSpecs.Count; i++) CreateModelTable(tableSpecs[i], i);
            BuildModelLayout(level.number);
        }
        else
        {
            CreateTable();
            List<WarfestLevelCatalog.BlockSpec> specs = new List<WarfestLevelCatalog.BlockSpec>();
            WarfestLevelCatalog.FillLayout(level.number - 1, specs);
            targetsRemaining = specs.Count;
            for (int i = 0; i < specs.Count; i++) CreateBlock(specs[i], i);
        }

        CreatePistol();
    }

    private void CreateTable()
    {
        if (tableSprite == null) return;

        GameObject table = CreateSprite("Table", tableSprite, new Vector3(0f, -0.95f, 0f), new Vector2(0.315f, 0.315f), 0);
        int tableLayer = LayerMask.NameToLayer("WarfestTable");
        if (tableLayer >= 0) table.layer = tableLayer;

        BoxCollider2D surface = table.AddComponent<BoxCollider2D>();
        // The sprite contains transparent space above the physical tabletop. This offset
        // aligns the collision top with the visible metal surface rather than the image bounds.
        surface.size = new Vector2(TableColliderWidth, TableColliderHeight);
        surface.offset = new Vector2(0f, TableColliderOffsetY);
    }

private void CreateModelTable(WarfestLevelCatalog.ModelTableSpec spec, int index)
    {
        const float surfaceThickness = 0.05f;

        GameObject table = Instantiate(tableModelPrefab, worldRoot);
        ApplyModelMaterial(table, tableModelMaterial);
        table.name = "Level Table Model " + (index + 1).ToString("00");
        table.transform.localPosition = Vector3.zero;
        table.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

        // Measure after the X rotation so the support collider and box stack follow the real rendered tabletop.
        Bounds sourceBounds = GetModelBounds(table);
        float scale = spec.width / sourceBounds.size.x;
        table.transform.localScale *= scale;

        Bounds scaledBounds = GetModelBounds(table);
        table.transform.position += new Vector3(
            spec.x - scaledBounds.center.x,
            spec.visibleTopY - scaledBounds.max.y,
            spec.depth - scaledBounds.center.z);
        Bounds tableBounds = GetModelBounds(table);
        // The model's AABB top sits on a raised back lip, not the flat playable surface, so
        // measure the true top by raycasting down onto the mesh. Crates rest on THIS height.
        float modelTableTopY = MeasureTableSurfaceY(table, tableBounds);
        modelTableTopYs.Add(modelTableTopY);

        GameObject surface = new GameObject("Level Table Surface " + (index + 1).ToString("00"), typeof(BoxCollider2D));
        surface.transform.SetParent(worldRoot, false);
        surface.transform.position = new Vector3(tableBounds.center.x, modelTableTopY - surfaceThickness * 0.5f, 0.12f);
        int tableLayer = LayerMask.NameToLayer("WarfestTable");
        if (tableLayer >= 0) surface.layer = tableLayer;

        BoxCollider2D collider = surface.GetComponent<BoxCollider2D>();
        collider.size = new Vector2(tableBounds.size.x, surfaceThickness);
    }


    private void CreateBlock(WarfestLevelCatalog.BlockSpec spec, int index)
    {
        if (blockSprites == null || blockSprites.Length == 0) return;
        int spriteIndex = spec.spriteIndex >= 0 ? spec.spriteIndex % blockSprites.Length : index % blockSprites.Length;
        Sprite sprite = blockSprites[spriteIndex];

        Vector2 spriteSize = sprite.bounds.size;
        Vector2 localScale = new Vector2(
            spriteSize.x > 0.0001f ? spec.size.x / spriteSize.x : 1f,
            spriteSize.y > 0.0001f ? spec.size.y / spriteSize.y : 1f);

        GameObject block = CreateSprite("Target " + (index + 1).ToString("00"), sprite, new Vector3(spec.position.x, spec.position.y, 0f), localScale, 2);
        block.transform.rotation = Quaternion.Euler(0f, 0f, spec.rotation);
        block.GetComponent<SpriteRenderer>().color = spec.color;

        BoxCollider2D collider = block.AddComponent<BoxCollider2D>();
        collider.size = spriteSize;
        Rigidbody2D body = block.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Dynamic;
        body.mass = 0.7f;
        body.gravityScale = 1f;
        body.linearDamping = 0.12f;

        body.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        body.angularDamping = 0.4f;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        WarfestTarget target = block.AddComponent<WarfestTarget>();
        target.Initialize(this);
        blocks.Add(block);
    }

private void BuildModelLayout(int levelNumber)
    {
        List<WarfestLevelCatalog.ModelBlockSpec> specs = new List<WarfestLevelCatalog.ModelBlockSpec>();
        WarfestLevelCatalog.FillModelLayout(levelNumber - 1, specs);
        targetsRemaining = specs.Count;

        for (int i = 0; i < specs.Count; i++)
        {
            CreateModelBox(specs[i], i);
        }
    }

private void CreateModelBox(WarfestLevelCatalog.ModelBlockSpec spec, int index)
    {
        GameObject prefab = GetBoxPrefab(spec.variant);
        if (prefab == null) return;

        const float tabletopGap = 0.001f;
        const float visualOverlap = 0.012f;
        const float colliderInset = 0.006f;

        string layerName = spec.depthLayer == 0 ? "Front" : "Rear";
        GameObject block = new GameObject(layerName + " Crate " + (index + 1).ToString("00"));
        block.transform.SetParent(worldRoot, false);
        float renderDepth = spec.depthLayer == 0 ? 0.08f : 0.72f;
        block.transform.localPosition = new Vector3(spec.x, 0f, renderDepth);

        GameObject visual = Instantiate(prefab);
        visual.name = "Visual";
        visual.transform.SetParent(block.transform, false);
        visual.transform.localRotation = Quaternion.Euler(-90f, GetModelYRotation(spec.variant), 0f);
        ApplyModelMaterial(visual, GetBoxMaterial(spec.variant));

        Bounds sourceBounds = GetModelBounds(visual);
        float widthScale = (spec.width + visualOverlap) / sourceBounds.size.x;
        float heightScale = (spec.height + visualOverlap) / sourceBounds.size.y;
        float depthScale = Mathf.Min(widthScale, heightScale);
        // box3 is rotated to face across the table: local Y becomes world width and local Z
        // becomes world height. Fit those axes explicitly so its visual matches the authored
        // sandbag footprint and collider.
        Vector3 fittedScale = spec.variant == 2
            ? new Vector3(depthScale, widthScale, heightScale)
            : new Vector3(widthScale, heightScale, depthScale);
        visual.transform.localScale = Vector3.Scale(visual.transform.localScale, fittedScale);

        Bounds fittedBounds = GetModelBounds(visual);
        Vector3 blockPosition = block.transform.position;
        visual.transform.position += blockPosition - fittedBounds.center;
        float visualHeight = GetModelBounds(visual).size.y;

        int tableIndex = Mathf.Clamp(spec.tableIndex, 0, Mathf.Max(0, modelTableTopYs.Count - 1));
        float tableTopY = modelTableTopYs.Count > 0 ? modelTableTopYs[tableIndex] : -0.351f;
        float desiredBottomY = tableTopY + tabletopGap + spec.yOffset;
        block.transform.position = new Vector3(
            blockPosition.x,
            desiredBottomY + visualHeight * 0.5f,
            blockPosition.z);

        // Keep the physics footprint aligned with the visible bottom. Every base row touches the
        // table surface, and each higher 0.72-unit row contacts the collider directly below it.
        float colliderHeight = Mathf.Max(0.05f, spec.height);
        BoxCollider2D gameplayCollider = block.AddComponent<BoxCollider2D>();
        gameplayCollider.size = new Vector2(
            Mathf.Max(0.05f, spec.width - colliderInset),
            colliderHeight);
        gameplayCollider.offset = new Vector2(0f, (colliderHeight - visualHeight) * 0.5f);

        // The two visual depth planes form independent physical stacks. Ignoring cross-layer
        // contacts keeps the slightly offset rear design from pushing the front layer apart.
        for (int i = 0; i < blocks.Count && i < blockDepthLayers.Count; i++)
        {
            if (blockDepthLayers[i] == spec.depthLayer) continue;
            BoxCollider2D otherCollider = blocks[i] != null ? blocks[i].GetComponent<BoxCollider2D>() : null;
            if (otherCollider != null) Physics2D.IgnoreCollision(gameplayCollider, otherCollider, true);
        }

        Rigidbody2D body = block.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.mass = GetModelMass(spec.variant);
        body.gravityScale = 0f;
        body.linearDamping = 0.16f;
        body.angularDamping = 0.55f;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        WarfestTarget target = block.AddComponent<WarfestTarget>();
        target.Initialize(this, spec.variant == 6);
        blocks.Add(block);
        blockDepthLayers.Add(spec.depthLayer);
    }

private static float GetModelYRotation(int variant)
    {
        switch (variant)
        {
            case 2: return 90f;  // box3: sandbags face along the table width.
            case 4: return 180f; // soldier faces the player.
            case 6: return 180f; // bomb label and fuse face the player.
            default: return 0f;
        }
    }


    private static Bounds GetModelBounds(GameObject model)
    {
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }

    // Finds the flat playable top of the table by dropping a ray onto its mesh, so crates rest
    // on the visible surface rather than the higher back-lip that dominates the model's AABB.
    private static float MeasureTableSurfaceY(GameObject table, Bounds tableBounds)
    {
        MeshFilter meshFilter = table.GetComponentInChildren<MeshFilter>();
        if (meshFilter == null) return tableBounds.max.y;

        MeshCollider probe = meshFilter.gameObject.AddComponent<MeshCollider>();
        Physics.SyncTransforms();
        float surfaceY = tableBounds.max.y;
        Ray ray = new Ray(new Vector3(tableBounds.center.x, tableBounds.max.y + 2f, tableBounds.center.z), Vector3.down);
        if (probe.Raycast(ray, out RaycastHit hit, tableBounds.size.y + 4f))
        {
            surfaceY = hit.point.y;
        }
        DestroyImmediate(probe);
        return surfaceY;
    }


private void CreatePistol()
    {
        if (pistolSprite == null) return;

        CreateCannonBase();
        pistolPivot = new GameObject("Pistol Pivot").transform;
        pistolPivot.SetParent(worldRoot, false);
        pistolPivot.position = new Vector3(0f, -3.68f, -1f);

        pistolVisual = CreateSprite(
            "Pistol",
            pistolSprite,
            new Vector3(0f, 0.43f, 0f),
            new Vector2(0.25f, 0.25f),
            5).transform;
        pistolVisual.SetParent(pistolPivot, false);
        pistolRestLocalPosition = pistolVisual.localPosition;

        muzzle = new GameObject("Muzzle").transform;
        muzzle.SetParent(pistolPivot, false);
        muzzle.localPosition = new Vector3(0f, 1.14f, 0f);

        CreateAimLine();
    }

    private void CreateAimLine()
    {
        GameObject aimLineObject = new GameObject("Aim Line", typeof(LineRenderer));
        aimLineObject.transform.SetParent(worldRoot, false);
        aimLine = aimLineObject.GetComponent<LineRenderer>();
        aimLine.useWorldSpace = true;
        aimLine.positionCount = 2;
        aimLine.numCapVertices = 4;
        aimLine.alignment = LineAlignment.View;
        aimLine.startWidth = 0.10f;
        aimLine.endWidth = 0.10f;

        Shader lineShader = Shader.Find("Sprites/Default");
        if (lineShader == null) lineShader = Shader.Find("Universal Render Pipeline/Unlit");
        if (lineShader != null) aimLine.material = new Material(lineShader);

        aimLine.startColor = new Color(1f, 0.95f, 0.35f, 0.90f);
        aimLine.endColor = new Color(1f, 0.95f, 0.35f, 0.10f);
        aimLine.sortingOrder = 7;
        aimLine.enabled = false;
    }

private void CreateCannonBase()
    {
        if (pistolBaseSprite == null) return;
        CreateSprite(
            "Pistol Base",
            pistolBaseSprite,
            new Vector3(0f, -4.06f, -0.25f),
            new Vector2(0.27f, 0.27f),
            4);
    }

    private void CreateBasePad(string name, Vector3 position, Vector2 scale, Color color)
    {
        GameObject pad = CreateSprite(name, GetCannonBaseSprite(), position, scale, 1);
        pad.GetComponent<SpriteRenderer>().color = color;
    }

    private Sprite GetCannonBaseSprite()
    {
        if (cannonBaseSprite != null) return cannonBaseSprite;

        const int textureSize = 64;
        Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[textureSize * textureSize];
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float normalizedX = (x + 0.5f) / textureSize * 2f - 1f;
                float normalizedY = (y + 0.5f) / textureSize * 2f - 1f;
                pixels[y * textureSize + x] = normalizedX * normalizedX + normalizedY * normalizedY <= 1f ? Color.white : Color.clear;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        cannonBaseSprite = Sprite.Create(texture, new Rect(0f, 0f, textureSize, textureSize), new Vector2(0.5f, 0.5f), textureSize);
        return cannonBaseSprite;
    }

private bool TryGetPointer(out Vector2 screenPosition, out bool held, out bool pressedThisFrame)
    {
        Touchscreen touchscreen = Touchscreen.current;
        if (touchscreen != null)
        {
            screenPosition = touchscreen.primaryTouch.position.ReadValue();
            held = touchscreen.primaryTouch.press.isPressed;
            pressedThisFrame = touchscreen.primaryTouch.press.wasPressedThisFrame;
            return true;
        }

        Mouse mouse = Mouse.current;
        Pointer pointer = Pointer.current;
        if (pointer == null || mouse == null)
        {
            screenPosition = default;
            held = false;
            pressedThisFrame = false;
            return false;
        }

        screenPosition = pointer.position.ReadValue();
        held = mouse.leftButton.isPressed;
        pressedThisFrame = mouse.leftButton.wasPressedThisFrame;
        return true;
    }

    private void AimAt(Vector2 screenPosition)
    {
        float aimPlaneDistance = Mathf.Abs(gameplayCamera.transform.position.z);
        Vector3 worldPosition = gameplayCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, aimPlaneDistance));
        aimWorldPosition = new Vector2(worldPosition.x, worldPosition.y);
        hasAimPoint = true;
        Vector2 direction = aimWorldPosition - (Vector2)pistolPivot.position;
        if (direction.sqrMagnitude < 0.0001f) return;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        pistolPivot.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    private void Fire()
    {
        if (remainingBalls <= 0 || levelEnded) return;

        remainingBalls--;
        recoilTime = 0.11f;
        RefreshHud();
        Vector2 launchPosition = muzzle.position;
        Vector2 launchDirection = hasAimPoint ? aimWorldPosition - launchPosition : (Vector2)muzzle.up;
        CreateBall(launchPosition, launchDirection);

        if (remainingBalls <= 0 && targetsRemaining > 0) StartCoroutine(ShowFailureAfterDelay());
    }

    private void CreateBall(Vector2 position, Vector2 direction)
    {
        GameObject ball = new GameObject("Shot Ball", typeof(CircleCollider2D), typeof(Rigidbody2D), typeof(WarfestBall));

        int shotLayer = LayerMask.NameToLayer("WarfestShot");
        if (shotLayer >= 0) ball.layer = shotLayer;
        ball.transform.SetParent(worldRoot, false);
        ball.transform.position = new Vector3(position.x, position.y, -0.22f);

        if (ballModelPrefab != null)
        {
            GameObject visual = Instantiate(ballModelPrefab, ball.transform);
            visual.name = "3D Ball Visual";
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            ApplyModelMaterial(visual, ballModelMaterial);
            Bounds sourceBounds = GetModelBounds(visual);
            float diameter = ShotRadius * 2f;
            float visualScale = diameter / Mathf.Max(sourceBounds.size.x, sourceBounds.size.y);
            visual.transform.localScale *= visualScale;
            Bounds fittedBounds = GetModelBounds(visual);
            Vector3 centerInBall = ball.transform.InverseTransformPoint(fittedBounds.center);
            visual.transform.localPosition -= centerInBall;
        }
        else
        {
            GameObject fallbackVisual = new GameObject("Fallback Ball Visual", typeof(SpriteRenderer));
            fallbackVisual.transform.SetParent(ball.transform, false);
            fallbackVisual.transform.localScale = Vector3.one * (ShotRadius * 2f);
            SpriteRenderer renderer = fallbackVisual.GetComponent<SpriteRenderer>();
            renderer.sprite = GetCannonBaseSprite();
            renderer.color = new Color(1f, 0.82f, 0.22f, 1f);
            renderer.sortingOrder = 6;
        }

        CircleCollider2D collider = ball.GetComponent<CircleCollider2D>();
        collider.radius = ShotRadius;
        Rigidbody2D body = ball.GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Dynamic;
        body.mass = 0.22f;
        body.gravityScale = 0f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.linearVelocity = direction.normalized * ShotSpeed;
        body.angularVelocity = -direction.normalized.x * 540f;
        ball.GetComponent<WarfestBall>().Initialize(direction, 5.8f + level.difficulty * 0.5f, ShotLifetime);
    }

    private IEnumerator ShowFailureAfterDelay()
    {
        yield return new WaitForSeconds(0.7f);
        if (!levelEnded && targetsRemaining > 0) ShowFailure();
    }

public void RegisterTargetBroken(WarfestTarget target)
    {
        if (levelEnded) return;
        ReleaseModelPhysics();

        targetsRemaining = Mathf.Max(0, targetsRemaining - 1);
        if (targetText != null) targetText.text = targetsRemaining.ToString("00");
        if (target != null) Destroy(target.gameObject, target.IsBomb ? 0.08f : 0.55f);
        if (targetsRemaining == 0)
        {
            levelEnded = true;
            StartCoroutine(CompleteLevelAfterDelay());
        }
    }

    public void DetonateBomb(WarfestTarget bomb)
    {
        if (bomb == null || levelEnded) return;

        Vector2 center = bomb.transform.position;
        RegisterTargetBroken(bomb);
        CreateExplosionVfx(center);

        const float blastRadius = 0.48f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, blastRadius);
        HashSet<Rigidbody2D> pushedBodies = new HashSet<Rigidbody2D>();
        for (int i = 0; i < hits.Length; i++)
        {
            WarfestTarget target = hits[i].GetComponent<WarfestTarget>();
            if (target != null && target != bomb) target.BreakFromExplosion();

            Rigidbody2D body = hits[i].attachedRigidbody;
            if (body == null || !pushedBodies.Add(body)) continue;
            Vector2 offset = body.worldCenterOfMass - center;
            float distance = Mathf.Max(0.12f, offset.magnitude);
            float falloff = 1f - Mathf.Clamp01(distance / blastRadius);
            body.AddForce(offset.normalized * Mathf.Lerp(2.8f, 9.5f, falloff), ForceMode2D.Impulse);
            body.AddTorque((body.worldCenterOfMass.x < center.x ? -1f : 1f) * Mathf.Lerp(0.4f, 2.4f, falloff), ForceMode2D.Impulse);
        }
    }

    private IEnumerator CompleteLevelAfterDelay()
    {
        yield return new WaitForSeconds(0.8f);
        WarfestSession.CompleteLevel(level.number - 1);
    }

    private void CreateExplosionVfx(Vector2 center)
    {
        Sprite flash = GetExplosionSprite();
        GameObject flashObject = CreateSprite("Bomb Flash", flash, new Vector3(center.x, center.y, -0.35f), Vector2.one * 0.16f, 30);
        SpriteRenderer flashRenderer = flashObject.GetComponent<SpriteRenderer>();
        flashRenderer.color = new Color(1f, 0.96f, 0.34f, 0.95f);
        StartCoroutine(AnimateExplosionFlash(flashObject.transform, flashRenderer));

        GameObject sparksObject = new GameObject("Bomb Sparks", typeof(ParticleSystem));
        sparksObject.transform.SetParent(worldRoot, false);
        sparksObject.transform.position = new Vector3(center.x, center.y, -0.40f);
        ParticleSystem sparks = sparksObject.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule main = sparks.main;
        main.duration = 0.45f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.30f, 0.68f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2.4f, 5.4f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.07f, 0.20f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.95f, 0.25f), new Color(1f, 0.18f, 0.02f));
        main.gravityModifier = 0.55f;
        main.maxParticles = 48;

        ParticleSystem.EmissionModule emission = sparks.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 36) });
        ParticleSystem.ShapeModule shape = sparks.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.16f;

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = sparks.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(new Color(1f, 0.95f, 0.2f), 0f), new GradientColorKey(new Color(1f, 0.12f, 0.01f), 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        colorOverLifetime.color = gradient;

        ParticleSystemRenderer particleRenderer = sparksObject.GetComponent<ParticleSystemRenderer>();
        Shader particleShader = Shader.Find("Sprites/Default");
        if (particleShader != null) particleRenderer.material = new Material(particleShader);
        particleRenderer.sortingOrder = 31;
        sparks.Play();
        Destroy(sparksObject, 1.25f);
    }

    private IEnumerator AnimateExplosionFlash(Transform flashTransform, SpriteRenderer flashRenderer)
    {
        const float duration = 0.38f;
        float time = 0f;
        while (time < duration && flashTransform != null && flashRenderer != null)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            float scale = Mathf.Lerp(0.16f, 2.65f, 1f - (1f - t) * (1f - t));
            flashTransform.localScale = Vector3.one * scale;
            Color color = flashRenderer.color;
            color.a = 1f - t;
            flashRenderer.color = color;
            yield return null;
        }
        if (flashTransform != null) Destroy(flashTransform.gameObject);
    }

    private Sprite GetExplosionSprite()
    {
        if (explosionSprite != null) return explosionSprite;
        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = "Bomb Flash Texture";
        texture.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 point = new Vector2((x + 0.5f) / size * 2f - 1f, (y + 0.5f) / size * 2f - 1f);
                float distance = point.magnitude;
                float alpha = Mathf.Clamp01(1f - distance);
                alpha = alpha * alpha * (3f - 2f * alpha);
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        texture.SetPixels(pixels);
        texture.Apply();
        explosionSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        explosionSprite.name = "Bomb Flash";
        return explosionSprite;
    }

    private void UpdateRecoil()
    {
        if (pistolVisual == null) return;
        if (recoilTime <= 0f)
        {
            pistolVisual.localPosition = pistolRestLocalPosition;
            return;
        }
        recoilTime -= Time.deltaTime;
        float offset = Mathf.Sin(Mathf.Clamp01(recoilTime / 0.11f) * Mathf.PI) * 0.14f;
        pistolVisual.localPosition = pistolRestLocalPosition - Vector3.up * offset;
    }

private void BuildHud()
    {
        hudCanvas = CreateCanvas("Game HUD Canvas");
        RectTransform root = hudCanvas.transform as RectTransform;
        safeAreaRoot = CreateSafeAreaRoot(root);

        Color blue = new Color(0.10f, 0.43f, 0.76f, 1f);
        Color cream = new Color(1f, 0.98f, 0.91f, 0.97f);
        Color navy = new Color(0.075f, 0.19f, 0.47f, 1f);

        CreateSpriteImage(safeAreaRoot, "Balls Card", ballsPanelSprite, cream,
            new Vector2(0.145f, 0.912f), new Vector2(0.255f, 0.145f), false);
        CreateSpriteImage(safeAreaRoot, "Balls Header", blueLabelSprite, blue,
            new Vector2(0.145f, 0.969f), new Vector2(0.205f, 0.052f), false);
        Text ballsHeader = CreateText(safeAreaRoot, "Balls Header Label", "BALLS", 17, Color.white,
            TextAnchor.MiddleCenter, new Vector2(0.145f, 0.969f), new Vector2(0.18f, 0.035f), headingFont);
        AddTextOutline(ballsHeader, navy, new Vector2(1.4f, -1.4f));
        ballsText = CreateText(safeAreaRoot, "Balls Count", remainingBalls.ToString("00"), 39, navy,
            TextAnchor.MiddleCenter, new Vector2(0.145f, 0.912f), new Vector2(0.19f, 0.078f), headingFont);
        AddTextOutline(ballsText, Color.white, new Vector2(1.2f, -1.2f));

        CreateSpriteImage(safeAreaRoot, "Level Card", levelPanelSprite, Color.white,
            new Vector2(0.515f, 0.912f), new Vector2(0.43f, 0.148f), false);
        CreateText(safeAreaRoot, "Level Label", "LEVEL " + level.number.ToString("00"), 25, navy,
            TextAnchor.MiddleCenter, new Vector2(0.515f, 0.951f), new Vector2(0.34f, 0.042f), headingFont);
        CreateText(safeAreaRoot, "Target Caption", "TARGETS", 14, blue,
            TextAnchor.MiddleLeft, new Vector2(0.455f, 0.909f), new Vector2(0.17f, 0.032f), bodyFont);
        targetText = CreateText(safeAreaRoot, "Target Count", targetsRemaining.ToString("00"), 23, navy,
            TextAnchor.MiddleCenter, new Vector2(0.615f, 0.909f), new Vector2(0.09f, 0.038f), headingFont);
        CreateText(safeAreaRoot, "Level Subtitle", level.title.ToUpperInvariant(), 13, Ink,
            TextAnchor.MiddleCenter, new Vector2(0.515f, 0.875f), new Vector2(0.34f, 0.028f), bodyFont);

        Button menu = CreateSpriteButton(safeAreaRoot, "Settings Menu", settingsPanelSprite,
            new Vector2(0.865f, 0.937f), new Vector2(0.18f, 0.092f));
        menu.onClick.AddListener(WarfestSession.ReturnToMenu);

        if (level.number != 1)
        {
            CreateText(safeAreaRoot, "Instruction", "DRAG TO AIM  -  RELEASE TO FIRE", 15, navy,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0.807f), new Vector2(0.82f, 0.045f), bodyFont);
        }
    }

private void RefreshHud()
    {
        if (ballsText != null) ballsText.text = remainingBalls.ToString("00");
        for (int i = 0; i < ballSlots.Count; i++)
        {
            ballSlots[i].color = i < remainingBalls ? Ink : DisabledBall;
        }
    }

private void ShowFailure()
    {
        levelEnded = true;
        Transform parent = safeAreaRoot != null ? safeAreaRoot : (hudCanvas != null ? hudCanvas.transform : null);
        if (parent == null) return;

        CreateImage(parent, "Failure Panel", new Color(1f, 1f, 1f, 0.97f), new Vector2(0.5f, 0.5f), new Vector2(0.50f, 0.54f));
        CreateText(parent, "Failure Title", "OUT OF BALLS", 34, Ink, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.65f), new Vector2(0.42f, 0.10f));
        Button retry = CreateButton(parent, "Retry", "RETRY LEVEL", Ink, new Vector2(0.5f, 0.49f), new Vector2(0.34f, 0.12f), 15);
        retry.onClick.AddListener(() => WarfestSession.LoadLevel(level.number - 1));
        Button menu = CreateButton(parent, "Back to Menu", "MAIN MENU", new Color(0.35f, 0.48f, 0.58f), new Vector2(0.5f, 0.33f), new Vector2(0.34f, 0.12f), 15);
        menu.onClick.AddListener(WarfestSession.ReturnToMenu);
    }

private Canvas CreateCanvas(string name)
    {
        GameObject gameObject = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = gameObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        hudScaler = gameObject.GetComponent<CanvasScaler>();
        hudScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        hudScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        hudScaler.matchWidthOrHeight = 0.5f;
        ApplyCanvasScale();
        return canvas;
    }

    private void ApplyCanvasScale()
    {
        if (hudScaler == null) return;

        // Use iPhone point dimensions rather than Retina pixel dimensions so text and touch targets
        // retain a readable physical size on both device orientations.
        Vector2 referenceResolution = Screen.width >= Screen.height
            ? new Vector2(844f, 390f)
            : new Vector2(390f, 844f);
        if (referenceResolution == appliedReferenceResolution) return;

        appliedReferenceResolution = referenceResolution;
        hudScaler.referenceResolution = referenceResolution;
    }

    private RectTransform CreateSafeAreaRoot(RectTransform parent)
    {
        GameObject gameObject = new GameObject("Safe Area", typeof(RectTransform));
        gameObject.transform.SetParent(parent, false);
        safeAreaRoot = gameObject.GetComponent<RectTransform>();
        appliedSafeArea = new Rect();
        ApplySafeArea();
        return safeAreaRoot;
    }

    private void ApplySafeArea()
    {
        if (safeAreaRoot == null || Screen.width <= 0 || Screen.height <= 0) return;
        Rect safeArea = Screen.safeArea;
        if (safeArea == appliedSafeArea) return;

        appliedSafeArea = safeArea;
        safeAreaRoot.anchorMin = new Vector2(safeArea.xMin / Screen.width, safeArea.yMin / Screen.height);
        safeAreaRoot.anchorMax = new Vector2(safeArea.xMax / Screen.width, safeArea.yMax / Screen.height);
        safeAreaRoot.offsetMin = Vector2.zero;
        safeAreaRoot.offsetMax = Vector2.zero;
    }

    private GameObject CreateSprite(string name, Sprite sprite, Vector3 position, Vector2 scale, int sortingOrder)
    {
        GameObject gameObject = new GameObject(name, typeof(SpriteRenderer));
        gameObject.transform.SetParent(worldRoot, false);
        gameObject.transform.localPosition = position;
        gameObject.transform.localScale = new Vector3(scale.x, scale.y, 1f);
        SpriteRenderer renderer = gameObject.GetComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = Color.white;
        renderer.sortingOrder = sortingOrder;
        return gameObject;
    }

    private Image CreateImage(Transform parent, string name, Color color, Vector2 center, Vector2 size)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        gameObject.transform.SetParent(parent, false);
        ApplyRect(gameObject.GetComponent<RectTransform>(), parent, center, size);
        Image image = gameObject.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private Image CreateSpriteImage(Transform parent, string name, Sprite sprite, Color fallbackColor,
        Vector2 center, Vector2 size, bool preserveAspect)
    {
        Image image = CreateImage(parent, name, sprite != null ? Color.white : fallbackColor, center, size);
        image.sprite = sprite;
        image.preserveAspect = preserveAspect;
        return image;
    }

    private RectTransform CreateRect(Transform parent, string name, Vector2 center, Vector2 size)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform));
        gameObject.transform.SetParent(parent, false);
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        ApplyRect(rect, parent, center, size);
        return rect;
    }

private Text CreateText(Transform parent, string name, string value, int size, Color color, TextAnchor alignment,
        Vector2 center, Vector2 dimensions, Font typeface = null)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(Text));
        gameObject.transform.SetParent(parent, false);
        ApplyRect(gameObject.GetComponent<RectTransform>(), parent, center, dimensions);
        Text text = gameObject.GetComponent<Text>();
        text.font = typeface != null ? typeface : font;
        text.text = value;
        text.fontSize = size;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = Mathf.Max(10, size - 12);
        text.resizeTextMaxSize = size;
        text.color = color;
        text.alignment = alignment;
        text.fontStyle = FontStyle.Bold;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        return text;
    }

    private static void AddTextOutline(Text text, Color color, Vector2 distance)
    {
        if (text == null) return;
        Outline outline = text.gameObject.AddComponent<Outline>();
        outline.effectColor = color;
        outline.effectDistance = distance;
        outline.useGraphicAlpha = true;
    }

    private Button CreateSpriteButton(Transform parent, string name, Sprite sprite, Vector2 center, Vector2 size)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        gameObject.transform.SetParent(parent, false);
        ApplyRect(gameObject.GetComponent<RectTransform>(), parent, center, size);
        Image image = gameObject.GetComponent<Image>();
        image.sprite = sprite;
        image.color = Color.white;
        image.preserveAspect = true;
        Button button = gameObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.92f);
        colors.pressedColor = new Color(0.82f, 0.88f, 1f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;
        return button;
    }

    private Button CreateButton(Transform parent, string name, string label, Color color, Vector2 center, Vector2 size, int fontSize)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        gameObject.transform.SetParent(parent, false);
        ApplyRect(gameObject.GetComponent<RectTransform>(), parent, center, size);
        Image image = gameObject.GetComponent<Image>();
        image.color = color;
        Button button = gameObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.18f);
        colors.pressedColor = Color.Lerp(color, Color.black, 0.16f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;
        CreateText(gameObject.transform, "Label", label, fontSize, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.one);
        return button;
    }

private static void ApplyRect(RectTransform rect, Transform parent, Vector2 center, Vector2 size)
    {
        if (size == Vector2.one)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return;
        }

        rect.anchorMin = center - size * 0.5f;
        rect.anchorMax = center + size * 0.5f;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }


private void ReleaseModelPhysics()
    {
        if (modelPhysicsReleased || level.number > WarfestLevelCatalog.AuthoredLevelCount) return;
        modelPhysicsReleased = true;

        for (int i = 0; i < blocks.Count; i++)
        {
            if (blocks[i] == null) continue;
            Rigidbody2D body = blocks[i].GetComponent<Rigidbody2D>();
            if (body == null) continue;
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 1f;
            body.WakeUp();
        }
    }


private void CreateBackground()
    {
        if (backgroundSprite == null || gameplayCamera == null) return;

        float worldHeight = gameplayCamera.orthographicSize * 2f;
        float worldWidth = worldHeight * gameplayCamera.aspect;
        Vector2 spriteSize = backgroundSprite.bounds.size;
        float coverScale = Mathf.Max(
            worldWidth / Mathf.Max(0.001f, spriteSize.x),
            worldHeight / Mathf.Max(0.001f, spriteSize.y));

        GameObject background = CreateSprite(
            "Gameplay Background",
            backgroundSprite,
            new Vector3(gameplayCamera.transform.position.x, gameplayCamera.transform.position.y, 8f),
            Vector2.one * coverScale,
            -100);
        background.GetComponent<SpriteRenderer>().color = Color.white;
    }
}

public sealed class WarfestTarget : MonoBehaviour
{
    private WarfestGameController controller;
    private bool broken;
    private bool bomb;

    public bool IsBomb => bomb;
    public bool IsBroken => broken;

    public void Initialize(WarfestGameController owner, bool isBomb = false)
    {
        controller = owner;
        bomb = isBomb;
    }

    public void Break()
    {
        if (broken) return;
        broken = true;
        if (bomb) controller.DetonateBomb(this);
        else controller.RegisterTargetBroken(this);
    }

    public void BreakFromExplosion()
    {
        if (broken) return;
        broken = true;
        controller.RegisterTargetBroken(this);
    }

    private void Update()
    {
        if (broken || controller == null) return;
        Vector3 position = transform.position;
        if (position.y < -4.65f || Mathf.Abs(position.x) > 5.4f) BreakFromExplosion();
    }
}
