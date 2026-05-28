using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_InGame : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text emberText;

    [SerializeField] private Image dashImage;
    [SerializeField] private Image swordImage;
    [SerializeField] private Image preciseDodgeImage;
    [SerializeField] private Image sunImage;
    [SerializeField] private Image awakeningImage;
    [SerializeField] private Image invisibilityImage;
    [SerializeField] private Image flaskImage;
    [SerializeField] private Image parryImage;

    private SkillManager skills;
    private PlayerEmberWallet emberWallet;

    void Start()
    {
        if (playerStats != null)
            playerStats.onHealthChanged += UpdateHealthUI;

        skills = SkillManager.instance;
        BindCooldownImages();
        InitializeCooldownImages();
        BindEmberText();
        BindEmberWallet();
    }

    void Update()
    {
        if (skills == null)
            skills = SkillManager.instance;

        if (skills == null)
            return;

        CheakCooldownOf(dashImage, skills.dash);
        CheakCooldownOf(preciseDodgeImage, skills.preciseDodge);
        CheakCooldownOf(sunImage, skills.sun);
        CheakCooldownOf(awakeningImage, skills.awakening);
        CheakCooldownOf(invisibilityImage, skills.invisibility);
        CheakCooldownOf(flaskImage, Inventory.instance != null ? Inventory.instance.flaskCooldown : 0);
    }

    private void OnDestroy()
    {
        if (emberWallet != null)
            emberWallet.OnEmbersChanged -= HandleEmbersChanged;
    }

    private void UpdateHealthUI()
    {
        slider.maxValue = playerStats.GetMaxHealthValue();
        slider.value = playerStats.currentHealth;
    }

    private void BindCooldownImages()
    {
        dashImage = FindCooldownImage(dashImage,
            new[] { "DashCooldown", "Dash Cooldown", "Dash_UI", "\u51B2\u523A\u51B7\u5374" },
            new[] { "dash", "\u51B2\u523A" });

        swordImage = FindCooldownImage(swordImage,
            new[] { "SwordCooldown", "Sword Cooldown", "ThrowSwordCooldown", "Sword_UI", "\u63B7\u5251\u51B7\u5374", "\u98DE\u5251\u51B7\u5374" },
            new[] { "sword", "throwsword", "\u63B7\u5251", "\u98DE\u5251" });

        preciseDodgeImage = FindCooldownImage(preciseDodgeImage,
            new[] { "PreciseDodgeCooldown", "Precise Dodge Cooldown", "Dodge_UI", "\u7CBE\u51C6\u95EA\u907F\u51B7\u5374", "\u7CBE\u95EA\u51B7\u5374" },
            new[] { "precisedodge", "dodge", "\u7CBE\u51C6\u95EA\u907F", "\u7CBE\u95EA" });

        sunImage = FindCooldownImage(sunImage,
            new[] { "SunCooldown", "SunSkillCooldown", "Sun Skill Cooldown", "Sun_UI", "\u65E5\u5195\u51B7\u5374", "\u592A\u9633\u51B7\u5374", "\u707C\u65E5\u51CC\u7A7A\u51B7\u5374" },
            new[] { "sun", "sunskill", "\u65E5\u5195", "\u592A\u9633", "\u707C\u65E5" });

        awakeningImage = FindCooldownImage(awakeningImage,
            new[] { "AwakeningCooldown", "Awakening Cooldown", "AwakenCooldown", "Awaken_UI", "\u89C9\u9192\u51B7\u5374" },
            new[] { "awakening", "awaken", "\u89C9\u9192" });

        invisibilityImage = FindCooldownImage(invisibilityImage,
            new[] { "InvisibilityCooldown", "InvisibleCooldown", "StealthCooldown", "Hide_UI", "\u9690\u8EAB\u51B7\u5374" },
            new[] { "invisibility", "invisible", "stealth", "hide", "\u9690\u8EAB" });
    }

    private void BindEmberText()
    {
        if (emberText != null)
            return;

        Transform emberRoot = FindTransformByName(transform, "yujin_UI");

        if (emberRoot == null)
            emberRoot = FindTransformByName(transform, "\u4F59\u70EC_UI");

        if (emberRoot != null)
            emberText = emberRoot.GetComponentInChildren<TMP_Text>(true);

        if (emberText != null)
            RefreshEmberText();
    }

    private void BindEmberWallet()
    {
        PlayerEmberWallet wallet = PlayerEmberWallet.GetOrCreate();

        if (emberWallet == wallet)
        {
            RefreshEmberText();
            return;
        }

        if (emberWallet != null)
            emberWallet.OnEmbersChanged -= HandleEmbersChanged;

        emberWallet = wallet;

        if (emberWallet != null)
        {
            emberWallet.OnEmbersChanged += HandleEmbersChanged;
            emberWallet.Load();
        }

        RefreshEmberText();
    }

    private void HandleEmbersChanged(int currentEmbers)
    {
        if (emberText == null)
            BindEmberText();

        if (emberText != null)
            emberText.text = currentEmbers.ToString();
    }

    private void RefreshEmberText()
    {
        if (emberText == null)
            return;

        emberText.text = emberWallet != null ? emberWallet.CurrentEmbers.ToString() : "0";
    }

    private void InitializeCooldownImages()
    {
        PrepareCooldownImage(dashImage);
        PrepareCooldownImage(preciseDodgeImage);
        PrepareCooldownImage(sunImage);
        PrepareCooldownImage(awakeningImage);
        PrepareCooldownImage(invisibilityImage);
        PrepareCooldownImage(flaskImage);
    }

    private Image FindCooldownImage(Image assignedImage, string[] exactNames, string[] keywords)
    {
        if (assignedImage != null)
            return assignedImage;

        Image[] childImages = GetComponentsInChildren<Image>(true);

        foreach (string exactName in exactNames)
        {
            Transform matchedRoot = FindTransformByName(transform, exactName);

            if (matchedRoot == null)
                continue;

            Image cooldownImage = FindImageInRoot(matchedRoot, "cooldown", "\u51B7\u5374");

            if (cooldownImage != null)
                return cooldownImage;

            Image rootImage = matchedRoot.GetComponent<Image>();

            if (rootImage != null)
                return rootImage;

            Image anyChildImage = matchedRoot.GetComponentInChildren<Image>(true);

            if (anyChildImage != null)
                return anyChildImage;
        }

        foreach (Image image in childImages)
        {
            if (ImageNameContains(image, keywords) && ImageNameContains(image, "cooldown", "\u51B7\u5374"))
                return image;
        }

        foreach (Image image in childImages)
        {
            if (ImageNameContains(image, keywords))
                return image;
        }

        return null;
    }

    private Transform FindTransformByName(Transform root, string expectedName)
    {
        string normalizedExpectedName = NormalizeName(expectedName);

        if (NormalizeName(root.name) == normalizedExpectedName)
            return root;

        foreach (Transform child in root)
        {
            Transform matchedChild = FindTransformByName(child, expectedName);

            if (matchedChild != null)
                return matchedChild;
        }

        return null;
    }

    private Image FindImageInRoot(Transform root, params string[] keywords)
    {
        Image[] images = root.GetComponentsInChildren<Image>(true);

        foreach (Image image in images)
        {
            if (ImageNameContains(image, keywords))
                return image;
        }

        return null;
    }

    private bool ImageNameContains(Image image, params string[] keywords)
    {
        for (Transform current = image.transform; current != null; current = current.parent)
        {
            string normalizedName = NormalizeName(current.name);

            foreach (string keyword in keywords)
            {
                if (normalizedName.Contains(NormalizeName(keyword)))
                    return true;
            }

            if (current == transform)
                break;
        }

        return false;
    }

    private string NormalizeName(string value)
    {
        return value
            .Replace(" ", "")
            .Replace("_", "")
            .Replace("-", "")
            .Replace("(", "")
            .Replace(")", "")
            .ToLowerInvariant();
    }

    private void PrepareCooldownImage(Image image)
    {
        if (image == null)
            return;

        SyncCooldownSpriteWithIcon(image);
        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Radial360;
        image.fillOrigin = (int)Image.Origin360.Top;
        image.fillClockwise = false;
        image.fillAmount = 0;
    }

    private void SyncCooldownSpriteWithIcon(Image cooldownImage)
    {
        Image iconImage = FindIconImageForCooldown(cooldownImage.transform);

        if (iconImage == null || iconImage == cooldownImage || iconImage.sprite == null)
            return;

        cooldownImage.sprite = iconImage.sprite;
        cooldownImage.preserveAspect = iconImage.preserveAspect;
    }

    private Image FindIconImageForCooldown(Transform cooldownTransform)
    {
        for (Transform current = cooldownTransform.parent; current != null; current = current.parent)
        {
            if (current == transform)
                break;

            Image image = current.GetComponent<Image>();

            if (image != null && image.sprite != null)
                return image;
        }

        return null;
    }

    private void CheakCooldownOf(Image _image, Skill _skill)
    {
        if (_image == null || _skill == null || _skill.cooldown <= 0)
        {
            if (_image != null)
                _image.fillAmount = 0;

            return;
        }

        _image.fillAmount = Mathf.Clamp01(_skill.CooldownRemaining / _skill.cooldown);
    }

    private void CheakCooldownOf(Image _image, float _cooldown)
    {
        if (_image == null)
            return;

        if (_cooldown <= 0)
        {
            _image.fillAmount = 0;
            return;
        }

        if (_image.fillAmount > 0)
            _image.fillAmount -= 1 / _cooldown * Time.deltaTime;
    }
}
