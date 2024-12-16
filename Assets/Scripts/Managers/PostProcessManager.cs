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

    private static PostProcessManager _instance;
    public static PostProcessManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<PostProcessManager>();
            return _instance;
        }
    }

    float m_LastKnownPlayerHealth;

    //Flags
    bool m_PlayerIsNearDeath;

    // Coroutines
    Coroutine OnNearDeathCoroutine;
    Coroutine OnHealedBeforeDeathCoroutine;
    Coroutine OnPlayerHurtCoroutine;
    Coroutine OnPauseCoroutine;
    Coroutine OnResumeCoroutine;
    Coroutine OnTriggerCameraShake;

    private void Awake() => CheckForNullReferences();
    
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

    #region Events

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
    private void OnGameResume()
    {
        m_PausedFX.weight = 0;
        if (m_PausedFX.enabled) m_PausedFX.enabled = false;
    } 
    private void OnGamePaused()
    {
        if (m_PausedFX.enabled == false) m_PausedFX.enabled = true;
        m_PausedFX.weight = 1;
    }
    private void OnPlayerTakeDamage()
    {
        if (OnPlayerHurtCoroutine != null)
            StopCoroutine(OnPlayerHurt());

        OnPlayerHurtCoroutine = StartCoroutine(OnPlayerHurt());
    }
    private void OnPlayerHealthChange(float health, float armor)
    {
        m_LastKnownPlayerHealth = health;
        m_PlayerIsNearDeath = health <= 30.0F;

        // If the player is about to die
        if (m_PlayerIsNearDeath && !m_NearDeathFX.enabled && OnNearDeathCoroutine == null)
        {
            if (OnHealedBeforeDeathCoroutine != null)
            {
                StopCoroutine(OnHealedBeforeDeathCoroutine);
                OnHealedBeforeDeathCoroutine = null;
            }

            OnNearDeathCoroutine = StartCoroutine(OnNearDeath());
        }

        // if the player is actually okay
        if (!m_PlayerIsNearDeath && m_NearDeathFX.enabled && OnHealedBeforeDeathCoroutine == null)
        {
            if (OnNearDeathCoroutine != null)
            {
                StopCoroutine(OnNearDeathCoroutine);
                OnNearDeathCoroutine = null;
            }

            OnHealedBeforeDeathCoroutine = StartCoroutine(OnHealedBeforeDeath());
        }
    }
    public void TriggerCameraShakeFX(float duration, float intensity)
    {
        if (OnTriggerCameraShake != null)
            StopCoroutine(OnTriggerCameraShake);

        OnTriggerCameraShake = StartCoroutine(TriggerCamShakeFX(duration, intensity));
    }

    #endregion

    #region Coroutines
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

        Debug.Log("done (camera shake coroutine)");
    }

    private IEnumerator OnHealedBeforeDeath()
    {
        if (!m_NearDeathFX.enabled) yield break;
        else
        {
            float duration = 2;
            float refValue = 0;
            float targetValue = 0;

            while (m_NearDeathFX.weight > targetValue)
            {
                m_NearDeathFX.weight = Mathf.SmoothDamp(m_NearDeathFX.weight, targetValue, ref refValue, duration);

                m_MainFX.weight = 1 - m_NearDeathFX.weight;

                yield return null;
            }

            m_NearDeathFX.enabled = false;
        }

        Debug.Log("done (on healed before death coroutine)");
    }

    private IEnumerator OnNearDeath()
    {
        Debug.Log("started (near death coroutine)");

        //Only enabled the near death fx when the player is about to die...
        m_NearDeathFX.enabled = m_PlayerIsNearDeath;

        float duration = .25f;

        float refValue = 0;
        float targetValue = 1;

        while (m_NearDeathFX.weight < targetValue)
        {
            m_NearDeathFX.weight = Mathf.SmoothDamp(m_NearDeathFX.weight, targetValue, ref refValue, duration);

            // Slowly fade out the regular post processing
            m_MainFX.weight = 1 - m_NearDeathFX.weight;

            yield return null;
        }

        Debug.Log("done (near death coroutine)");
    }

    private IEnumerator OnPlayerHurt()
    {
        m_PlayerHurtFX.enabled = true;

        // Max out the effect on hit
        m_PlayerHurtFX.weight = 1;

        float targetValue = 0;
        float refValue = 0;

        while (m_PlayerHurtFX.weight != targetValue)
        {
            // Fade out the effect in due time.
            m_PlayerHurtFX.weight = Mathf.SmoothDamp(m_PlayerHurtFX.weight, targetValue, ref refValue, m_TakeDamageDuration);
            yield return null;
        }

        // Disable the non-visible effect
        m_PlayerHurtFX.enabled = false;

        Debug.Log("done (player hurt coroutine)");
    }

    #endregion

}