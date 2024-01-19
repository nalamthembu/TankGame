using UnityEngine;

public class TankEngineAudio : MonoBehaviour
{
    [Header("---------General----------")]
    [SerializeField] float m_IdleRPM = 1000;
    [SerializeField] float m_MaxRPM = 5000;
    [SerializeField] float m_EngineResponse = 2;
    [SerializeField] RealisticEngineSound m_EngineSoundModule;

    [Header("---------Track Audio----------")]
    [Tooltip("This is where the wheel track movement sounds will be played")]
    [SerializeField] AudioSource m_TrackAudioSource;
    [SerializeField] AudioSource m_TrackSweetenerSource;
    [SerializeField] float m_TrackSweetenerMaxVol = 0.5F;
    [Tooltip("This is the speed at which the tank has to be for the track volume to be at its maximum")]
    [SerializeField] float m_MaxAudioSpeed = 30;

    float m_CurrentRPM;

    float m_CurrVelocity;

    BaseTank m_Tank;

    public float RPM { get { return m_CurrentRPM; } }

    private void Awake()
    {
        if (m_EngineSoundModule is null)
        {
            Debug.LogError("There is no engine sound module attached!");
            enabled = false;
            return;
        }

        if (m_TrackAudioSource is null)
        {
            Debug.LogError("There is no track audio source attached!");
            enabled = false;
            return;
        }

        if (m_TrackSweetenerSource is null)
        {
            Debug.LogError("There is no track sweetener audio source attached!");
            enabled = false;
            return;
        }

        if (TryGetComponent<BaseTank>(out var tank))
        {
            m_Tank = tank;
        }
        else
        {
            Debug.LogError("There is no tank attached to this object!");
            enabled = false;
            return;
        }
    }

    private void PlayTrackSound()
    {
        if (m_TrackAudioSource)
        {
            if (!m_TrackAudioSource.isPlaying || !m_TrackAudioSource.enabled)
            {
                if (SoundManager.Instance)
                {
                    m_TrackAudioSource.enabled = true;

                    SoundManager.Instance.PlayInGameSound("TankFX_TrackLoop", m_TrackAudioSource, true, true, false, 15);
                }
            }
            else
            {
                if (m_Tank)
                {
                    m_TrackAudioSource.volume = Mathf.Lerp(0, 1, m_Tank.Speed / m_MaxAudioSpeed);

                    if (m_TrackAudioSource.volume - 0.001F <= 0)
                    {
                        m_TrackAudioSource.Stop();
                        m_TrackAudioSource.enabled = false;
                    }
                }
            }
        }
    }

    private void PlayTrackSweetener()
    {
        if (m_TrackSweetenerSource)
        {
            if (!m_TrackSweetenerSource.isPlaying || !m_TrackSweetenerSource.enabled)
            {
                if (SoundManager.Instance)
                {
                    m_TrackSweetenerSource.enabled = true;

                    //Play the mono track for NPCs, stereo for Player...
                    SoundManager.Instance.PlayInGameSound("TankFX_TrackSweetenerLoop_Mono", m_TrackSweetenerSource, true, true, m_Tank is Tank, 1.0F);
                }
            }
            else
            {
                if (m_Tank)
                {
                    m_TrackSweetenerSource.volume = Mathf.Lerp(0, m_TrackSweetenerMaxVol, m_Tank.Speed / m_MaxAudioSpeed);

                    if (m_TrackSweetenerSource.volume - 0.001F <= 0)
                    {
                        m_TrackSweetenerSource.Stop();
                        m_TrackSweetenerSource.enabled = false;
                    }
                }
            }
        }
    }

    private void Update()
    {
        float desiredRPM = m_IdleRPM + Mathf.Clamp01(PlayerInput.Instance.InputDir.y) * m_MaxRPM;

        m_CurrentRPM = Mathf.SmoothDamp(m_CurrentRPM, desiredRPM, ref m_CurrVelocity, m_EngineResponse);

        PlayTrackSound();

        PlayTrackSweetener();

        if (m_EngineSoundModule)
            m_EngineSoundModule.engineCurrentRPM = m_CurrentRPM;
    }
}