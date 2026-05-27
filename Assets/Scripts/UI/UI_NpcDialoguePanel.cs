using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_NpcDialoguePanel : MonoBehaviour
{
    private static UI_NpcDialoguePanel instance;
    private static int lastClosedFrame = -1;

    [Header("文本")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TMP_FontAsset dialogueFont;
    [SerializeField] private string fontResourcePath = "Fonts & Materials/xiangsuziti SDF";
    [SerializeField] private UI_NpcStyle npcStyle;
    [SerializeField] private string styleResourcePath = "NpcUiStyle";

    [Header("显示")]
    [SerializeField] private Vector2 panelSize = new Vector2(760f, 150f);
    [SerializeField] private float bottomOffset = 86f;
    [SerializeField] private Color backgroundColor = new Color(0.1f, 0.025f, 0.025f, 0.92f);
    [SerializeField] private Color borderColor = new Color(0.75f, 0.14f, 0.04f, 0.95f);
    [SerializeField] private Color nameColor = new Color(1f, 0.74f, 0.32f, 1f);
    [SerializeField] private Color textColor = Color.white;

    private RectTransform rectTransform;
    private float hideAt;
    private float inputReadyAt;

    public static UI_NpcDialoguePanel Instance => instance;
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
        if (!gameObject.activeSelf)
            return;

        if (hideAt > 0f && Time.unscaledTime >= hideAt)
        {
            Hide();
            return;
        }

        if (Time.unscaledTime >= inputReadyAt &&
            (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape)))
        {
            Hide();
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    public static UI_NpcDialoguePanel GetOrCreate()
    {
        if (instance != null)
            return instance;

        UI_NpcDialoguePanel existingPanel = FindObjectOfType<UI_NpcDialoguePanel>(true);

        if (existingPanel != null)
        {
            instance = existingPanel;
            return instance;
        }

        Transform parent = FindDefaultParent();
        GameObject panelObject = new GameObject("NpcDialoguePanel_UI", typeof(RectTransform), typeof(Image), typeof(Outline));

        if (parent != null)
            panelObject.transform.SetParent(parent, false);

        return panelObject.AddComponent<UI_NpcDialoguePanel>();
    }

    public void Show(string npcName, string text, float displaySeconds)
    {
        gameObject.SetActive(true);
        BuildRuntimeStyle();

        if (nameText != null)
            nameText.text = string.IsNullOrWhiteSpace(npcName) ? "NPC" : npcName;

        if (dialogueText != null)
            dialogueText.text = string.IsNullOrWhiteSpace(text) ? "……" : text;

        hideAt = displaySeconds > 0f ? Time.unscaledTime + displaySeconds : 0f;
        inputReadyAt = Time.unscaledTime + 0.15f;
    }

    public void Hide()
    {
        hideAt = 0f;
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

        GameObject canvasObject = new GameObject("NpcDialogue_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
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

        nameText = EnsureText("Name", nameText, new Vector2(22f, -46f), new Vector2(-22f, -18f), 24f, nameColor, FontStyles.Bold);
        dialogueText = EnsureText("Dialogue", dialogueText, new Vector2(22f, -56f), new Vector2(-22f, -20f), 22f, textColor, FontStyles.Normal);
    }

    private TextMeshProUGUI EnsureText(string objectName, TextMeshProUGUI currentText, Vector2 offsetMin, Vector2 offsetMax, float fontSize, Color color, FontStyles style)
    {
        if (currentText != null)
            return currentText;

        Transform existing = transform.Find(objectName);

        if (existing != null && existing.TryGetComponent(out TextMeshProUGUI existingText))
            return existingText;

        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 1f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.pivot = new Vector2(0f, 1f);
        textRect.offsetMin = offsetMin;
        textRect.offsetMax = offsetMax;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.font = ResolveFont();
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = color;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.raycastTarget = false;
        text.enableWordWrapping = true;

        return text;
    }

    private TMP_FontAsset ResolveFont()
    {
        if (dialogueFont != null)
            return dialogueFont;

        if (!string.IsNullOrWhiteSpace(fontResourcePath))
            dialogueFont = Resources.Load<TMP_FontAsset>(fontResourcePath);

        return dialogueFont;
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
