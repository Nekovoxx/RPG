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
        if (Input.GetKeyDown(KeyCode.LeftShift))
            SetCooldownOf(dashImage);

        if(Input.GetKeyDown(KeyCode.Mouse1))
            SetCooldownOf(swordImage);

        if(Input.GetKeyDown(KeyCode.R))
            SetCooldownOf(flaskImage);

        CheakCooldownOf(dashImage, skills.dash.cooldown);
        CheakCooldownOf(swordImage, skills.sword.cooldown);
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

    private void CheakCooldownOf(Image _image, float _cooldown)
    {
        if (_image.fillAmount > 0)
            _image.fillAmount -= 1 / _cooldown * Time.deltaTime;
    }
}
