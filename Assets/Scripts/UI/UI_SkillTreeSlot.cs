using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class UI_SkillTreeSlot : MonoBehaviour, IPointerEnterHandler , IPointerExitHandler
{
    private static readonly string[] DeprecatedSkillKeywords = { "\u5E7B\u8C61", "\u6C34\u6676", "\u9ED1\u6D1E" };

    private UI ui;
    private Image skillImage;

    [SerializeField] private string skillName;
    [TextArea]
    [SerializeField] private string skillDescription;
    [SerializeField] private Color lockedSkillColor;


    public bool unlocked;

    [SerializeField] private UI_SkillTreeSlot[] shouldBeUnlocked;
    [SerializeField] private UI_SkillTreeSlot[] shouldBeLocked;

    private void OnValidate()
    {
        gameObject.name = "SkillTreeSlot_UI -" + skillName;
    }
    private void Start()
    {
        if (ShouldHideDeprecatedSkill())
        {
            gameObject.SetActive(false);
            return;
        }

        skillImage = GetComponent<Image>();
        ui= GetComponentInParent<UI>();

        skillImage.color = lockedSkillColor;

        GetComponent<Button>().onClick.AddListener(() => UnlockSkillSlot());
    }

    public void UnlockSkillSlot()
    {
        for (int i = 0; i < shouldBeUnlocked.Length; i++)
        {
            if (shouldBeUnlocked[i] != null && !shouldBeUnlocked[i].ShouldHideDeprecatedSkill() && shouldBeUnlocked[i].unlocked == false)
            {
                Debug.Log("Cannot unlock skill");
                return;
            }
        }

        for (int i = 0; i < shouldBeLocked.Length; i++)
        {
            if (shouldBeLocked[i] != null && !shouldBeLocked[i].ShouldHideDeprecatedSkill() && shouldBeLocked[i].unlocked == true)
            {
                Debug.Log("Cannot unlock skill");
                return;
            }
        }

        unlocked = true;
        skillImage.color = Color.white;

        ApplyUnlockEffect();
    }

    private bool ShouldHideDeprecatedSkill()
    {
        for (int i = 0; i < DeprecatedSkillKeywords.Length; i++)
        {
            if (!string.IsNullOrEmpty(skillName) && skillName.Contains(DeprecatedSkillKeywords[i]))
                return true;
        }

        return false;
    }

    private void ApplyUnlockEffect()
    {
        if (skillName == "\u7CBE\u51C6\u95EA\u907F\u5EF6\u957F")
            SkillManager.instance?.preciseDodge?.AddTimeStopDuration(0.25f);

        if (skillName == "\u95EA\u907F\u8FDE\u6BB5" || skillName == "\u7CBE\u51C6\u95EA\u907F\u8FDE\u6BB5" || skillName == "\u7CBE\u51C6\u95EA\u907F\u653B\u51FB")
            SkillManager.instance?.preciseDodge?.UnlockFollowUpAttack();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ui.skillToolTip.ShowToolTip(skillDescription, skillName);

        Vector2 mousePosition = Input.mousePosition;

        float xOffset = 0;
        float yOffset = 0;

        if (mousePosition.x > 600)
            xOffset = -150;
        else
            xOffset = 150;

        if (mousePosition.y > 320)
            yOffset = -150;
        else
            yOffset = 150;

        ui.skillToolTip.transform.position = new Vector2(mousePosition.x + xOffset, mousePosition.y + yOffset);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ui.skillToolTip.HideToolTip();
    }
}
