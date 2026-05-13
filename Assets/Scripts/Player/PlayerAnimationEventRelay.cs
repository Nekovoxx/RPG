using UnityEngine;

public class AnimationEventRelay : MonoBehaviour
{
    private Player player;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
    }

    public void HealEvent()
    {
        player.HealEvent();
    }
}