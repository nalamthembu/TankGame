using UnityEngine;
using System;
using System.Collections.Generic;

public class PauseMenuManager : MonoBehaviour
{
    [SerializeField] MenuButton[] m_MenuButtons;

    [SerializeField] Prompt[] m_Prompts;

    [SerializeField] GameObject m_PauseMenuPanel;

    [SerializeField] GameObject m_PauseMenuButtonPanel;

    [SerializeField] GameObject m_PromptPanel;

    private Prompt m_PromptContext;

    Dictionary<PromptType, Prompt> m_PromptDictionary = new();

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

    private void OnPromptPositive(Prompt prompt)
    {
        if (prompt != m_PromptContext)
            return;

        switch (m_PromptContext.PromptType)
        {
            case PromptType.QUIT_DESKTOP:

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

            case PromptType.QUIT_MAINMENU:

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

    private void OnPromptNegative(Prompt prompt)
    {
        if (prompt != m_PromptContext)
            return;

        m_PromptPanel.SetActive(false);
        m_PauseMenuButtonPanel.SetActive(true);
    }

    private void OnMenuButtonClick(PromptType type)
    {
        //Decide what to do depending on which button was clicked...
        switch (type)
        {
            case PromptType.RESUME:
                m_PauseMenuPanel.SetActive(false);
                OnPauseMenuClose?.Invoke();
                m_PromptContext = null;
                return;

            case PromptType.QUIT_DESKTOP:
            case PromptType.QUIT_MAINMENU:
                m_PromptContext = m_PromptDictionary[type];
                m_PromptContext.Initialise(type);
                m_PromptPanel.SetActive(true);
                m_PauseMenuButtonPanel.SetActive(false);
                break;
        }
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
        GameManager.OnGameResume -= OnGameResume;
    }
}

public enum PromptType
{
    RESUME,
    QUIT_DESKTOP,
    QUIT_MAINMENU,
    RESTART_LEVEL
}