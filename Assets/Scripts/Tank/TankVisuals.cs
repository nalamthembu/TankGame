using UnityEngine;

public class TankVisuals : MonoBehaviour
{
    BaseTank m_Tank;

    [Tooltip("The tanks wheel track mesh renderers")]
    [SerializeField] MeshRenderer m_LeftTrack, m_RightTrack;
    [SerializeField][Min(0.1f)] float m_TrackRollRateDivider = 17.0F;    
    

    //This is the material instance that will be manipulated in real time.
    Material m_RealtimeMaterial;

    private void Awake()
    {
        if (TryGetComponent<BaseTank>(out var tank))
        {
            m_Tank = tank;

            if (!m_LeftTrack || !m_RightTrack)
            {
                Debug.LogError("Both tracks must be assigned to this tank!");

                enabled = false;

                return;
            }

            m_RealtimeMaterial = new(m_LeftTrack.material);

            m_RightTrack.material = m_RealtimeMaterial;
            m_LeftTrack.material = m_RealtimeMaterial;
        }
        else
            Debug.LogError("There is no Tank attached to this object!");
    }

    private void Update()
    {
        if (m_RealtimeMaterial)
        {
            //if the rpm is negative the direction is in reverse, if its positive, the tank is moving forward.
            float direction = Mathf.Clamp(m_Tank.GetAverageRPM(), -1, 1);

            m_RealtimeMaterial.mainTextureOffset += (m_Tank.Speed * -direction / m_TrackRollRateDivider) * Time.deltaTime * Vector2.up;
        }
    }
}