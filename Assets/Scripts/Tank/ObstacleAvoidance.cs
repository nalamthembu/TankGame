using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(AITank))]
public class ObstacleAvoidance : MonoBehaviour
{
    [Header("----------General----------")]

    [SerializeField] Sensor[] m_Sensors;

    AITank m_Tank;

    public bool OverrideSteering { get; private set; }

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
        DoAvoidObstacles();
    }

    private void DoAvoidObstacles()
    {
        float steeringToApply = 0;

        OverrideSteering = false;

        foreach (Sensor sensor in m_Sensors)
        {
            OverrideSteering = sensor.HasHitSomething;

            if (sensor.HasHitSomething)
            {
                switch (sensor.dectorType)
                {
                    case Sensor.DetectorType.LEFT:
                    case Sensor.DetectorType.CORNERLF:

                        steeringToApply += -sensor.SteerAmount;

                        break;

                    case Sensor.DetectorType.RIGHT:
                    case Sensor.DetectorType.CORNERRF:

                        steeringToApply += sensor.SteerAmount;

                        break;
                }
            }
        }

        steeringToApply = Mathf.Clamp(-steeringToApply, -1, 1);

        m_Tank.ApplySteer(steeringToApply);
    }

    private void OnDrawGizmosSelected()
    {
        if (m_Sensors != null && m_Sensors.Length > 0)
        {
            for (int i = 0; i < m_Sensors.Length; i++)
            {
                m_Sensors[i].OnDrawGizmosSelected();
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
            CORNERRF
        }

        public DetectorType dectorType;

        public Transform transform;

        [Min(1)] public float maxDistance;

        public Vector3 HitPoint { get; private set; }

        public float SteerAmount { get; private set; }

        private bool m_HasHitSomething;

        public bool HasHitSomething { get { return m_HasHitSomething; } }

        public void DetectCollision()
        {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, maxDistance))
            {
                HitPoint = hit.point;

                SteerAmount = Mathf.InverseLerp(1, 0, hit.distance / maxDistance);

                m_HasHitSomething = true;
            }
            else
            {
                SteerAmount = 0;

                m_HasHitSomething = false;
            }
        }


#if UNITY_EDITOR

        public void OnDrawGizmosSelected()
        {
            DetectCollision();

            Gizmos.color = Color.green;

            Vector3 end = transform.position + transform.forward * maxDistance;

            Gizmos.DrawLine(transform.position, m_HasHitSomething ? HitPoint : end);

            Gizmos.color = m_HasHitSomething ? Color.blue : Color.red;

            Gizmos.DrawWireCube(m_HasHitSomething ? HitPoint : end, Vector3.one * 0.25F);

            Vector3 labelPos = transform.position + transform.forward * maxDistance / 2;

            Handles.Label(labelPos, "Steer Amount : " + SteerAmount); 
        }

#endif
    }
}

