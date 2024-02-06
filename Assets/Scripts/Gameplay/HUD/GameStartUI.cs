using TMPro;
using UnityEngine;

public class GameStartUI : MonoBehaviour
{
    [Header("----------General----------")]
    [SerializeField] TMP_Text m_StartTimerText;
    [SerializeField] GameObject m_StartGameTimerGameObject;

    private void OnEnable()
    {
        GameManager.OnGameIsStarting += OnGameIsStarting;
        GameManager.OnGameStarted += OnGameStarted;
    }

    private void OnDisable()
    {
        GameManager.OnGameIsStarting -= OnGameIsStarting;
        GameManager.OnGameStarted -= OnGameStarted;
    }

    private void OnGameStarted() => m_StartGameTimerGameObject.SetActive(false);
    private void OnGameIsStarting()
    {
        m_StartTimerText.text = "Get Ready";
        m_StartGameTimerGameObject.SetActive(true);
    }

    private void Update()
    {
        if (GameManager.Instance)
        {
            switch ((int)GameManager.Instance.StartingTimer)
            {
                case 3:
                case 2:
                case 1:
                    int timer = (int)GameManager.Instance.StartingTimer;
                    m_StartTimerText.text = timer.ToString();
                    break;
                case 0:
                    m_StartTimerText.text = "Go!";
                    break;
            }
        }
    }
}