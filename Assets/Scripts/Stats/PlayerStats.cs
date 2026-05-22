public class PlayerStats : CharacterStats
{
    private Player player;
    protected override void Start()
    {
        base.Start();

        player = GetComponent<Player>();
    }
    public override void TakeDamage(int _damage)
    {
        base.TakeDamage(_damage);

        if (player == null)
            player = GetComponent<Player>();

        if (_damage > 0)
            player?.InterruptPreciseDodgeFollowUp();
    }
    protected override void Die()
    {
        base.Die();

        player.Die();

        GetComponent<PlayerItemDrop>()?.GenerateDrop();
    }

    protected override void DecreaseHealthBy(int _damage)
    {
        base.DecreaseHealthBy(_damage);

        ItemData_Equipment currentArmor = Inventory.instance.GetEquipment(EquipmentType.Armor);

        if (currentArmor != null)
            currentArmor.Effect(player.transform);
    }
}
