using UnityEngine;

public class PlayerWallJumpState : PlayerState
{
    public PlayerWallJumpState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        stateTimer = 1f;
        player.SetVelocity(5 * -player.facingDir, player.jumpForce);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.Space) && player.CanDoubleJump())
        {
            stateMachine.ChangeState(player.doubleJumpState);
            return;
        }

        if (player.IsWallDetected() && !player.IsGroundDetected())
        {
            stateMachine.ChangeState(player.wallSlide);
            return;
        }

        if (stateTimer < 0)
        {
            stateMachine.ChangeState(player.airState);
            return;
        }

        if (player.IsGroundDetected())
            stateMachine.ChangeState(player.idleState);
    }
}
