using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(AITank))]
public class ObstacleAvoidance : MonoBehaviour
{
    [Header("----------General----------")]
    [SerializeField] Sensor[] m_Sensors;
    [SerializeField] [Min(0.01F)] float m_CountersteerIntensity = 1;
    [SerializeField] [Min(0.01F)] float m_BrakeIntensity = 1;
    AITank m_Tank;

    [Header("----------Debugging----------")]
    [SerializeField] bool m_IsDebuggingEnabled = false;
    [SerializeField] bool m_ShowEvenWhenNotSelected = false;

    public bool OverrideSteering { get; private set; }
    public bool OverrideBrake { get; private set; }

    private void Awake()
    {
        m_Tank = GetComponent<AITank>();

        if (m_Tank is null)
        {
            Debug.LogError("This object must be attached to an AI Tank");

            enabled = false;
        }
    }

    private void FixedUpdate()
    {
        if (m_Tank.IsReversing)
            return;

        DoAvoidObstacles();
    }

    private void DoAvoidObstacles()
    {
        float steeringToApply = 0;

        float brakeToApply = 0;

        OverrideSteering = false;
        OverrideBrake = false;

        for (int i = 0; i < m_Sensors.Length; i++)
        {
            m_Sensors[i].DetectCollision();
            OverrideSteering = m_Sensors[i].HasHitSomething;
            OverrideBrake = m_Sensors[i].HasHitSomething;

            if (m_Sensors[i].HasHitSomething)
            {
                //Steering...
                switch (m_Sensors[i].dectorType)
                {
                    case Sensor.DetectorType.LEFT:
                    case Sensor.DetectorType.CORNERLF:
                    case Sensor.DetectorType.CORNERFWDL:

                        steeringToApply += m_Sensors[i].SteerAmount * m_CountersteerIntensity;

                        break;

                    case Sensor.DetectorType.RIGHT:
                    case Sensor.DetectorType.CORNERRF:
                    case Sensor.DetectorType.CORNERFWDR:

                        steeringToApply -= m_Sensors[i].SteerAmount * m_CountersteerIntensity;

                        break;
                }

                //Braking
                switch (m_Sensors[i].dectorType)
                {
                    case Sensor.DetectorType.CORNERLF:
                    case Sensor.DetectorType.CORNERFWDL:

                        brakeToApply += m_Sensors[i].BrakeAmount * m_BrakeIntensity;

                        break;

                    case Sensor.DetectorType.CORNERRF:
                    case Sensor.DetectorType.CORNERFWDR:

                        brakeToApply += m_Sensors[i].BrakeAmount * m_BrakeIntensity;

                        break;
                }
            }
        }

        m_Tank.ApplySteer(steeringToApply);
    }

    private void OnDrawGizmosSelected()
    {
        if (!m_IsDebuggingEnabled)
            return;

        if (m_Sensors != null && m_Sensors.Length > 0)
        {
            foreach(Sensor sensor in m_Sensors)
            {
                sensor.OnDrawGizmosSelected();
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!m_IsDebuggingEnabled && !m_ShowEvenWhenNotSelected)
            return;

        if (m_Sensors != null && m_Sensors.Length > 0)
        {
            foreach (Sensor sensor in m_Sensors)
            {
                sensor.OnDrawGizmos();
            }
        }
    }

    [System.Serializable]
    public struct Sensor
    {
        public enum DetectorType
        {
            FRONT,
            BACK,
            LEFT,
            RIGHT,
            CORNERLF,
            CORNERRF,
            CORNERFWDL,
            CORNERFWDR
        }

        public DetectorType dectorType;

        public Transform transform;

        [Min(1)] public float maxDistance;

        public Vector3 HitPoint { get; private set; }

        public float SteerAmount { get; private set; }

        public float BrakeAmount { get; private set; }


        private bool m_HasHitSomething;

        //On a scale of 0 and 1 how close are we from the obstacle?
        public float PercentageDistFromObstacle { get; private set; }

        public bool HasHitSomething { get { return m_HasHitSomething; } }

        public void DetectCollision()
        {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, maxDistance))
            {
                HitPoint = hit.point;

                PercentageDistFromObstacle = Mathf.InverseLerp(1, 0, hit.distance / maxDistance);

                SteerAmount = PercentageDistFromObstacle;

                BrakeAmount = PercentageDistFromObstacle;

                m_HasHitSomething = true;
            }
            else
            {
                SteerAmount = 0;

                BrakeAmount = 0;

                m_HasHitSomething = false;
            }
        }

        public void OnDrawGizmos() => ShowDebugGizmos();

        public void OnDrawGizmosSelected() => ShowDebugGizmos();

        private void ShowDebugGizmos()
        {
            Gizmos.color = Color.green;

            Vector3 end = transform.position + transform.forward * maxDistance;

            Gizmos.DrawLine(transform.position, m_HasHitSomething ? HitPoint : end);

            Gizmos.color = m_HasHitSomething ? Color.blue : Color.red;

            Gizmos.DrawWireCube(m_HasHitSomething ? HitPoint : end, Vector3.one * 0.25F);

#if UNITY_EDITOR            
            Vector3 labelPos = transform.position + transform.forward * maxDistance / 2;
            Handles.Label(labelPos, "Steer Amount : " + SteerAmount + "\nBrake Amount : " + BrakeAmount);
#endif
        }

    }
}

