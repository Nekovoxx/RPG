using System.Collections;
using UnityEngine;


public class Player : Entity
{

    [Header("攻击详情")]
    public Vector2[] attackMovement;
    public float counterAttackDuration = 0.2f;

    public bool isBusy { get;  set; }
    public bool IsInvisible { get; private set; }


    [Header("移动信息")]
    public float moveSpeed = 12f;
    public float jumpForce;
    public float swordReturnImpact;
    private float defaultMoveSpeed;
    private float defaultJumpForce;

    [Header("二段跳信息")]
    public float doubleJumpForce;
    [SerializeField] private int extraJumpCount = 1;
    private int availableExtraJumps;

    [Header("冲刺信息")]
    public float dashSpeed;
    public float dashDuration;
    private float defaultDashSpeed;
    public float dashDir { get; private set; }
    private Coroutine queuedDashCoroutine;
    private float queuedDashDir;
    private int lastPreciseDodgeFrame = -1000;
    private float lastShiftPressedTime = -999f;

    public SkillManager skill { get; private set; }
    public PlayerInteraction interaction { get; private set; }
    public GameObject sword;
    public bool canHeal;


    //状态机设置
    #region States
    public PlayerStateMachine stateMachine { get; private set; }
    public PlayerIdleState idleState { get; private set; }
    public PlayerMoveState moveState { get; private set; }
    public PlayerJumpState jumpState { get; private set; }
    public PlayerDoubleJumpState doubleJumpState { get; private set; }
    public PlayerAirState airState { get; private set; }
    public PlayerWallSlideState wallSlide { get; private set; }
    public PlayerWallJumpState wallJump { get; private set; }
    public PlayerDashState dashState { get; private set; }
    public PlayerPrimaryAttackState primaryAttack { get; private set; }
    public PlayerCounterAttackState counterAttack { get; private set; }
    public PlayerAimSwordState aimSword { get; private set; }
    public PlayerCatchSwordState catchSword { get; private set; }
    public PlayerPreciseDodgeState preciseDodgeState { get; private set; }
    public PlayerPreciseDodgeAttackState preciseDodgeAttackState { get; private set; }
    public PlayerSunSkillState sunSkillState { get; private set; }
    public PlayerDeadState deadState { get; private set; }
    public PlayerHealingState healState { get; private set; }
    #endregion

    protected override void Awake()
    {
        base.Awake();
        EnsureInteractionComponent();

        stateMachine = new PlayerStateMachine();

        idleState = new PlayerIdleState(this, stateMachine, "Idle");
        moveState = new PlayerMoveState(this, stateMachine, "Move");
        jumpState = new PlayerJumpState(this, stateMachine, "Jump");
        doubleJumpState = new PlayerDoubleJumpState(this, stateMachine, "DoubleJump");
        airState = new PlayerAirState(this, stateMachine, "Jump");
        dashState = new PlayerDashState(this, stateMachine, "Dash");
        wallSlide = new PlayerWallSlideState(this, stateMachine, "WallSlide");
        wallJump = new PlayerWallJumpState(this, stateMachine, "Jump");

        primaryAttack = new PlayerPrimaryAttackState(this, stateMachine, "Attack");
        counterAttack = new PlayerCounterAttackState(this, stateMachine, "CounterAttack");

        aimSword = new PlayerAimSwordState(this, stateMachine, "AimSword");
        catchSword = new PlayerCatchSwordState(this, stateMachine, "CatchSword");
        preciseDodgeState = new PlayerPreciseDodgeState(this, stateMachine, "PreciseDodge");
        preciseDodgeAttackState = new PlayerPreciseDodgeAttackState(this, stateMachine, "PreciseDodgeAttack");
        sunSkillState = new PlayerSunSkillState(this, stateMachine, "SunSkill");
        deadState = new PlayerDeadState(this, stateMachine, "Die");
        healState = new PlayerHealingState(this, stateMachine, "Healing");
    }

    private void EnsureInteractionComponent()
    {
        if (!TryGetComponent<PlayerInteraction>(out PlayerInteraction playerInteraction))
            playerInteraction = gameObject.AddComponent<PlayerInteraction>();

        interaction = playerInteraction;
    }

    protected override void Start()
    {

        base.Start();
        skill = SkillManager.instance;
        ResetExtraJumps();

        stateMachine.Initialize(idleState);

        defaultMoveSpeed = moveSpeed;
        defaultJumpForce = jumpForce;
        defaultDashSpeed = dashSpeed;
    }



