using UnityEngine;

/// <summary>
/// This class is used on the tank carcasses that are pooled in the object pool manager.
/// </summary>
/// 
[RequireComponent(typeof(MeshCollider), typeof(Rigidbody))]
public class DestroyedTank : MonoBehaviour
{
    [Header("---------Lifetime----------")]
    [Tooltip("How long is the destroyed tank allowed to be in the scene?")]
    [SerializeField] float m_LifeTimeInSeconds = 10.0F;
    float m_TimeInScene = 0;

    [Header("----------General----------")]
    [SerializeField] float m_UpwardForceOnInitialise = 100;

    Rigidbody m_RigidBody;

    private void Awake()
    {
        m_RigidBody = GetComponent<Rigidbody>();
    }

    MeshCollider m_MeshCollider;

    public void InitialiseDestroyedTank(Vector3 initialVelocity)
    {
        m_RigidBody.velocity = initialVelocity;

        m_RigidBody.AddForce(transform.position + Vector3.up * m_UpwardForceOnInitialise, ForceMode.Impulse);

        if (ObjectPoolManager.Instance != null)
        {
            //Spawn a bunch of fire fx around the destoyed tank...

            int FireFXCount = Random.Range(1, 10);

            for (int i = 0; i < FireFXCount; i++)
            {
                if (ObjectPoolManager.Instance.TryGetPool("FireFX", out var fireFXPool))
                {
                    if (fireFXPool.TryGetGameObject(out var fireFX))
                    {
                        Vector3 randomPosInRadius = Random.insideUnitSphere * 30;

                        randomPosInRadius.y = 0;

                        fireFX.transform.position = transform.position + randomPosInRadius;
                    }
                }
            }

            //Spawn an initial explosion

            if (ObjectPoolManager.Instance.TryGetPool("ShellExplosionGO", out var explosionPool))
            {
                if (explosionPool.TryGetGameObject(out var explosion))
                {
                    explosion.transform.position = transform.position + transform.up * 2;

                    explosion.transform.eulerAngles = transform.up;
                }
            }
        }
        else
            Debug.LogError("There is no Object Pool Manager in this scene!");
    }

    private void OnValidate()
    {
        if (!m_MeshCollider)
        {
            if (TryGetComponent<MeshCollider>(out var meshCollider))
            {
                m_MeshCollider = meshCollider;

                m_MeshCollider.convex = true;

                return;
            }
        }

        if (m_MeshCollider && !m_MeshCollider.convex)
            m_MeshCollider.convex = true;

    }

    private void Update()
    {
        //Do not allow this projectile to be in the scene for > m_LifeTimeSeconds without exploding

        m_TimeInScene += Time.deltaTime;

        if (m_TimeInScene >= m_LifeTimeInSeconds)
        {
            gameObject.SetActive(false);

            m_TimeInScene = 0;
        }
    }
}