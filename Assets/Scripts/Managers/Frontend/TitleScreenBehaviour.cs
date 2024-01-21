using TMPro;
using System;
using UnityEngine;

public class TitleScreenBehaviour : MonoBehaviour
{
    [SerializeField] TMP_Text m_TitleScreenText; //'Press Any Key' Text

    [Header("----------Sin Wave Parameters-----------")]
    [SerializeField][Range(0, 1)] float m_Amplitude = 1;
    [SerializeField][Range(0, 4)] float m_Periodicity = 4;
    [SerializeField][Range(0, 5)] float m_Multiplier = 1.25F;

    public static event Action OnGameStart;

    private void Update()
    { 
        m_TitleScreenText.alpha = Mathf.Clamp(m_Amplitude * Mathf.Sin(Time.time * m_Periodicity) * m_Multiplier, 0, 1);

        if (LevelManager.Instance != null && Input.anyKeyDown)
        {
            LevelManager.Instance.LoadLevel(1);

            //Disable the text...
            if (m_TitleScreenText.transform.parent)
                m_TitleScreenText.transform.parent.gameObject.SetActive(false);
            else
                m_TitleScreenText.gameObject.SetActive(false);

            OnGameStart?.Invoke();

            if (SoundManager.Instance)
            {
                SoundManager.Instance.PlayFESound("GunReady");
            }
            else
                Debug.LogError("There is no Sound Manager in this scene!");

            //Disable this script.
            this.enabled = false;

            return;
        }

    }
}