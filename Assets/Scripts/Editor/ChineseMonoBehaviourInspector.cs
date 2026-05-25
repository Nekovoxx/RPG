using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(MonoBehaviour), true)]
public class ChineseMonoBehaviourInspector : Editor
{
    private static readonly Dictionary<string, string> ExactLabels = new Dictionary<string, string>
    {
        { "m_Script", "脚本" },
        { "baseValue", "基础数值" },
        { "modifiers", "加成列表" },

        { "attackMovement", "攻击位移" },
        { "counterAttackDuration", "反击持续时间" },
        { "moveSpeed", "移动速度" },
        { "jumpForce", "跳跃力度" },
        { "swordReturnImpact", "飞剑返回冲击力" },
        { "doubleJumpForce", "二段跳力度" },
        { "extraJumpCount", "额外跳跃次数" },
        { "dashSpeed", "冲刺速度" },
        { "dashDuration", "冲刺持续时间" },
        { "sword", "当前飞剑" },
        { "canHeal", "可以治疗" },

        { "knockbackDirection", "击退方向" },
        { "knockbackDuration", "击退持续时间" },
        { "attackCheak", "攻击检测点" },
        { "attackCheakRidus", "攻击检测半径" },
        { "groundCheak", "地面检测点" },
        { "groundCheakDistance", "地面检测距离" },
        { "wallCheak", "墙体检测点" },
        { "wallCheakDistance", "墙体检测距离" },
        { "whatIsGround", "地面图层" },

        { "strength", "力量" },
        { "agility", "敏捷" },
        { "intelligence", "智慧" },
        { "vitality", "活力" },
        { "damage", "攻击力" },
        { "critChance", "暴击几率" },
        { "critPower", "暴击伤害" },
        { "maxHealth", "最大生命值" },
        { "armor", "护甲" },
        { "evasion", "闪避" },
        { "magicResistance", "魔法抗性" },
        { "fireDamage", "火焰伤害" },
        { "iceDamage", "冰霜伤害" },
        { "lightingDamage", "雷电伤害" },
        { "isIgnited", "燃烧状态" },
        { "isChilled", "冰冷状态" },
        { "isShocked", "感电状态" },
        { "ailmentsDuration", "异常状态持续时间" },
        { "shockStrikePrefab", "感电打击预制体" },
        { "currentHealth", "当前生命值" },

        { "flashDuration", "闪烁持续时间" },
        { "hitMat", "受击材质" },
        { "igniteColor", "燃烧颜色" },
        { "chillColor", "冰冷颜色" },
        { "shockColor", "感电颜色" },

        { "cooldown", "冷却时间" },
        { "dodgeDuration", "闪避持续时间" },
        { "dodgeSpeed", "闪避速度" },
        { "timeStopDuration", "时间停止持续时间" },
        { "inputBufferDuration", "输入缓冲时间" },
        { "followUpWindow", "追击窗口" },
        { "followUpUnlocked", "追击已解锁" },
        { "followUpAttackDuration", "追击持续时间" },
        { "followUpSegmentDurations", "追击每段持续时间" },
        { "followUpSegmentHitTimes", "追击每段命中时间" },
        { "followUpSegmentLungeDurations", "追击每段突进时间" },
        { "followUpSegmentLungeSpeeds", "追击每段突进速度" },
        { "followUpLungeDuration", "追击突进时间" },
        { "followUpLungeSpeed", "追击突进速度" },
        { "followUpHitRadiusMultiplier", "追击命中半径倍率" },
        { "followUpHitForwardOffset", "追击命中前移距离" },
        { "followUpCanHitSameEnemyMultipleTimes", "可重复命中同一敌人" },
        { "castDuration", "施法时间" },
        { "sunPrefab", "太阳预制体" },
        { "spawnOffsetFromPlayerTop", "从玩家顶部生成偏移" },
        { "maxSunDiameter", "太阳最大直径" },
        { "growDuration", "变大持续时间" },
        { "lifeTime", "存在时间" },
        { "duration", "持续时间" },
        { "statBonusPercent", "属性提升比例" },
        { "auraPrefab", "光环预制体" },
        { "auraOffset", "光环偏移" },
        { "auraRadiusX", "光环横向半径" },
        { "auraRadiusY", "光环纵向半径" },
        { "auraLineWidth", "光环线宽" },
        { "auraInnerGold", "光环内侧金色" },
        { "auraOuterGold", "光环外侧金色" },
        { "invisibleAlpha", "隐身透明度" },

        { "swordType", "飞剑类型" },
        { "bounceAmount", "弹跳次数" },
        { "bounceGravity", "弹跳重力" },
        { "bounceSpeed", "弹跳速度" },
        { "pierceAmount", "穿透次数" },
        { "pierceGravity", "穿透重力" },
        { "hitCooldown", "命中冷却" },
        { "maxTraveDistance", "最大飞行距离" },
        { "spinDuration", "旋转持续时间" },
        { "SpinGravity", "旋转重力" },
        { "swordPrefab", "飞剑预制体" },
        { "launchForce", "发射力度" },
        { "swordGravity", "飞剑重力" },
        { "freezeTimeDuration", "冻结时间" },
        { "returnSpeed", "返回速度" },
        { "numberOfDots", "指示点数量" },
        { "spaceBeetwenDots", "指示点间距" },
        { "dotPrefab", "指示点预制体" },
        { "dotsParent", "指示点父对象" },

        { "itemName", "物品名称" },
        { "itemDescription", "物品描述" },
        { "itemIcon", "物品图标" },
        { "itemType", "物品类型" },
        { "equipmentType", "装备类型" },
        { "itemCooldown", "物品冷却" },
        { "maxStackSize", "最大堆叠数量" },
        { "dropChance", "掉落几率" },
    };

