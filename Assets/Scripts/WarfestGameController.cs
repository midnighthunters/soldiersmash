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
    private Sprite tableSprite;
    private Sprite cannonBaseSprite;
    private GameObject[] boxModelPrefabs;
    private Texture2D[] boxModelTextures;
    private Material[] boxModelMaterials;
    private GameObject tableModelPrefab;
    private Material tableModelMaterial;
    private readonly List<float> modelTableTopYs = new List<float>();
    private readonly List<int> blockDepthLayers = new List<int>();
    private Sprite[] blockSprites;
    private int ballCapacity;
    private int remainingBalls;
    private int targetsRemaining;


    private bool fireInputArmed;
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
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
        };
        boxModelTextures = new[]
        {
            Resources.Load<Texture2D>("box/shaded"),
            Resources.Load<Texture2D>("box2/shaded"),
            Resources.Load<Texture2D>("box3/shaded"),
            Resources.Load<Texture2D>("long_box/shaded"),
            Resources.Load<Texture2D>("soldier/shaded"),
            Resources.Load<Texture2D>("cannister/shaded"),
        };
        boxModelMaterials = new Material[boxModelTextures.Length];
        for (int i = 0; i < boxModelTextures.Length; i++)
        {
            boxModelMaterials[i] = CreateBrightModelMaterial(boxModelTextures[i], "Warfest Block " + (i + 1));
        }

        tableModelPrefab = Resources.Load<GameObject>("table/base_basic_shaded");
        tableModelMaterial = CreateBrightModelMaterial(Resources.Load<Texture2D>("table/shaded"), "Warfest Table");
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

        if (!TryGetPointer(out Vector2 position, out bool held, out bool pressedThisFrame))
        {
            fireInputArmed = true;
            return;
        }

        // A complete release is required before every shot. This prevents a held touch,
        // play-mode focus change, or stale pointer state from ever auto-firing.
        if (!held)
        {
            fireInputArmed = true;
            return;
        }
        if (!fireInputArmed || !pressedThisFrame) return;

        fireInputArmed = false;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        AimAt(position);
        Fire();
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

        const float tabletopGap = 0.004f;
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
        visual.transform.localRotation = Quaternion.Euler(-90f, spec.variant == 4 ? 180f : 0f, 0f);
        ApplyModelMaterial(visual, GetBoxMaterial(spec.variant));

        Bounds sourceBounds = GetModelBounds(visual);
        float widthScale = (spec.width + visualOverlap) / sourceBounds.size.x;
        float heightScale = (spec.height + visualOverlap) / sourceBounds.size.y;
        float depthScale = Mathf.Min(widthScale, heightScale);
        visual.transform.localScale = Vector3.Scale(
            visual.transform.localScale,
            new Vector3(widthScale, heightScale, depthScale));

        Bounds bounds = GetModelBounds(visual);
        Vector3 blockPos = block.transform.position;
        visual.transform.localPosition += new Vector3(
            blockPos.x - bounds.center.x,
            blockPos.y - bounds.center.y,
            blockPos.z - bounds.center.z);

        int tableIndex = Mathf.Clamp(spec.tableIndex, 0, Mathf.Max(0, modelTableTopYs.Count - 1));
        float tableTopY = modelTableTopYs.Count > 0 ? modelTableTopYs[tableIndex] : -0.351f;
        float desiredBottomY = tableTopY + tabletopGap + spec.yOffset;
        block.transform.position = new Vector3(
            blockPos.x,
            desiredBottomY + spec.height * 0.5f,
            blockPos.z);

        BoxCollider2D gameplayCollider = block.AddComponent<BoxCollider2D>();
        gameplayCollider.size = new Vector2(
            Mathf.Max(0.05f, spec.width - colliderInset),
            Mathf.Max(0.05f, spec.height - colliderInset));

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
        body.mass = spec.width * spec.height * 1.45f;
        body.gravityScale = 0f;
        body.linearDamping = 0.16f;
        body.angularDamping = 0.55f;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        WarfestTarget target = block.AddComponent<WarfestTarget>();
        target.Initialize(this);
        blocks.Add(block);
        blockDepthLayers.Add(spec.depthLayer);
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
        pistolPivot.position = new Vector3(0f, -3.47f, -1f);

        pistolVisual = CreateSprite(
            "Pistol",
            pistolSprite,
            new Vector3(0f, 0.38f, 0f),
            new Vector2(0.21f, 0.21f),
            5).transform;
        pistolVisual.SetParent(pistolPivot, false);
        pistolRestLocalPosition = pistolVisual.localPosition;

        muzzle = new GameObject("Muzzle").transform;
        muzzle.SetParent(pistolPivot, false);
        muzzle.localPosition = new Vector3(0f, 0.98f, 0f);
    }

