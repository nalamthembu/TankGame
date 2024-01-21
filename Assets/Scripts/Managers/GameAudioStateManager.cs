using UnityEngine;

public class GameAudioStateManager : MonoBehaviour
{
    [Header("----------General----------")]
    [Tooltip("How fast we transition from whatever state to low health state.")]
    [SerializeField] float m_LowHealthTransitionTime = 0.5F;
    [Tooltip("How fast we transition from whatever state to death state.")]
    [SerializeField] float m_DeathTransitionTime = 0.25F;
    [Tooltip("How fast we transition from whatever state to normal state.")]
    [SerializeField] float m_NormalStateTransitionTime = 2.5F;
    [Tooltip("How fast we transition from whatever state to no sound except UI state")]
    [SerializeField] float m_FrontendOnlyTransitionTime = 3.0F;

    private void Start()
    {
        if (!SoundManager.Instance)
            Debug.LogError("There is no Sound Manager in this scene!");
    }

    private void OnEnable()
    {
        PlayerTankHealth.OnHealthChange += OnPlayerHealthChange;
        PlayerTankHealth.OnDeath += OnPlayerDeath;
        LevelManager.OnLoadingStart += OnLoadingStart;
        LevelManager.OnLoadingComplete += OnLoadingComplete;
    }

    private void OnDisable()
    {
        PlayerTankHealth.OnHealthChange -= OnPlayerHealthChange;
        PlayerTankHealth.OnDeath -= OnPlayerDeath;
        LevelManager.OnLoadingStart -= OnLoadingStart;
        LevelManager.OnLoadingComplete -= OnLoadingComplete;
    }
    private void OnLoadingComplete()
    {
        string CurrentMixerState = SoundManager.Instance.CurrentMixerStateID;

        if (CurrentMixerState != "Normal")
        {
            SoundManager.Instance.TransitionToMixerState("Normal", m_NormalStateTransitionTime);
        }
    }

    private void OnLoadingStart()
    {
        string CurrentMixerState = SoundManager.Instance.CurrentMixerStateID;

        if (CurrentMixerState != "FrontendOnly")
        {
            SoundManager.Instance.TransitionToMixerState("FrontendOnly", m_FrontendOnlyTransitionTime);
        }
    }


    private void OnPlayerDeath()
    {
        string CurrentMixerState = SoundManager.Instance.CurrentMixerStateID;

        if (CurrentMixerState != "Death")
        {
            SoundManager.Instance.TransitionToMixerState("Death", m_DeathTransitionTime);
            SoundManager.Instance.PlayFESound("Death");
        }
    }

    private void OnPlayerHealthChange(float health, float armor)
    {
        if (!SoundManager.Instance)
            return;

        string CurrentMixerState = SoundManager.Instance.CurrentMixerStateID;

        if (health > 0 && health <= 30 && CurrentMixerState != "LowHealth")
        {
            SoundManager.Instance.TransitionToMixerState("LowHealth", m_LowHealthTransitionTime);
        }

        if (health > 30 && CurrentMixerState != "Normal")
        {
            SoundManager.Instance.TransitionToMixerState("Normal", m_NormalStateTransitionTime);
        }
    }

}