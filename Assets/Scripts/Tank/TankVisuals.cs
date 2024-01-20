using UnityEngine;

public enum TankVariant
{
    US,
    RUS,
    GER
}

public class TankVisuals : MonoBehaviour
{
    BaseTank m_Tank;
    TankHealth m_TankHealth;

    [Header("---------General----------")]
    [Tooltip("The tanks wheel track mesh renderers")]
    [SerializeField] MeshRenderer m_LeftTrack, m_RightTrack;
    [SerializeField][Min(0.1f)] float m_TrackRollRateDivider = 17.0F;

    [Tooltip("Which tank is this?")]
    [SerializeField] TankVariant m_TankVariant;

    //This is the material instance that will be manipulated in real time.
    Material m_RealtimeMaterial;

    //Flags
    bool m_HasSpawnedCarcass;

    private void Awake()
    {
        if (TryGetComponent<BaseTank>(out var tank))
        {
            m_Tank = tank;

            if (!m_LeftTrack || !m_RightTrack)
            {
                Debug.LogError("Both tracks must be assigned to this tank! Disabling...");

                enabled = false;

                return;
            }

            m_RealtimeMaterial = new(m_LeftTrack.material);

            m_RightTrack.material = m_RealtimeMaterial;
            m_LeftTrack.material = m_RealtimeMaterial;

            if (TryGetComponent<TankHealth>(out var tankHealth))
            {
                m_TankHealth = tankHealth;
            }
            else
            {
                Debug.LogError("There is no tank health on this object! Disabling...");

                enabled = false;
            }

        }
        else
            Debug.LogError("There is no Tank attached to this object!");
    }

    private void Update()
    {
        if (m_TankHealth && m_TankHealth.IsDead && !m_HasSpawnedCarcass)
        {
            SpawnCarcass();

            return;
        }

        if (m_RealtimeMaterial)
        {
            //if the rpm is negative the direction is in reverse, if its positive, the tank is moving forward.
            float direction = Mathf.Clamp(m_Tank.GetAverageRPM(), -1, 1);

            m_RealtimeMaterial.mainTextureOffset += (m_Tank.Speed * -direction / m_TrackRollRateDivider) * Time.deltaTime * Vector2.up;
        }
    }

    private void SpawnCarcass()
    {
        if (ObjectPoolManager.Instance != null)
        {
            //Which tank is this?
            switch (m_TankVariant)
            {
                case TankVariant.US:

                    if (ObjectPoolManager.Instance.TryGetPool("Tank_Destroyed_US", out var destroyedUSTankPool))
                    {
                        if (destroyedUSTankPool.TryGetGameObject(out var destroyedUSTank))
                        {
                            destroyedUSTank.transform.SetPositionAndRotation(transform.position, transform.rotation);

                            if (destroyedUSTank.TryGetComponent<DestroyedTank>(out var component))
                            {
                                component.InitialiseDestroyedTank(m_Tank.CurrentVelocity);
                            }
                            else
                                Debug.LogError("There is no destoyed tank component on the US Tank!");
                        }
                    }

                    break;

                case TankVariant.RUS:

                    if (ObjectPoolManager.Instance.TryGetPool("Tank_Destroyed_RUS", out var destroyedRUSTankPool))
                    {
                        if (destroyedRUSTankPool.TryGetGameObject(out var destroyedUSTank))
                        {
                            destroyedUSTank.transform.SetPositionAndRotation(transform.position, transform.rotation);

                            if (destroyedUSTank.TryGetComponent<DestroyedTank>(out var component))
                            {
                                component.InitialiseDestroyedTank(m_Tank.CurrentVelocity);
                            }
                            else
                                Debug.LogError("There is no destoyed tank component on the RUS Tank!");
                        }
                    }

                    break;

                case TankVariant.GER:

                    if (ObjectPoolManager.Instance.TryGetPool("Tank_Destroyed_GER", out var destroyedGERTankPool))
                    {
                        if (destroyedGERTankPool.TryGetGameObject(out var destroyedUSTank))
                        {
                            destroyedUSTank.transform.SetPositionAndRotation(transform.position, transform.rotation);

                            if (destroyedUSTank.TryGetComponent<DestroyedTank>(out var component))
                            {
                                component.InitialiseDestroyedTank(m_Tank.CurrentVelocity);
                            }
                            else
                                Debug.LogError("There is no destoyed tank component on the GER Tank!");
                        }
                    }

                    break;
            }

            m_HasSpawnedCarcass = true;

            gameObject.SetActive(false);
        }
    }
}