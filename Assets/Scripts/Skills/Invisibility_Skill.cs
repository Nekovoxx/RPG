using System.Collections;
using UnityEngine;

public class Invisibility_Skill : Skill
{
    [Header("Invisibility")]
    [SerializeField, Min(0.1f)] private float duration = 4f;
    [SerializeField, Range(0.05f, 1f)] private float invisibleAlpha = 0.35f;

    private Coroutine invisibilityRoutine;
    private SpriteRenderer[] renderers;
    private Color[] originalColors;

    public bool IsInvisible { get; private set; }
    public float Duration => duration;

    private void Reset()
    {
        cooldown = 10f;
    }

    protected override void Start()
    {
        base.Start();

        if (cooldown <= 0)
            cooldown = 10f;
    }

    private void OnDisable()
    {
        if (IsInvisible)
            EndInvisibility();
    }

    public bool TryActivate()
    {
        EnsurePlayerReference();

        if (player == null || IsInvisible)
            return false;

        if (!IsReady())
            return false;

        StartCooldown();

        if (invisibilityRoutine != null)
            StopCoroutine(invisibilityRoutine);

        invisibilityRoutine = StartCoroutine(InvisibilityCoroutine());
        return true;
    }

    public void CancelInvisibility()
    {
        if (invisibilityRoutine != null)
            StopCoroutine(invisibilityRoutine);

        EndInvisibility();
    }

    private IEnumerator InvisibilityCoroutine()
    {
        BeginInvisibility();

        yield return new WaitForSeconds(duration);

        EndInvisibility();
    }

    private void BeginInvisibility()
    {
        IsInvisible = true;

        if (player != null)
            player.SetInvisible(true);

        CacheRenderers();
        SetRendererAlpha(invisibleAlpha);
    }

    private void EndInvisibility()
    {
        IsInvisible = false;
        invisibilityRoutine = null;

        if (player != null)
            player.SetInvisible(false);

        RestoreRendererColors();
    }

    private void CacheRenderers()
    {
        if (player == null)
            return;

        renderers = player.GetComponentsInChildren<SpriteRenderer>();
        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
            originalColors[i] = renderers[i].color;
    }

    private void SetRendererAlpha(float alpha)
    {
        if (renderers == null)
            return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null)
                continue;

            Color color = renderers[i].color;
            color.a = alpha;
            renderers[i].color = color;
        }
    }

    private void RestoreRendererColors()
    {
        if (renderers == null || originalColors == null)
            return;

        for (int i = 0; i < renderers.Length && i < originalColors.Length; i++)
        {
            if (renderers[i] != null)
                renderers[i].color = originalColors[i];
        }
    }

    private void EnsurePlayerReference()
    {
        if (player != null)
            return;

        if (PlayerManager.instance != null)
            player = PlayerManager.instance.player;
    }
}
