using UnityEngine;

/// <summary>
/// This class is meant to draw an overlap sphere and cause area of affect damage,
/// this object is typically attached to a projectile or explosive of some sort.
/// </summary>
public class Explosion : MonoBehaviour
{
    [Tooltip("How long is this explosion allowed to be in the scene after spawning?")]
    [SerializeField][Range(2, 10)] float m_LifeTimeInSeconds = 5.0F;
    [SerializeField][Range(0.1F, 100)] float m_AreaOfAffect = 25.0F;
    [SerializeField][Range(0, 1000)] float m_ExplosionForce = 150.0F;
    [Tooltip("How intense the camera shake is when the explosion happens right next to it")]
    [SerializeField][Range(0, 100)] float m_CameraShakeIntensityAtPointBlank = 10.0F;
    [SerializeField][Range(0, 100)] float m_MaximumCameraShakeDistance = 50.0F;
    [Tooltip("How match damage will this inflict on anything in the area of affect radius?")]
    [SerializeField][Range(0, 100)] float m_Damage = 30.0F;

    [SerializeField] bool m_DebugShowAreaOfAffect;

    float m_TimeAlive;

    //Flags
    bool m_IsExploding = false;

    private void Update()
    {
        m_TimeAlive += Time.deltaTime;

        if (m_TimeAlive >= m_LifeTimeInSeconds)
            gameObject.SetActive(false);

    }

    private void OnValidate()
    {
        if (m_AreaOfAffect <= 0)
        {
            Debug.LogWarning("Area of affect is too low, setting to 0.1F");
            m_AreaOfAffect = 0.1F;
        }
    }

    public void Explode()
    {
        m_IsExploding = true;


        //Apply Camera Shake FX
        if (ThirdPersonTankCamera.Instance != null)
        {
            //Shake the camera based on how far this object is from it...

            float distanceFromCamera = Vector3.Distance(transform.position, ThirdPersonTankCamera.Instance.transform.position);

            float camShakeIntensity = Mathf.Lerp(0, m_CameraShakeIntensityAtPointBlank, m_MaximumCameraShakeDistance / distanceFromCamera);

            float camShakeDuration = 0.25F;

            StartCoroutine(ThirdPersonTankCamera.Instance.DoCameraShake(camShakeDuration, 5.0F, camShakeIntensity - 0.1F));

            if (PostProcessManager.Instance != null)
            {
                //Do the shake fx based on how far this object is from it...
                float postFXIntensity = m_MaximumCameraShakeDistance / distanceFromCamera;
                PostProcessManager.Instance.TriggerCameraShakeFX(camShakeDuration * 2, postFXIntensity);
            }
            else
                Debug.LogError("Post Process Manager Instance is non existent!");

        }
        else
            Debug.LogError("Third Person Tank Camera instance is non existent!");

        //Collect an array of all the colliders affected
        Collider[] affectedColliders = Physics.OverlapSphere(transform.position, m_AreaOfAffect);

        if (affectedColliders.Length > 0)
        {
            for (int i = 0; i < affectedColliders.Length; i++)
            {
                if (affectedColliders[i] != null)
                {
                    if (affectedColliders[i].TryGetComponent(out Rigidbody affectedRigidbody))
                    {
                        MeshRenderer renderer = affectedRigidbody.GetComponent<MeshRenderer>();

                        //Draw A line from explosion point to collider and add impulse to the object 
                        if (Physics.Linecast(transform.position, renderer.bounds.center, out RaycastHit hit))
                        {
                            affectedRigidbody.AddForce(2 * m_ExplosionForce * -hit.normal, ForceMode.Impulse);

                            Debug.DrawRay(hit.point, -hit.normal * 2, Color.green, 5.0F);
                        }

                        //Any object that needs to take damage will be given damage here...

                        if (affectedRigidbody.TryGetComponent<TankHealth>(out var tankHealth))
                        {
                            //Damage is scaled by how far you are from ground zero...

                            float damageScalar = Mathf.Clamp01(hit.distance / m_AreaOfAffect);
                           
                            tankHealth.TakeDamage(m_Damage * damageScalar);
                        }
                        
                    }
                }
            }
        }

        //Play Sound
        if (SoundManager.Instance)
            SoundManager.Instance.PlayInGameSound("Explosion", transform.position, true, 30.0F);
        else
            Debug.LogError("There is no Sound Manager in the scene!");
    }

    private void OnDisable()
    {
        m_IsExploding = false;

        m_TimeAlive = 0;
    }

    private void OnDrawGizmos()
    {
        if (m_IsExploding && m_DebugShowAreaOfAffect)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, m_AreaOfAffect);
        }
    }
}