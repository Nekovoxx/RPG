using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[Flags]
public enum LargeSkullNpcFunction
{
    None = 0,
    Dialogue = 1,
    Save = 2,
    Upgrade = 4
}

public class LargeSkullNpcInteractable : Interactable
{
    [Header("出现/消失")]
    [SerializeField, Min(0.1f)] private float revealRange = 5f;
    [SerializeField, Min(0.1f)] private float hideRange = 6.5f;
    [SerializeField, Min(0f)] private float entranceDuration = 0.8f;
    [SerializeField, Min(0f)] private float disappearDuration = 0.8f;
    [SerializeField, Min(0.1f)] private float reverseEntranceSpeedMultiplier = 1.15f;
    [SerializeField] private string entranceAnimation = "death_reverse";
    [SerializeField] private string idleAnimation = "Idle/Move";
    [SerializeField] private string disappearAnimation = "death";
    [SerializeField] private bool startHidden = true;

    [Header("NPC 功能")]
    [SerializeField] private string npcName = "无名骸骨";
    [SerializeField] private LargeSkullNpcFunction enabledFunctions = LargeSkullNpcFunction.Dialogue;
    [SerializeField] private string dialogueOptionText = "对话";
    [SerializeField] private string upgradeOptionText = "祭礼";
    [SerializeField] private string saveOptionText = "保存";
    [SerializeField, TextArea(2, 6)] private string dialogueText = "……";
    [SerializeField] private bool showDialoguePanel = true;
    [SerializeField] private bool showFunctionFeedbackPanel = true;
    [SerializeField, Min(0f)] private float dialogueDisplaySeconds = 4f;
    [SerializeField] private string saveCompleteMessage = "已保存。";
    [SerializeField] private string upgradePendingMessage = "祭礼界面尚未连接。";
    [SerializeField] private bool savePlayerPosition = true;

    [Header("事件")]
    [SerializeField] private UnityEvent onDialogue;
    [SerializeField] private UnityEvent onSave;
    [SerializeField] private UnityEvent onUpgrade;

    private Animator animator;
    private Renderer[] renderers;
    private Player player;
    private Coroutine visibilityRoutine;
    private bool visible;
    private bool readyToInteract;
    private bool changingVisibility;

    public override string InteractionPrompt => "按 E 对话";

    public override bool CanInteract(Player interactingPlayer)
    {
        return base.CanInteract(interactingPlayer) && visible && readyToInteract;
    }

    protected override void Awake()
    {
        base.Awake();

        animator = GetComponentInChildren<Animator>(true);
        renderers = GetComponentsInChildren<Renderer>(true);

        if (hideRange < revealRange)
            hideRange = revealRange + 0.5f;

        if (startHidden)
        {
            visible = false;
            readyToInteract = false;
            SetVisualsVisible(false);
        }
        else
        {
            visible = true;
            readyToInteract = true;
            SetVisualsVisible(true);
            PlayIdleAnimation();
        }
    }

    private void Update()
    {
        Player currentPlayer = ResolvePlayer();

        if (currentPlayer == null)
            return;

        float sqrDistance = ((Vector2)currentPlayer.transform.position - (Vector2)transform.position).sqrMagnitude;

        if (!changingVisibility && !visible && sqrDistance <= revealRange * revealRange)
        {
            ShowNpc();
        }
        else if (!changingVisibility && visible && sqrDistance >= hideRange * hideRange)
        {
            HideNpc();
        }
    }

    protected override void OnInteract(Player interactingPlayer)
    {
        List<UI_NpcInteractionMenu.MenuOption> options = BuildMenuOptions(interactingPlayer);

        if (options.Count <= 0)
            return;

        UI_NpcInteractionMenu.GetOrCreate().Show(npcName, options);
    }

    private void ShowNpc()
    {
        if (visibilityRoutine != null)
            StopCoroutine(visibilityRoutine);

        visibilityRoutine = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        changingVisibility = true;
        visible = true;
        readyToInteract = false;
        SetVisualsVisible(true);

        if (IsReversedEntranceAnimation(entranceAnimation))
        {
            yield return PlayAnimationReversed(disappearAnimation, disappearDuration);
        }
        else
        {
            PlayAnimation(entranceAnimation);

            float waitTime = GetAnimationLength(entranceAnimation);

            if (waitTime <= 0f)
                waitTime = entranceDuration;

            if (waitTime > 0f)
                yield return new WaitForSeconds(waitTime);
        }

        PlayIdleAnimation();
        readyToInteract = true;
        changingVisibility = false;
        visibilityRoutine = null;
    }

