using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_StatSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private UI ui;

    [SerializeField] private string statName;
    [SerializeField] private StatType statType;
    [SerializeField] private TextMeshProUGUI statValueText;
    [SerializeField] private TextMeshProUGUI statNameText;

    [TextArea]
    [SerializeField] private string statDescription;

    public StatType StatType => statType;
    public string StatName => statName;

    public TextMeshProUGUI StatValueText
    {
        get
        {
            if (statValueText == null)
                statValueText = GetComponent<TextMeshProUGUI>();

            return statValueText;
        }
    }

    public TextMeshProUGUI StatNameText => statNameText;

    private void OnValidate()
    {
        gameObject.name = "Stat -" + statName;

        if (statNameText != null)
            statNameText.text = statName;
    }

    private void Start()
    {
        UpdateStatValueUI();

        ui = GetComponentInParent<UI>();
    }

    public void UpdateStatValueUI()
    {
        if (PlayerManager.instance == null || PlayerManager.instance.player == null)
            return;

        PlayerStats playerState = PlayerManager.instance.player.GetComponent<PlayerStats>();

        if (playerState == null || StatValueText == null)
            return;

        StatValueText.text = UpgradeCalculator.GetCurrentStatValue(playerState, statType).ToString();
    }

    public void SetStatValueText(string text)
    {
        if (StatValueText != null)
            StatValueText.text = text;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ui != null && ui.statToolTip != null)
            ui.statToolTip.ShowStatToolTip(statDescription);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ui != null && ui.statToolTip != null)
            ui.statToolTip.HideStatToolTip();
    }
}
