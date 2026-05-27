using UnityEngine;

public class UI : MonoBehaviour
{
    [SerializeField] private GameObject characterUI;
    [SerializeField] private GameObject skillTreeUI;
    [SerializeField] private GameObject craftUI;
    [SerializeField] private GameObject optionsUI;
    [SerializeField] private GameObject levelUpUI;
    [SerializeField] private GameObject inGameUI;
    [SerializeField] private bool pauseGameWhenMenuOpen = true;

    public UI_SkillToolTip skillToolTip;
    public UI_ItemTooltip itemTooltip;
    public UI_StatToolTip statToolTip;
    public UI_InteractionPrompt interactionPrompt;
    public UI_CraftWindow craftWindow;

    private bool uiPauseActive;
    private float timeScaleBeforeUIPause = 1f;

    public Transform InteractionPromptRoot => inGameUI != null ? inGameUI.transform : transform;

    void Start()
    {
        ResolveMenuReferences();
        SwtichTo(inGameUI);

        itemTooltip?.gameObject.SetActive(false);
        statToolTip?.gameObject.SetActive(false);
        interactionPrompt = UI_InteractionPrompt.GetOrCreate(InteractionPromptRoot);
        interactionPrompt?.Hide();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SwitchWithKeyTo(characterUI);
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            SwitchWithKeyTo(skillTreeUI);
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            SwitchWithKeyTo(craftUI);
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            SwitchWithKeyTo(optionsUI);
        }

        RefreshGamePauseState();
    }

    public void SwtichTo(GameObject _menu)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }

        if (_menu != null)
        {
            _menu.SetActive(true);
        }

        RefreshGamePauseState();
    }

    public void SwitchWithKeyTo(GameObject _menu)
    {
        if (_menu != null && _menu.activeSelf)
        {
            _menu.SetActive(false);
            CheakForInGameUI();
            RefreshGamePauseState();
            return;
        }
        SwtichTo(_menu);
    }

    public void OpenSkillTree()
    {
        SwtichTo(skillTreeUI);
    }

    public void OpenLevelUp()
    {
        ResolveMenuReferences();
        PlayerEmberWallet.GetOrCreate();

        if (levelUpUI == null)
        {
            Debug.LogWarning("LevelUp_UI was not found under Canvas/UI.", this);
            return;
        }

        SwtichTo(levelUpUI);
    }

    private void ResolveMenuReferences()
    {
        if (levelUpUI == null)
            levelUpUI = FindChildMenu("LevelUp_UI");
    }

    private GameObject FindChildMenu(string menuName)
    {
        if (string.IsNullOrWhiteSpace(menuName))
            return null;

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);

            if (child != null && child.name == menuName)
                return child.gameObject;
        }

        return null;
    }

    private void CheakForInGameUI()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if(transform.GetChild(i).gameObject.activeSelf)
                return;
        }
        SwtichTo(inGameUI);
    }

    private void RefreshGamePauseState()
    {
        ApplyGamePause(ShouldPauseForActiveMenu());
    }

    private bool ShouldPauseForActiveMenu()
    {
        if (!pauseGameWhenMenuOpen)
            return false;

        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;

            if (!child.activeInHierarchy || child == inGameUI)
                continue;

            return true;
        }

        return false;
    }

    private void ApplyGamePause(bool shouldPause)
    {
        if (shouldPause == uiPauseActive)
            return;

        if (shouldPause)
        {
            timeScaleBeforeUIPause = Time.timeScale;
            Time.timeScale = 0f;
            uiPauseActive = true;
            return;
        }

        Time.timeScale = timeScaleBeforeUIPause;
        uiPauseActive = false;
    }

    private void OnDisable()
    {
        ApplyGamePause(false);
    }

    private void OnDestroy()
    {
        ApplyGamePause(false);
    }

}
