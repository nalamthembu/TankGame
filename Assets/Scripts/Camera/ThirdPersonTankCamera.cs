using UnityEngine;

[System.Serializable]
public class ThirdPersonTankCamera : ThirdPersonCameraSingleTarget
{
    [SerializeField] float m_CameraRotationSpeed = 1;
    [SerializeField] float m_Height = 1.5F;
    [SerializeField] float m_XAxisAngle = 15.0F;

    Tank m_Tank;

    new public static ThirdPersonTankCamera Instance;

    public override void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        if (m_Tank is null)
        {
            if (m_Target.parent.TryGetComponent<Tank>(out var Tank))
            {
                m_Tank = Tank;
            }
            else
                Debug.LogError("There is no tank attached to the parent of the target!");
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