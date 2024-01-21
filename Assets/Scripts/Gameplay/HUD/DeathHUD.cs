using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections.Generic;

public class DeathHUD : HUD
{
    [Header("----------General----------")]

    [SerializeField] DeathScreenTextScriptable m_TextData;

    [SerializeField] TMP_Text m_DeathScreenText;

    [SerializeField] RectTransform m_DeathTextPanel;

    [SerializeField] GameObject m_ResultsPanel;
    [SerializeField] TMP_Text m_ScoreBeforeBonusText;
    [SerializeField] TMP_Text m_ScoreAfterBonusText;
    [SerializeField] TMP_Text m_TimeElapsedText;

    [Tooltip("This is the final position of the death text when the final result is shown.")]
    [SerializeField] Vector3 m_DTextEndPosition;
    [Tooltip("How long it takes for the death text panel to move over.")]
    [SerializeField] float m_TextMoveDuration = 1;
    [SerializeField] float m_TimeBeforeShowingResults = 5;

    [Tooltip("This must be a child transform of this transform")]
    [SerializeField] GameObject m_Elements;

    //FLAGS
    bool m_IsShiftingDeathTextForResults;
    bool m_IsReturningToMainMenu;
    bool m_IsRestartingLevel;

    //Refs
    Vector3 m_DeathTextPanelVelocity;

    //Timers
    float m_TimeBeforeShowingResultTimer = 0;

    //Prompts
    [Header("----------Prompts----------")]
    [SerializeField] GameObject m_PromptPanel;
    [SerializeField] Prompt[] m_Prompts;
    private Prompt m_PromptContext;
    Dictionary<PromptType, Prompt> m_PromptDictionary = new();
    [SerializeField] MenuButton[] m_MenuButtons;


    private void SetupPrompts()
    {
        m_PromptDictionary.Clear();

        foreach (Prompt prompt in m_Prompts)
        {
            m_PromptDictionary.Add(prompt.PromptType, prompt);
            prompt.OnEnable();
        }
    }

    protected override void OnEnable()
    {
        PlayerTankHealth.OnDeath += OnPlayerDead;
        GameManager.OnShowEndOfGameScreen += OnShowEndOfGameResults;

        //Button Handling Code
        SetupPrompts();

        foreach (MenuButton button in m_MenuButtons)
            button.OnEnable();
        MenuButton.OnClickMenuButton += OnMenuButtonClick;
        Prompt.OnPromptPositive += OnPromptPositive;
        Prompt.OnPromptNegative += OnPromptNegative;
    }

    private void OnMenuButtonClick(PromptType type)
    {
        switch (type)
        {
            case PromptType.QUIT_MAINMENU:
            case PromptType.RESTART_LEVEL:
                m_ResultsPanel.SetActive(false);
                m_DeathTextPanel.gameObject.SetActive(false);
                m_PromptContext = m_PromptDictionary[type];
                m_PromptContext.Initialise(type);
                m_PromptPanel.SetActive(true);
                break;
        }
    }

    private void OnPromptPositive(Prompt prompt)
    {
        if (prompt != m_PromptContext)
        {
            print("not from here");
            return;
        }

            switch (m_PromptContext.PromptType)
        {
            case PromptType.RESTART_LEVEL:

                if (m_IsRestartingLevel)
                    return;

                m_PromptPanel.SetActive(false);
                m_ResultsPanel.SetActive(false);
                m_DeathTextPanel.gameObject.SetActive(false);

                if (LevelManager.Instance)
                {
                    m_IsRestartingLevel = true;
                    int currentScene = SceneManager.GetActiveScene().buildIndex;
                    LevelManager.Instance.LoadLevel(currentScene);
                }

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
        {
            print("not from here");
            return;
        }

        m_PromptPanel.SetActive(false);
        m_ResultsPanel.SetActive(true);
        m_DeathTextPanel.gameObject.SetActive(true);
    }


    private void OnShowEndOfGameResults(int scoreBeforeBonus, int scoreAfterBonus, float timeSinceStartOfGame)
    {
        m_IsShiftingDeathTextForResults = true;
        m_ScoreBeforeBonusText.text = scoreBeforeBonus.ToString();
        m_ScoreAfterBonusText.text = scoreAfterBonus.ToString();
        m_TimeElapsedText.text = timeSinceStartOfGame.GetFloatStopWatchFormat();
    }

    private void Update()
    {
        if (m_IsShiftingDeathTextForResults)
        {
            ShowFinalScore();
        }
    }

    private void ShowFinalScore()
    {
        m_TimeBeforeShowingResultTimer += Time.deltaTime * Time.timeScale;

        if (m_TimeBeforeShowingResultTimer >= m_TimeBeforeShowingResults)
        {
            m_DeathTextPanel.anchoredPosition =
                Vector3.SmoothDamp(
                        m_DeathTextPanel.anchoredPosition,
                        m_DTextEndPosition,
                        ref m_DeathTextPanelVelocity,
                        m_TextMoveDuration * Time.timeScale
                    );

            if (Vector2.Distance(m_DeathTextPanel.anchoredPosition, m_DTextEndPosition) <= 2.0F)
            {
                m_DeathTextPanel.anchoredPosition = m_DTextEndPosition;
            }

            if ((Vector3)m_DeathTextPanel.anchoredPosition == m_DTextEndPosition)
            {
                m_ResultsPanel.SetActive(true);
                m_IsShiftingDeathTextForResults = false;
            }
        }
    }

    protected override void OnDisable()
    {
        GameManager.OnShowEndOfGameScreen -= OnShowEndOfGameResults;
        PlayerTankHealth.OnDeath -= OnPlayerDead;

        //Prompts
        foreach (MenuButton button in m_MenuButtons)
            button.OnDisable();

        MenuButton.OnClickMenuButton -= OnMenuButtonClick;

        foreach (Prompt prompt in m_Prompts)
        {
            prompt.OnDisable();
        }
    }

    private void OnPlayerDead()
    {
        m_Elements.SetActive(true);

        m_DeathScreenText.text = m_TextData.Text[Random.Range(0, m_TextData.Text.Length)];
    }
}