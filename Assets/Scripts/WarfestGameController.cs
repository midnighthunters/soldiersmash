using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class WarfestGameController : MonoBehaviour
{
    private static readonly Color LightBackground = new Color(0.93f, 0.96f, 0.98f, 1f);
    private static readonly Color Ink = new Color(0.08f, 0.14f, 0.22f, 1f);
    private static readonly Color LightPanel = new Color(1f, 1f, 1f, 0.95f);

    private readonly List<GameObject> blocks = new List<GameObject>();
    private readonly List<Rigidbody2D> blockBodies = new List<Rigidbody2D>();
    private readonly List<Collider2D> blockColliders = new List<Collider2D>();
    private readonly List<WarfestTarget> blockTargets = new List<WarfestTarget>();
    public List<WarfestTarget> BlockTargets => blockTargets;
    private readonly Collider2D[] targetOverlapResults = new Collider2D[16];
    private readonly Collider2D[] blastOverlapResults = new Collider2D[64];
    private readonly HashSet<Rigidbody2D> pushedBodies = new HashSet<Rigidbody2D>();
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
    private Sprite leaveIconSprite;
    private Sprite soundIconSprite;
    private Sprite musicIconSprite;
    private Sprite settingsEnabledSprite;
    private Sprite settingsDisabledSprite;
    private Sprite explosionSprite;
    private Material explosionParticleMaterial;
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
    private readonly List<float> modelTableCenterXs = new List<float>();
    private readonly List<float> modelTableDepths = new List<float>();
    private readonly List<int> blockDepthLayers = new List<int>();
    private Sprite[] blockSprites;
    private Sprite[] boosterSprites;

    // Persistent static caches to eliminate redundant Resources.Load, Material instantiation, and procedural synthesis
    private static bool s_spritesLoaded;
    private static Sprite s_pistolSprite;
    private static Sprite s_pistolBaseSprite;
    private static Sprite s_backgroundSprite;
    private static Sprite s_ballsPanelSprite;
    private static Sprite s_levelPanelSprite;
    private static Sprite s_blueLabelSprite;
    private static Sprite s_settingsPanelSprite;
    private static Sprite s_leaveIconSprite;
    private static Sprite s_soundIconSprite;
    private static Sprite s_musicIconSprite;
    private static Sprite s_settingsDisabledSprite;
    private static Sprite s_settingsEnabledSprite;
    private static Sprite[] s_blockSprites;
    private static Sprite[] s_boosterSprites;
    private static GameObject[] s_boxModelPrefabs;
    private static Texture2D[] s_boxModelTextures;
    private static Material[] s_boxModelMaterials;
    private static GameObject s_tableModelPrefab;
    private static Material s_tableModelMaterial;
    private static GameObject s_ballModelPrefab;
    private static Material s_ballModelMaterial;
    private static Sprite s_tableSprite;
    private static Material s_aimLineMaterial;
    private static Material s_skullBallMaterial;
    private static Sprite s_explosionSprite;
    private static Material s_explosionParticleMaterial;
    private static AudioClip s_shootClip;
    private static AudioClip s_blockPopClip;
    private static AudioClip s_musicClip;
    private static bool s_ballVisualConfigured;
    private static Vector3 s_ballVisualScaleUnit;
    private static Vector3 s_ballVisualCenterOffsetUnit;

    // High-performance explosion VFX pool
    private sealed class ExplosionVfxItem
    {
        public GameObject rootObject;
        public Transform flashTransform;
        public SpriteRenderer flashRenderer;
        public ParticleSystem sparks;
        public Coroutine flashRoutine;
    }
    private readonly List<ExplosionVfxItem> explosionPool = new List<ExplosionVfxItem>();
    private int nextExplosionPoolIndex;
    private const int ExplosionPoolCapacity = 6;

    private int ballCapacity;
    private int remainingBalls;
    private int targetsRemaining;
    private int activeBalls;
    private bool checkFallenTargets;
    private float nextFallenTargetCheckTime;

    // Booster flow state. A shot-modifying booster (skull/spread/missile) is "armed" until the
    // next shot consumes it; infinite balls is a short timed effect applied the moment it is tapped.
    private bool hasArmedBooster;
    private WarfestBooster armedBooster;
    private bool infiniteBallsActive;
    private bool infiniteBallsWaitingForFirstShot;
    private float infiniteBallsTimeRemaining;
    private readonly Button[] boosterButtons = new Button[WarfestSession.BoosterCount];
    private readonly Image[] boosterButtonImages = new Image[WarfestSession.BoosterCount];
    private readonly Text[] boosterCountLabels = new Text[WarfestSession.BoosterCount];
    private readonly GameObject[] boosterArmGlows = new GameObject[WarfestSession.BoosterCount];
    private GameObject boosterStatusPanel;
    private Text boosterStatusText;
    private GameObject boosterProgressTrack;
    private Image boosterProgressFill;


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
    private GameObject settingsFlyout;
    private GameObject leaveConfirmation;
    private Image soundButtonBackground;
    private Image musicButtonBackground;
    private Text failureLifeStatus;
    private Button retryButton;
    private bool settingsOpen;
    private bool soundEnabled;
    private bool musicEnabled;
    private int displayedFailureLives = -1;
    private int displayedFailureSeconds = -1;
    private float nextLifeStatusRefreshTime;
    private AudioSource sfxSource;
    private AudioSource musicSource;
    private AudioClip shootClip;
    private AudioClip blockPopClip;
    private AudioClip musicClip;
    private float lastBlockPopTime = -1f;

    private const float TableColliderWidth = 16.10f;
    private const float TableColliderHeight = 0.52f;
    private const float TableColliderOffsetY = 1.64f;
    private const float ShotRadius = 0.16f;
    private const float ShotSpeed = 17f;
    private const float ShotLifetime = 4f;
    private const float BlockSizeMultiplier = 1.15f;
    private const float CannonBasePositionY = -4.72f;
    private const float CannonPivotOffsetY = 0.52f;
    private const float FallenTargetCheckInterval = 0.10f;
    private const float BoosterUiUpdateInterval = 0.05f;
    private const float InfiniteBallsDurationSeconds = 3f;
    private const string SoundEnabledKey = "Warfest.SoundEnabled";
    private const string MusicEnabledKey = "Warfest.MusicEnabled";
    private const int AudioSampleRate = 22050;
    private float recoilTime;
    private float nextBoosterUiUpdateTime;
    private int aimCollisionMask = ~0;

    private void Start()
    {
        Application.runInBackground = true;
        Application.targetFrameRate = 120;
        QualitySettings.vSyncCount = 0;
        Time.fixedDeltaTime = Mathf.Min(0.04f, 0.02f * Mathf.Max(1f, Time.timeScale));

        font = bodyFont != null ? bodyFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (headingFont == null) headingFont = font;
        if (bodyFont == null) bodyFont = font;
        soundEnabled = WarfestAudio.SoundEnabled;
        musicEnabled = WarfestAudio.MusicEnabled;
        LoadOriginalSprites();
        EnsureEventSystem();
        EnsureCamera();
        CachePhysicsLayers();
        level = WarfestLevelCatalog.Get(WarfestSession.SelectedLevel);
        ballCapacity = WarfestSession.GetBallAllowance(level.number - 1);
        remainingBalls = ballCapacity;
        BuildWorld();
        BuildHud();
        BuildAudio();
        RefreshHud();
        ApplyAudioPreferences();
    }

    private void LoadOriginalSprites()
    {
        if (s_spritesLoaded)
        {
            pistolSprite = s_pistolSprite;
            pistolBaseSprite = s_pistolBaseSprite;
            backgroundSprite = s_backgroundSprite;
            ballsPanelSprite = s_ballsPanelSprite;
            levelPanelSprite = s_levelPanelSprite;
            blueLabelSprite = s_blueLabelSprite;
            settingsPanelSprite = s_settingsPanelSprite;
            leaveIconSprite = s_leaveIconSprite;
            soundIconSprite = s_soundIconSprite;
            musicIconSprite = s_musicIconSprite;
            settingsDisabledSprite = s_settingsDisabledSprite;
            settingsEnabledSprite = s_settingsEnabledSprite;
            blockSprites = s_blockSprites;
            boosterSprites = s_boosterSprites;
            boxModelPrefabs = s_boxModelPrefabs;
            boxModelTextures = s_boxModelTextures;
            boxModelMaterials = s_boxModelMaterials;
            tableModelPrefab = s_tableModelPrefab;
            tableModelMaterial = s_tableModelMaterial;
            ballModelPrefab = s_ballModelPrefab;
            ballModelMaterial = s_ballModelMaterial;
            tableSprite = s_tableSprite;
            return;
        }

        Sprite[] pistolSprites = Resources.LoadAll<Sprite>("new_gun");
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
            float panelScaleX = panelTexture.width / 1536f;
            float panelScaleY = panelTexture.height / 1024f;
            ballsPanelSprite = CreateSheetSprite(panelTexture, ScaleSheetRect(new Rect(35f, 426f, 480f, 431f), panelScaleX, panelScaleY), "Balls Panel");
            levelPanelSprite = CreateSheetSprite(panelTexture, ScaleSheetRect(new Rect(571f, 423f, 931f, 471f), panelScaleX, panelScaleY), "Level Panel");
            blueLabelSprite = CreateSheetSprite(panelTexture, ScaleSheetRect(new Rect(222f, 95f, 494f, 201f), panelScaleX, panelScaleY), "Blue Label");
            settingsPanelSprite = CreateSheetSprite(panelTexture, ScaleSheetRect(new Rect(930f, 0f, 410f, 404f), panelScaleX, panelScaleY), "Settings Button");
        }

        leaveIconSprite = WarfestAudio.GetLeaveIconSprite();
        soundIconSprite = WarfestAudio.GetSoundIconSprite();
        musicIconSprite = WarfestAudio.GetMusicIconSprite();
        settingsDisabledSprite = WarfestAudio.GetSettingsDisabledSprite();
        settingsEnabledSprite = WarfestAudio.GetSettingsEnabledSprite();
        blockSprites = Resources.LoadAll<Sprite>("blocks");
        boosterSprites = Resources.LoadAll<Sprite>("boosters");

        for (int i = 0; i < pistolSprites.Length; i++)
        {
            if (pistolSprites[i].name == "gun") pistolSprite = pistolSprites[i];
            if (pistolSprites[i].name == "base") pistolBaseSprite = pistolSprites[i];
        }
        if (pistolSprite == null && pistolSprites.Length > 0) pistolSprite = pistolSprites[0];
        if (pistolBaseSprite == null && pistolSprites.Length > 1) pistolBaseSprite = pistolSprites[1];

        string[] modelFolders = { "box", "box2", "box3", "long_box", "long_box2", "soldier", "cannister", "bomb", "king" };
        boxModelPrefabs = new GameObject[modelFolders.Length];
        boxModelTextures = new Texture2D[modelFolders.Length];
        for (int i = 0; i < modelFolders.Length; i++)
        {
            boxModelPrefabs[i] = Resources.Load<GameObject>(modelFolders[i] + "/base");
            boxModelTextures[i] = Resources.Load<Texture2D>(modelFolders[i] + "/shaded");
        }
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

        s_pistolSprite = pistolSprite;
        s_pistolBaseSprite = pistolBaseSprite;
        s_backgroundSprite = backgroundSprite;
        s_ballsPanelSprite = ballsPanelSprite;
        s_levelPanelSprite = levelPanelSprite;
        s_blueLabelSprite = blueLabelSprite;
        s_settingsPanelSprite = settingsPanelSprite;
        s_leaveIconSprite = leaveIconSprite;
        s_soundIconSprite = soundIconSprite;
        s_musicIconSprite = musicIconSprite;
        s_settingsDisabledSprite = settingsDisabledSprite;
        s_settingsEnabledSprite = settingsEnabledSprite;
        s_blockSprites = blockSprites;
        s_boosterSprites = boosterSprites;
        s_boxModelPrefabs = boxModelPrefabs;
        s_boxModelTextures = boxModelTextures;
        s_boxModelMaterials = boxModelMaterials;
        s_tableModelPrefab = tableModelPrefab;
        s_tableModelMaterial = tableModelMaterial;
        s_ballModelPrefab = ballModelPrefab;
        s_ballModelMaterial = ballModelMaterial;
        s_tableSprite = tableSprite;
        s_spritesLoaded = true;

        if (pistolSprite == null || pistolBaseSprite == null || tableSprite == null || blockSprites == null || blockSprites.Length == 0)
        {
            Debug.LogError("Warfest requires both pistol sprites plus the original table and blocks sprites.");
        }
        if (!HasAnyBoxPrefab())
        {
            Debug.LogError("The 3D levels require the gameplay model set under Resources/{box,box2,box3,long_box,long_box2,soldier,cannister,bomb,king}/base.fbx.");
        }
        if (tableModelPrefab == null)
        {
            Debug.LogError("The 3D crate levels require Resources/table/base_basic_shaded.fbx.");
        }
    }

