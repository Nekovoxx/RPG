using System;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

[InitializeOnLoad]
public static class LargeSkullNpcPrefabSetup
{
    private const string SourcePrefabPath = "Assets/Prefabs/Large skull.ase";
    private const string TargetPrefabPath = "Assets/Prefabs/Interactables/LargeSkull_NPC.prefab";
    private const string TargetAnimatorControllerPath = "Assets/Prefabs/Interactables/LargeSkull_NPC.controller";
    private const string HorrorUiSpriteSheetPath = "Assets/Graphics/UI-background/FreeHorrorUi.png";
    private const string NpcUiStylePath = "Assets/Resources/NpcUiStyle.asset";

    static LargeSkullNpcPrefabSetup()
    {
        EditorApplication.delayCall += EnsureNpcPrefab;
    }

    private static void EnsureNpcPrefab()
    {
        GameObject sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SourcePrefabPath);

        if (sourcePrefab == null)
            return;

        ConfigureAnimationLooping();
        EnsureNpcUiStyle();
        EnsureDirectory("Assets/Prefabs/Interactables");
        EnsureNpcAnimatorController();

        if (AssetDatabase.LoadAssetAtPath<GameObject>(TargetPrefabPath) == null)
        {
            CreateNpcPrefab(sourcePrefab);
            return;
        }

