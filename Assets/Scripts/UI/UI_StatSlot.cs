using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_StatSlot : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    private UI ui;

    [SerializeField] private string statName;
    [SerializeField] private StatType statType;
    [SerializeField] private TextMeshProUGUI statValueText;
    [SerializeField] private TextMeshProUGUI statNameText;

    [TextArea]
    [SerializeField] private string statDescription;

    private void OnValidate()
    {
        gameObject.name = "Stat -" + statName;
        if (statNameText != null)
        {
            statNameText.text = statName;
        }
    }
    void Start()
    {
        UpdateStatValueUI();

        ui = GetComponentInParent<UI>();
    }

    public void UpdateStatValueUI()
    {
     PlayerStats playerState = PlayerManager.instance.player.GetComponent<PlayerStats>();

     if(playerState != null)
        {
             statValueText.text = playerState.GetStat(statType).GetValue().ToString();

            if (statType == StatType.ÉúÃü)
                statValueText.text = playerState.GetMaxHealthValue().ToString();

            if(statType == StatType.¹¥»÷Á¦)
                statValueText.text = (playerState.damage.GetValue()+playerState.strength.GetValue()).ToString();

             if(statType == StatType.±©»÷ÉËº¦)
                statValueText.text = (playerState.critPower.GetValue() + playerState.strength.GetValue()).ToString();

            if (statType == StatType.±©»÷¼¸ÂÊ)
                statValueText.text =(playerState.critChance.GetValue() + playerState.agility.GetValue()).ToString();
            if (statType == StatType.ÉÁ±Ü)
                statValueText.text= (playerState.evasion.GetValue() + playerState.agility.GetValue()).ToString();
            if (statType == StatType.Ä§·¨¿¹ÐÔ)
                statValueText.text = (playerState.magicResistance.GetValue() + (playerState.intelligence.GetValue() * 3)).ToString();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ui.statToolTip.ShowStatToolTip(statDescription);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ui.statToolTip.HideStatToolTip();
    }
}