    private void HideNpc()
    {
        if (visibilityRoutine != null)
            StopCoroutine(visibilityRoutine);

        UI_NpcInteractionMenu.Instance?.Hide();
        UI_NpcDialoguePanel.Instance?.Hide();
        visibilityRoutine = StartCoroutine(HideRoutine());
    }

    private List<UI_NpcInteractionMenu.MenuOption> BuildMenuOptions(Player interactingPlayer)
    {
        List<UI_NpcInteractionMenu.MenuOption> options = new List<UI_NpcInteractionMenu.MenuOption>();

        if (enabledFunctions.HasFlag(LargeSkullNpcFunction.Dialogue))
            options.Add(new UI_NpcInteractionMenu.MenuOption(dialogueOptionText, StartDialogue));

        if (enabledFunctions.HasFlag(LargeSkullNpcFunction.Upgrade))
            options.Add(new UI_NpcInteractionMenu.MenuOption(upgradeOptionText, StartUpgrade));

        if (enabledFunctions.HasFlag(LargeSkullNpcFunction.Save))
            options.Add(new UI_NpcInteractionMenu.MenuOption(saveOptionText, () => SaveAtNpc(interactingPlayer)));

        return options;
    }

    private IEnumerator HideRoutine()
    {
        changingVisibility = true;
        readyToInteract = false;
        visible = true;
        SetVisualsVisible(true);
        PlayAnimation(disappearAnimation);

        float waitTime = GetAnimationLength(disappearAnimation);

        if (waitTime <= 0f)
            waitTime = disappearDuration;

        if (waitTime > 0f)
            yield return new WaitForSeconds(waitTime);

        visible = false;
        SetVisualsVisible(false);
        FreezeAnimator();
        changingVisibility = false;
        visibilityRoutine = null;
    }

    private void StartDialogue()
    {
        if (showDialoguePanel)
            UI_NpcDialoguePanel.GetOrCreate().Show(npcName, dialogueText, dialogueDisplaySeconds);

        if (!string.IsNullOrWhiteSpace(dialogueText))
            Debug.Log(npcName + ": " + dialogueText, this);

        onDialogue?.Invoke();
    }

    private void SaveAtNpc(Player interactingPlayer)
    {
        if (savePlayerPosition && interactingPlayer != null)
        {
            Vector3 position = interactingPlayer.transform.position;
            PlayerPrefs.SetString("LastSaveScene", SceneManager.GetActiveScene().name);
            PlayerPrefs.SetFloat("LastSavePositionX", position.x);
            PlayerPrefs.SetFloat("LastSavePositionY", position.y);
            PlayerPrefs.SetFloat("LastSavePositionZ", position.z);

            if (interactingPlayer.stats != null)
                PlayerPrefs.SetInt("LastSaveHealth", interactingPlayer.stats.currentHealth);

            PlayerPrefs.Save();
        }

        Debug.Log(npcName + " 已保存。", this);
        ShowNpcMessage(saveCompleteMessage);
        onSave?.Invoke();
    }

    private void StartUpgrade()
    {
        UI ui = FindObjectOfType<UI>(true);

        if (ui != null)
        {
            ui.OpenLevelUp();
        }
        else if (!string.IsNullOrWhiteSpace(upgradePendingMessage))
        {
            Debug.LogWarning(npcName + ": " + upgradePendingMessage, this);
            ShowNpcMessage(upgradePendingMessage);
        }

        onUpgrade?.Invoke();
    }

    private void ShowNpcMessage(string message)
    {
        if (!showFunctionFeedbackPanel || string.IsNullOrWhiteSpace(message))
            return;

        UI_NpcDialoguePanel.GetOrCreate().Show(npcName, message, dialogueDisplaySeconds);
    }

    private Player ResolvePlayer()
    {
        if (player != null)
            return player;

        if (PlayerManager.instance != null)
            player = PlayerManager.instance.player;

        if (player == null)
            player = FindObjectOfType<Player>();

        return player;
    }

