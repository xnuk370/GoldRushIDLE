using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Visual-only theme for Gold Rush IDLE. No scene bindings or gameplay logic are changed.
/// The theme is created when MainMenu or Gameplay loads, including in WebGL builds.
/// </summary>
public static class GoldRushVisualTheme
{
    private static Sprite roundedSprite;
    private static Sprite backgroundSprite;
    private static Sprite glowSprite;
    private static Sprite oreSprite;

    private static readonly Color Gold = new Color32(244, 183, 69, 255);
    private static readonly Color Dark = new Color32(27, 35, 45, 255);
    private static readonly Color Cream = new Color32(250, 237, 207, 255);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        Apply(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Apply(scene);
    }

    private static void Apply(Scene scene)
    {
        if (scene.name != "MainMenu" && scene.name != "Gameplay")
            return;

        var canvases = Object.FindObjectsByType<Canvas>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var canvas in canvases)
        {
            if (!canvas.isRootCanvas || canvas.gameObject.scene != scene)
                continue;

            if (Find(canvas, "GoldRushVisualBackground") != null)
                continue;

            AddBackground(canvas);
            if (scene.name == "MainMenu")
                StyleMainMenu(canvas);
            else
                StyleGameplay(canvas);
        }
    }

    private static GameObject Find(Canvas canvas, string name)
    {
        foreach (var item in canvas.GetComponentsInChildren<Transform>(true))
            if (item.name == name)
                return item.gameObject;

        return null;
    }

    private static void AddBackground(Canvas canvas)
    {
        var background = CreateImage(canvas.transform, "GoldRushVisualBackground");
        Stretch(background.rectTransform);
        background.sprite = BackgroundSprite();
        background.color = Color.white;
        background.transform.SetAsFirstSibling();

        var glow = CreateImage(canvas.transform, "GoldRushAmberGlow");
        var rect = glow.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.52f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(800, 800);
        glow.sprite = GlowSprite();
        glow.color = new Color(1f, 0.64f, 0.19f, 0.32f);
        glow.transform.SetSiblingIndex(1);
    }

    private static Image CreateImage(Transform parent, string name)
    {
        var item = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        item.transform.SetParent(parent, false);
        var image = item.GetComponent<Image>();
        image.raycastTarget = false;
        return image;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void StyleMainMenu(Canvas canvas)
    {
        StyleButton(Find(canvas, "PlayButton"), true);

        AddText(canvas, "GoldRushMenuTitle", "GOLD RUSH", 0.77f, 74, Gold);
        AddText(canvas, "GoldRushMenuSubtitle", "IDLE MINING ADVENTURE", 0.68f, 25, Cream);
        AddText(canvas, "GoldRushMenuFooter", "DIG  •  UPGRADE  •  GROW", 0.12f, 18,
            new Color32(175, 161, 143, 255));
    }

    private static void StyleGameplay(Canvas canvas)
    {
        StylePanel(Find(canvas, "ShopPanel"));
        StylePanel(Find(canvas, "AchievementsPanel"));
        StylePanel(Find(canvas, "DailyMissionsPanel"));

        string[] mainButtons =
        {
            "UpgradeButton", "AutoMineButton", "DailyRewardButton",
            "MissionClicksButton", "MissionGoldButton", "MissionUpgradeButton"
        };
        foreach (string name in mainButtons)
            StyleButton(Find(canvas, name), true);

        string[] secondaryButtons =
        {
            "OpenShopButton", "OpenAchievementsButton",
            "CloseShopButton", "CloseAchievementsButton"
        };
        foreach (string name in secondaryButtons)
            StyleButton(Find(canvas, name), false);

        SetTextColor(Find(canvas, "GoldText"), Gold);
        SetTextColor(Find(canvas, "TutorialText"), Cream);
        SetTextColor(Find(canvas, "OfflineIncomeText"),
            new Color32(143, 233, 165, 255));

        var ore = Find(canvas, "OreButton");
        if (ore != null)
        {
            var button = ore.GetComponent<Button>();
            var image = ore.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = OreSprite();
                image.type = Image.Type.Simple;
                image.preserveAspect = true;
                image.color = Color.white;

                var shadow = ore.GetComponent<Shadow>();
                if (shadow == null)
                    shadow = ore.AddComponent<Shadow>();
                shadow.effectColor = new Color(0.95f, 0.56f, 0.12f, 0.45f);
                shadow.effectDistance = new Vector2(0, -7);
            }

            if (button != null && ore.GetComponent<GoldRushOreFeedback>() == null)
                ore.AddComponent<GoldRushOreFeedback>();
        }

        AddText(canvas, "GoldRushGameplayFooter", "GOLD RUSH  •  IDLE",
            0.045f, 16, new Color32(174, 150, 113, 255));
    }

    private static void StylePanel(GameObject panel)
    {
        if (panel == null)
            return;

        var image = panel.GetComponent<Image>();
        if (image == null)
        {
            image = panel.AddComponent<Image>();
            image.raycastTarget = false;
        }

        image.sprite = RoundedSprite();
        image.type = Image.Type.Sliced;
        image.color = new Color32(31, 43, 53, 247);

        var outline = panel.GetComponent<Outline>();
        if (outline == null)
            outline = panel.AddComponent<Outline>();
        outline.effectColor = new Color(0.68f, 0.43f, 0.18f, 0.42f);
        outline.effectDistance = new Vector2(2, -2);
    }

    private static void StyleButton(GameObject target, bool primary)
    {
        if (target == null)
            return;

        var button = target.GetComponent<Button>();
        var image = target.GetComponent<Image>();
        if (button == null || image == null)
            return;

        image.sprite = RoundedSprite();
        image.type = Image.Type.Sliced;
        image.color = primary ? Gold : new Color32(54, 70, 79, 255);

        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        var colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.96f, 0.86f, 1f);
        colors.selectedColor = Color.white;
        colors.pressedColor = new Color(0.81f, 0.79f, 0.75f, 1f);
        colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.75f);
        colors.fadeDuration = 0.12f;
        button.colors = colors;

        var shadow = target.GetComponent<Shadow>();
        if (shadow == null)
            shadow = target.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.40f);
        shadow.effectDistance = new Vector2(0, -4);

        foreach (var text in target.GetComponentsInChildren<TMP_Text>(true))
            text.color = primary ? Dark : Cream;
    }

    private static void SetTextColor(GameObject target, Color color)
    {
        if (target != null && target.TryGetComponent<TMP_Text>(out var text))
            text.color = color;
    }

    private static void AddText(Canvas canvas, string name, string content,
        float anchorY, float fontSize, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform),
            typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(canvas.transform, false);

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, anchorY);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(980, fontSize * 1.8f);

        var text = go.GetComponent<TextMeshProUGUI>();
        var existingText = canvas.GetComponentInChildren<TMP_Text>(true);
        if (existingText != null)
            text.font = existingText.font;
        text.text = content;
        text.fontSize = fontSize;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.enableAutoSizing = true;
        text.fontSizeMin = 10;
        text.fontSizeMax = fontSize;
        text.color = color;
        text.raycastTarget = false;
    }

    private static Sprite RoundedSprite()
    {
        if (roundedSprite != null)
            return roundedSprite;

        const int size = 64;
        var texture = NewTexture(size, size);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = Mathf.Max(Mathf.Abs(x - 31.5f) - 19.5f, 0);
            float dy = Mathf.Max(Mathf.Abs(y - 31.5f) - 19.5f, 0);
            float a = Mathf.Clamp01(12.5f - Mathf.Sqrt(dx * dx + dy * dy));
            texture.SetPixel(x, y, new Color(1f, 1f, 1f, a));
        }
        texture.Apply();
        roundedSprite = Sprite.Create(texture, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect,
            new Vector4(15, 15, 15, 15));
        return roundedSprite;
    }

    private static Sprite BackgroundSprite()
    {
        if (backgroundSprite != null)
            return backgroundSprite;

        var texture = NewTexture(2, 256);
        var bottom = new Color32(16, 21, 28, 255);
        var top = new Color32(40, 54, 59, 255);
        for (int y = 0; y < 256; y++)
        {
            Color color = Color.Lerp(bottom, top, y / 255f);
            texture.SetPixel(0, y, color);
            texture.SetPixel(1, y, color);
        }
        texture.Apply();
        backgroundSprite = Sprite.Create(texture, new Rect(0, 0, 2, 256),
            new Vector2(0.5f, 0.5f));
        return backgroundSprite;
    }

    private static Sprite GlowSprite()
    {
        if (glowSprite != null)
            return glowSprite;

        const int size = 128;
        var texture = NewTexture(size, size);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float r = Vector2.Distance(new Vector2(x, y),
                new Vector2(63.5f, 63.5f)) / 64f;
            float alpha = Mathf.Pow(Mathf.Clamp01(1f - r), 2f);
            texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
        }
        texture.Apply();
        glowSprite = Sprite.Create(texture, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f));
        return glowSprite;
    }

    private static Sprite OreSprite()
    {
        if (oreSprite != null)
            return oreSprite;

        const int size = 192;
        var texture = NewTexture(size, size);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float nx = (x - 95.5f) / 95.5f;
            float ny = (y - 95.5f) / 95.5f;
            float r = Mathf.Sqrt(nx * nx + ny * ny);
            float a = Mathf.Atan2(ny, nx);
            float edge = 0.79f + 0.045f * Mathf.Sin(a * 7f)
                + 0.038f * Mathf.Sin(a * 13f + 0.9f)
                + 0.022f * Mathf.Sin(a * 23f - 1.1f);
            float opacity = Mathf.Clamp01((edge - r) * 85f);

            float vein = Mathf.Sin(11f * nx + 7f * ny
                + 2.8f * Mathf.Sin(ny * 9f));
            float facet = Mathf.Sin(nx * 19f + ny * 8f) * 0.06f;
            float light = Mathf.Clamp01(0.58f - nx * 0.26f + ny * 0.27f + facet);

            Color stone = Color.Lerp(
                new Color32(64, 54, 46, 255),
                new Color32(132, 119, 96, 255), light);
            Color gold = Color.Lerp(
                new Color32(145, 82, 21, 255),
                new Color32(255, 216, 91, 255), light);

            float mineral = Mathf.SmoothStep(0f, 1f,
                Mathf.Clamp01((vein - 0.02f) * 2.8f));
            if (r < 0.38f)
                mineral = Mathf.Max(mineral, 0.70f);
            Color color = Color.Lerp(stone, gold, mineral);
            color.a = opacity;
            texture.SetPixel(x, y, color);
        }
        texture.Apply();
        oreSprite = Sprite.Create(texture, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f));
        return oreSprite;
    }

    private static Texture2D NewTexture(int width, int height)
    {
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        return texture;
    }
}