    private static readonly Dictionary<string, string> WordLabels = new Dictionary<string, string>
    {
        { "Base", "基础" },
        { "Value", "数值" },
        { "Values", "数值" },
        { "Modifier", "加成" },
        { "Modifiers", "加成列表" },
        { "Player", "玩家" },
        { "Enemy", "敌人" },
        { "Attack", "攻击" },
        { "Counter", "反击" },
        { "Damage", "伤害" },
        { "Crit", "暴击" },
        { "Chance", "几率" },
        { "Power", "倍率" },
        { "Health", "生命值" },
        { "Max", "最大" },
        { "Min", "最小" },
        { "Move", "移动" },
        { "Speed", "速度" },
        { "Duration", "持续时间" },
        { "Time", "时间" },
        { "Cooldown", "冷却" },
        { "Force", "力度" },
        { "Jump", "跳跃" },
        { "Dash", "冲刺" },
        { "Dodge", "闪避" },
        { "Precise", "精准" },
        { "Follow", "追击" },
        { "Up", "" },
        { "Window", "窗口" },
        { "Input", "输入" },
        { "Buffer", "缓冲" },
        { "Range", "范围" },
        { "Radius", "半径" },
        { "Multiplier", "倍率" },
        { "Offset", "偏移" },
        { "Forward", "前方" },
        { "Hit", "命中" },
        { "Ground", "地面" },
        { "Wall", "墙体" },
        { "Check", "检测" },
        { "Cheak", "检测" },
        { "Layer", "图层" },
        { "Mask", "遮罩" },
        { "Sword", "飞剑" },
        { "Prefab", "预制体" },
        { "Material", "材质" },
        { "Mat", "材质" },
        { "Color", "颜色" },
        { "Colors", "颜色" },
        { "Sprite", "精灵图" },
        { "Icon", "图标" },
        { "Name", "名称" },
        { "Description", "描述" },
        { "Type", "类型" },
        { "Amount", "数量" },
        { "Count", "数量" },
        { "Size", "大小" },
        { "Parent", "父对象" },
        { "Target", "目标" },
        { "Armor", "护甲" },
        { "Evasion", "闪避" },
        { "Magic", "魔法" },
        { "Resistance", "抗性" },
        { "Fire", "火焰" },
        { "Ice", "冰霜" },
        { "Lighting", "雷电" },
        { "Lightning", "雷电" },
        { "Ignite", "燃烧" },
        { "Chill", "冰冷" },
        { "Shock", "感电" },
        { "Ailment", "异常状态" },
        { "Ailments", "异常状态" },
        { "Slow", "减速" },
        { "Percent", "百分比" },
        { "Percentage", "百分比" },
        { "Gravity", "重力" },
        { "Launch", "发射" },
        { "Return", "返回" },
        { "Dot", "指示点" },
        { "Dots", "指示点" },
        { "Space", "间距" },
        { "Between", "" },
        { "Beetwen", "" },
        { "Spin", "旋转" },
        { "Bounce", "弹跳" },
        { "Pierce", "穿透" },
        { "Life", "存在" },
        { "Grow", "变大" },
        { "Aura", "光环" },
        { "Gold", "金色" },
        { "Inner", "内侧" },
        { "Outer", "外侧" },
    };