    private void SetVisualsVisible(bool value)
    {
        if (renderers == null)
            return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                renderers[i].enabled = value;
        }
    }

    private IEnumerator PlayAnimationReversed(string stateName, float targetDuration)
    {
        if (animator == null || string.IsNullOrWhiteSpace(stateName))
            yield break;

        float duration = targetDuration;

        if (duration <= 0f)
            duration = GetAnimationLength(stateName);

        if (duration <= 0f)
        {
            PlayAnimation(stateName, 0f);
            yield break;
        }

        float elapsedTime = 0f;
        animator.speed = 0f;

        while (elapsedTime < duration)
        {
            float normalizedTime = Mathf.Clamp01(1f - elapsedTime / duration);
            PlayAnimation(stateName, normalizedTime, false);
            animator.Update(0f);

            elapsedTime += Time.deltaTime * reverseEntranceSpeedMultiplier;
            yield return null;
        }

        PlayAnimation(stateName, 0f, false);
        animator.Update(0f);
    }

    private bool PlayAnimation(string stateName, float normalizedTime = 0f, bool resetAnimatorSpeed = true)
    {
        if (animator == null || string.IsNullOrWhiteSpace(stateName))
            return false;

        if (resetAnimatorSpeed)
            animator.speed = 1f;

        if (TryPlayAnimationState(stateName, normalizedTime))
            return true;

        string titleCaseState = char.ToUpperInvariant(stateName[0]) + stateName.Substring(1);

        if (TryPlayAnimationState(titleCaseState, normalizedTime))
            return true;

        string safeStateName = GetSafeAnimationStateName(stateName);

        if (!string.IsNullOrWhiteSpace(safeStateName) && TryPlayAnimationState(safeStateName, normalizedTime))
            return true;

        string matchingClipName = FindAnimationClipName(stateName);

        if (!string.IsNullOrWhiteSpace(matchingClipName) && TryPlayAnimationState(matchingClipName, normalizedTime))
            return true;

        if (!string.IsNullOrWhiteSpace(matchingClipName) && TryPlayAnimationState(GetSafeAnimationStateName(matchingClipName), normalizedTime))
            return true;

        return false;
    }

    private bool TryPlayAnimationState(string stateName, float normalizedTime)
    {
        if (animator == null || string.IsNullOrWhiteSpace(stateName))
            return false;

        if (animator.HasState(0, Animator.StringToHash(stateName)))
        {
            animator.Play(stateName, 0, normalizedTime);
            return true;
        }

        string baseLayerStateName = "Base Layer." + stateName;

        if (animator.HasState(0, Animator.StringToHash(baseLayerStateName)))
        {
            animator.Play(baseLayerStateName, 0, normalizedTime);
            return true;
        }

        return false;
    }

    private string FindAnimationClipName(string stateName)
    {
        if (animator == null || animator.runtimeAnimatorController == null || string.IsNullOrWhiteSpace(stateName))
            return null;

        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;

        if (clips == null)
            return null;

        string normalizedRequested = NormalizeAnimationName(stateName);

        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i] == null)
                continue;

            if (NormalizeAnimationName(clips[i].name) == normalizedRequested)
                return clips[i].name;
        }

        if (normalizedRequested.Contains("idle") || normalizedRequested.Contains("move"))
        {
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i] == null)
                    continue;

                string normalizedClipName = NormalizeAnimationName(clips[i].name);

                if (normalizedClipName.Contains("idle") && normalizedClipName.Contains("move"))
                    return clips[i].name;
            }
        }

        return null;
    }

    private float GetAnimationLength(string stateName)
    {
        if (animator == null || animator.runtimeAnimatorController == null || string.IsNullOrWhiteSpace(stateName))
            return 0f;

        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;

        if (clips == null)
            return 0f;

        string normalizedRequested = NormalizeAnimationName(stateName);

        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i] == null)
                continue;

            if (NormalizeAnimationName(clips[i].name) == normalizedRequested)
                return clips[i].length;
        }

        return 0f;
    }

    private static bool IsReversedEntranceAnimation(string stateName)
    {
        return NormalizeAnimationName(stateName).Contains("deathreverse");
    }

    private static string NormalizeAnimationName(string animationName)
    {
        return animationName.Replace("/", string.Empty)
            .Replace("_", string.Empty)
            .Replace(" ", string.Empty)
            .ToLowerInvariant();
    }

    private static string GetSafeAnimationStateName(string animationName)
    {
        return animationName.Replace("/", string.Empty)
            .Replace("_", string.Empty)
            .Replace(" ", string.Empty);
    }

    private void PlayIdleAnimation()
    {
        if (!PlayAnimation(idleAnimation))
            FreezeAnimator();
    }

    private void FreezeAnimator()
    {
        if (animator != null)
            animator.speed = 0f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.75f, 0.1f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, revealRange);

        Gizmos.color = new Color(0.8f, 0.15f, 0.05f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, hideRange);
    }
}
