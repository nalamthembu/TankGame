using TMPro;
using UnityEngine;

public class GameStatsHUD : HUD
{
    [Header("----------Variables----------")]
    [SerializeField] TMP_Text m_ElapsedTimeText;
    [SerializeField] TMP_Text m_KillCounterText;
    [SerializeField] TMP_Text m_WaveCounterText;

    public static GameStatsHUD Instance;

    private void Awake()
    {
        if (m_ElapsedTimeText is null)
            Debug.LogError("ELAPSED TIME text var is not assigned!");

        if (m_KillCounterText is null)
            Debug.LogError("KILL COUNTER text var is not assigned!");

        if (m_KillCounterText is null)
            Debug.LogError("WAVE COUNTER text var is not assigned!");

        if (Instance == null)
        {
            Instance = this;
        }
        else
            Destroy(gameObject);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    private void OnDestroy()
    {
        Instance = null;

        Debug.Log("Destroyed GameStatHUD Instance!");
    }

    private void Update()
    {
        if (GameManager.Instance != null)
        {
            m_ElapsedTimeText.text = GameManager.Instance.GameElapsedTime.GetFloatStopWatchFormat();
            m_KillCounterText.text = GameManager.Instance.TotalKillsByPlayer.ToString();
            m_WaveCounterText.text = GameManager.Instance.WavesSurvived.ToString();
        }
    }
}
