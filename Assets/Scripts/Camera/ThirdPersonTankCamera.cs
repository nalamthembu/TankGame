using UnityEngine;

[System.Serializable]
public class ThirdPersonTankCamera : ThirdPersonCameraSingleTarget
{
    [SerializeField] float m_CameraRotationSpeed = 1;
    [SerializeField] float m_Height = 1.5F;
    [SerializeField] float m_XAxisAngle = 15.0F;
    [Tooltip("What is the max speed when the screen rumbles the most?")]
    [SerializeField] float m_MaxSpeedAtPeakRumble = 80.0F;

    Tank m_Tank;

    new public static ThirdPersonTankCamera Instance;

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
            if (Tank.PlayerTankInstance)
            {
                m_Tank = Tank.PlayerTankInstance;

                for (int i = 0; i < Tank.PlayerTankInstance.transform.childCount; i++)
                {
                    if (Tank.PlayerTankInstance.transform.GetChild(i).name.Equals("Tank_Camera_Focus"))
                    {
                        m_Target = Tank.PlayerTankInstance.transform.GetChild(i);
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

    protected override void DoUpdatePosition()
    {
        Vector3 desiredPosition = m_Target.position - transform.forward * m_DistFromTarget + Vector3.up * m_Height;

        transform.position = desiredPosition;
    }

    public override void LateUpdate()
    {
        base.LateUpdate();

        if (m_HandHeldEffectEnabled)
        {
            DoHandHeldEffect(Tank.PlayerTankInstance.Speed / m_MaxSpeedAtPeakRumble, m_HandHeldSmoothing);
        }
    }

    protected override void DoUpdateRotation()
    {
        if (Input.GetKey(KeyCode.C))
        {
            Vector3 LookRotation = Quaternion.LookRotation(-m_Tank.transform.forward, Vector3.up).eulerAngles;

            LookRotation.x = m_XAxisAngle;

            transform.eulerAngles = LookRotation;

            return;
        }

        Vector3 targetRotation = new(m_XAxisAngle, m_Tank.GetTurretTransform().eulerAngles.y, 0);

        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(targetRotation), Time.deltaTime * m_CameraRotationSpeed);
    }
}