    protected override void Update()
    {
        base.Update();
        RecordShiftInput();
        stateMachine.currentState.Update();

        CheckForPreciseDodgeFollowUpInput();

        CheckForAwakeningInput();
        CheckForInvisibilityInput();

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

    public void ResetExtraJumps()
    {
        availableExtraJumps = extraJumpCount;
    }

    public bool CanDoubleJump() => availableExtraJumps > 0;

    public void ConsumeExtraJump()
    {
        availableExtraJumps = Mathf.Max(availableExtraJumps - 1, 0);
    }

    public float GetDoubleJumpForce()
    {
        return doubleJumpForce > 0 ? doubleJumpForce : jumpForce;
    }


    public IEnumerator BusyFor(float _seconds)
    {
        isBusy = true;

        yield return new WaitForSeconds(_seconds);

        isBusy = false;
    }



    public void AnimationTrigger() => stateMachine.currentState.AnimationFinishTrigger();

    public void InterruptPreciseDodgeFollowUp()
    {
        if (stateMachine.currentState != preciseDodgeAttackState)
            return;

        skill?.preciseDodge?.CancelFollowUpAttack();

        if (IsGroundDetected())
            stateMachine.ChangeState(idleState);
        else
            stateMachine.ChangeState(airState);
    }

    public bool TryStartPreciseDodge(Transform attacker)
    {
        if (skill == null || skill.preciseDodge == null)
            return false;

        bool dodged = skill.preciseDodge.TryStartFromAttackFrame(attacker);

        if (dodged)
            MarkPreciseDodgeStarted();

        return dodged;
    }

    public void MarkPreciseDodgeStarted()
    {
        lastPreciseDodgeFrame = Time.frameCount;
    }

    public bool HasBufferedPreciseDodgeInput(float bufferDuration)
    {
        return ShiftWasPressedThisFrame() || Time.time <= lastShiftPressedTime + bufferDuration;
    }

    public void ConsumePreciseDodgeInput()
    {
        lastShiftPressedTime = -999f;
    }

    private void RecordShiftInput()
    {
        if (ShiftWasPressedThisFrame())
            lastShiftPressedTime = Time.time;
    }

    private void CheckForPreciseDodgeFollowUpInput()
    {
        if (stateMachine.currentState == preciseDodgeAttackState)
            return;

        if (Input.GetKeyDown(KeyCode.R))
            skill?.preciseDodge?.TryUseFollowUpAttack();
    }

    private void CheckForAwakeningInput()
    {
        if (Input.GetKeyDown(KeyCode.V))
            skill?.awakening?.TryActivate();
    }

    private void CheckForInvisibilityInput()
    {
        if (Input.GetKeyDown(KeyCode.X))
            skill?.invisibility?.TryActivate();
    }

    public void SetInvisible(bool invisible)
    {
        IsInvisible = invisible;
    }

    private void CheakForDashInput()
    {


        if (IsWallDetected() && !IsGroundDetected())
            return;


        if (!ShiftWasPressedThisFrame())
            return;

        if (lastPreciseDodgeFrame == Time.frameCount)
            return;

        if (SkillManager.instance.dash == null || !SkillManager.instance.dash.IsReady())
            return;

        queuedDashDir = Input.GetAxisRaw("Horizontal");

        if (queuedDashDir == 0)
            queuedDashDir = facingDir;

        if (queuedDashCoroutine != null)
            StopCoroutine(queuedDashCoroutine);

        queuedDashCoroutine = StartCoroutine(UseDashAfterPreciseDodgeCheck(Time.frameCount, queuedDashDir));
    }

    private IEnumerator UseDashAfterPreciseDodgeCheck(int inputFrame, float requestedDashDir)
    {
        yield return null;

        queuedDashCoroutine = null;

        if (lastPreciseDodgeFrame >= inputFrame)
            yield break;

        if (skill != null && skill.preciseDodge != null && skill.preciseDodge.IsDodging)
            yield break;

        if (IsWallDetected() && !IsGroundDetected())
            yield break;

        if (SkillManager.instance.dash != null && SkillManager.instance.dash.CanUseSkill())
        {
            dashDir = requestedDashDir;
            stateMachine.ChangeState(dashState);
        }
    }

    private bool ShiftWasPressedThisFrame()
    {
        return Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
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
