using UnityEngine;

public class ItemObject : MonoBehaviour, IInteractable
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private ItemData itemData;
    [SerializeField] private string pickupPrompt = "按 E 拾取";

    public string InteractionPrompt => itemData != null ? pickupPrompt + " " + itemData.itemName : pickupPrompt;
    public Transform InteractionTransform => transform;

    public bool CanInteract(Player player)
    {
        return itemData != null && Inventory.instance != null;
    }

    public void Interact(Player player)
    {
        if (player != null && player.stats != null && player.stats.isDead)
            return;

        PickupItem();
    }

    private void SetupVisuals()
    {
        if (itemData == null)
            return;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            spriteRenderer.sprite = itemData.itemicon;

        gameObject.name = "Item object-" + itemData.itemName;

    }
    public void SetupItem(ItemData _itemData, Vector2 _velocity)
    {
        itemData = _itemData;

        if (rb != null)
            rb.velocity = _velocity;

        SetupVisuals();
    }

    public void PickupItem()
    {
        if (itemData == null || Inventory.instance == null)
            return;

        if (!Inventory.instance.CanAddItem() && itemData.itemType == ItemType.Equipment)
        {
            if (rb != null)
                rb.velocity = new Vector2(0, 7);

            return;
        } 

        Inventory.instance.AddItem(itemData);
        Destroy(gameObject);
    }
}
