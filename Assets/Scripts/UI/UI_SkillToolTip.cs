using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_SkillToolTip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI skillText;
    [SerializeField] private TextMeshProUGUI skillName;
    [SerializeField] private Vector2 mouseOffset = new Vector2(150, 150);
    [SerializeField] private Vector2 screenPadding = new Vector2(12, 12);

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        UpdateToolTipPosition();
    }

    public void ShowToolTip(string _skillDescprtion,string _skillName)
    {
        skillName.text = _skillName;
        skillText.text = _skillDescprtion;
        gameObject.SetActive(true);

        if (rectTransform != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

        UpdateToolTipPosition();
    }

    public void UpdateToolTipPosition()
    {
        Vector2 mousePosition = Input.mousePosition;

        float xOffset = mousePosition.x > Screen.width * .5f ? -mouseOffset.x : mouseOffset.x;
        float yOffset = mousePosition.y > Screen.height * .5f ? -mouseOffset.y : mouseOffset.y;

        Vector2 tooltipPosition = new Vector2(mousePosition.x + xOffset, mousePosition.y + yOffset);

        if (rectTransform != null)
        {
            Vector2 size = rectTransform.rect.size;
            Vector2 pivot = rectTransform.pivot;

            float minX = screenPadding.x + size.x * pivot.x;
            float maxX = Screen.width - screenPadding.x - size.x * (1f - pivot.x);
            float minY = screenPadding.y + size.y * pivot.y;
            float maxY = Screen.height - screenPadding.y - size.y * (1f - pivot.y);

            tooltipPosition.x = Mathf.Clamp(tooltipPosition.x, minX, maxX);
            tooltipPosition.y = Mathf.Clamp(tooltipPosition.y, minY, maxY);
        }

        transform.position = tooltipPosition;
    }

    public void HideToolTip() => gameObject.SetActive(false);
    
}
