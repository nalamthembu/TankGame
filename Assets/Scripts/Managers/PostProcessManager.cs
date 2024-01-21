using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessManager : MonoBehaviour
{
    [Header("----------General-----------")]
    [SerializeField] PostProcessVolume m_CameraShakeFX;
    [SerializeField] PostProcessVolume m_MainFX;
    [SerializeField] PostProcessVolume m_NearDeathFX;
    [SerializeField] PostProcessVolume m_PlayerHurtFX;
    [SerializeField] PostProcessVolume m_PausedFX;

    [SerializeField] float m_TakeDamageDuration = .5F;
    [Tooltip("How long it takes for the screen to blur/become-clear when the pause menu is up")]
    [SerializeField] float m_PauseMenuBlurDuration = .5F;

    public static PostProcessManager Instance;

    float m_LastKnownPlayerHealth;

    float m_TakeDamageTimer;

    //refs
    float m_BlurVelocityPaused;

    //Flags
    bool m_PlayerIsNearDeath;
    bool m_IsPaused;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        CheckForNullReferences();
    }

    private void CheckForNullReferences()
    {
        if (m_CameraShakeFX == null)
            Debug.LogError("There is no camera shake fx volume assigned to the post fx manager!");

        if (m_MainFX == null)
            Debug.LogError("There is no main fx volume assigned to the post fx manager!");

        if (m_NearDeathFX == null)
            Debug.LogError("There is no near death fx volume assigned to the post fx manager!");

        if (m_PlayerHurtFX == null)
            Debug.LogError("There is no player hurt fx volume assigned to the post fx manager!");

        if (m_PausedFX == null)
            Debug.LogError("There is no paused fx volume assigned to the post fx manager!");
    }

    private void OnEnable()
    {
        PlayerTankHealth.OnHealthChange += OnPlayerHealthChange;
        PlayerTankHealth.OnTakeDamage += OnPlayerTakeDamage;
        GameManager.OnGamePaused += OnGamePaused;
        GameManager.OnGameResume += OnGameResume;
    }

    private void OnDisable()
    {
        PlayerTankHealth.OnHealthChange -= OnPlayerHealthChange;
        PlayerTankHealth.OnTakeDamage -= OnPlayerTakeDamage;
        GameManager.OnGamePaused -= OnGamePaused;
        GameManager.OnGameResume -= OnGameResume;
    }

    private void OnGameResume() => m_IsPaused = false;
    private void OnGamePaused() => m_IsPaused = true;
    private void OnPlayerTakeDamage()
    {
        m_PlayerHurtFX.enabled = true;
        m_PlayerHurtFX.weight = 1;
    }

    private void OnPlayerHealthChange(float health, float armor)
    {
        m_LastKnownPlayerHealth = health;
        m_PlayerIsNearDeath = health <= 30.0F;
    }

    private void Update()
    {
        ProcessPauseFX();

        ProcessMainFX();

        ProcessHurtFX();

        ProcessDeathFX();
    }

    private void ProcessDeathFX()
    {
        //Only enabled the near death fx when the player is about to die...
        m_NearDeathFX.enabled = m_PlayerIsNearDeath;

        if (m_NearDeathFX.enabled)
        {
            m_NearDeathFX.weight = m_PlayerIsNearDeath ?
                Mathf.Lerp(m_NearDeathFX.weight, 1 - m_LastKnownPlayerHealth / 100, Time.deltaTime)
                : Mathf.Lerp(m_NearDeathFX.weight, 0, Time.deltaTime);
        }
    }

    private void ProcessMainFX()
    {
        m_MainFX.weight = m_PlayerIsNearDeath ?
         Mathf.Lerp(m_MainFX.weight, 1 - m_NearDeathFX.weight, Time.deltaTime)
        : Mathf.Lerp(m_MainFX.weight, 1, Time.deltaTime);
    }

    private void ProcessHurtFX()
    {
        if (m_PlayerHurtFX.enabled)
        {
            m_PlayerHurtFX.weight = Mathf.Lerp(m_PlayerHurtFX.weight, 0, Time.deltaTime);

            m_TakeDamageTimer += Time.deltaTime;

            if (m_TakeDamageTimer >= m_TakeDamageDuration)
            {
                m_PlayerHurtFX.enabled = false;

                m_TakeDamageTimer = 0;
            }
        }
    }

    private void ProcessPauseFX()
    {
        if (m_IsPaused)
        {
            if (!m_PausedFX.enabled)
                m_PausedFX.enabled = true;

            m_PausedFX.weight =
                Mathf.SmoothDamp(
                    m_PausedFX.weight,
                    1,
                    ref m_BlurVelocityPaused,
                    m_PauseMenuBlurDuration
                );
        }

        if (!m_IsPaused && m_PausedFX.enabled)
        {
            m_PausedFX.weight =
                    Mathf.SmoothDamp(
                    m_PausedFX.weight,
                    0,
                    ref m_BlurVelocityPaused,
                    m_PauseMenuBlurDuration
                );
        }
    }

    private void OnDestroy()
    {
        Instance = null;
        Debug.Log("Destroyed Post FX Manger Instance!");
    }

    public void TriggerCameraShakeFX(float duration, float intensity) => StartCoroutine(TriggerCamShakeFX(duration, intensity));

    private IEnumerator TriggerCamShakeFX(float duration, float intensity)
    {
        if (m_CameraShakeFX != null)
        {
            m_CameraShakeFX.enabled = true;

            float timer = 0;

            float halfDuration = duration / 2;

            float intensityClamped = Mathf.Clamp01(intensity);

            //Fade in CAMERA SHAKE FX IN 
            do
            {
                //fade in 
                m_CameraShakeFX.weight = Mathf.Lerp(0, intensityClamped, timer / halfDuration - 0.1F);

                timer += Time.deltaTime;

                if (timer + 0.01F >= halfDuration)
                    timer = halfDuration;

                yield return new WaitForEndOfFrame();

            } while (timer < halfDuration);

            //Fade out camera fx

            do
            {
                //fade OUT 
                m_CameraShakeFX.weight = Mathf.Lerp(intensityClamped, 0, timer / halfDuration - 0.1F);

                timer += Time.deltaTime;

                if (timer + 0.01F >= halfDuration)
                    timer = halfDuration;

                yield return new WaitForEndOfFrame();

            } while (timer < halfDuration);

            m_CameraShakeFX.weight = 0;

            m_CameraShakeFX.enabled = false;

            yield break;
        }
        else
            Debug.LogError("Camera Shake FX not assigned in Post Process Manager");
    }

}