/// <summary>Click animation and decorative sparks; the original MineGold listener stays untouched.</summary>
public sealed class GoldRushOreFeedback : MonoBehaviour
{
    private RectTransform rect;
    private Button button;
    private Vector3 initialScale;
    private float impact;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        button = GetComponent<Button>();
        initialScale = rect.localScale;
        button.onClick.AddListener(PlayImpact);
    }

    private void Update()
    {
        impact = Mathf.MoveTowards(impact, 0f, Time.unscaledDeltaTime * 3f);
        float breathe = 0.014f * Mathf.Sin(Time.unscaledTime * 2.7f);
        rect.localScale = initialScale * (1f + breathe + impact * 0.12f);
    }

    private void PlayImpact()
    {
        impact = 1f;
        for (int i = 0; i < 7; i++)
        {
            var go = new GameObject("GoldSpark", typeof(RectTransform),
                typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(transform, false);
            var sparkRect = go.GetComponent<RectTransform>();
            sparkRect.anchorMin = sparkRect.anchorMax = new Vector2(0.5f, 0.5f);
            sparkRect.sizeDelta = Vector2.one * Random.Range(6f, 14f);
            sparkRect.localRotation = Quaternion.Euler(0f, 0f, 45f);

            var image = go.GetComponent<Image>();
            image.color = i % 2 == 0
                ? new Color32(255, 222, 109, 255)
                : new Color32(255, 166, 55, 255);
            image.raycastTarget = false;

            float angle = Random.Range(0f, Mathf.PI * 2f);
            var velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle))
                * Random.Range(80f, 165f);
            go.AddComponent<GoldRushSpark>().Initialize(velocity);
        }
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(PlayImpact);
        if (rect != null)
            rect.localScale = initialScale;
    }
}

/// <summary>Short-lived UI particle generated on an ore click.</summary>
public sealed class GoldRushSpark : MonoBehaviour
{
    private RectTransform rect;
    private Image image;
    private Vector2 velocity;
    private Color color;
    private float age;

    public void Initialize(Vector2 initialVelocity)
    {
        rect = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        velocity = initialVelocity;
        color = image.color;
    }

    private void Update()
    {
        float dt = Time.unscaledDeltaTime;
        age += dt;
        rect.anchoredPosition += velocity * dt;
        velocity.y -= 125f * dt;
        color.a = Mathf.Clamp01(1f - age / 0.72f);
        image.color = color;
        if (age >= 0.72f)
            Destroy(gameObject);
    }
}
