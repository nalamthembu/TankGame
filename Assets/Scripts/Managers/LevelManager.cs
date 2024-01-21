using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [SerializeField] LoadingScreen m_LoadingScreen;

    [SerializeField] Image m_BlackScreen;

    [SerializeField] int m_MainMenuIndex = 1;

    public static event Action OnLoadingComplete;

    public static event Action OnLoadingStart;

    //Flags
    bool m_IsLoading;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        Instance = null;
        Debug.Log("Destroyed Level Manager Instance");
    }

    private void OnEnable()
    {
        TitleScreenBehaviour.OnGameStart += OnGameStart;
    }

    private void OnDisable()
    {
        TitleScreenBehaviour.OnGameStart -= OnGameStart;
    }

    private void Update()
    {
        if (m_LoadingScreen.IsEnabled)
            m_LoadingScreen.Update();
    }

    private void OnGameStart()
    {
        LoadLevel(m_MainMenuIndex);
    }

    public int GetCurrentLevel() => SceneManager.GetActiveScene().buildIndex;

    public void LoadLevel(int index)
    {
        if (!m_IsLoading)
            StartCoroutine(LoadLevel_Coroutine(index));

        m_IsLoading = true;
    }

    IEnumerator LoadLevel_Coroutine(int index)
    {
        //Let everyone know...
        OnLoadingStart?.Invoke();

        //FADE SCREEN TO BLACK

        m_BlackScreen.gameObject.SetActive(true);

        Color blackScreenColor = m_BlackScreen.color;

        while (m_BlackScreen.color.a < 1)
        {
            blackScreenColor.a += Time.deltaTime;

            m_BlackScreen.color = blackScreenColor;

            if (blackScreenColor.a >= 1)
            {
                blackScreenColor.a = 1;
            }

            yield return new WaitForEndOfFrame();
        }

        //SETUP LOADING SCREEN
        m_LoadingScreen.SetEnabled(true);

        //WAIT
        yield return new WaitForSeconds(1);

        while (m_BlackScreen.color.a > 0)
        {
            blackScreenColor.a -= Time.deltaTime;

            m_BlackScreen.color = blackScreenColor;

            if (blackScreenColor.a <= 0)
            {
                blackScreenColor.a = 0;
            }

            yield return new WaitForEndOfFrame();
        }

        m_BlackScreen.gameObject.SetActive(false);

        //START LOADING LEVEL...
        AsyncOperation operation = SceneManager.LoadSceneAsync(index);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / .9f);
            m_LoadingScreen.SetProgress(progress);
            yield return null;
        }
        //END OF LOADING LEVEL...


        //WAIT...
        yield return new WaitForSeconds(1);


        //FADE SCREEN TO BLACK

        m_BlackScreen.gameObject.SetActive(true);

        while (m_BlackScreen.color.a < 1)
        {
            blackScreenColor.a += Time.deltaTime;

            m_BlackScreen.color = blackScreenColor;

            if (blackScreenColor.a >= 1)
            {
                blackScreenColor.a = 1;
            }

            yield return new WaitForEndOfFrame();
        }

        //TAKE DOWN LOADING SCREEN
        m_LoadingScreen.SetEnabled(false);

        //WAIT
        yield return new WaitForSeconds(1);

        while (m_BlackScreen.color.a > 0)
        {
            blackScreenColor.a -= Time.deltaTime;

            m_BlackScreen.color = blackScreenColor;

            if (blackScreenColor.a <= 0)
            {
                blackScreenColor.a = 0;
            }

            yield return new WaitForEndOfFrame();
        }

        m_BlackScreen.gameObject.SetActive(false);

        m_IsLoading = false;

        OnLoadingComplete?.Invoke();
    }
}


[System.Serializable]
public class LoadingScreen
{
    [SerializeField] GameObject m_LoadingScreenObject;

    [SerializeField] TMP_Text m_LoadingScreenTipText;

    [SerializeField] Slider m_ProgressBar;

    [SerializeField] LoadingScreenTipsScriptable m_LoadingScreenTips;

    [SerializeField] float m_DurationBetweenEachTip = 5.0F;

    public bool IsEnabled { get; private set; }

    public void SetProgress(float amount) => m_ProgressBar.value = amount;

    private float timer = float.MaxValue;

    public void Update()
    {
        timer += Time.deltaTime;

        if (timer >= m_DurationBetweenEachTip)
        {
            m_LoadingScreenTipText.text = m_LoadingScreenTips.GetRandomTip();

            if (SoundManager.Instance)
            {
                SoundManager.Instance.PlayFESound("Prompt");
            }
            else
                Debug.LogError("There is no Sound Manager in this scene!");

            timer = 0;
        }
    }

    public float GetProgress() => m_ProgressBar.value;

    public void SetEnabled(bool value)
    {
        m_LoadingScreenObject.SetActive(value);
        IsEnabled = value;
    }
}