private bool HasAnyBoxPrefab()
    {
        if (boxModelPrefabs == null || boxModelPrefabs.Length == 0) return false;
        for (int i = 0; i < boxModelPrefabs.Length; i++)
        {
            if (boxModelPrefabs[i] == null) return false;
        }
        return true;
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
        // Every runtime target receives this Rigidbody2D mass. Weights are tuned per gameplay
        // role, independent of visual size. box2 anchors the base, the king is the heavy crown
        // piece, and light toppers (soldiers/barrels) topple readily when the structure is hit.
        switch (variant)
        {
            case 0: return 1.20f; // box       : light structural brick
            case 1: return 2.20f; // box2      : heavy structural brick (stable base)
            case 2: return 1.00f; // box3      : slim turret / wedge
            case 3: return 1.40f; // long_box  : chunky beam / lintel
            case 4: return 1.10f; // long_box2 : flat plank / roof cap
            case 5: return 0.80f; // soldier   : objective topper
            case 6: return 1.00f; // cannister : barrel (rolls / shifts weight)
            case 7: return 1.05f; // bomb      : explosive target
            case 8: return 5.00f; // king      : important heavyweight crown piece
            default: return 1.20f;
        }
    }

    private static void ApplyModelMaterial(GameObject model, Material material)
    {
        if (model == null || material == null) return;
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            renderer.sharedMaterial = material;
            // Gameplay models use an unlit material, so lighting probes, shadows and motion
            // vectors only add render work without changing the final image.
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
        }
    }


    private void Update()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
