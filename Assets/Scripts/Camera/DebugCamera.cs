using UnityEngine;

[System.Serializable]
public class DebugCamera : BaseCamera
{
    public static DebugCamera Instance;

    [SerializeField] Vector2 m_PitchLimits = new(-45, 90);
    [SerializeField] Vector2 m_FOVLimits = new(10, 180);
    [SerializeField] [Range(0, 10)] float m_PositionSmoothTime = 0.5F;
    [SerializeField] [Range(0, 50)] float m_RotationSpeed = 10;
    [SerializeField] [Range(1, 50)] float m_Sensitivity = 10;
    [SerializeField] float m_CurrentSpeed = 1;

    Vector3 m_PositionVelocity;

    protected override void DoUpdatePosition()
    {
        Vector3 desiredPosition = transform.position;

        if (Input.GetKey(KeyCode.E))
            desiredPosition += m_CurrentSpeed * 100 * Time.deltaTime * Vector3.up;
        else if (Input.GetKey(KeyCode.Q))
            desiredPosition += m_CurrentSpeed * 100 * Time.deltaTime * Vector3.down;

        desiredPosition += Input.GetAxisRaw("Vertical") * Time.deltaTime * m_CurrentSpeed * 100 * transform.forward + Input.GetAxisRaw("Horizontal") * Time.deltaTime * m_CurrentSpeed * 100 * transform.right;

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref m_PositionVelocity, m_PositionSmoothTime);
    }

    protected override void DoUpdateFOV()
    {
        float desiredFOV = m_AttachedCamera.fieldOfView;

        if (Input.GetKey(KeyCode.LeftBracket))
            desiredFOV += Time.deltaTime * 1000;


        if (Input.GetKey(KeyCode.RightBracket))
            desiredFOV += Time.deltaTime * -1000;

        desiredFOV = Mathf.Clamp(desiredFOV, m_FOVLimits.x, m_FOVLimits.y);

        m_AttachedCamera.fieldOfView = Mathf.Lerp(m_AttachedCamera.fieldOfView, desiredFOV, Time.deltaTime * m_CurrentSpeed);
    }

    protected override void DoUpdateRotation()
    {
        m_Yaw += Input.GetAxisRaw("Mouse X") * m_Sensitivity;
        m_Pitch -= Input.GetAxisRaw("Mouse Y") * m_Sensitivity;
        m_Pitch = Mathf.Clamp(m_Pitch, m_PitchLimits.x, m_PitchLimits.y);

        Vector3 targetRotation = new()
        {
            x = m_Pitch,
            y = m_Yaw,
            z = 0
        };

        Quaternion desiredQuat = Quaternion.Euler(targetRotation);

        transform.rotation = Quaternion.Lerp(transform.rotation, desiredQuat, Time.deltaTime * m_RotationSpeed);
    }

    public override void Initialise(Transform selfTransform, float initialFOV)
    {
        if (Instance == null)
        {
            Instance = this;
        }

        base.Initialise(selfTransform, initialFOV);
    }

    protected override void DoUpdateSpeed()
    {
        m_CurrentSpeed += Input.mouseScrollDelta.y * 0.5F;

        m_CurrentSpeed = Mathf.Clamp(m_CurrentSpeed, 1, 100);
    }
}