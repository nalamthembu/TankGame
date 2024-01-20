using UnityEngine;

public class GameCamera : MonoBehaviour
{
    public ThirdPersonTankCamera m_ThirdPersonTankCamera;

    public DebugCamera m_DebugCamera;

    public static GameCamera Instance;

    private void OnDestroy()
    {
        m_ThirdPersonTankCamera.OnDestroy();
        m_DebugCamera.OnDestroy();

        Instance = null;

        Debug.Log("Destroyed Game Camera Instance!");
    }

    private void Start()
    {
        m_ThirdPersonTankCamera.Start();
        m_DebugCamera.Start();
    }

    private void Awake()
    {
        if (Instance is null)
            Instance = this;
        else
            Destroy(gameObject);

        m_ThirdPersonTankCamera.Initialise(transform, 60);
        m_DebugCamera.Initialise(transform, 50);
    }

    private void OnEnable()
    {
        Explosion.OnExplode += OnExplosionInWorld;
        PlayerTank.OnPlayerShot += OnExplosionInWorld;
        PlayerTank.OnPlayerBigCollision += OnExplosionInWorld;
    }

    private void OnDisable()
    {
        Explosion.OnExplode -= OnExplosionInWorld;
        PlayerTank.OnPlayerShot -= OnExplosionInWorld;
        PlayerTank.OnPlayerBigCollision -= OnExplosionInWorld;
    }

    private void OnExplosionInWorld(float duration,float magnitude)
    {
        if (!m_ThirdPersonTankCamera.DebugDebugDisableCameraShake)
            StartCoroutine(m_ThirdPersonTankCamera.DoCameraShake(duration, magnitude));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tilde))
        {
            m_DebugCamera.m_IsActive = !m_DebugCamera.m_IsActive;
            m_ThirdPersonTankCamera.m_IsActive = !m_ThirdPersonTankCamera.m_IsActive;
        }

        if (m_ThirdPersonTankCamera.m_IsActive)
            m_ThirdPersonTankCamera.Update();

        if (m_DebugCamera.m_IsActive)
            m_DebugCamera.Update();

    }

    private void LateUpdate()
    {
        if (m_ThirdPersonTankCamera.m_IsActive)
            m_ThirdPersonTankCamera.LateUpdate();

        if (m_DebugCamera.m_IsActive)
            m_DebugCamera.LateUpdate();
    }
}
