using TMPro;
using UnityEngine;

public class DeathHUD : HUD
{
    [Header("----------General----------")]

    [SerializeField] DeathScreenTextScriptable m_TextData;

    [SerializeField] TMP_Text m_DeathScreenText;

    [Tooltip("This must be a child transform of this transform")]
    [SerializeField] GameObject m_Elements;

    protected override void OnEnable()
    {
        PlayerTankHealth.OnDeath += OnPlayerDead;
    }

    protected override void OnDisable()
    {
        PlayerTankHealth.OnDeath -= OnPlayerDead;
    }

    private void OnPlayerDead()
    {
        m_Elements.SetActive(true);

        m_DeathScreenText.text = m_TextData.Text[Random.Range(0, m_TextData.Text.Length)];

        //Play Death Sound...
        if (SoundManager.Instance)
        {
            SoundManager.Instance.TransitionToMixerState("Death", 0.1F);
            SoundManager.Instance.PlayFESound("Death");
        }
    }
}