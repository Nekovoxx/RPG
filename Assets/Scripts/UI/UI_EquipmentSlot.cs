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
        gameObject.name = "×°±¸²Û-" + slotType.ToString();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if(item == null || item.data == null)
            return;

        Inventory.instance.UnequipItem(item.data as ItemData_Equipment);
        Inventory.instance.AddItem(item.data as ItemData_Equipment);

        ui.itemTooltip.HideTooltip();

        CleanUpSlot();
    }
    public void UpdateFlaskState(bool canUse)
    {
        if (icon == null) return;

        icon.color = canUse ? normalColor : disabledColor;
    }
}
