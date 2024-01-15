using UnityEngine;

public class EffectLifetime : MonoBehaviour
{
    [Tooltip("How long is the effect allowed to be in the scene?")]
    [SerializeField] float m_LifeTimeInSeconds = 10.0F;
    float m_TimeInScene = 0;

    [Header("---------Audio----------")]
    [SerializeField] AudioSource m_AudioSource;
    [SerializeField] string m_LoopAudioID = string.Empty;

    private void Awake()
    {
        if (m_AudioSource is null && m_LoopAudioID != string.Empty)
            Debug.LogError("There is no audio source attached!");
    }

    private void OnDisable()
    {
        if (m_AudioSource)
            m_AudioSource.Stop();
    }

    private void Update()
    {
        //Play Audio if applicable...
        if (m_LoopAudioID != string.Empty && m_AudioSource != null && !m_AudioSource.isPlaying)
        {
            if (SoundManager.Instance)
            {
                SoundManager.Instance.PlayInGameSound(m_LoopAudioID, m_AudioSource, true, true, false, 10.0F);
            }
            else
                Debug.LogError("There is no sound manager in the scene!");
        }

        //Do not allow this projectile to be in the scene for > m_LifeTimeSeconds without exploding

        m_TimeInScene += Time.deltaTime;

        if (m_TimeInScene >= m_LifeTimeInSeconds)
        {
            gameObject.SetActive(false);

            m_TimeInScene = 0;
        }
    }
}