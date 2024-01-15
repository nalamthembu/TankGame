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
