using UnityEngine;
using UnityEngine.UI;
public class UI_InGame : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private Slider slider;

    [SerializeField] private Image dashImage;
    [SerializeField] private Image swordImage;
    [SerializeField] private Image flaskImage;
    [SerializeField] private Image parryImage;

    private SkillManager skills;
    void Start()
    {
        if (playerStats != null)
            playerStats.onHealthChanged += UpdateHealthUI;

        skills = SkillManager.instance;
    }

    void Update()
    {
        if (skills == null)
            return;

        if(Input.GetKeyDown(KeyCode.Mouse1))
            SetCooldownOf(swordImage);

        CheakCooldownOf(dashImage, skills.dash);
        CheakCooldownOf(swordImage, skills.sword);
        CheakCooldownOf(flaskImage, Inventory.instance.flaskCooldown);
    }

    private void UpdateHealthUI()
    {
        slider.maxValue = playerStats.GetMaxHealthValue();
        slider.value = playerStats.currentHealth;
    }

    private void SetCooldownOf(Image _image)
    {
        if (_image.fillAmount <= 0)
            _image.fillAmount = 1;
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
        if (_image == null || _cooldown <= 0)
            return;

        if (_image.fillAmount > 0)
            _image.fillAmount -= 1 / _cooldown * Time.deltaTime;
    }
}
