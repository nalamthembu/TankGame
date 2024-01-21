using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class PauseMenuManager : MonoBehaviour
{
    [SerializeField] MenuButton[] m_MenuButtons;

    [SerializeField] Prompt[] m_Prompts;

    [SerializeField] GameObject m_PauseMenuPanel;

    [SerializeField] GameObject m_PauseMenuButtonPanel;

    [SerializeField] GameObject m_PromptPanel;

    private Prompt m_PromptContext;

    Dictionary<PauseMenuButtonType, Prompt> m_PromptDictionary = new();

    public static event Action OnPauseMenuClose;

    //flags
    bool m_IsQuittingGame;
    bool m_IsReturningToMainMenu;

    private void Awake()
    {
        foreach(Prompt prompt in m_Prompts)
        {
            m_PromptDictionary.Add(prompt.PromptType, prompt);
            prompt.OnEnable();
        }
    }

    private void OnEnable()
    {
        foreach (MenuButton button in m_MenuButtons)
            button.OnEnable();

        MenuButton.OnClickMenuButton += OnMenuButtonClick;
        Prompt.OnPromptPositive += OnPromptPositive;
        Prompt.OnPromptNegative += OnPromptNegative;
        GameManager.OnGamePaused += OnGamePaused;
        GameManager.OnGameResume += OnGameResume;
    }

    private void OnGameResume() => m_PauseMenuPanel.SetActive(false);

    private void OnGamePaused() => m_PauseMenuPanel.SetActive(true);

    private void OnPromptPositive()
    {
        switch (m_PromptContext.PromptType)
        {
            case PauseMenuButtonType.QUIT_DESKTOP:

                if (m_IsQuittingGame)
                    return;

                m_IsQuittingGame = true;
#if UNITY_EDITOR

                if (Application.isEditor)
                {
                    UnityEditor.EditorApplication.isPlaying = false;
                }
#endif

                Application.Quit();

                break;

            case PauseMenuButtonType.QUIT_MAINMENU:

                if (m_IsReturningToMainMenu)
                    return;

                if (LevelManager.Instance)
                {
                    m_IsReturningToMainMenu = true;

                    //HARD_CODED_MAIN_MENU_INDEX
                    LevelManager.Instance.LoadLevel(1);

                    gameObject.SetActive(false);
                }

                break;
        }
    }

    private void OnPromptNegative()
    {
        switch (m_PromptContext.PromptType)
        {
            default: print("negative"); break;
        }
    }

    private void OnMenuButtonClick(PauseMenuButtonType type)
    {
        //Decide what to do depending on which button was clicked...
        if (type == PauseMenuButtonType.RESUME)
        {
            m_PauseMenuPanel.SetActive(false);

            OnPauseMenuClose?.Invoke();

            m_PromptContext = null;

            return;
        }

        m_PromptContext = m_PromptDictionary[type];
        m_PromptContext.Initialise(type);
        m_PromptPanel.SetActive(true);
        m_PauseMenuButtonPanel.SetActive(false);
    }

    private void OnDisable()
    {
        foreach (MenuButton button in m_MenuButtons)
            button.OnDisable();

        MenuButton.OnClickMenuButton -= OnMenuButtonClick;

        foreach (Prompt prompt in m_Prompts)
        {
            prompt.OnDisable();
        }

        GameManager.OnGamePaused -= OnGamePaused;
    }
}

[Serializable]
public class Prompt
{
    [SerializeField] string m_Header;
    [SerializeField] string m_Question;
    [SerializeField] TMP_Text m_HeaderText;
    [SerializeField] TMP_Text m_QuestionText;
    [SerializeField] Button m_Positive;
    [SerializeField] Button m_Negative;
    [SerializeField] PauseMenuButtonType m_PromptType;

    public static event Action OnPromptPositive, OnPromptNegative;

    public PauseMenuButtonType PromptType { get { return m_PromptType; } }

    public void Initialise(PauseMenuButtonType type)
    {
        m_HeaderText.text = m_Header;
        m_QuestionText.text = m_Question;
        m_PromptType = type;
    }

    public void OnEnable()
    {
        m_Positive.onClick.AddListener(OnClickPos);
        m_Negative.onClick.AddListener(OnClickNeg);
    }

    public void OnDisable()
    {
        m_Positive.onClick.RemoveAllListeners();
        m_Negative.onClick.RemoveAllListeners();
    }

    private void OnClickPos() => OnPromptPositive?.Invoke();
    private void OnClickNeg() => OnPromptNegative?.Invoke();
}

[Serializable]
public class MenuButton
{
    [SerializeField] Button m_Button;

    [SerializeField] PauseMenuButtonType type;

    public static event Action<PauseMenuButtonType> OnClickMenuButton;

    public void OnEnable() => m_Button.onClick.AddListener(OnClick);
    public void OnDisable() => m_Button.onClick.RemoveAllListeners();
    private void OnClick() => OnClickMenuButton?.Invoke(type);
    
}

public enum PauseMenuButtonType
{
    RESUME,
    QUIT_DESKTOP,
    QUIT_MAINMENU
}