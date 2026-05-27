using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Awakening_Skill : Skill
{
    [Header("Awakening")]
    [SerializeField, Min(0.1f)] private float duration = 12f;
    [SerializeField, Range(0f, 1f)] private float statBonusPercent = 0.2f;

    [Header("Aura")]
    [SerializeField] private AwakeningAuraEffect auraPrefab;
    [SerializeField, Min(1)] private int auraPixelWidth = 1;
    [SerializeField] private Color auraInnerGold = new Color(1f, 0.9f, 0.25f, 1f);
    [SerializeField] private Color auraOuterGold = new Color(1f, 0.55f, 0.06f, 0.9f);

    [Header("Awakened Finisher")]
    [SerializeField, Min(0f)] private float finisherMotionStartTime = 0.16f;
    [SerializeField, Min(0.05f)] private float finisherMotionDuration = 0.68f;
    [SerializeField] private float finisherForwardDistance = 2.2f;

    private readonly List<AppliedStatModifier> appliedModifiers = new List<AppliedStatModifier>();
    private Coroutine awakeningRoutine;
    private AwakeningAuraEffect activeAura;

    public bool IsAwakened { get; private set; }
    public float Duration => duration;
    public float StatBonusPercent => statBonusPercent;
    public float FinisherMotionStartTime => finisherMotionStartTime;
    public float FinisherMotionDuration => finisherMotionDuration;
    public float FinisherForwardDistance => finisherForwardDistance;

    private struct AppliedStatModifier
    {
        public Stat stat;
        public int modifier;
    }

    private void Reset()
    {
        cooldown = 25f;
    }

    protected override void Start()
    {
        base.Start();

        if (cooldown <= 0)
            cooldown = 25f;
    }

    public bool TryActivate()
    {
        EnsurePlayerReference();

        if (player == null || IsAwakened)
            return false;

        if (!IsReady())
        {
            Debug.Log("Awakening skill is cooling down.");
            return false;
        }

        awakeningRoutine = StartCoroutine(AwakeningRoutine());
        return true;
    }

    public void CancelAwakening()
    {
        if (awakeningRoutine != null)
            StopCoroutine(awakeningRoutine);

        awakeningRoutine = null;
        EndAwakening();
    }

    private IEnumerator AwakeningRoutine()
    {
        BeginAwakening();

        yield return new WaitForSeconds(duration);

        awakeningRoutine = null;
        EndAwakening();
    }

    private void BeginAwakening()
    {
        IsAwakened = true;

        player.primaryAttack?.ResetCombo();

        ApplyStatBonuses();
        SpawnAura();
    }

    private void EndAwakening()
    {
        if (!IsAwakened)
            return;

        IsAwakened = false;
        player.primaryAttack?.ResetCombo();

        RemoveStatBonuses();
        ClearAura();
        StartCooldown();
    }

    private void ApplyStatBonuses()
    {
        appliedModifiers.Clear();

        if (player == null || player.stats == null)
            return;

        int previousMaxHealth = player.stats.GetMaxHealthValue();

        foreach (StatType statType in System.Enum.GetValues(typeof(StatType)))
        {
            Stat stat = player.stats.GetStat(statType);

            if (stat == null)
                continue;

            int currentValue = stat.GetValue();

            if (currentValue <= 0)
                continue;

            int modifier = Mathf.Max(1, Mathf.RoundToInt(currentValue * statBonusPercent));
            stat.AddModifier(modifier);
            appliedModifiers.Add(new AppliedStatModifier { stat = stat, modifier = modifier });
        }

        int newMaxHealth = player.stats.GetMaxHealthValue();
        int bonusHealth = Mathf.Max(0, newMaxHealth - previousMaxHealth);

        if (bonusHealth > 0)
            player.stats.IncreaseHealthBy(bonusHealth);
    }

    private void RemoveStatBonuses()
    {
        if (player == null || player.stats == null)
            return;

        foreach (AppliedStatModifier appliedModifier in appliedModifiers)
        {
            if (appliedModifier.stat != null)
                appliedModifier.stat.RemoveModifier(appliedModifier.modifier);
        }

        appliedModifiers.Clear();

        player.stats.currentHealth = Mathf.Min(player.stats.currentHealth, player.stats.GetMaxHealthValue());
        player.stats.onHealthChanged?.Invoke();
    }

    private void SpawnAura()
    {
        ClearAura();

        if (player == null)
            return;

        SpriteRenderer playerRenderer = GetPlayerRenderer();

        activeAura = auraPrefab != null
            ? Instantiate(auraPrefab)
            : new GameObject("Awakening Pixel Aura").AddComponent<AwakeningAuraEffect>();

        if (playerRenderer != null)
        {
            activeAura.AttachTo(playerRenderer);
        }
        else
        {
            activeAura.transform.SetParent(player.transform, false);
            activeAura.transform.localPosition = Vector3.zero;
            activeAura.transform.localRotation = Quaternion.identity;
            activeAura.transform.localScale = Vector3.one;
        }

        activeAura.FollowSortingOf(playerRenderer, -1);
        activeAura.Configure(auraPixelWidth, auraInnerGold, auraOuterGold);
    }

    private void ClearAura()
    {
        if (activeAura == null)
            return;

        Destroy(activeAura.gameObject);
        activeAura = null;
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

    private SpriteRenderer GetPlayerRenderer()
    {
        if (player == null)
            return null;

        if (player.sr != null)
            return player.sr;

        return player.GetComponentInChildren<SpriteRenderer>();
    }
}
