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
    private void OnGameIsStarting() => m_StartGameTimerGameObject.SetActive(true);
    
    private void Update()
    {
        if (GameManager.Instance)
        {
            if (GameManager.Instance.StartingTimer <= 5)
            {
                m_StartTimerText.text = Mathf.Ceil(GameManager.Instance.StartingTimer).ToString();

                switch (Mathf.Ceil(GameManager.Instance.StartingTimer))
                {
                    case 5:
                    case 4: 
                        m_StartTimerText.text = "Get Ready!";
                        break;

                    case 0:
                        m_StartTimerText.text = "Go!";
                        break;
                }
            }
        }
    }

}