using UnityEngine;

[System.Serializable]
public class ThirdPersonTankCamera : ThirdPersonCameraSingleTarget
{
    [SerializeField] float m_CameraRotationSpeed = 1;
    [SerializeField] CameraPositionSettings[] m_CameraSettings;
    [Tooltip("What is the max speed when the screen rumbles the most?")]
    [SerializeField] float m_MaxSpeedAtPeakRumble = 80.0F;

    float m_DesiredDistanceFromTarget;

    PlayerTank m_Tank;

    new public static ThirdPersonTankCamera Instance;

    int m_CurrentSettingIndex;

    public override void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }


    public override void Start()
    {
        base.Start();

        SetupCameraToFocusOnPlayer();
    }

    private void SetupCameraToFocusOnPlayer()
    {

        if (m_Tank is null)
        {
            if (PlayerTank.PlayerTankInstance)
            {
                m_Tank = PlayerTank.PlayerTankInstance;

                for (int i = 0; i < PlayerTank.PlayerTankInstance.transform.childCount; i++)
                {
                    if (PlayerTank.PlayerTankInstance.transform.GetChild(i).name.Equals("Tank_Camera_Focus"))
                    {
                        m_Target = PlayerTank.PlayerTankInstance.transform.GetChild(i);
                        break;
                    }
                }
            }
            else
                Debug.LogError("There is no player tank in this scene!");
        }
    }

    public override void OnDestroy()
    {
        Instance = null;

        Debug.Log("Third Person Tank Camera Instance destroyed!");
    }

    public override void Update()
    {
        if (GameManager.Instance && GameManager.Instance.GameIsPaused)
            return;

        base.Update();

        //Change Camera View
        if (Input.GetKeyDown(KeyCode.V))
        {
            m_CurrentSettingIndex++;

            if (m_CurrentSettingIndex >= m_CameraSettings.Length)
                m_CurrentSettingIndex = 0;
        }
    }

    protected override void DoUpdatePosition()
    {
        m_DesiredDistanceFromTarget = Mathf.Lerp(m_DesiredDistanceFromTarget, m_CameraSettings[m_CurrentSettingIndex].DistanceFromTarget, Time.deltaTime * 2);

        Vector3 desiredPosition = m_Target.position - transform.forward *
             m_DesiredDistanceFromTarget + Vector3.up *
            m_CameraSettings[m_CurrentSettingIndex].HeightAboveTarget;

        // Check for camera collision
        float obstacleOffset = 0.1f; // Adjust this offset value as needed
        if (Physics.Raycast(m_Target.position, -transform.forward, out RaycastHit hit, m_DesiredDistanceFromTarget + obstacleOffset, m_CollisionLayers))
        {
            // If there is an obstacle, pull the camera in closer with an offset
            desiredPosition = hit.point + transform.forward * obstacleOffset;
        }

        transform.position = desiredPosition;
    }

    public override void LateUpdate()
    {
        base.LateUpdate();

        if (m_HandHeldEffectEnabled)
        {
            DoHandHeldEffect(PlayerTank.PlayerTankInstance.Speed / m_MaxSpeedAtPeakRumble, m_HandHeldSmoothing);
        }
    }

    protected override void DoUpdateRotation()
    {
        if (Input.GetKey(KeyCode.C))
        {
            Vector3 LookRotation = Quaternion.LookRotation(-m_Tank.transform.forward, Vector3.up).eulerAngles;

            LookRotation.x = m_CameraSettings[m_CurrentSettingIndex].XAxisAngle;

            transform.eulerAngles = LookRotation;

            return;
        }

        Vector3 targetRotation = new(m_CameraSettings[m_CurrentSettingIndex].XAxisAngle, m_Tank.GetTurretTransform().eulerAngles.y, 0);

        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(targetRotation), Time.deltaTime * m_CameraRotationSpeed);
    }



    [System.Serializable]
    public struct CameraPositionSettings
    {
        [Range(1, 50)] public float DistanceFromTarget;

        [Range(1, 50)] public float HeightAboveTarget;

        [Range(1, 90)] public float XAxisAngle;

        public void OnValidate()
        {
            if (DistanceFromTarget <= 0)
                DistanceFromTarget = 3.0F;

            if (HeightAboveTarget <= 0)
                HeightAboveTarget = 1.5F;

            if (XAxisAngle <= 0)
                XAxisAngle = 15.0F;
        }
    }
}