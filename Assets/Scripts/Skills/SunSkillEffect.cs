using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
public class SunSkillEffect : MonoBehaviour
{
    [SerializeField, Min(0.05f)] private float defaultTargetDiameter = 8f;
    [SerializeField, Min(0.05f)] private float growDuration = 0.75f;
    [SerializeField, Range(0.01f, 1f)] private float startScalePercent = 0.1f;
    [SerializeField] private AnimationCurve growCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField, Min(0f)] private float lifeTime = 5f;
    [SerializeField] private bool destroyAfterLifeTime = true;

    [Header("Gravity")]
    [SerializeField] private bool pullWholeScreen = true;
    [SerializeField, Min(0f)] private float screenPullPadding = 2f;
    [SerializeField, Min(0.1f)] private float fallbackPullRadius = 30f;
    [SerializeField, Min(0.1f)] private float pullRadiusMultiplier = 0.8f;
    [SerializeField, Min(0.1f)] private float pullSpeed = 8f;
    [SerializeField, Min(0.01f)] private float captureRadius = 0.2f;
    [SerializeField] private LayerMask enemyLayers = ~0;

    [Header("Damage")]
    [SerializeField, Min(1)] private int damagePerTick = 8;
    [SerializeField, Min(0.05f)] private float damageInterval = 0.4f;

    private SpriteRenderer spriteRenderer;
    private Vector3 startScale;
    private Vector3 targetScale;
    private float growTimer;
    private float lifeTimer;
    private float activeTargetDiameter;
    private float currentPullRadius;
    private bool isPlaying;
    private readonly Dictionary<Enemy, AffectedEnemy> affectedEnemies = new Dictionary<Enemy, AffectedEnemy>();
    private readonly List<Enemy> enemiesToRemove = new List<Enemy>();

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        Play(defaultTargetDiameter, growDuration, lifeTime);
    }

    public void Play(float targetDiameter, float duration, float activeLifeTime)
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        float spriteDiameter = GetSpriteDiameter();
        float scale = Mathf.Max(targetDiameter, 0.1f) / spriteDiameter;
        targetScale = Vector3.one * scale;
        startScale = targetScale * startScalePercent;

        growDuration = Mathf.Max(duration, 0.05f);
        lifeTime = Mathf.Max(activeLifeTime, 0f);
        activeTargetDiameter = Mathf.Max(targetDiameter, 0.1f);
        currentPullRadius = CalculatePullRadius();
        growTimer = 0f;
        lifeTimer = 0f;
        isPlaying = true;
        transform.localScale = startScale;
    }

    private void Update()
    {
        if (!isPlaying)
            return;

        growTimer += Time.deltaTime;
        lifeTimer += Time.deltaTime;

        float progress = Mathf.Clamp01(growTimer / growDuration);
        float curvedProgress = growCurve != null ? growCurve.Evaluate(progress) : progress;
        transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, curvedProgress);
        currentPullRadius = CalculatePullRadius();

        PullAndDamageEnemies();

        if (destroyAfterLifeTime && lifeTime > 0f && lifeTimer >= lifeTime)
            Destroy(gameObject);
    }

    private void OnDisable()
    {
        ReleaseAffectedEnemies();
    }

    private void PullAndDamageEnemies()
    {
        RegisterEnemiesInRange();
        UpdateAffectedEnemies();
    }

    private void RegisterEnemiesInRange()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, currentPullRadius, enemyLayers);

        foreach (Collider2D hit in hits)
        {
            Enemy enemy = hit.GetComponentInParent<Enemy>();
            EnemyStats enemyStats = hit.GetComponentInParent<EnemyStats>();

            if (enemy == null || enemyStats == null || enemyStats.isDead || affectedEnemies.ContainsKey(enemy))
                continue;

            enemy.FreezeTime(true);
            affectedEnemies.Add(enemy, new AffectedEnemy(enemyStats));
        }
    }

    private void UpdateAffectedEnemies()
    {
        enemiesToRemove.Clear();

        foreach (KeyValuePair<Enemy, AffectedEnemy> pair in affectedEnemies)
        {
            Enemy enemy = pair.Key;
            AffectedEnemy affectedEnemy = pair.Value;

            if (enemy == null || affectedEnemy.Stats == null || affectedEnemy.Stats.isDead)
            {
                enemiesToRemove.Add(enemy);
                continue;
            }

            enemy.FreezeTime(true);
            DamageEnemyOverTime(affectedEnemy);

            if (affectedEnemy.Stats.isDead)
            {
                enemiesToRemove.Add(enemy);
                continue;
            }

            PullEnemy(enemy);
        }

        foreach (Enemy enemy in enemiesToRemove)
            RemoveAffectedEnemy(enemy);
    }

    private void PullEnemy(Enemy enemy)
    {
        if (enemy.rb == null)
            return;

        Vector2 currentPosition = enemy.rb.position;
        Vector2 targetPosition = transform.position;
        float distance = Vector2.Distance(currentPosition, targetPosition);

        if (distance <= captureRadius)
        {
            enemy.rb.velocity = Vector2.zero;
            return;
        }

        Vector2 nextPosition = Vector2.MoveTowards(currentPosition, targetPosition, pullSpeed * Time.deltaTime);
        enemy.rb.velocity = Vector2.zero;
        enemy.transform.position = new Vector3(nextPosition.x, nextPosition.y, enemy.transform.position.z);
    }

    private void DamageEnemyOverTime(AffectedEnemy affectedEnemy)
    {
        affectedEnemy.DamageTimer -= Time.deltaTime;

        if (affectedEnemy.DamageTimer > 0f)
            return;

        affectedEnemy.Stats.TakeDamage(damagePerTick);
        affectedEnemy.DamageTimer = damageInterval;
    }

    private void RemoveAffectedEnemy(Enemy enemy)
    {
        if (enemy != null)
        {
            enemy.FreezeTime(false);

            if (enemy.rb != null)
                enemy.rb.velocity = Vector2.zero;
        }

        affectedEnemies.Remove(enemy);
    }

    private void ReleaseAffectedEnemies()
    {
        foreach (Enemy enemy in affectedEnemies.Keys)
        {
            if (enemy != null)
            {
                enemy.FreezeTime(false);

                if (enemy.rb != null)
                    enemy.rb.velocity = Vector2.zero;
            }
        }

        affectedEnemies.Clear();
    }

    private float GetSpriteDiameter()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null)
            return 1f;

        Vector3 spriteSize = spriteRenderer.sprite.bounds.size;
        return Mathf.Max(spriteSize.x, spriteSize.y, 0.1f);
    }

    private float CalculatePullRadius()
    {
        if (!pullWholeScreen)
            return Mathf.Max(activeTargetDiameter * pullRadiusMultiplier, activeTargetDiameter * 0.5f);

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
            return fallbackPullRadius;

        float zDistance = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
        float pullRadius = 0f;
        Vector3 sunPosition = transform.position;

        pullRadius = Mathf.Max(pullRadius, Vector2.Distance(sunPosition, mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, zDistance))));
        pullRadius = Mathf.Max(pullRadius, Vector2.Distance(sunPosition, mainCamera.ViewportToWorldPoint(new Vector3(0f, 1f, zDistance))));
        pullRadius = Mathf.Max(pullRadius, Vector2.Distance(sunPosition, mainCamera.ViewportToWorldPoint(new Vector3(1f, 0f, zDistance))));
        pullRadius = Mathf.Max(pullRadius, Vector2.Distance(sunPosition, mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, zDistance))));

        return pullRadius + screenPullPadding;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.65f, 0.1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, currentPullRadius);
    }

    private class AffectedEnemy
    {
        public readonly EnemyStats Stats;
        public float DamageTimer;

        public AffectedEnemy(EnemyStats stats)
        {
            Stats = stats;
            DamageTimer = 0f;
        }
    }
}
