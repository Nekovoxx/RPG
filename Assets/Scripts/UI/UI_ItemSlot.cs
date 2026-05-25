using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_ItemSlot : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    [SerializeField ]protected Image itemImage;
    [SerializeField ]protected TextMeshProUGUI itemText;

    protected UI ui;
    public InventoryItem item;

    protected virtual void Start()
    {
        ui = GetComponentInParent<UI>();
    }
    public void UpdateSlot(InventoryItem _newitem)
    {
        item = _newitem;

        itemImage.color = Color.white;
        if (item != null)
        {
            itemImage.sprite = item.data.itemicon;
            if (item.stackSize > 1)
            {
                itemText.text = item.stackSize.ToString();
            }
            else
            {
                itemText.text = "";
            }
        }
    }

    public void CleanUpSlot()
    {
        item = null;

        itemImage.sprite = null;
        itemImage.color = Color.clear;

        itemText.text = ""; 
    }

    public virtual  void OnPointerDown(PointerEventData eventData)
    {
        if (item == null)
            return;

        if(Input.GetKey(KeyCode.LeftControl))
        {
            Inventory.instance.RemoveItem(item.data);
            ui?.itemTooltip?.HideTooltip();
            return;
        }

        if (item.data.itemType == ItemType.Equipment)
            Inventory.instance.EquipItem(item.data);

        ui?.itemTooltip?.HideTooltip();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(item == null || item.data == null || ui == null || ui.itemTooltip == null)
            return;

       ui.itemTooltip.ShowTooltip(item.data);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (item == null || item.data == null || ui == null || ui.itemTooltip == null)
            return;

        ui.itemTooltip.UpdateTooltipPosition();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ui?.itemTooltip?.HideTooltip();
    }
}
