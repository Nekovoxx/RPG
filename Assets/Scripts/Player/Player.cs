using System.Collections;
using UnityEngine;


public class Player : Entity
{

    [Header("攻击详情")]
    public Vector2[] attackMovement;
    public float counterAttackDuration = 0.2f;

    public bool isBusy { get;  set; }


    [Header("移动信息")]
    public float moveSpeed = 12f;
    public float jumpForce;
    public float swordReturnImpact;
    private float defaultMoveSpeed;
    private float defaultJumpForce;

    [Header("冲刺信息")]
    public float dashSpeed;
    public float dashDuration;
    private float defaultDashSpeed;
    public float dashDir { get; private set; }

    public SkillManager skill { get; private set; }
    public GameObject sword;
    public bool canHeal;


    //状态机设置
    #region States
    public PlayerStateMachine stateMachine { get; private set; }
    public PlayerIdleState idleState { get; private set; }
    public PlayerMoveState moveState { get; private set; }
    public PlayerJumpState jumpState { get; private set; }
    public PlayerAirState airState { get; private set; }
    public PlayerWallSlideState wallSlide { get; private set; }
    public PlayerWallJumpState wallJump { get; private set; }
    public PlayerDashState dashState { get; private set; }
    public PlayerPrimaryAttackState primaryAttack { get; private set; }
    public PlayerCounterAttackState counterAttack { get; private set; }
    public PlayerAimSwordState aimSword { get; private set; }
    public PlayerCatchSwordState catchSword { get; private set; }
    public PlayerDeadState deadState { get; private set; }
    public PlayerHealingState healState { get; private set; }
    #endregion

    protected override void Awake()
    {
        base.Awake();
        stateMachine = new PlayerStateMachine();

        idleState = new PlayerIdleState(this, stateMachine, "Idle");
        moveState = new PlayerMoveState(this, stateMachine, "Move");
        jumpState = new PlayerJumpState(this, stateMachine, "Jump");
        airState = new PlayerAirState(this, stateMachine, "Jump");
        dashState = new PlayerDashState(this, stateMachine, "Dash");
        wallSlide = new PlayerWallSlideState(this, stateMachine, "WallSlide");
        wallJump = new PlayerWallJumpState(this, stateMachine, "Jump");

        primaryAttack = new PlayerPrimaryAttackState(this, stateMachine, "Attack");
        counterAttack = new PlayerCounterAttackState(this, stateMachine, "CounterAttack");

        aimSword = new PlayerAimSwordState(this, stateMachine, "AimSword");
        catchSword = new PlayerCatchSwordState(this, stateMachine, "CatchSword");
        deadState = new PlayerDeadState(this, stateMachine, "Die");
        healState = new PlayerHealingState(this, stateMachine, "Healing");
    }

    protected override void Start()
    {

        base.Start();
        skill = SkillManager.instance;

        stateMachine.Initialize(idleState);

        defaultMoveSpeed = moveSpeed;
        defaultJumpForce = jumpForce;
        defaultDashSpeed = dashSpeed;
    }



    protected override void Update()
    {
        base.Update();
        stateMachine.currentState.Update();


        CheakForDashInput();
    }
    public override void SlowEntityBy(float _slowPercentage, float slowDuration)
    {
        moveSpeed = moveSpeed * (1- _slowPercentage);
        jumpForce = jumpForce * (1- _slowPercentage);
        dashSpeed = dashSpeed * (1- _slowPercentage);
        anim.speed = anim.speed * (1- _slowPercentage);

        Invoke("ReturnDefaultsSpeed", _slowPercentage); 


    }
    public void AssignNewSword(GameObject _newSword)
    {
        sword = _newSword;
    }

    protected override void ReturnDefaultSpeed()
    {
        base.ReturnDefaultSpeed();

        moveSpeed = defaultMoveSpeed;
        jumpForce = defaultJumpForce;
        dashSpeed = defaultDashSpeed;
    }

    public void CatchTheSword()
    {
        stateMachine.ChangeState(catchSword);
        Destroy(sword);
    }

    public IEnumerator BusyFor(float _seconds)
    {
        isBusy = true;

        yield return new WaitForSeconds(_seconds);

        isBusy = false;
    }



    public void AnimationTrigger() => stateMachine.currentState.AnimationFinishTrigger();

    private void CheakForDashInput()
    {


        if (IsWallDetected() && !IsGroundDetected())
            return;




        if (Input.GetKeyDown(KeyCode.LeftShift) && SkillManager.instance.dash.CanUseSkill())
        {


            dashDir = Input.GetAxisRaw("Horizontal");

            if (dashDir == 0)
                dashDir = facingDir;

            stateMachine.ChangeState(dashState);

        }
    }
    public void HealEvent()
    {
        if (!canHeal) return;

        Inventory.instance.UesFlask();

        canHeal = false; // 防止重复触发
    }
    public override void Die()
    {
        base.Die();

        stateMachine.ChangeState(deadState);
    }
}
