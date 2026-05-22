using UnityEngine;

public class PlayerPreciseDodgeAttackState : PlayerState
{
    private float elapsedTime;
    private int nextHitIndex;

    public PlayerPreciseDodgeAttackState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        elapsedTime = 0;
        nextHitIndex = 0;
        stateTimer = player.skill.preciseDodge.FollowUpAttackDuration;
        player.skill.preciseDodge.BeginFollowUpAttack();
    }

    public override void Exit()
    {
        base.Exit();

        player.SetZeroVelocity();
        player.skill.preciseDodge.EndFollowUpAttack();
        player.StartCoroutine("BusyFor", 0.08f);
    }

    public override void Update()
    {
        base.Update();

        if (ShouldInterruptWithAttackInput())
            return;

        elapsedTime += Time.deltaTime;
        ApplyMovement();
        TryDealTimedHits();

        if (triggerCalled || stateTimer < 0)
        {
            if (player.IsGroundDetected())
                stateMachine.ChangeState(player.idleState);
            else
                stateMachine.ChangeState(player.airState);
        }
    }

    private bool ShouldInterruptWithAttackInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            ChangeToLocomotionState();
            return true;
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            stateMachine.ChangeState(player.primaryAttack);
            return true;
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            stateMachine.ChangeState(player.aimSword);
            return true;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            stateMachine.ChangeState(player.counterAttack);
            return true;
        }

        return false;
    }

    private void ChangeToLocomotionState()
    {
        if (player.IsGroundDetected())
            stateMachine.ChangeState(player.idleState);
        else
            stateMachine.ChangeState(player.airState);
    }

    private void ApplyMovement()
    {
        if (elapsedTime <= player.skill.preciseDodge.FollowUpLungeDuration)
            rb.velocity = new Vector2(player.facingDir * player.skill.preciseDodge.FollowUpLungeSpeed, 0);
        else
            player.SetZeroVelocity();
    }

    private void TryDealTimedHits()
    {
        while (nextHitIndex < player.skill.preciseDodge.FollowUpHitCount &&
               elapsedTime >= player.skill.preciseDodge.GetFollowUpHitTime(nextHitIndex))
        {
            player.skill.preciseDodge.DealFollowUpDamage();
            nextHitIndex++;
        }
    }
}
