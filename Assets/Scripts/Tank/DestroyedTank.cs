﻿using UnityEngine;

/// <summary>
/// This class is used on the tank carcasses that are pooled in the object pool manager.
/// </summary>
/// 
public class DestroyedTank : MonoBehaviour
{
    [Header("---------Lifetime----------")]
    [Tooltip("How long is the destroyed tank allowed to be in the scene?")]
    [SerializeField] float m_LifeTimeInSeconds = 10.0F;
    float m_TimeInScene = 0;

    MeshCollider m_MeshCollider;

    public void InitialiseDestroyedTank()
    {
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