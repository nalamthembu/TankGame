using UnityEngine;

public class ShellProjectile : MonoBehaviour
{
    [Tooltip("How long is the shell projectile allowed to be alive in the scene without exploding?")]
    [SerializeField] float m_LifeTimeInSeconds = 10.0F;

    float m_TimeAlive = 0;

    Vector3 m_ForwardDirection, m_SpawnPosition;
    float m_ThrustForce;
    float m_ProjectileRange;

    public void SendProjectile(Vector3 forwardDirection, float thrustForce, float projectileRange)
    {
        m_ForwardDirection = forwardDirection;
        m_ThrustForce = thrustForce;
        m_SpawnPosition = transform.position;
        m_ProjectileRange = projectileRange;
        m_TimeAlive = 0;
    }

    private void Update()
    {
        //Look at target
        transform.forward = m_ForwardDirection;

        //Move forward 
        transform.position += m_ThrustForce * Time.deltaTime * transform.forward;

        //Move Down (Gravity)
        transform.position += Physics.gravity / 8 * Time.deltaTime;

        //If the distance from our spawn pos to whereever we are now is equal or beyond this range, 
        //the projectile should explode.
        if (Vector3.Distance(transform.position, m_SpawnPosition) >= m_ProjectileRange)
        {
            TriggerExplosion();

            return;
        }

        //Do not allow this projectile to be in the scene for > m_LifeTimeSeconds without exploding

        m_TimeAlive = Time.deltaTime;

        if (m_TimeAlive >= m_LifeTimeInSeconds)
        {
            gameObject.SetActive(false);

            m_TimeAlive = 0;
        }
    }

    private void TriggerExplosion(Collision collision = null)
    {
        //Spawn Explosion Particle From Object pool and dissappear.

        //lets check if the object pool manager exists in the scene first...

        if (ObjectPoolManager.Instance != null)
        {
            if (ObjectPoolManager.Instance.TryGetPool("ShellExplosionGO", out Pool pool))
            {
                if (pool.TryGetGameObject(out GameObject GOExplosion))
                {
                    //if this is trigger by a collision event
                    if (collision != null)
                    {
                        //Place Explosion at the first contact point.
                        GOExplosion.transform.position = collision.contacts[0].point;
                        //Orient it to face the negative normal direction
                        GOExplosion.transform.forward = -collision.contacts[0].normal;
                    }
                    else
                    {
                        GOExplosion.transform.position = transform.position;
                        //Orient it to face the negative forward direction
                        GOExplosion.transform.forward = -transform.forward;
                    }

                    if (GOExplosion.TryGetComponent(out Explosion explosion))
                    {
                        explosion.Explode();
                    }
                    else
                        Debug.LogError("There is no explosion object attached to this shell explosion game object!");
                }
            }
        }

        gameObject.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision) => TriggerExplosion(collision);
}