using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class AwakeningAuraPrefabSetup
{
    private const string AuraPrefabPath = "Assets/Prefabs/Skills/AwakeningAuraEffect.prefab";

    static AwakeningAuraPrefabSetup()
    {
        EditorApplication.delayCall += EnsureAuraPrefabAndBindings;
    }

    [MenuItem("Tools/技能/刷新觉醒光环预制体")]
    private static void RefreshAuraPrefabAndBindings()
    {
        EnsureAuraPrefabAndBindings();
        AssetDatabase.Refresh();
        Debug.Log("觉醒光环预制体已刷新。");
    }

    private static void EnsureAuraPrefabAndBindings()
    {
        AwakeningAuraEffect auraPrefab = EnsureAuraPrefab();

        if (auraPrefab == null)
            return;

        BindLoadedAwakeningSkills(auraPrefab);
    }

    private static AwakeningAuraEffect EnsureAuraPrefab()
    {
        AwakeningAuraEffect auraPrefab = AssetDatabase.LoadAssetAtPath<AwakeningAuraEffect>(AuraPrefabPath);

        if (auraPrefab != null)
            return auraPrefab;

        GameObject auraObject = new GameObject("AwakeningAuraEffect");
        auraPrefab = auraObject.AddComponent<AwakeningAuraEffect>();

        PrefabUtility.SaveAsPrefabAsset(auraObject, AuraPrefabPath);
        Object.DestroyImmediate(auraObject);
        AssetDatabase.ImportAsset(AuraPrefabPath);

        return AssetDatabase.LoadAssetAtPath<AwakeningAuraEffect>(AuraPrefabPath);
    }

    private static void BindLoadedAwakeningSkills(AwakeningAuraEffect auraPrefab)
    {
        Awakening_Skill[] awakeningSkills = Resources.FindObjectsOfTypeAll<Awakening_Skill>();

        for (int i = 0; i < awakeningSkills.Length; i++)
        {
            Awakening_Skill skill = awakeningSkills[i];

            if (skill == null || EditorUtility.IsPersistent(skill))
                continue;

            SerializedObject serializedSkill = new SerializedObject(skill);
            SerializedProperty auraProperty = serializedSkill.FindProperty("auraPrefab");

            if (auraProperty == null || auraProperty.objectReferenceValue != null)
                continue;

            auraProperty.objectReferenceValue = auraPrefab;
            serializedSkill.ApplyModifiedPropertiesWithoutUndo();
            MarkSceneDirty(skill.gameObject.scene);
        }
    }

    private static void MarkSceneDirty(Scene scene)
    {
        if (scene.IsValid() && scene.isLoaded)
            EditorSceneManager.MarkSceneDirty(scene);
    }
}
