using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_NpcInteractionMenu : MonoBehaviour
{
    public class MenuOption
    {
        public readonly string Label;
        public readonly Action OnSelected;

        public MenuOption(string label, Action onSelected)
        {
            Label = label;
            OnSelected = onSelected;
        }
    }

    private static UI_NpcInteractionMenu instance;
    private static int lastClosedFrame = -1;

    [Header("文本")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TMP_FontAsset menuFont;
    [SerializeField] private string fontResourcePath = "Fonts & Materials/xiangsuziti SDF";
    [SerializeField] private UI_NpcStyle npcStyle;
    [SerializeField] private string styleResourcePath = "NpcUiStyle";

    [Header("布局")]
    [SerializeField] private Vector2 panelSize = new Vector2(360f, 270f);
    [SerializeField] private float bottomOffset = 165f;
    [SerializeField] private Vector2 optionSize = new Vector2(260f, 42f);
    [SerializeField] private float optionSpacing = 10f;

    [Header("样式")]
    [SerializeField] private Color backgroundColor = new Color(0.1f, 0.025f, 0.025f, 0.94f);
    [SerializeField] private Color borderColor = new Color(0.75f, 0.14f, 0.04f, 0.95f);
    [SerializeField] private Color normalOptionColor = new Color(0.25f, 0.035f, 0.035f, 0.95f);
    [SerializeField] private Color selectedOptionColor = new Color(0.78f, 0.14f, 0.04f, 0.95f);
    [SerializeField] private Color normalTextColor = Color.white;
    [SerializeField] private Color selectedTextColor = new Color(1f, 0.92f, 0.62f, 1f);

    private readonly List<MenuOption> options = new List<MenuOption>();
    private readonly List<Image> optionBackgrounds = new List<Image>();
    private readonly List<TextMeshProUGUI> optionTexts = new List<TextMeshProUGUI>();

    private RectTransform rectTransform;
    private VerticalLayoutGroup optionsLayout;
    private int selectedIndex;
    private float inputReadyAt;

    public static UI_NpcInteractionMenu Instance => instance;
    public static bool IsOpen => instance != null && instance.gameObject.activeSelf;
    public static bool WasClosedThisFrame => lastClosedFrame == Time.frameCount;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        rectTransform = GetComponent<RectTransform>();
        BuildRuntimeStyle();
        Hide();
    }

    private void Update()
    {
        if (!gameObject.activeSelf || Time.unscaledTime < inputReadyAt)
            return;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            MoveSelection(-1);
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveSelection(1);
        }
        else if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return))
        {
            SelectCurrent();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            Hide();
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    public static UI_NpcInteractionMenu GetOrCreate()
    {
        if (instance != null)
            return instance;

        UI_NpcInteractionMenu existingMenu = FindObjectOfType<UI_NpcInteractionMenu>(true);

        if (existingMenu != null)
        {
            instance = existingMenu;
            return instance;
        }

        Transform parent = FindDefaultParent();
        GameObject menuObject = new GameObject("NpcInteractionMenu_UI", typeof(RectTransform), typeof(Image), typeof(Outline));

        if (parent != null)
            menuObject.transform.SetParent(parent, false);

        return menuObject.AddComponent<UI_NpcInteractionMenu>();
    }

    public void Show(string npcName, List<MenuOption> menuOptions)
    {
        if (menuOptions == null || menuOptions.Count == 0)
            return;

        options.Clear();
        options.AddRange(menuOptions);
        selectedIndex = 0;
        inputReadyAt = Time.unscaledTime + 0.15f;

        gameObject.SetActive(true);
        BuildRuntimeStyle();

        if (titleText != null)
            titleText.text = string.IsNullOrWhiteSpace(npcName) ? "交互" : npcName;

        RebuildOptions();
        RefreshSelection();
    }

    public void Hide()
    {
        options.Clear();
        lastClosedFrame = Time.frameCount;

        if (gameObject.activeSelf)
            gameObject.SetActive(false);
    }

    private static Transform FindDefaultParent()
    {
        UI ui = FindObjectOfType<UI>(true);

        if (ui != null && ui.InteractionPromptRoot != null)
            return ui.InteractionPromptRoot;

        Canvas canvas = FindObjectOfType<Canvas>(true);

        if (canvas != null)
            return canvas.transform;

        GameObject canvasObject = new GameObject("NpcInteractionMenu_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas newCanvas = canvasObject.GetComponent<Canvas>();
        newCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        return canvasObject.transform;
    }

    private void BuildRuntimeStyle()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0f);
            rectTransform.anchorMax = new Vector2(0.5f, 0f);
            rectTransform.pivot = new Vector2(0.5f, 0f);
            rectTransform.anchoredPosition = new Vector2(0f, bottomOffset);
            rectTransform.sizeDelta = panelSize;
        }

        Image background = GetComponent<Image>();

        if (background == null)
            background = gameObject.AddComponent<Image>();

        UI_NpcStyle resolvedStyle = ResolveStyle();
        Sprite panelSprite = resolvedStyle != null ? resolvedStyle.PanelSprite : null;
        background.sprite = panelSprite;
        background.type = GetImageType(panelSprite);
        background.color = panelSprite != null ? Color.white : backgroundColor;
        background.raycastTarget = false;

        Outline outline = GetComponent<Outline>();

        if (outline == null)
            outline = gameObject.AddComponent<Outline>();

        outline.effectColor = borderColor;
        outline.effectDistance = new Vector2(2f, -2f);

        titleText = EnsureTitleText();
        optionsLayout = EnsureOptionsRoot();
    }

    private TextMeshProUGUI EnsureTitleText()
    {
        if (titleText != null)
            return titleText;

        Transform existing = transform.Find("Title");

        if (existing != null && existing.TryGetComponent(out TextMeshProUGUI existingText))
            return existingText;

        GameObject titleObject = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObject.transform.SetParent(transform, false);

        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.offsetMin = new Vector2(18f, -58f);
        titleRect.offsetMax = new Vector2(-18f, -16f);

        TextMeshProUGUI text = titleObject.GetComponent<TextMeshProUGUI>();
        text.font = ResolveFont();
        text.fontSize = 24f;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = selectedTextColor;
        text.raycastTarget = false;
        text.enableWordWrapping = false;

        return text;
    }

    private VerticalLayoutGroup EnsureOptionsRoot()
    {
        Transform existing = transform.Find("Options");

        if (existing != null && existing.TryGetComponent(out VerticalLayoutGroup existingLayout))
            return existingLayout;

        GameObject optionsObject = new GameObject("Options", typeof(RectTransform), typeof(VerticalLayoutGroup));
        optionsObject.transform.SetParent(transform, false);

        RectTransform optionsRect = optionsObject.GetComponent<RectTransform>();
        optionsRect.anchorMin = new Vector2(0.5f, 0f);
        optionsRect.anchorMax = new Vector2(0.5f, 1f);
        optionsRect.pivot = new Vector2(0.5f, 0f);
        optionsRect.anchoredPosition = new Vector2(0f, 22f);
        optionsRect.sizeDelta = new Vector2(optionSize.x, panelSize.y - 78f);

        VerticalLayoutGroup layout = optionsObject.GetComponent<VerticalLayoutGroup>();
        layout.spacing = optionSpacing;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        return layout;
    }

    private void RebuildOptions()
    {
        optionBackgrounds.Clear();
        optionTexts.Clear();

        Transform optionsRoot = optionsLayout != null ? optionsLayout.transform : transform;

        for (int i = optionsRoot.childCount - 1; i >= 0; i--)
            Destroy(optionsRoot.GetChild(i).gameObject);

        for (int i = 0; i < options.Count; i++)
            CreateOptionVisual(optionsRoot, options[i].Label);
    }

    private void CreateOptionVisual(Transform parent, string label)
    {
        GameObject optionObject = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        optionObject.transform.SetParent(parent, false);

        LayoutElement layoutElement = optionObject.GetComponent<LayoutElement>();
        layoutElement.preferredWidth = optionSize.x;
        layoutElement.preferredHeight = optionSize.y;
        layoutElement.minWidth = optionSize.x;
        layoutElement.minHeight = optionSize.y;

        Image background = optionObject.GetComponent<Image>();
        UI_NpcStyle resolvedStyle = ResolveStyle();
        Sprite buttonSprite = resolvedStyle != null ? resolvedStyle.ButtonSprite : null;
        background.sprite = buttonSprite;
        background.type = GetImageType(buttonSprite);
        background.color = normalOptionColor;
        background.raycastTarget = false;
        optionBackgrounds.Add(background);

        GameObject textObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(optionObject.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.font = ResolveFont();
        text.text = label;
        text.fontSize = 22f;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = normalTextColor;
        text.raycastTarget = false;
        text.enableWordWrapping = false;
        optionTexts.Add(text);
    }

    private void MoveSelection(int direction)
    {
        if (options.Count <= 0)
            return;

        selectedIndex = (selectedIndex + direction + options.Count) % options.Count;
        RefreshSelection();
    }

    private void RefreshSelection()
    {
        for (int i = 0; i < optionBackgrounds.Count; i++)
        {
            bool selected = i == selectedIndex;

            if (optionBackgrounds[i] != null)
                optionBackgrounds[i].color = selected ? selectedOptionColor : normalOptionColor;

            if (optionTexts[i] != null)
                optionTexts[i].color = selected ? selectedTextColor : normalTextColor;
        }
    }

    private void SelectCurrent()
    {
        if (selectedIndex < 0 || selectedIndex >= options.Count)
            return;

        Action action = options[selectedIndex].OnSelected;
        Hide();
        action?.Invoke();
    }

    private TMP_FontAsset ResolveFont()
    {
        if (menuFont != null)
            return menuFont;

        if (!string.IsNullOrWhiteSpace(fontResourcePath))
            menuFont = Resources.Load<TMP_FontAsset>(fontResourcePath);

        return menuFont;
    }

    private UI_NpcStyle ResolveStyle()
    {
        if (npcStyle != null)
            return npcStyle;

        if (!string.IsNullOrWhiteSpace(styleResourcePath))
            npcStyle = Resources.Load<UI_NpcStyle>(styleResourcePath);

        return npcStyle;
    }

    private static Image.Type GetImageType(Sprite sprite)
    {
        if (sprite != null && sprite.border.sqrMagnitude > 0f)
            return Image.Type.Sliced;

        return Image.Type.Simple;
    }
}
