using UnityEngine;

public class ItemObject_Trigger : MonoBehaviour
{
    private ItemObject myItemObject;

    private void Awake()
    {
        myItemObject = GetComponentInParent<ItemObject>();

        
        if (myItemObject == null)
        {
            Debug.LogError("父物体上未找到ItemObject组件！", this);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
      
        if (collision.TryGetComponent(out Player player) && myItemObject != null)
        { 
            if(collision.GetComponent<CharacterStats>().isDead)
                return;

            Debug.Log("拾取物品");
            myItemObject.PickupItem();
        }
    }
}
