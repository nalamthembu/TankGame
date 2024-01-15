using UnityEngine;

[RequireComponent(typeof(WheelCollider))]
public class Wheel : MonoBehaviour
{
    [SerializeField] Transform m_WheelMesh;

    [SerializeField] bool m_IsSteeringWheel;

    private WheelSlip slip;

    public WheelSlip WheelSlip { get { return slip; } }


    public bool IsSteeringWheel { get { return m_IsSteeringWheel; } }

    private WheelCollider m_WheelCollider;

    public bool IsGrounded
    {
        get
        {
            return m_WheelCollider.isGrounded;
        }
    }

    private void Awake()
    {
        if (m_WheelCollider is null)
            m_WheelCollider = GetComponent<WheelCollider>();

        if (m_WheelMesh is null)
            Debug.LogError("There is no wheel mesh attached to this wheel!");
    }

    private void FixedUpdate()
    {
        if (m_WheelMesh)
        {
            m_WheelCollider.GetWorldPose(out Vector3 pos, out Quaternion quat);

            if (m_IsSteeringWheel)
            {
                m_WheelMesh.position = pos;

                m_WheelMesh.transform.localEulerAngles = Vector3.zero;
            }

            m_WheelMesh.SetPositionAndRotation(pos, quat);

            m_WheelCollider.GetGroundHit(out WheelHit hit);

            slip.forward = hit.forwardSlip;
            slip.sideways = hit.sidewaysSlip;
        }
    }

    public void SetWheelStiffness(float sideWaysStiffness, float fwdStiffness)
    {
        WheelFrictionCurve sidewaysFric = m_WheelCollider.sidewaysFriction;
        WheelFrictionCurve fwdFric = m_WheelCollider.forwardFriction;

        sidewaysFric.stiffness = sideWaysStiffness;
        fwdFric.stiffness = fwdStiffness;

        m_WheelCollider.sidewaysFriction = sidewaysFric;
        m_WheelCollider.forwardFriction = fwdFric;
    }

    public void SetMotorTorque(float torque) => m_WheelCollider.motorTorque = torque * 1.25F;
    public void SetBrakeTorque(float bTorque) => m_WheelCollider.brakeTorque = bTorque * bTorque;
    public void SetSteerAngle(float steerAngle) => m_WheelCollider.steerAngle = steerAngle;
    public float GetRPM() => m_WheelCollider.rpm;
}

[System.Serializable]
public struct WheelSlip
{
    public float forward;
    public float sideways;
}