#endif
        float targetFixedDelta = Mathf.Min(0.04f, 0.02f * Mathf.Max(1f, Time.timeScale));
        if (Mathf.Abs(Time.fixedDeltaTime - targetFixedDelta) > 0.0001f)
        {
            Time.fixedDeltaTime = targetFixedDelta;
        }

        ApplyCanvasScale();
        ApplySafeArea();
        UpdateRecoil();
        UpdateInfiniteBallsTimer();
        if (failureLifeStatus != null && Time.unscaledTime >= nextLifeStatusRefreshTime)
        {
            nextLifeStatusRefreshTime = Time.unscaledTime + 1f;
            RefreshFailureLifeStatus();
        }
        if (checkFallenTargets && Time.time >= nextFallenTargetCheckTime)
        {
            nextFallenTargetCheckTime = Time.time + FallenTargetCheckInterval;
            CheckFallenTargets();
        }

        if (levelEnded || settingsOpen || gameplayCamera == null || muzzle == null) return;

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

        const float maxLength = 14f;
        RaycastHit2D hit = Physics2D.Raycast(origin, dir, maxLength, aimCollisionMask);
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
        var existing = FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (existing != null && existing.Length > 1)
        {
            for (int i = 1; i < existing.Length; i++)
            {
                if (existing[i] != null && existing[i].gameObject != null)
                {
                    Destroy(existing[i].gameObject);
                }
            }
            return;
        }
        if ((existing != null && existing.Length == 1) || EventSystem.current != null) return;
        new GameObject("EventSystem", typeof(EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
    }

    private void CachePhysicsLayers()
    {
        aimCollisionMask = ~0;
        int tableLayer = LayerMask.NameToLayer("WarfestTable");
        int shotLayer = LayerMask.NameToLayer("WarfestShot");
        if (tableLayer >= 0) aimCollisionMask &= ~(1 << tableLayer);
        if (shotLayer >= 0) aimCollisionMask &= ~(1 << shotLayer);
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

    private void ClearExistingWorld()
    {
        explosionPool.Clear();
        nextExplosionPoolIndex = 0;
        UnityEngine.SceneManagement.Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (!activeScene.isLoaded) return;
        GameObject[] roots = activeScene.GetRootGameObjects();
        for (int i = roots.Length - 1; i >= 0; i--)
        {
            GameObject root = roots[i];
            if (root == null) continue;
            if (root.name.StartsWith("Runtime Level") || root.name == "Gameplay Background")
            {
                if (Application.isPlaying) Destroy(root);
                else DestroyImmediate(root);
            }
        }
    }

private void BuildWorld()
    {
        ClearExistingWorld();
        worldRoot = new GameObject("Runtime Level // " + level.number.ToString("00")).transform;
        CreateBackground();
        bool isModelLevel = level.number >= 1 && level.number <= WarfestLevelCatalog.AuthoredLevelCount;
        bool canBuildModels = isModelLevel && tableModelPrefab != null && HasAnyBoxPrefab();

        if (canBuildModels)
        {
            List<WarfestLevelCatalog.ModelTableSpec> tableSpecs = new List<WarfestLevelCatalog.ModelTableSpec>();
            WarfestLevelCatalog.FillModelTables(level.number - 1, tableSpecs);
            modelTableTopYs.Clear();
            modelTableCenterXs.Clear();
            modelTableDepths.Clear();
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
        table.transform.localRotation = Quaternion.Euler(-90f, spec.yawDegrees, 0f);

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
        modelTableCenterXs.Add(tableBounds.center.x);
        modelTableDepths.Add(spec.depth);

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
        Vector2 scaledBlockSize = spec.size * BlockSizeMultiplier;
        Vector2 localScale = new Vector2(
            spriteSize.x > 0.0001f ? scaledBlockSize.x / spriteSize.x : 1f,
            spriteSize.y > 0.0001f ? scaledBlockSize.y / spriteSize.y : 1f);

        const float legacyTableTopY = -0.3515f;
        Vector2 scaledPosition = new Vector2(
            spec.position.x * BlockSizeMultiplier,
            legacyTableTopY + (spec.position.y - legacyTableTopY) * BlockSizeMultiplier);
        GameObject block = CreateSprite("Target " + (index + 1).ToString("00"), sprite, new Vector3(scaledPosition.x, scaledPosition.y, 0f), localScale, 2);
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
        // Only the small, fast projectiles need continuous collision. Blocks move under gravity
        // after impact, so discrete contacts avoid a costly continuous solver for every target.
        body.interpolation = RigidbodyInterpolation2D.None;
        body.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
        WarfestTarget target = block.AddComponent<WarfestTarget>();
        target.Initialize(this);
        blocks.Add(block);
        blockBodies.Add(body);
        blockColliders.Add(collider);
        blockTargets.Add(target);
        checkFallenTargets = true;
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
        int tableIndex = Mathf.Clamp(spec.tableIndex, 0, Mathf.Max(0, modelTableTopYs.Count - 1));
        float tableCenterX = modelTableCenterXs.Count > tableIndex ? modelTableCenterXs[tableIndex] : 0f;
        float scaledX = tableCenterX + (spec.x - tableCenterX) * BlockSizeMultiplier;
        GameObject block = new GameObject(layerName + " Crate " + (index + 1).ToString("00"));
        block.transform.SetParent(worldRoot, false);
        float renderDepth = modelTableDepths.Count > tableIndex
            ? modelTableDepths[tableIndex]
            : (spec.depthLayer == 0 ? WarfestLevelCatalog.FrontLayerZ : WarfestLevelCatalog.RearLayerZ);
        block.transform.localPosition = new Vector3(scaledX, 0f, renderDepth);

        GameObject visual = Instantiate(prefab);
        visual.name = "Visual";
        visual.transform.SetParent(block.transform, false);
        visual.transform.localRotation = Quaternion.Euler(-90f, GetModelYRotation(spec.variant), 0f);
        ApplyModelMaterial(visual, GetBoxMaterial(spec.variant));

        Bounds sourceBounds = GetModelBounds(visual);
        float scaledWidth = spec.width * BlockSizeMultiplier;
        float scaledHeight = spec.height * BlockSizeMultiplier;
        float widthScale = (scaledWidth + visualOverlap) / sourceBounds.size.x;
        float heightScale = (scaledHeight + visualOverlap) / sourceBounds.size.y;
        float depthScale = Mathf.Min(widthScale, heightScale);
        // Both requested models are fitted uniformly in depth and explicitly in the 2D gameplay plane.
        Vector3 fittedScale = new Vector3(widthScale, depthScale, heightScale);
        visual.transform.localScale = Vector3.Scale(visual.transform.localScale, fittedScale);

        // The long_box2 plank mesh is nearly flat in its depth axis, so a uniform fit would make
        // it vanish edge-on. Pin its depth-axis local scale to a solid slab per art direction.
        if (spec.variant == WarfestLevelCatalog.LONG_BOX2)
        {
            Vector3 ls = visual.transform.localScale;
            visual.transform.localScale = new Vector3(ls.x, 30f, ls.z);
        }

        Bounds fittedBounds = GetModelBounds(visual);
        Vector3 blockPosition = block.transform.position;
        visual.transform.position += blockPosition - fittedBounds.center;
        float visualHeight = GetModelBounds(visual).size.y;

        float tableTopY = modelTableTopYs.Count > 0 ? modelTableTopYs[tableIndex] : -0.351f;
        float desiredBottomY = tableTopY + tabletopGap + spec.yOffset * BlockSizeMultiplier;
        block.transform.position = new Vector3(
            blockPosition.x,
            desiredBottomY + visualHeight * 0.5f,
            blockPosition.z);

        // Tilted layouts (e.g. the 45-degree cannister tables) rotate the whole piece about its
        // centre so both the visual and the physics collider tilt together.
        if (Mathf.Abs(spec.rotation) > 0.01f)
        {
            block.transform.rotation = Quaternion.Euler(0f, 0f, spec.rotation);
        }

        // Keep the physics footprint aligned with the visible bottom. Every base row touches the
        // table surface, and each higher 0.72-unit row contacts the collider directly below it.
        float colliderHeight = Mathf.Max(0.05f, scaledHeight);
        BoxCollider2D gameplayCollider = block.AddComponent<BoxCollider2D>();
        gameplayCollider.size = new Vector2(
            Mathf.Max(0.05f, scaledWidth - colliderInset),
            colliderHeight);
        gameplayCollider.offset = new Vector2(0f, (colliderHeight - visualHeight) * 0.5f);

        // The two visual depth planes form independent physical stacks. Ignoring cross-layer
        // contacts keeps the slightly offset rear design from pushing the front layer apart.
        for (int i = 0; i < blocks.Count && i < blockDepthLayers.Count; i++)
        {
            if (blockDepthLayers[i] == spec.depthLayer) continue;
            BoxCollider2D otherCollider = i < blockColliders.Count ? blockColliders[i] as BoxCollider2D : null;
            if (otherCollider != null) Physics2D.IgnoreCollision(gameplayCollider, otherCollider, true);
        }

        Rigidbody2D body = block.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.mass = GetModelMass(spec.variant);
        body.gravityScale = 0f;
        body.linearDamping = 0.16f;
        body.angularDamping = 0.55f;
        body.interpolation = RigidbodyInterpolation2D.None;
        body.collisionDetectionMode = CollisionDetectionMode2D.Discrete;

        WarfestTarget target = block.AddComponent<WarfestTarget>();
        // Only the bomb variant carries explosive chain behaviour. Cannisters are ordinary
        // physical targets (they shift weight and topple, but do not detonate).
        target.Initialize(this, spec.variant == WarfestLevelCatalog.BOMB);
        blocks.Add(block);
        blockBodies.Add(body);
        blockColliders.Add(gameplayCollider);
        blockTargets.Add(target);
        blockDepthLayers.Add(spec.depthLayer);
    }

private static float GetModelYRotation(int variant)
    {
        // Both models face the player. long_box2 (variant 0) is flipped 180 on Y per art direction,
        // and the king keeps its front-facing 180 orientation.
        return 180f;
    }


    private static Bounds GetModelBounds(GameObject model)
    {
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
        if (renderers == null || renderers.Length == 0)
        {
            return new Bounds(model.transform.position, Vector3.zero);
        }
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
        pistolPivot.localScale = new Vector3(1.7f, 1.7f, 1f);
        pistolPivot.position = new Vector3(0f, CannonBasePositionY + CannonPivotOffsetY, -1f);

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
        muzzle.localPosition = new Vector3(0f, 1.45f, 0f);

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

        if (s_aimLineMaterial == null)
        {
            Shader lineShader = Shader.Find("Sprites/Default");
            if (lineShader == null) lineShader = Shader.Find("Universal Render Pipeline/Unlit");
            if (lineShader != null) s_aimLineMaterial = new Material(lineShader) { name = "Warfest Aim Line" };
        }
        if (s_aimLineMaterial != null) aimLine.sharedMaterial = s_aimLineMaterial;

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
            new Vector3(0f, CannonBasePositionY, -0.25f),
            new Vector2(0.46f, 0.46f),
            4);
    }

    private float ScreenPixelsToWorldHeight(float pixels)
    {
        const float referenceDesignHeight = 844f;
        float worldCameraHeight = gameplayCamera != null ? gameplayCamera.orthographicSize * 2f : 12.7f;
        return pixels * (worldCameraHeight / referenceDesignHeight);
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
            // A Touchscreen device can exist even on desktop/editor where the player is using a
            // mouse (it simply never reports a press). Only take the touch branch when the touch
            // is actually active, otherwise fall through to the mouse so clicks still fire.
            var primaryTouch = touchscreen.primaryTouch;
            bool touchActive = primaryTouch.press.isPressed
                || primaryTouch.press.wasPressedThisFrame
                || primaryTouch.press.wasReleasedThisFrame;
            if (touchActive)
            {
                screenPosition = primaryTouch.position.ReadValue();
                held = primaryTouch.press.isPressed;
                pressedThisFrame = primaryTouch.press.wasPressedThisFrame;
                return true;
            }
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
        if (levelEnded) return;
        checkFallenTargets = modelPhysicsReleased || level.number > WarfestLevelCatalog.AuthoredLevelCount;
        nextFallenTargetCheckTime = Time.time;

        // Infinite balls is selected in advance, but its three-second timer starts on this first shot.
        if (infiniteBallsActive && infiniteBallsWaitingForFirstShot)
        {
            infiniteBallsWaitingForFirstShot = false;
            infiniteBallsTimeRemaining = InfiniteBallsDurationSeconds;
            RefreshBoosterStatus();
        }

        // Infinite balls skips the allowance entirely; otherwise a shot needs a ball to spend.
        if (!infiniteBallsActive)
        {
            if (remainingBalls <= 0) return;
            remainingBalls--;
        }

        bool boosted = hasArmedBooster;
        WarfestBooster booster = armedBooster;

        recoilTime = 0.11f;
        PlayShootSound();
        RefreshHud();
        Vector2 launchPosition = muzzle.position;
        Vector2 launchDirection = hasAimPoint ? aimWorldPosition - launchPosition : (Vector2)muzzle.up;

        // Precision aiming: if the player tapped directly on a block, the shot is committed to that
        // exact block so it lands where they aimed instead of stopping at whatever crate happens to
        // sit lower in the trajectory.
        WarfestTarget intendedTarget = hasAimPoint ? FindTargetAt(aimWorldPosition) : null;

        if (boosted && booster == WarfestBooster.SpreadShot)
        {
            FireSpread(launchPosition, launchDirection);
        }
        else if (boosted && booster == WarfestBooster.SkullShot)
        {
            CreateBall(launchPosition, launchDirection, null, WarfestBall.ShotMode.Piercing);
        }
        else if (boosted && booster == WarfestBooster.Missile)
        {
            CreateBall(launchPosition, launchDirection, intendedTarget, WarfestBall.ShotMode.Explosive);
        }
        else
        {
            CreateBall(launchPosition, launchDirection, intendedTarget);
        }

        // Shot boosters were paid for when selected. Firing only clears the lock and status message.
        if (boosted)
        {
            hasArmedBooster = false;
            RefreshBoosterHud();
        }

        if (!infiniteBallsActive && remainingBalls <= 0 && targetsRemaining > 0)
        {
            StartCoroutine(ShowFailureAfterDelay());
        }
    }

    // Spread booster: three balls launched in a fan around the aim direction. The whole fan counts
    // as the single shot already spent by Fire().
    private void FireSpread(Vector2 launchPosition, Vector2 launchDirection)
    {
        float baseAngle = Mathf.Atan2(launchDirection.y, launchDirection.x) * Mathf.Rad2Deg;
        float[] offsets = { -13f, 0f, 13f };
        for (int i = 0; i < offsets.Length; i++)
        {
            float radians = (baseAngle + offsets[i]) * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
            CreateBall(launchPosition, direction, null);
        }
    }

    // Returns the front-most unbroken target whose collider contains the tapped world point, so a
    // shot resolves against the exact block the player aimed at (front plane wins ties on depth).
    private WarfestTarget FindTargetAt(Vector2 point)
    {
        int overlapCount = Physics2D.OverlapPoint(point, ContactFilter2D.noFilter, targetOverlapResults);
        WarfestTarget best = null;
        float bestZ = float.MaxValue;
        for (int i = 0; i < overlapCount; i++)
        {
            Collider2D overlap = targetOverlapResults[i];
            WarfestTarget target = overlap != null ? overlap.GetComponent<WarfestTarget>() : null;
            if (target == null || target.IsBroken) continue;
            float z = overlap.transform.position.z;
            if (z < bestZ)
            {
                bestZ = z;
                best = target;
            }
        }
        return best;
    }

    private void CreateBall(Vector2 position, Vector2 direction, WarfestTarget intendedTarget = null,
        WarfestBall.ShotMode mode = WarfestBall.ShotMode.Normal)
    {
        GameObject ball = new GameObject("Shot Ball", typeof(CircleCollider2D), typeof(Rigidbody2D), typeof(WarfestBall));

        // The skull ball is a heavier, larger wrecking ball so its plough-through reads clearly.
        bool piercing = mode == WarfestBall.ShotMode.Piercing;
        float radius = piercing ? ShotRadius * 1.55f : ShotRadius;

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
            ApplyModelMaterial(visual, piercing ? GetSkullBallMaterial() : ballModelMaterial);
            if (!s_ballVisualConfigured)
            {
                Bounds sourceBounds = GetModelBounds(visual);
                float diameter = radius * 2f;
                float visualScale = diameter / Mathf.Max(sourceBounds.size.x, sourceBounds.size.y);
                visual.transform.localScale *= visualScale;
                Bounds fittedBounds = GetModelBounds(visual);
                Vector3 centerInBall = ball.transform.InverseTransformPoint(fittedBounds.center);
                visual.transform.localPosition -= centerInBall;

                s_ballVisualScaleUnit = visual.transform.localScale / radius;
                s_ballVisualCenterOffsetUnit = centerInBall / radius;
                s_ballVisualConfigured = true;
            }
            else
            {
                visual.transform.localScale = s_ballVisualScaleUnit * radius;
                visual.transform.localPosition = -s_ballVisualCenterOffsetUnit * radius;
            }
        }
        else
        {
            GameObject fallbackVisual = new GameObject("Fallback Ball Visual", typeof(SpriteRenderer));
            fallbackVisual.transform.SetParent(ball.transform, false);
            fallbackVisual.transform.localScale = Vector3.one * (radius * 2f);
            SpriteRenderer renderer = fallbackVisual.GetComponent<SpriteRenderer>();
            renderer.sprite = GetCannonBaseSprite();
            renderer.color = piercing ? new Color(0.12f, 0.14f, 0.2f, 1f) : new Color(1f, 0.82f, 0.22f, 1f);
            renderer.sortingOrder = 6;
        }

        CircleCollider2D collider = ball.GetComponent<CircleCollider2D>();
        collider.radius = radius;

        // When the shot is committed to a specific tapped block, only ignore blocks that sit in front
        // of it on depth so the projectile reaches the intended target without wasting collision pair updates.
        if (intendedTarget != null && !piercing)
        {
            Collider2D targetCollider = intendedTarget.Collider;
            float targetZ = intendedTarget.transform.position.z;
            for (int i = 0; i < blockColliders.Count; i++)
            {
                Collider2D blockCollider = blockColliders[i];
                if (blockCollider != null && blockCollider != targetCollider && blockCollider.transform.position.z < targetZ - 0.02f)
                {
                    Physics2D.IgnoreCollision(collider, blockCollider, true);
                }
            }
        }

        Rigidbody2D body = ball.GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Dynamic;
        body.mass = piercing ? 0.6f : 0.22f;
        body.gravityScale = 0f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.linearVelocity = direction.normalized * ShotSpeed;
        body.angularVelocity = -direction.normalized.x * 540f;
        float impact = (5.8f + level.difficulty * 0.5f) * (piercing ? 1.8f : 1f);
        activeBalls++;
        ball.GetComponent<WarfestBall>().Initialize(direction, impact, ShotLifetime, mode, this);
    }

    // Lazily-built dark material for the skull wrecking ball so it reads distinct from the standard
    // projectile. Reuses the ball mesh's texture tinted down toward the skull-bomb art.
    private Material skullBallMaterial;
    private Material GetSkullBallMaterial()
    {
        if (s_skullBallMaterial != null) return s_skullBallMaterial;
        s_skullBallMaterial = ballModelMaterial != null
            ? new Material(ballModelMaterial)
            : CreateBrightModelMaterial(Resources.Load<Texture2D>("ball/shaded"), "Warfest Skull Ball");
        if (s_skullBallMaterial != null)
        {
            s_skullBallMaterial.name = "Warfest Skull Ball";
            Color tint = new Color(0.22f, 0.24f, 0.32f, 1f);
            if (s_skullBallMaterial.HasProperty("_BaseColor")) s_skullBallMaterial.SetColor("_BaseColor", tint);
            if (s_skullBallMaterial.HasProperty("_Color")) s_skullBallMaterial.SetColor("_Color", tint);
        }
        skullBallMaterial = s_skullBallMaterial;
        return s_skullBallMaterial;
    }

    private IEnumerator ShowFailureAfterDelay()
    {
        // The final shot may still be travelling, and the structure it strikes may still be
        // toppling. Blocks only count as cleared once they fall off the table, so wait for the
        // world to come to rest before judging the level - otherwise a winning last shot can be
        // called a loss while the pieces are still mid-fall. A timeout guards against jitter.
        yield return new WaitForSeconds(0.7f);

        float guard = 0f;
        while (!levelEnded && targetsRemaining > 0 && !WorldHasSettled() && guard < 6f)
        {
            guard += 0.15f;
            yield return new WaitForSeconds(0.15f);
        }

        if (!levelEnded && targetsRemaining > 0) ShowFailure();
    }

    // True once no shot is still in flight and every remaining block has effectively stopped
    // moving, meaning nothing else can clear a target this attempt.
    private bool WorldHasSettled()
    {
        if (activeBalls > 0) return false;

        for (int i = 0; i < blockBodies.Count; i++)
        {
            Rigidbody2D body = blockBodies[i];
            if (body == null) continue;
            if (body.bodyType == RigidbodyType2D.Dynamic && body.linearVelocity.sqrMagnitude > 0.04f)
            {
                return false;
            }
        }
        return true;
    }

    private void CheckFallenTargets()
    {
        bool hasMovingTarget = false;
        for (int i = 0; i < blockTargets.Count; i++)
        {
            WarfestTarget target = blockTargets[i];
            Rigidbody2D body = i < blockBodies.Count ? blockBodies[i] : null;
            if (target == null || target.IsBroken || body == null || body.bodyType != RigidbodyType2D.Dynamic) continue;

            if (body.IsAwake()) hasMovingTarget = true;
            Vector2 position = body.position;
            if (position.y < -4.65f || Mathf.Abs(position.x) > 5.4f) target.BreakFromExplosion();
        }

        // Sleeping bodies no longer need a continuous scan. Fire() re-arms this before a future
        // impact; while a structure is moving it is sampled at 10 Hz instead of every frame.
        checkFallenTargets = hasMovingTarget;
    }

    public void NotifyBallDestroyed()
    {
        activeBalls = Mathf.Max(0, activeBalls - 1);
    }

public void RegisterTargetBroken(WarfestTarget target)
    {
        if (levelEnded) return;
        ReleaseModelPhysics();
        checkFallenTargets = true;
        nextFallenTargetCheckTime = Time.time + FallenTargetCheckInterval;
        PlayBlockPopSound();

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
        ApplyBlast(center, 0.48f, bomb);
    }

    // Missile booster impact: the striking shell breaks and damages everything in a slightly wider
    // radius than a bomb. There is no source target to exclude, so the block it lands on is broken
    // by the blast like any other neighbour.
    public void ExplodeAt(Vector2 center)
    {
        if (levelEnded) return;

        CreateExplosionVfx(center);
        ApplyBlast(center, 0.62f, null);
    }

    // Shared radial blast used by both bombs and the missile booster: breaks unbroken targets in
    // range and shoves every rigidbody outward with distance falloff.
    private void ApplyBlast(Vector2 center, float blastRadius, WarfestTarget source)
    {
        int hitCount = Physics2D.OverlapCircle(center, blastRadius, ContactFilter2D.noFilter, blastOverlapResults);
        pushedBodies.Clear();
        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = blastOverlapResults[i];
            if (hit == null) continue;
            WarfestTarget target = hit.GetComponent<WarfestTarget>();
            if (target != null && target != source && !target.IsBroken)
            {
                // A bomb caught in the blast chains into its own explosion; everything else just
                // breaks. Each target flips its broken flag before recursing, so an already-spent
                // neighbour is skipped and the chain terminates instead of looping forever.
                if (target.IsBomb) target.DetonateFromChain();
                else target.BreakFromExplosion();
            }

            Rigidbody2D body = hit.attachedRigidbody;
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
        yield return new WaitForSeconds(0.45f);
        if (musicSource != null) musicSource.Stop();
        WarfestAudio.StopGameplayAudio();
        WarfestVictoryScreen.Show(level.number - 1);
    }

#if UNITY_EDITOR
    [ContextMenu("Test Victory Screen")]
    private void TestVictoryScreen()
    {
        if (musicSource != null) musicSource.Stop();
        WarfestAudio.StopGameplayAudio();
        WarfestVictoryScreen.Show(level.number > 0 ? level.number - 1 : 0);
    }
#endif

    private void EnsureExplosionPool()
    {
        if (explosionPool.Count > 0) return;
        Transform parent = worldRoot != null ? worldRoot : transform;
        for (int i = 0; i < ExplosionPoolCapacity; i++)
        {
            GameObject root = new GameObject($"Explosion VFX {i}");
            root.transform.SetParent(parent, false);

            Sprite flash = GetExplosionSprite();
            GameObject flashObj = CreateSprite("Bomb Flash", flash, Vector3.zero, Vector2.one * 0.16f, 30);
            flashObj.transform.SetParent(root.transform, false);
            SpriteRenderer flashRen = flashObj.GetComponent<SpriteRenderer>();
            flashRen.color = new Color(1f, 0.96f, 0.34f, 0f);

            GameObject sparksObj = new GameObject("Bomb Sparks", typeof(ParticleSystem));
            sparksObj.transform.SetParent(root.transform, false);
            ParticleSystem sparks = sparksObj.GetComponent<ParticleSystem>();
            sparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.MainModule main = sparks.main;
            main.playOnAwake = false;
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

            ParticleSystemRenderer particleRenderer = sparksObj.GetComponent<ParticleSystemRenderer>();
            Material particleMaterial = GetExplosionParticleMaterial();
            if (particleMaterial != null) particleRenderer.sharedMaterial = particleMaterial;
            particleRenderer.sortingOrder = 31;

            root.SetActive(false);

            explosionPool.Add(new ExplosionVfxItem
            {
                rootObject = root,
                flashTransform = flashObj.transform,
                flashRenderer = flashRen,
                sparks = sparks
            });
        }
    }

    private void CreateExplosionVfx(Vector2 center)
    {
        EnsureExplosionPool();
        if (explosionPool.Count == 0) return;

        ExplosionVfxItem item = explosionPool[nextExplosionPoolIndex];
        nextExplosionPoolIndex = (nextExplosionPoolIndex + 1) % explosionPool.Count;

        if (item.flashRoutine != null)
        {
            StopCoroutine(item.flashRoutine);
            item.flashRoutine = null;
        }

        item.rootObject.SetActive(true);
        item.flashTransform.position = new Vector3(center.x, center.y, -0.35f);
        item.flashTransform.localScale = Vector3.one * 0.16f;
        item.flashRenderer.color = new Color(1f, 0.96f, 0.34f, 0.95f);

        item.sparks.transform.position = new Vector3(center.x, center.y, -0.40f);
        item.sparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        item.sparks.Play();

        item.flashRoutine = StartCoroutine(AnimateExplosionFlash(item));
    }

    private IEnumerator AnimateExplosionFlash(ExplosionVfxItem item)
    {
        const float duration = 0.38f;
        float time = 0f;
        while (time < duration && item.flashTransform != null && item.flashRenderer != null)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            float scale = Mathf.Lerp(0.16f, 2.65f, 1f - (1f - t) * (1f - t));
            item.flashTransform.localScale = Vector3.one * scale;
            Color color = item.flashRenderer.color;
            color.a = 1f - t;
            item.flashRenderer.color = color;
            yield return null;
        }
        if (item.flashRenderer != null)
        {
            Color c = item.flashRenderer.color;
            c.a = 0f;
            item.flashRenderer.color = c;
        }
        item.flashRoutine = null;
    }

    private Material GetExplosionParticleMaterial()
    {
        if (s_explosionParticleMaterial != null) return s_explosionParticleMaterial;
        Shader particleShader = Shader.Find("Sprites/Default");
        if (particleShader == null) particleShader = Shader.Find("Universal Render Pipeline/Unlit");
        if (particleShader == null) return null;
        s_explosionParticleMaterial = new Material(particleShader) { name = "Warfest Explosion Sparks" };
        explosionParticleMaterial = s_explosionParticleMaterial;
        return s_explosionParticleMaterial;
    }

    private Sprite GetExplosionSprite()
    {
        if (s_explosionSprite != null) return s_explosionSprite;
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
        s_explosionSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        s_explosionSprite.name = "Bomb Flash";
        explosionSprite = s_explosionSprite;
        return s_explosionSprite;
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
        GameObject existingCanvas = GameObject.Find("Game HUD Canvas");
        if (existingCanvas != null)
        {
            if (Application.isPlaying) Destroy(existingCanvas);
            else DestroyImmediate(existingCanvas);
        }

        hudCanvas = CreateCanvas("Game HUD Canvas");
        RectTransform root = hudCanvas.transform as RectTransform;
        safeAreaRoot = CreateSafeAreaRoot(root);

        Color blue = new Color(0.10f, 0.43f, 0.76f, 1f);
        Color cream = new Color(1f, 0.98f, 0.91f, 0.97f);
        Color navy = new Color(0.075f, 0.19f, 0.47f, 1f);

        CreateSpriteImage(safeAreaRoot, "Balls Card", ballsPanelSprite, cream,
            new Vector2(0.145f, 0.938f), new Vector2(0.240f, 0.096f), false);
        CreateSpriteImage(safeAreaRoot, "Balls Header", blueLabelSprite, blue,
            new Vector2(0.145f, 0.968f), new Vector2(0.190f, 0.046f), false);
        Text ballsHeader = CreateText(safeAreaRoot, "Balls Header Label", "BALLS", 17, Color.white,
            TextAnchor.MiddleCenter, new Vector2(0.145f, 0.968f), new Vector2(0.18f, 0.035f), headingFont);
        AddTextOutline(ballsHeader, navy, new Vector2(1.4f, -1.4f));
        ballsText = CreateText(safeAreaRoot, "Balls Count", remainingBalls.ToString("00"), 39, navy,
            TextAnchor.MiddleCenter, new Vector2(0.145f, 0.918f), new Vector2(0.180f, 0.054f), headingFont);
        AddTextOutline(ballsText, Color.white, new Vector2(1.2f, -1.2f));

        Button menu = CreateSpriteButton(safeAreaRoot, "Settings Menu", settingsPanelSprite,
            new Vector2(0.865f, 0.937f), new Vector2(0.18f, 0.092f));
        menu.onClick.AddListener(ToggleSettingsMenu);

        BuildBoosterHud();
        BuildSettingsFlyout();
    }

    private void BuildSettingsFlyout()
    {
        RectTransform flyout = CreateRect(safeAreaRoot, "Settings Flyout", new Vector2(0.5f, 0.5f), Vector2.one);
        settingsFlyout = flyout.gameObject;

        Vector2 buttonSize = new Vector2(0.145f, 0.070f);
        const float flyoutX = 0.865f;

        Button leave = CreateSettingsIconButton(flyout, "Leave Level", settingsDisabledSprite, leaveIconSprite,
            new Vector2(flyoutX, 0.852f), buttonSize, out _);
        leave.onClick.AddListener(ShowLeaveConfirmation);

        Button sound = CreateSettingsIconButton(flyout, "Sound Toggle", settingsEnabledSprite, soundIconSprite,
            new Vector2(flyoutX, 0.772f), buttonSize, out soundButtonBackground);
        sound.onClick.AddListener(ToggleSound);

        Button music = CreateSettingsIconButton(flyout, "Music Toggle", settingsEnabledSprite, musicIconSprite,
            new Vector2(flyoutX, 0.692f), buttonSize, out musicButtonBackground);
        music.onClick.AddListener(ToggleMusic);

        // Backdrop click / tap outside closes the menu without leaving.
        Button backdrop = CreateSpriteButton(flyout, "Settings Backdrop", null,
            new Vector2(0.5f, 0.5f), Vector2.one);
        backdrop.transform.SetAsFirstSibling();
        Image backdropImage = backdrop.GetComponent<Image>();
        backdropImage.color = Color.clear;
        backdrop.onClick.AddListener(ToggleSettingsMenu);

        RectTransform confirmation = CreateRect(safeAreaRoot, "Leave Confirmation", new Vector2(0.5f, 0.5f), Vector2.one);
        leaveConfirmation = confirmation.gameObject;
        CreateImage(confirmation, "Dim Background", new Color(0.02f, 0.04f, 0.08f, 0.76f),
            new Vector2(0.5f, 0.5f), Vector2.one);
        CreateImage(confirmation, "Confirmation Card", new Color(1f, 0.99f, 0.94f, 0.99f),
            new Vector2(0.5f, 0.5f), new Vector2(0.52f, 0.50f));
        CreateText(confirmation, "Confirmation Title", "LEAVE LEVEL?", 32, Ink, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.62f), new Vector2(0.43f, 0.10f), headingFont);
        CreateText(confirmation, "Confirmation Copy", "ARE YOU SURE?\nYOU WILL LOSE 1 LIFE", 18,
            new Color(0.32f, 0.37f, 0.43f, 1f), TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.51f), new Vector2(0.42f, 0.13f), bodyFont);
        Button confirm = CreateButton(confirmation, "Confirm Leave", "LEAVE", new Color(0.78f, 0.12f, 0.12f, 1f),
            new Vector2(0.41f, 0.37f), new Vector2(0.18f, 0.11f), 16);
        confirm.onClick.AddListener(ConfirmLeaveLevel);
        Button cancel = CreateButton(confirmation, "Cancel Leave", "CANCEL", new Color(0.18f, 0.52f, 0.22f, 1f),
            new Vector2(0.59f, 0.37f), new Vector2(0.18f, 0.11f), 16);
        cancel.onClick.AddListener(CancelLeaveConfirmation);

        settingsFlyout.SetActive(false);
        leaveConfirmation.SetActive(false);
        RefreshSettingsButtons();
    }

    private Button CreateSettingsIconButton(Transform parent, string name, Sprite backgroundSprite, Sprite iconSprite,
        Vector2 center, Vector2 size, out Image background)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        gameObject.transform.SetParent(parent, false);
        ApplyRect(gameObject.GetComponent<RectTransform>(), parent, center, size);
        background = gameObject.GetComponent<Image>();
        background.sprite = backgroundSprite;
        background.color = Color.white;
        background.preserveAspect = true;

        Button button = gameObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.92f);
        colors.pressedColor = new Color(0.82f, 0.86f, 0.9f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        Image icon = CreateSpriteImage(gameObject.transform, "Icon", iconSprite, Color.white,
            new Vector2(0.5f, 0.5f), new Vector2(0.56f, 0.56f), true);
        icon.raycastTarget = false;
        return button;
    }

    private void ToggleSettingsMenu()
    {
        if (levelEnded || settingsFlyout == null) return;
        settingsOpen = !settingsOpen;
        settingsFlyout.SetActive(settingsOpen);
        if (leaveConfirmation != null) leaveConfirmation.SetActive(false);
        CancelAim();
    }

    private void ShowLeaveConfirmation()
    {
        if (levelEnded || leaveConfirmation == null) return;
        settingsOpen = true;
        settingsFlyout.SetActive(false);
        leaveConfirmation.SetActive(true);
        CancelAim();
    }

    private void CancelLeaveConfirmation()
    {
        if (levelEnded) return;
        leaveConfirmation.SetActive(false);
        settingsFlyout.SetActive(true);
        settingsOpen = true;
    }

    private void ConfirmLeaveLevel()
    {
        if (levelEnded) return;
        levelEnded = true;
        settingsOpen = false;
        if (settingsFlyout != null) settingsFlyout.SetActive(false);
        if (leaveConfirmation != null) leaveConfirmation.SetActive(false);
        if (musicSource != null) musicSource.Stop();
        WarfestAudio.StopGameplayAudio();
        WarfestSession.ConsumeLife();
        CreateFailurePanel(false, "MISSION ABANDONED");
        StartCoroutine(ReturnToMenuAfterFailure());
    }

    private IEnumerator ReturnToMenuAfterFailure()
    {
        if (musicSource != null) musicSource.Stop();
        WarfestAudio.StopGameplayAudio();
        yield return new WaitForSecondsRealtime(1.25f);
        WarfestSession.ReturnToMenu();
    }

    private void ToggleSound()
    {
        WarfestAudio.SoundEnabled = !WarfestAudio.SoundEnabled;
        soundEnabled = WarfestAudio.SoundEnabled;
        RefreshSettingsButtons();
    }

    private void ToggleMusic()
    {
        WarfestAudio.MusicEnabled = !WarfestAudio.MusicEnabled;
        musicEnabled = WarfestAudio.MusicEnabled;
        RefreshSettingsButtons();
    }

    private void RefreshSettingsButtons()
    {
        soundEnabled = WarfestAudio.SoundEnabled;
        musicEnabled = WarfestAudio.MusicEnabled;
        if (soundButtonBackground != null)
        {
            soundButtonBackground.sprite = soundEnabled ? settingsEnabledSprite : settingsDisabledSprite;
        }
        if (musicButtonBackground != null)
        {
            musicButtonBackground.sprite = musicEnabled ? settingsEnabledSprite : settingsDisabledSprite;
        }
    }

    private void ApplyAudioPreferences()
    {
        WarfestAudio.ApplyAudioPreferences();
    }

    private void BuildAudio()
    {
        GameObject audioRoot = new GameObject("Gameplay Audio");
        audioRoot.transform.SetParent(transform, false);

        GameObject sfxObject = new GameObject("Gameplay SFX");
        sfxObject.transform.SetParent(audioRoot.transform, false);
        sfxSource = sfxObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;
        sfxSource.volume = 0.72f;

        GameObject musicObject = new GameObject("Gameplay Music");
        musicObject.transform.SetParent(audioRoot.transform, false);
        musicSource = musicObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.volume = 0.35f;

        if (s_shootClip == null) s_shootClip = CreateShootClip();
        shootClip = s_shootClip;

        if (s_blockPopClip == null)
        {
            s_blockPopClip = WarfestAudio.GetMatchClip();
            if (s_blockPopClip == null) s_blockPopClip = CreateBlockPopClip();
        }
        blockPopClip = s_blockPopClip;

        if (s_musicClip == null)
        {
            s_musicClip = WarfestAudio.GetLevelClip();
            if (s_musicClip == null) s_musicClip = CreateMusicClip();
        }
        musicClip = s_musicClip;
        musicSource.clip = musicClip;
        musicSource.mute = !WarfestAudio.MusicEnabled;
        musicSource.Play();
    }

    private void OnDestroy()
    {
        if (musicSource != null) musicSource.Stop();
        WarfestAudio.StopGameplayAudio();
    }

    private void PlayShootSound()
    {
        if (soundEnabled && sfxSource != null && shootClip != null)
        {
            sfxSource.PlayOneShot(shootClip, 0.72f);
        }
    }

    private void PlayBlockPopSound()
    {
        if (!soundEnabled || sfxSource == null || blockPopClip == null) return;

        // A chain reaction may remove many blocks in one frame. Keep that readable without
        // stacking dozens of identical samples on top of each other.
        if (Time.unscaledTime - lastBlockPopTime < 0.045f) return;
        lastBlockPopTime = Time.unscaledTime;
        sfxSource.PlayOneShot(blockPopClip, 0.58f);
    }

    private static AudioClip CreateShootClip()
    {
        const float duration = 0.16f;
        int sampleCount = Mathf.CeilToInt(AudioSampleRate * duration);
        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float time = i / (float)AudioSampleRate;
            float envelope = Mathf.Pow(1f - time / duration, 2.4f);
            float sweep = Mathf.Lerp(740f, 210f, time / duration);
            samples[i] = Mathf.Sin(time * sweep * Mathf.PI * 2f) * envelope * 0.52f;
        }
        AudioClip clip = AudioClip.Create("Sample Shoot", sampleCount, 1, AudioSampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static AudioClip CreateBlockPopClip()
    {
        const float duration = 0.13f;
        int sampleCount = Mathf.CeilToInt(AudioSampleRate * duration);
        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float time = i / (float)AudioSampleRate;
            float envelope = Mathf.Pow(1f - time / duration, 3.2f);
            float tone = Mathf.Sin(time * 980f * Mathf.PI * 2f) * 0.54f;
            float thump = Mathf.Sin(time * 190f * Mathf.PI * 2f) * 0.26f;
            samples[i] = (tone + thump) * envelope;
        }
        AudioClip clip = AudioClip.Create("Sample Block Pop", sampleCount, 1, AudioSampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static AudioClip CreateMusicClip()
    {
        const float duration = 8f;
        int sampleCount = Mathf.CeilToInt(AudioSampleRate * duration);
        float[] samples = new float[sampleCount];
        float[] notes = { 110f, 130.81f, 146.83f, 164.81f, 146.83f, 130.81f, 123.47f, 146.83f };
        for (int i = 0; i < sampleCount; i++)
        {
            float time = i / (float)AudioSampleRate;
            int step = Mathf.FloorToInt(time * 2f) % notes.Length;
            float stepTime = time % 0.5f;
            float noteEnvelope = Mathf.Exp(-5.2f * stepTime);
            float note = notes[step];
            float bass = Mathf.Sin(time * note * Mathf.PI * 2f) * noteEnvelope * 0.18f;
            float lead = Mathf.Sin(time * note * 2f * Mathf.PI * 2f) * noteEnvelope * 0.045f;
            float pad = (Mathf.Sin(time * 55f * Mathf.PI * 2f) + Mathf.Sin(time * 82.41f * Mathf.PI * 2f)) * 0.035f;
            samples[i] = bass + lead + pad;
        }
        AudioClip clip = AudioClip.Create("Sample Gameplay Music", sampleCount, 1, AudioSampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    // Lays the four boosters out in the bottom corners, mirroring the reference art: infinite balls
    // above the spread fan on the left, the skull shot above the missile on the right.
private void BuildBoosterHud()
    {
        Vector2 buttonSize = new Vector2(0.235f, 0.115f);
        CreateBoosterButton(WarfestBooster.InfiniteBalls, new Vector2(0.135f, 0.205f), buttonSize);
        CreateBoosterButton(WarfestBooster.SpreadShot, new Vector2(0.135f, 0.078f), buttonSize);
        CreateBoosterButton(WarfestBooster.SkullShot, new Vector2(0.865f, 0.205f), buttonSize);
        CreateBoosterButton(WarfestBooster.Missile, new Vector2(0.865f, 0.078f), buttonSize);

        Image panel = CreateImage(safeAreaRoot, "Booster Status", new Color(0.055f, 0.12f, 0.20f, 0.94f),
            new Vector2(0.5f, 0.835f), new Vector2(0.74f, 0.075f));
        boosterStatusPanel = panel.gameObject;
        boosterStatusText = CreateText(panel.transform, "Booster Status Text", string.Empty, 14, Color.white,
            TextAnchor.MiddleCenter, new Vector2(0.5f, 0.62f), new Vector2(0.96f, 0.48f), headingFont);
        boosterStatusText.horizontalOverflow = HorizontalWrapMode.Overflow;
        boosterStatusText.verticalOverflow = VerticalWrapMode.Overflow;
        boosterStatusText.raycastTarget = false;
        AddTextOutline(boosterStatusText, new Color(0f, 0f, 0f, 0.65f), new Vector2(1f, -1f));

        Image progressTrack = CreateImage(panel.transform, "Booster Progress Track",
            new Color(1f, 1f, 1f, 0.22f), new Vector2(0.5f, 0.16f), new Vector2(0.92f, 0.13f));
        progressTrack.raycastTarget = false;
        boosterProgressTrack = progressTrack.gameObject;

        boosterProgressFill = CreateImage(panel.transform, "Booster Progress Fill",
            new Color(0.25f, 0.88f, 0.35f, 1f), new Vector2(0.5f, 0.16f), new Vector2(0.92f, 0.13f));
        boosterProgressFill.raycastTarget = false;
        boosterProgressFill.type = Image.Type.Filled;
        boosterProgressFill.fillMethod = Image.FillMethod.Horizontal;
        boosterProgressFill.fillOrigin = 0;
        boosterProgressFill.fillAmount = 1f;

        boosterStatusPanel.SetActive(false);
        RefreshBoosterHud();
    }

private void RefreshBoosterStatus()
    {
        if (boosterStatusPanel == null) return;

        bool showingInfinite = infiniteBallsActive;
        bool showingShotBooster = hasArmedBooster;
        if (!showingInfinite && !showingShotBooster)
        {
            boosterStatusPanel.SetActive(false);
            return;
        }

        if (!boosterStatusPanel.activeSelf) boosterStatusPanel.SetActive(true);
        WarfestBooster booster = showingInfinite ? WarfestBooster.InfiniteBalls : armedBooster;
        switch (booster)
        {
            case WarfestBooster.InfiniteBalls:
                boosterStatusText.text = "INFINITE BALLS - SHOOT TO START 3s";
                break;
            case WarfestBooster.SkullShot:
                boosterStatusText.text = "SKULL SHOT - PIERCES ALL BLOCKS";
                break;
            case WarfestBooster.SpreadShot:
                boosterStatusText.text = "SPREAD SHOT - FIRES 3 BALLS";
                break;
            default:
                boosterStatusText.text = "MISSILE - EXPLODES ON IMPACT";
                break;
        }

        bool showProgress = booster == WarfestBooster.InfiniteBalls;
        if (boosterProgressTrack != null) boosterProgressTrack.SetActive(showProgress);
        if (boosterProgressFill != null)
        {
            boosterProgressFill.gameObject.SetActive(showProgress);
            boosterProgressFill.fillAmount = infiniteBallsWaitingForFirstShot
                ? 1f
                : Mathf.Clamp01(infiniteBallsTimeRemaining / InfiniteBallsDurationSeconds);
        }
    }


private void CreateBoosterButton(WarfestBooster booster, Vector2 center, Vector2 size)
    {
        int index = (int)booster;
        Button button = CreateSpriteButton(safeAreaRoot, "Booster " + booster, GetBoosterSprite(booster), center, size);
        button.transition = Selectable.Transition.None;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        boosterButtons[index] = button;
        boosterButtonImages[index] = button.GetComponent<Image>();

        // The count badge is the only visual that changes when a booster is selected. Reuse the
        // green settings sprite so the count reads as a consistent button badge in the game HUD.
        Sprite boosterCountBackground = settingsEnabledSprite != null ? settingsEnabledSprite : GetCannonBaseSprite();
        Image badge = CreateSpriteImage(button.transform, "Booster Badge", boosterCountBackground,
            new Color(0.30f, 0.72f, 0.19f, 1f), new Vector2(0.82f, 0.12f), new Vector2(0.44f, 0.42f), true);
        badge.color = new Color(0.30f, 0.72f, 0.19f, 1f);
        badge.raycastTarget = false;
        Text badgeText = CreateText(badge.transform, "Booster Badge Label", "0", 19, Color.white,
            TextAnchor.MiddleCenter, new Vector2(0.5f, 0.52f), Vector2.one, headingFont);
        badgeText.raycastTarget = false;
        AddTextOutline(badgeText, new Color(0.10f, 0.30f, 0.08f, 1f), new Vector2(1f, -1f));
        boosterCountLabels[index] = badgeText;

        button.onClick.AddListener(() => OnBoosterClicked(booster));
    }

    private Sprite GetBoosterSprite(WarfestBooster booster)
    {
        if (boosterSprites == null || boosterSprites.Length == 0) return null;
        string wanted = "boosters_" + (int)booster;
        for (int i = 0; i < boosterSprites.Length; i++)
        {
            if (boosterSprites[i] != null && boosterSprites[i].name == wanted) return boosterSprites[i];
        }
        int index = (int)booster;
        return index >= 0 && index < boosterSprites.Length ? boosterSprites[index] : null;
    }

private void RefreshHud()
    {
        if (ballsText != null) ballsText.text = infiniteBallsActive ? "\u221E" : remainingBalls.ToString("00");
    }

    // Repaints every booster button: dims the ones the player is out of, shows the owned count (or
    // a "+" prompt when empty) on the badge, and lights the glow on whichever booster is active.
private void RefreshBoosterHud()
    {
        bool selectionLocked = hasArmedBooster || infiniteBallsActive;
        for (int i = 0; i < WarfestSession.BoosterCount; i++)
        {
            WarfestBooster booster = (WarfestBooster)i;
            int count = WarfestSession.GetBoosterCount(booster);
            bool owned = count > 0;

            if (boosterButtons[i] != null)
            {
                boosterButtons[i].interactable = owned && !selectionLocked;
            }
            if (boosterButtonImages[i] != null)
            {
                // Keep the original sprite and color in every state; only the count and interactivity change.
                boosterButtonImages[i].color = Color.white;
            }
            if (boosterCountLabels[i] != null)
            {
                boosterCountLabels[i].text = count.ToString();
            }
            if (boosterArmGlows[i] != null)
            {
                boosterArmGlows[i].SetActive(false);
            }
        }

        RefreshBoosterStatus();
    }

    // Handles a tap on a booster: infinite balls applies immediately, the shot boosters arm the
    // next shot (tapping the armed one again cancels it). Empty boosters are inert.
private void OnBoosterClicked(WarfestBooster booster)
    {
        if (levelEnded || settingsOpen) return;
        if (hasArmedBooster || infiniteBallsActive) return;
        if (!WarfestSession.ConsumeBooster(booster)) return;

        if (booster == WarfestBooster.InfiniteBalls)
        {
            infiniteBallsActive = true;
            infiniteBallsWaitingForFirstShot = true;
            infiniteBallsTimeRemaining = InfiniteBallsDurationSeconds;
            RefreshHud();
        }
        else
        {
            hasArmedBooster = true;
            armedBooster = booster;
        }

        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        RefreshBoosterHud();
    }

private void UpdateInfiniteBallsTimer()
    {
        if (!infiniteBallsActive) return;

        // The status and full progress bar remain visible until the player fires the first ball.
        if (infiniteBallsWaitingForFirstShot)
        {
            return;
        }

        infiniteBallsTimeRemaining -= Time.deltaTime;
        if (Time.unscaledTime >= nextBoosterUiUpdateTime)
        {
            nextBoosterUiUpdateTime = Time.unscaledTime + BoosterUiUpdateInterval;
            if (boosterProgressFill != null)
            {
                boosterProgressFill.fillAmount = Mathf.Clamp01(infiniteBallsTimeRemaining / InfiniteBallsDurationSeconds);
            }
        }
        if (infiniteBallsTimeRemaining > 0f) return;

        infiniteBallsTimeRemaining = 0f;
        infiniteBallsActive = false;
        infiniteBallsWaitingForFirstShot = false;
        RefreshHud();
        RefreshBoosterHud();

        if (remainingBalls <= 0 && targetsRemaining > 0 && !levelEnded)
        {
            StartCoroutine(ShowFailureAfterDelay());
        }
    }

private void ShowFailure()
    {
        if (levelEnded) return;
        levelEnded = true;
        settingsOpen = false;
        if (settingsFlyout != null) settingsFlyout.SetActive(false);
        if (leaveConfirmation != null) leaveConfirmation.SetActive(false);
        WarfestSession.ConsumeLife();
        CreateFailurePanel(true, "OUT OF BALLS");
    }

    private void CreateFailurePanel(bool showActions, string reason)
    {
        Transform parent = safeAreaRoot != null ? safeAreaRoot : (hudCanvas != null ? hudCanvas.transform : null);
        if (parent == null) return;

        RectTransform overlay = CreateRect(parent, "Failure Overlay", new Vector2(0.5f, 0.5f), Vector2.one);
        CreateImage(overlay, "Failure Dim", new Color(0.02f, 0.04f, 0.08f, 0.76f),
            new Vector2(0.5f, 0.5f), Vector2.one);
        CreateImage(overlay, "Failure Panel", new Color(1f, 1f, 1f, 0.98f),
            new Vector2(0.5f, 0.5f), new Vector2(0.50f, showActions ? 0.62f : 0.45f));
        CreateText(overlay, "Failure Title", "LEVEL FAILED", 34, Ink, TextAnchor.MiddleCenter,
            new Vector2(0.5f, showActions ? 0.68f : 0.58f), new Vector2(0.42f, 0.10f), headingFont);
        CreateText(overlay, "Failure Reason", reason, 18, new Color(0.66f, 0.12f, 0.12f, 1f),
            TextAnchor.MiddleCenter, new Vector2(0.5f, showActions ? 0.57f : 0.47f), new Vector2(0.40f, 0.07f), bodyFont);
        failureLifeStatus = CreateText(overlay, "Failure Lives", string.Empty, 16,
            new Color(0.28f, 0.36f, 0.43f, 1f), TextAnchor.MiddleCenter,
            new Vector2(0.5f, showActions ? 0.49f : 0.39f), new Vector2(0.42f, 0.07f), bodyFont);

        if (showActions)
        {
            retryButton = CreateButton(overlay, "Retry", "RETRY LEVEL", Ink,
                new Vector2(0.5f, 0.38f), new Vector2(0.34f, 0.11f), 15);
            retryButton.onClick.AddListener(() => WarfestSession.LoadLevel(level.number - 1));
            Button menu = CreateButton(overlay, "Back to Menu", "MAIN MENU", new Color(0.35f, 0.48f, 0.58f),
                new Vector2(0.5f, 0.24f), new Vector2(0.34f, 0.11f), 15);
            menu.onClick.AddListener(WarfestSession.ReturnToMenu);
        }

        displayedFailureLives = -1;
        displayedFailureSeconds = -1;
        RefreshFailureLifeStatus();
    }

    private void RefreshFailureLifeStatus()
    {
        if (failureLifeStatus == null) return;

        int lives = WarfestSession.Lives;
        int seconds = WarfestSession.SecondsUntilNextLife;
        if (lives == displayedFailureLives && seconds == displayedFailureSeconds) return;

        displayedFailureLives = lives;
        displayedFailureSeconds = seconds;
        failureLifeStatus.text = lives >= WarfestSession.MaxLives
            ? "LIVES " + lives + " / " + WarfestSession.MaxLives
            : "LIVES " + lives + " / " + WarfestSession.MaxLives + "   +1 IN " + WarfestSession.LifeTimerText;
        if (retryButton != null) retryButton.interactable = lives > 0;
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

        for (int i = 0; i < blockBodies.Count; i++)
        {
            Rigidbody2D body = blockBodies[i];
            if (body == null) continue;
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 1f;
            body.WakeUp();
        }
        checkFallenTargets = true;
        nextFallenTargetCheckTime = Time.time;
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

#if UNITY_EDITOR
    [ContextMenu("Build Edit Mode Preview")]
    public void BuildEditModePreview() => BuildEditModePreview(0);

    public void BuildEditModePreview(int levelIndex)
    {
        font = bodyFont != null ? bodyFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (headingFont == null) headingFont = font;
        if (bodyFont == null) bodyFont = font;
        LoadOriginalSprites();
        EnsureEventSystem();
        EnsureCamera();
        int safeLvl = Mathf.Clamp(levelIndex, 0, WarfestLevelCatalog.AuthoredLevelCount - 1);
        level = WarfestLevelCatalog.Get(safeLvl);
        ballCapacity = WarfestSession.GetBallAllowance(safeLvl);
        remainingBalls = ballCapacity;
        BuildWorld();
        BuildHud();
        RefreshHud();
    }

    [ContextMenu("Clear Edit Mode Preview")]
    public void ClearEditModePreview()
    {
        ClearExistingWorld();
        GameObject existingCanvas = GameObject.Find("Game HUD Canvas");
        if (existingCanvas != null) DestroyImmediate(existingCanvas);
    }
#endif
}

public sealed class WarfestTarget : MonoBehaviour
{
    private WarfestGameController controller;
    private Collider2D cachedCollider;
    private Rigidbody2D cachedBody;
    private bool broken;
    private bool bomb;

    public bool IsBomb => bomb;
    public bool IsBroken => broken;
    public Collider2D Collider => cachedCollider;
    public Rigidbody2D Body => cachedBody;

    public void Initialize(WarfestGameController owner, bool isBomb = false)
    {
        controller = owner;
        bomb = isBomb;
        cachedCollider = GetComponent<Collider2D>();
        cachedBody = GetComponent<Rigidbody2D>();
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

    // A bomb shoved by a neighbouring blast triggers its own explosion, producing the
    // chain-reaction the bomb-heavy levels are designed around.
    public void DetonateFromChain()
    {
        if (broken) return;
        broken = true;
        controller.DetonateBomb(this);
    }

}