    public override void OnInspectorGUI()
    {
        if (!IsProjectScript())
        {
            DrawDefaultInspector();
            return;
        }

        serializedObject.Update();

        SerializedProperty property = serializedObject.GetIterator();
        bool enterChildren = true;

        while (property.NextVisible(enterChildren))
        {
            using (new EditorGUI.DisabledScope(property.propertyPath == "m_Script"))
            {
                EditorGUILayout.PropertyField(property, GetLabel(property), true);
            }

            enterChildren = false;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private bool IsProjectScript()
    {
        MonoBehaviour behaviour = target as MonoBehaviour;

        if (behaviour == null)
            return false;

        MonoScript script = MonoScript.FromMonoBehaviour(behaviour);

        if (script == null)
            return false;

        string path = AssetDatabase.GetAssetPath(script);
        return path.StartsWith("Assets/Scripts/");
    }

    private static GUIContent GetLabel(SerializedProperty property)
    {
        if (ExactLabels.TryGetValue(property.propertyPath, out string exactByPath))
            return new GUIContent(exactByPath, property.tooltip);

        if (ExactLabels.TryGetValue(property.name, out string exactByName))
            return new GUIContent(exactByName, property.tooltip);

        return new GUIContent(TranslateDisplayName(property.displayName), property.tooltip);
    }

    private static string TranslateDisplayName(string displayName)
    {
        string[] words = displayName.Split(' ');
        bool translatedAny = false;

        for (int i = 0; i < words.Length; i++)
        {
            if (!WordLabels.TryGetValue(words[i], out string translated))
                continue;

            words[i] = translated;
            translatedAny = true;
        }

        if (!translatedAny)
            return displayName;

        return string.Join("", words);
    }
}

[CustomPropertyDrawer(typeof(Stat))]
public class StatPropertyDrawer : PropertyDrawer
{
    private const float ButtonWidth = 28f;
    private const float Spacing = 2f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty baseValue = property.FindPropertyRelative("baseValue");
        SerializedProperty modifiers = property.FindPropertyRelative("modifiers");

        Rect foldoutRect = NextLine(position, 0);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            EditorGUI.PropertyField(NextLine(position, 1), baseValue, new GUIContent("基础数值"));

            Rect modifiersRect = NextLine(position, 2);
            modifiers.isExpanded = EditorGUI.Foldout(modifiersRect, modifiers.isExpanded, "加成列表", true);

            if (modifiers.isExpanded)
            {
                EditorGUI.indentLevel++;

                DrawListSize(NextLine(position, 3), modifiers);
                int line = 4;

                if (modifiers.arraySize == 0)
                {
                    EditorGUI.LabelField(NextLine(position, line), "列表为空");
                    line++;
                }
                else
                {
                    for (int i = 0; i < modifiers.arraySize; i++)
                    {
                        EditorGUI.PropertyField(NextLine(position, line), modifiers.GetArrayElementAtIndex(i), new GUIContent($"加成 {i + 1}"));
                        line++;
                    }
                }

                DrawListButtons(NextLine(position, line), modifiers);
                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        int lines = 1;

        if (property.isExpanded)
        {
            lines += 2;

            SerializedProperty modifiers = property.FindPropertyRelative("modifiers");

            if (modifiers.isExpanded)
                lines += 2 + Mathf.Max(1, modifiers.arraySize);
        }

        return lines * (EditorGUIUtility.singleLineHeight + Spacing);
    }

    private Rect NextLine(Rect position, int line)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        return new Rect(position.x, position.y + line * (lineHeight + Spacing), position.width, lineHeight);
    }

    private void DrawListSize(Rect rect, SerializedProperty modifiers)
    {
        int newSize = Mathf.Max(0, EditorGUI.IntField(rect, "数量", modifiers.arraySize));

        if (newSize != modifiers.arraySize)
            modifiers.arraySize = newSize;
    }

    private void DrawListButtons(Rect rect, SerializedProperty modifiers)
    {
        Rect addRect = new Rect(rect.xMax - ButtonWidth * 2f - Spacing, rect.y, ButtonWidth, rect.height);
        Rect removeRect = new Rect(rect.xMax - ButtonWidth, rect.y, ButtonWidth, rect.height);

        if (GUI.Button(addRect, "+"))
            modifiers.arraySize++;

        EditorGUI.BeginDisabledGroup(modifiers.arraySize <= 0);

        if (GUI.Button(removeRect, "-"))
            modifiers.arraySize--;

        EditorGUI.EndDisabledGroup();
    }
}