private void CreateCannonBase()
    {
        if (pistolBaseSprite == null) return;
        CreateSprite(
            "Pistol Base",
            pistolBaseSprite,
            new Vector3(0f, -3.82f, -0.25f),
            new Vector2(0.22f, 0.22f),
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
        Vector3 worldPosition = gameplayCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10f));
        Vector2 direction = (Vector2)(worldPosition - pistolPivot.position);
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
        CreateBall(muzzle.position, muzzle.up);

        if (remainingBalls <= 0 && targetsRemaining > 0) StartCoroutine(ShowFailureAfterDelay());
    }

    private void CreateBall(Vector2 position, Vector2 direction)
    {
        GameObject ball = new GameObject("Shot Ball", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D), typeof(WarfestBall));

        int shotLayer = LayerMask.NameToLayer("WarfestShot");
        if (shotLayer >= 0) ball.layer = shotLayer;
        ball.transform.SetParent(worldRoot, false);
        ball.transform.position = position;
        ball.transform.localScale = Vector3.one * ShotRadius;

        SpriteRenderer renderer = ball.GetComponent<SpriteRenderer>();
        renderer.sprite = GetCannonBaseSprite();
        renderer.color = new Color(1f, 0.82f, 0.22f, 1f);
        renderer.sortingOrder = 6;

        CircleCollider2D collider = ball.GetComponent<CircleCollider2D>();
        collider.radius = 0.5f;
        Rigidbody2D body = ball.GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Dynamic;
        body.mass = 0.22f;
        body.gravityScale = 0.35f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.linearVelocity = direction.normalized * ShotSpeed;
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
        if (targetsRemaining == 0)
        {
            levelEnded = true;
            WarfestSession.CompleteLevel(level.number - 1);
        }
        else if (level.number > WarfestLevelCatalog.AuthoredLevelCount)
        {
            Destroy(target.gameObject, 0.65f);
        }
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
        Color orange = new Color(0.98f, 0.57f, 0.12f, 1f);
        Color cream = new Color(1f, 0.98f, 0.91f, 0.97f);

        CreateImage(safeAreaRoot, "Balls Card", cream, new Vector2(0.15f, 0.93f), new Vector2(0.25f, 0.13f));
        CreateImage(safeAreaRoot, "Balls Header", blue, new Vector2(0.15f, 0.978f), new Vector2(0.22f, 0.035f));
        CreateText(safeAreaRoot, "Balls Header Label", "BALLS", 13, Color.white, TextAnchor.MiddleCenter, new Vector2(0.15f, 0.978f), new Vector2(0.20f, 0.03f));
        ballsText = CreateText(safeAreaRoot, "Balls Count", remainingBalls.ToString("00"), 30, Ink, TextAnchor.MiddleCenter, new Vector2(0.15f, 0.925f), new Vector2(0.20f, 0.075f));

        CreateImage(safeAreaRoot, "Level Card", new Color(1f, 1f, 1f, 0.92f), new Vector2(0.51f, 0.93f), new Vector2(0.35f, 0.14f));
        CreateText(safeAreaRoot, "Level Label", "LEVEL " + level.number.ToString("00"), 17, Ink, TextAnchor.MiddleLeft, new Vector2(0.45f, 0.962f), new Vector2(0.20f, 0.04f));
        CreateText(safeAreaRoot, "Target Caption", "TARGETS", 11, blue, TextAnchor.MiddleLeft, new Vector2(0.43f, 0.915f), new Vector2(0.15f, 0.03f));
        targetText = CreateText(safeAreaRoot, "Target Count", targetsRemaining.ToString("00"), 17, Ink, TextAnchor.MiddleCenter, new Vector2(0.56f, 0.915f), new Vector2(0.10f, 0.035f));
        CreateText(safeAreaRoot, "Level Subtitle", level.title.ToUpperInvariant(), 11, Ink, TextAnchor.MiddleCenter, new Vector2(0.51f, 0.885f), new Vector2(0.32f, 0.03f));

        Button menu = CreateButton(safeAreaRoot, "Menu", "MENU", orange, new Vector2(0.87f, 0.95f), new Vector2(0.17f, 0.075f), 14);
        menu.onClick.AddListener(WarfestSession.ReturnToMenu);

        if (level.number != 1)
        {
            CreateText(safeAreaRoot, "Instruction", "TAP A TARGET TO FIRE", 14, Ink, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.82f), new Vector2(0.58f, 0.045f));
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

    private RectTransform CreateRect(Transform parent, string name, Vector2 center, Vector2 size)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform));
        gameObject.transform.SetParent(parent, false);
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        ApplyRect(rect, parent, center, size);
        return rect;
    }

private Text CreateText(Transform parent, string name, string value, int size, Color color, TextAnchor alignment, Vector2 center, Vector2 dimensions)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(Text));
        gameObject.transform.SetParent(parent, false);
        ApplyRect(gameObject.GetComponent<RectTransform>(), parent, center, dimensions);
        Text text = gameObject.GetComponent<Text>();
        text.font = font;
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

    public void Initialize(WarfestGameController owner)
    {
        controller = owner;
    }

    public void Break()
    {
        if (broken) return;
        broken = true;
        controller.RegisterTargetBroken(this);
    }
}
