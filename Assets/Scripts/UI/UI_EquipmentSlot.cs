using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_EquipmentSlot : UI_ItemSlot
{
    [SerializeField] private Image icon;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color disabledColor = new Color(1, 1, 1, 0.3f);
    public EquipmentType slotType;

    private void OnValidate()
    {
        gameObject.name = "装备槽-" + slotType.ToString();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        ItemData_Equipment equipmentData = item != null ? item.data as ItemData_Equipment : null;

        if(equipmentData == null || Inventory.instance == null)
            return;

        Inventory.instance.UnequipItem(equipmentData);
        Inventory.instance.AddItem(equipmentData);

        ui?.itemTooltip?.HideTooltip();

        CleanUpSlot();
    }
    public void UpdateFlaskState(bool canUse)
    {
        if (icon == null) return;

        icon.color = canUse ? normalColor : disabledColor;
    }
}