        UpdateNpcPrefab();
    }

    private static void CreateNpcPrefab(GameObject sourcePrefab)
    {
        GameObject instance = PrefabUtility.InstantiatePrefab(sourcePrefab) as GameObject;

        if (instance == null)
            instance = UnityEngine.Object.Instantiate(sourcePrefab);

        instance.name = "LargeSkull_NPC";
        EnsureNpcComponents(instance, true);

        PrefabUtility.SaveAsPrefabAsset(instance, TargetPrefabPath);
        UnityEngine.Object.DestroyImmediate(instance);
        AssetDatabase.ImportAsset(TargetPrefabPath);
    }

    private static void UpdateNpcPrefab()
    {
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(TargetPrefabPath);

        if (prefabRoot == null)
            return;

        EnsureNpcComponents(prefabRoot, false);
        PrefabUtility.SaveAsPrefabAsset(prefabRoot, TargetPrefabPath);
        PrefabUtility.UnloadPrefabContents(prefabRoot);
    }

    private static void EnsureNpcComponents(GameObject root, bool setDefaults)
    {
        if (root.GetComponent<LargeSkullNpcInteractable>() == null)
        {
            LargeSkullNpcInteractable npc = root.AddComponent<LargeSkullNpcInteractable>();

            if (setDefaults)
                ApplyDefaultNpcSettings(npc);
        }

        CircleCollider2D interactionCollider = root.GetComponent<CircleCollider2D>();
        bool addedCollider = interactionCollider == null;

        if (addedCollider)
            interactionCollider = root.AddComponent<CircleCollider2D>();

        if (setDefaults || addedCollider)
        {
            interactionCollider.isTrigger = true;
            interactionCollider.radius = 1.1f;
            interactionCollider.offset = new Vector2(0f, 1.4f);
        }

        Animator animator = root.GetComponentInChildren<Animator>(true);
        RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(TargetAnimatorControllerPath);

        if (animator != null && controller != null)
            animator.runtimeAnimatorController = controller;
    }

    private static void ConfigureAnimationLooping()
    {
        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(SourcePrefabPath);

        for (int i = 0; i < assets.Length; i++)
        {
            AnimationClip clip = assets[i] as AnimationClip;

            if (clip == null)
                continue;

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);

            if (IsIdleClipName(clip.name))
            {
                settings.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(clip, settings);
            }
            else if (clip.name.Equals("attack", StringComparison.OrdinalIgnoreCase) ||
                     clip.name.Equals("death", StringComparison.OrdinalIgnoreCase))
            {
                settings.loopTime = false;
                AnimationUtility.SetAnimationClipSettings(clip, settings);
            }
        }
    }

    private static void ApplyDefaultNpcSettings(LargeSkullNpcInteractable npc)
    {
        SerializedObject serializedNpc = new SerializedObject(npc);

        SetFloat(serializedNpc, "revealRange", 5f);
        SetFloat(serializedNpc, "hideRange", 6.5f);
        SetFloat(serializedNpc, "entranceDuration", 0.8f);
        SetFloat(serializedNpc, "disappearDuration", 0.8f);
        SetFloat(serializedNpc, "reverseEntranceSpeedMultiplier", 1.15f);
        SetString(serializedNpc, "entranceAnimation", "death_reverse");
        SetString(serializedNpc, "idleAnimation", "Idle/Move");
        SetString(serializedNpc, "disappearAnimation", "death");
        SetBool(serializedNpc, "startHidden", true);
        SetString(serializedNpc, "npcName", "巫女");
        SetInt(serializedNpc, "enabledFunctions", (int)(LargeSkullNpcFunction.Dialogue | LargeSkullNpcFunction.Upgrade | LargeSkullNpcFunction.Save));
        SetString(serializedNpc, "dialogueOptionText", "对话");
        SetString(serializedNpc, "upgradeOptionText", "祭礼");
        SetString(serializedNpc, "saveOptionText", "保存");
        SetString(serializedNpc, "dialogueText", "……");
        SetBool(serializedNpc, "showDialoguePanel", true);
        SetBool(serializedNpc, "showFunctionFeedbackPanel", true);
        SetFloat(serializedNpc, "dialogueDisplaySeconds", 4f);
        SetString(serializedNpc, "saveCompleteMessage", "已保存。");
        SetString(serializedNpc, "upgradePendingMessage", "祭礼界面尚未连接。");
        SetBool(serializedNpc, "savePlayerPosition", true);

        serializedNpc.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void EnsureNpcUiStyle()
    {
        EnsureDirectory("Assets/Resources");

        UI_NpcStyle style = AssetDatabase.LoadAssetAtPath<UI_NpcStyle>(NpcUiStylePath);

        if (style == null)
        {
            style = ScriptableObject.CreateInstance<UI_NpcStyle>();
            AssetDatabase.CreateAsset(style, NpcUiStylePath);
        }

        Sprite panelSprite = LoadSpriteByName(HorrorUiSpriteSheetPath, "FreeHorrorUi_59");
        Sprite buttonSprite = LoadSpriteByName(HorrorUiSpriteSheetPath, "FreeHorrorUi_88");

        SerializedObject serializedStyle = new SerializedObject(style);
        SetObjectReference(serializedStyle, "panelSprite", panelSprite);
        SetObjectReference(serializedStyle, "buttonSprite", buttonSprite);
        serializedStyle.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(style);
    }

    private static void EnsureNpcAnimatorController()
    {
        AnimationClip idleClip = LoadIdleAnimationClip();
        AnimationClip disappearClip = LoadAnimationClipByName("death");

        if (idleClip == null && disappearClip == null)
            return;

        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(TargetAnimatorControllerPath);

        if (controller == null)
            controller = AnimatorController.CreateAnimatorControllerAtPath(TargetAnimatorControllerPath);

        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        RemoveStateIfExists(stateMachine, "attack");
        RemoveStateIfExists(stateMachine, "death_reverse");
        AnimatorState idleState = EnsureState(stateMachine, "IdleMove", idleClip);
        AnimatorState disappearState = EnsureState(stateMachine, "death", disappearClip);

        if (idleState != null)
            stateMachine.defaultState = idleState;

        ClearTransitions(idleState);
        ClearTransitions(disappearState);

        AssetDatabase.SaveAssets();
    }

    private static AnimatorState EnsureState(AnimatorStateMachine stateMachine, string stateName, Motion motion, float speed = 1f)
    {
        if (stateMachine == null || motion == null)
            return null;

        AnimatorState state = FindState(stateMachine, stateName);

        if (state == null)
            state = stateMachine.AddState(stateName);

        state.motion = motion;
        state.speed = speed;
        state.writeDefaultValues = true;
        return state;
    }

    private static AnimatorState FindState(AnimatorStateMachine stateMachine, string stateName)
    {
        ChildAnimatorState[] states = stateMachine.states;

        for (int i = 0; i < states.Length; i++)
        {
            if (states[i].state != null && states[i].state.name == stateName)
                return states[i].state;
        }

        return null;
    }

    private static void ClearTransitions(AnimatorState state)
    {
        if (state == null)
            return;

        AnimatorStateTransition[] transitions = state.transitions;

        for (int i = transitions.Length - 1; i >= 0; i--)
            state.RemoveTransition(transitions[i]);
    }

    private static void RemoveStateIfExists(AnimatorStateMachine stateMachine, string stateName)
    {
        if (stateMachine == null)
            return;

        AnimatorState state = FindState(stateMachine, stateName);

        if (state != null)
            stateMachine.RemoveState(state);
    }

    private static AnimationClip LoadAnimationClipByName(string clipName)
    {
        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(SourcePrefabPath);

        for (int i = 0; i < assets.Length; i++)
        {
            AnimationClip clip = assets[i] as AnimationClip;

            if (clip != null && clip.name.Equals(clipName, StringComparison.OrdinalIgnoreCase))
                return clip;
        }

        return null;
    }

    private static AnimationClip LoadIdleAnimationClip()
    {
        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(SourcePrefabPath);

        for (int i = 0; i < assets.Length; i++)
        {
            AnimationClip clip = assets[i] as AnimationClip;

            if (clip != null && IsIdleClipName(clip.name))
                return clip;
        }

        return null;
    }

    private static Sprite LoadSpriteByName(string assetPath, string spriteName)
    {
        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);

        for (int i = 0; i < assets.Length; i++)
        {
            Sprite sprite = assets[i] as Sprite;

            if (sprite != null && sprite.name == spriteName)
                return sprite;
        }

        return null;
    }

    private static void SetFloat(SerializedObject serializedObject, string propertyName, float value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);

        if (property != null)
            property.floatValue = value;
    }

    private static void SetString(SerializedObject serializedObject, string propertyName, string value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);

        if (property != null)
            property.stringValue = value;
    }

    private static void SetBool(SerializedObject serializedObject, string propertyName, bool value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);

        if (property != null)
            property.boolValue = value;
    }

    private static void SetInt(SerializedObject serializedObject, string propertyName, int value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);

        if (property != null)
            property.intValue = value;
    }

    private static void SetObjectReference(SerializedObject serializedObject, string propertyName, UnityEngine.Object value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);

        if (property != null)
            property.objectReferenceValue = value;
    }

    private static bool IsIdleClipName(string clipName)
    {
        string normalizedName = NormalizeClipName(clipName);
        return normalizedName == "idle" || normalizedName == "idlemove" || normalizedName.Contains("idle");
    }

    private static string NormalizeClipName(string clipName)
    {
        return clipName.Replace("/", string.Empty)
            .Replace("_", string.Empty)
            .Replace(" ", string.Empty)
            .ToLowerInvariant();
    }

    private static void EnsureDirectory(string directory)
    {
        if (AssetDatabase.IsValidFolder(directory))
            return;

        string[] parts = directory.Split('/');
        string current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];

            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);

            current = next;
        }
    }
}
