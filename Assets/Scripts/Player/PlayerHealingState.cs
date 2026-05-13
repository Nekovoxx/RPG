using UnityEngine;

public class PlayerHealingState : PlayerGroundState
{
    public PlayerHealingState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName)
        : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        if (!Inventory.instance.CanUseFlask())
        {
            stateMachine.ChangeState(player.idleState);
            return;
        }

        player.SetZeroVelocity();
        player.canHeal = true;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        //  动画播放完 → 回 Idle
        if (triggerCalled)
        {
            stateMachine.ChangeState(player.idleState);
            return;
        }

        //  移动打断
        if (xInput != 0)
        {
            stateMachine.ChangeState(player.moveState);
            return;
        }

        //  跳跃打断
        if (Input.GetKeyDown(KeyCode.Space))
        {
            stateMachine.ChangeState(player.jumpState);
            return;
        }

        //  攻击打断
        if (Input.GetMouseButtonDown(0))
        {
            stateMachine.ChangeState(player.primaryAttack);
            return;
        }

        //  冲刺打断
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            stateMachine.ChangeState(player.dashState);
        }
    }
}