using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreciseDodge_Skill : Skill
{
    [Header("Precise Dodge")]
    [SerializeField] private float dodgeDuration = 0.4f;
    [SerializeField] private float dodgeSpeed = 14f;
    [SerializeField] private float timeStopDuration = 1f;
    [SerializeField] private float inputBufferDuration = 0.18f;
    [SerializeField] private float followUpWindow = 1f;
    [SerializeField] private bool followUpUnlocked = true;

    [Header("Follow Up Attack")]
    [SerializeField] private float followUpAttackDuration = 3f;
    [SerializeField] private float followUpLungeDuration = 0.28f;
    [SerializeField] private float followUpLungeSpeed = 16f;
    [SerializeField] private float followUpHitRadiusMultiplier = 1.35f;
    [SerializeField] private float followUpHitForwardOffset = 0.55f;
    [SerializeField] private bool followUpCanHitSameEnemyMultipleTimes = true;
    [SerializeField] private float[] followUpHitTimes = { 0.5f, 1.35f, 2.25f };

    private readonly List<Enemy> frozenEnemies = new List<Enemy>();
    private readonly HashSet<EnemyStats> followUpHitTargets = new HashSet<EnemyStats>();
    private Coroutine timeStopRoutine;
    private float bonusTimeStopDuration;
    private float followUpExpiresAt;
    private bool followUpQueued;

    public bool IsDodging { get; private set; }
    public bool IsFollowUpAttacking { get; private set; }
    public float DodgeDuration => dodgeDuration;
    public float DodgeSpeed => dodgeSpeed;
    public float FollowUpAttackDuration => followUpAttackDuration;
    public float FollowUpLungeDuration => followUpLungeDuration;
    public float FollowUpLungeSpeed => followUpLungeSpeed;
    public int FollowUpHitCount => followUpHitTimes != null ? followUpHitTimes.Length : 0;
    public int DodgeDirection { get; private set; } = -1;

    public bool TryStartFromAttackFrame(Transform attacker)
    {
        if (IsDodging)
            return true;

        EnsurePlayerReference();

        if (player == null)
            return false;

        if (!player.HasBufferedPreciseDodgeInput(inputBufferDuration))
            return false;

        if (!IsReady())
            return false;

        player.ConsumePreciseDodgeInput();
        StartCooldown();
        StartPreciseDodge(attacker);

        return true;
    }

    public void BeginDodgeState()
    {
        EnsurePlayerReference();
        IsDodging = true;
    }

    public void EndDodgeState()
    {
        IsDodging = false;
    }

    public void AddTimeStopDuration(float extraDuration)
    {
        bonusTimeStopDuration += Mathf.Max(0, extraDuration);
    }

    public void UnlockFollowUpAttack()
    {
        followUpUnlocked = true;
    }

    public bool TryUseFollowUpAttack()
    {
        EnsurePlayerReference();

        if (!followUpUnlocked || player == null || followUpExpiresAt <= 0 || Time.time > followUpExpiresAt)
            return false;

        if (player.stateMachine.currentState == player.preciseDodgeAttackState)
            return false;

        if (player.stateMachine.currentState == player.preciseDodgeState)
        {
            followUpQueued = true;
            return true;
        }

        return StartFollowUpAttack();
    }

    public bool TryStartQueuedFollowUpAttack()
    {
        if (!followUpQueued)
            return false;

        followUpQueued = false;

        if (!followUpUnlocked || player == null || followUpExpiresAt <= 0 || Time.time > followUpExpiresAt)
            return false;

        return StartFollowUpAttack();
    }

    public void CancelFollowUpAttack()
    {
        followUpQueued = false;
        followUpExpiresAt = 0;
        EndFollowUpAttack();
    }

    private bool StartFollowUpAttack()
    {
        followUpQueued = false;
        followUpExpiresAt = 0;
        player.stateMachine.ChangeState(player.preciseDodgeAttackState);
        return true;
    }

    public float GetFollowUpHitTime(int index)
    {
        if (followUpHitTimes == null || followUpHitTimes.Length == 0)
            return 0;

        return followUpHitTimes[Mathf.Clamp(index, 0, followUpHitTimes.Length - 1)];
    }

    public void BeginFollowUpAttack()
    {
        EnsurePlayerReference();
        IsFollowUpAttacking = true;
        followUpHitTargets.Clear();
    }

    public void EndFollowUpAttack()
    {
        IsFollowUpAttacking = false;
        followUpHitTargets.Clear();
    }

    public void DealFollowUpDamage()
    {
        EnsurePlayerReference();

        if (!IsFollowUpAttacking || player == null || player.attackCheak == null)
            return;

        Vector2 center = player.attackCheak.position;
        center += Vector2.right * player.facingDir * followUpHitForwardOffset;

        float radius = Mathf.Max(0.1f, player.attackCheakRidus * followUpHitRadiusMultiplier);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(center, radius);
        HashSet<EnemyStats> hitThisPulse = new HashSet<EnemyStats>();

        foreach (Collider2D hit in colliders)
        {
            EnemyStats target = hit.GetComponentInParent<EnemyStats>();

            if (target == null || !hitThisPulse.Add(target))
                continue;

            if (!followUpCanHitSameEnemyMultipleTimes && !followUpHitTargets.Add(target))
                continue;

            player.stats.DoDamage(target);

            ItemData_Equipment weaponData = Inventory.instance != null ? Inventory.instance.GetEquipment(EquipmentType.Weapon) : null;

            if (weaponData != null)
                weaponData.Effect(target.transform);
        }
    }

    private void StartPreciseDodge(Transform attacker)
    {
        float totalTimeStopDuration = timeStopDuration + bonusTimeStopDuration;

        FaceAttackerAndSetDodgeDirection(attacker);

        IsDodging = true;
        followUpQueued = false;
        followUpExpiresAt = Time.time + totalTimeStopDuration + followUpWindow;

        player.MarkPreciseDodgeStarted();
        player.stateMachine.ChangeState(player.preciseDodgeState);

        StartEnemyTimeStop(attacker, totalTimeStopDuration);
    }

    private void FaceAttackerAndSetDodgeDirection(Transform attacker)
    {
        DodgeDirection = -player.facingDir;

        if (attacker == null)
            return;

        float directionToAttacker = attacker.position.x - player.transform.position.x;

        if (Mathf.Abs(directionToAttacker) < 0.05f)
            return;

        int desiredFacingDir = directionToAttacker > 0 ? 1 : -1;

        if (player.facingDir != desiredFacingDir)
            player.Flip();

        DodgeDirection = -desiredFacingDir;
    }

    private void StartEnemyTimeStop(Transform attacker, float duration)
    {
        if (timeStopRoutine != null)
        {
            StopCoroutine(timeStopRoutine);
            ReleaseFrozenEnemies();
        }

        Enemy dodgedEnemy = FindDodgedEnemy(attacker);

        if (dodgedEnemy == null)
            return;

        timeStopRoutine = StartCoroutine(EnemyTimeStopCoroutine(dodgedEnemy, duration));
    }

    private IEnumerator EnemyTimeStopCoroutine(Enemy dodgedEnemy, float duration)
    {
        frozenEnemies.Clear();

        dodgedEnemy.FreezeTime(true);
        frozenEnemies.Add(dodgedEnemy);

        yield return new WaitForSeconds(duration);

        ReleaseFrozenEnemies();
        timeStopRoutine = null;
    }

    private void ReleaseFrozenEnemies()
    {
        foreach (Enemy enemy in frozenEnemies)
        {
            if (enemy != null)
                enemy.FreezeTime(false);
        }

        frozenEnemies.Clear();
    }

    private Enemy FindDodgedEnemy(Transform attacker)
    {
        if (attacker == null)
            return null;

        Enemy enemy = attacker.GetComponentInParent<Enemy>();

        if (enemy != null)
            return enemy;

        return attacker.GetComponentInChildren<Enemy>();
    }

    private void EnsurePlayerReference()
    {
        if (player != null)
            return;

        if (PlayerManager.instance != null)
            player = PlayerManager.instance.player;

        if (player == null)
            player = FindObjectOfType<Player>();
    }
}
