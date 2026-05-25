using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class AwakeningAuraEffect : MonoBehaviour
{
    [SerializeField, Min(12)] private int segments = 96;
    [SerializeField, Min(0.05f)] private float radiusX = 1.08f;
    [SerializeField, Min(0.05f)] private float radiusY = 1.5f;
    [SerializeField, Min(0.01f)] private float lineWidth = 0.12f;
    [SerializeField] private Color innerGold = new Color(1f, 0.9f, 0.25f, 1f);
    [SerializeField] private Color outerGold = new Color(1f, 0.55f, 0.06f, 0.9f);
    [SerializeField, Min(0f)] private float pulseSpeed = 6.5f;
    [SerializeField, Min(0f)] private float pulseAmount = 0.1f;
    [SerializeField, Min(0f)] private float rotateSpeed = 70f;

    private static Sprite glowSprite;

    private LineRenderer ring;
    private SpriteRenderer glow;
    private Material runtimeMaterial;
    private float angleOffset;

    private void Awake()
    {
        SetupVisuals();
    }

    private void OnEnable()
    {
        DrawAura();
    }

    private void Update()
    {
        angleOffset += rotateSpeed * Mathf.Deg2Rad * Time.deltaTime;
        DrawAura();
    }

    private void OnDestroy()
    {
        if (runtimeMaterial != null)
            Destroy(runtimeMaterial);
    }

    public void Configure(float newRadiusX, float newRadiusY, float newLineWidth, Color newInnerGold, Color newOuterGold)
    {
        radiusX = Mathf.Max(0.05f, newRadiusX);
        radiusY = Mathf.Max(0.05f, newRadiusY);
        lineWidth = Mathf.Max(0.01f, newLineWidth);
        innerGold = newInnerGold;
        outerGold = newOuterGold;

        SetupVisuals();
        DrawAura();
    }

    private void SetupVisuals()
    {
        if (ring == null)
            ring = GetComponent<LineRenderer>();

        if (runtimeMaterial == null)
            runtimeMaterial = new Material(Shader.Find("Sprites/Default"));

        ring.useWorldSpace = false;
        ring.loop = true;
        ring.positionCount = segments;
        ring.widthMultiplier = lineWidth;
        ring.startColor = innerGold;
        ring.endColor = outerGold;
        ring.material = runtimeMaterial;
        ring.textureMode = LineTextureMode.Stretch;
        ring.sortingOrder = 22;
        ring.numCapVertices = 4;
        ring.numCornerVertices = 4;

        if (glow == null)
            glow = CreateGlowRenderer();

        glow.sprite = GetGlowSprite();
        glow.color = new Color(innerGold.r, innerGold.g, innerGold.b, 0.32f);
        glow.sortingOrder = 18;
    }

    private SpriteRenderer CreateGlowRenderer()
    {
        Transform child = transform.Find("Awakening Glow");
        GameObject glowObject = child != null ? child.gameObject : new GameObject("Awakening Glow");

        glowObject.transform.SetParent(transform, false);
        glowObject.transform.localPosition = Vector3.zero;
        glowObject.transform.localRotation = Quaternion.identity;

        SpriteRenderer spriteRenderer = glowObject.GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
            spriteRenderer = glowObject.AddComponent<SpriteRenderer>();

        return spriteRenderer;
    }

    private Sprite GetGlowSprite()
    {
        if (glowSprite != null)
            return glowSprite;

        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float maxDistance = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center) / maxDistance;
                float alpha = Mathf.Clamp01(1f - distance);
                alpha = alpha * alpha * 0.75f;
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        glowSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
        glowSprite.name = "Awakening Generated Glow";

        return glowSprite;
    }

    private void DrawAura()
    {
        if (ring == null)
            return;

        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        float secondaryPulse = 1f + Mathf.Sin(Time.time * (pulseSpeed * 0.65f) + 1.4f) * (pulseAmount * 0.55f);

        if (glow != null)
            glow.transform.localScale = new Vector3(radiusX * 2.8f * secondaryPulse, radiusY * 2.55f * secondaryPulse, 1f);

        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)segments;
            float angle = t * Mathf.PI * 2f + angleOffset;
            float shimmer = 1f + Mathf.Sin(angle * 5f + Time.time * pulseSpeed) * 0.035f;
            Vector3 point = new Vector3(Mathf.Cos(angle) * radiusX * pulse * shimmer, Mathf.Sin(angle) * radiusY * pulse, 0f);

            ring.SetPosition(i, point);
        }
    }
}
