using UnityEngine;

public class SingleTargetCamera : BaseCamera
{
    [SerializeField] protected Transform m_Target;

    public override void Awake()
    {
        base.Awake();

        if (m_Target is null)
        {
            Debug.LogError("Target is null on this camera! Disabling!");

            m_IsActive = false;
        }
    }
}