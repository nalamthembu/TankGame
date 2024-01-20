using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using Random = UnityEngine.Random;

/// <summary>
/// This is the base class for the objects with turrets
/// </summary>

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(TankVisuals))]
[RequireComponent(typeof(TankEngineAudio))]
public class BaseTank : MonoBehaviour
{
    [Header("----------Components----------")]
    [Tooltip("Where the projectiles will spawn from.")]
    [SerializeField] protected Transform m_ProjectileSpawnPoint;
    [SerializeField] protected Transform m_TurretTransform;

    [Header("----------Movement----------")]
    [Tooltip("How fast the turret to rotates to face the target.")]
    [SerializeField][Range(0, 10)] protected float m_TurretRotationSpeed = 1;
    [Tooltip("The Maximum Motor Torque")]
    [SerializeField][Range(1, 10000)] protected float m_MaxMotorTorque = 1000;
    [SerializeField][Range(1, 10000)] protected float m_MaxBrakeTorque = 1000;
    [SerializeField][Range(0, 5)] protected float m_TorqueResponseTime = 1;
    [Tooltip("The maximum angle that the wheel can steer at")]
    [SerializeField][Range(0.1f, 80)] protected float m_MaxSteerAngle = 35;
    [Tooltip("These are all the wheels attached to the tank")]
    [SerializeField] protected Wheel[] m_Wheels;
    [Tooltip("How much 'kickback' should the tank has")]
    [SerializeField] protected float m_RecoilForce = 500;
    [Tooltip("The tank will not fire if the mouse is pointing at any distance lower than this")]
    [SerializeField] protected float m_MinumumProjectileFireDistance = 5.0F;

    [Header("----------Collision----------")]
    [Tooltip("This is the maximum relative velocity vector magnitude before registering a collision as a big crash")]
    [SerializeField] protected float m_BigCrashRelativeVelocityMagnitude = 15.0F;
    [SerializeField] protected float m_MedCrashRelativeVelocityMagnitude = 5.0F;
    [SerializeField] protected float m_SmallCrashRelativeVelocityMagnitude = 2.0F;
    [SerializeField] bool m_DisplayRelativeVelocityMagnitude = false;

    public Wheel[] Wheels { get { return m_Wheels; } }

    protected float m_MotorTVelocity;
    protected float m_BrakeTVelocity;
    protected float m_MotorTorque;
    protected float m_BrakeTorque;

    [Header("----------Combat----------")]
    [SerializeField] protected int m_TotalAmmo;
    [SerializeField] protected int m_CurrentClip;
    [Tooltip("The maxmimum amount of ammo a clip can have")]
    [SerializeField] [Min(1)] protected int m_MaxClip;
    [Tooltip("How long does it takes to reload")]
    [SerializeField] protected float m_ReloadTimeInSeconds = 3;

    [Header("----------Projectile Variables----------")]
    [SerializeField] float m_ProjectileThrust = 100.0F;
    private GameObject m_GOProjectile;
    [SerializeField] protected float m_ProjectileRange = 1000.0F;

    [Header("----------VFX----------")]
    [SerializeField] ParticleSystem m_ShotParticleFX;

    [Header("----------Sound----------")]
    [Tooltip("This is where the shell shot sound will be played")]
    [SerializeField] AudioSource m_TurretShotAudioSource;
    [Tooltip("This is where the shell shot sweetener sound will be played")]
    [SerializeField] AudioSource m_TurretShotThumpAudioSource;
    [Tooltip("This is where the turret movement sounds will be played")]
    [SerializeField] AudioSource m_TurretMechanicsSource;
    [SerializeField] AudioSource m_TankBodyAudioSource;

    //Flags
    protected bool m_IsReloading;
    public bool IsFiring { get; private set; }
    public Vector3 CurrentVelocity { get { return m_RigidBody.velocity; } }
    protected bool m_IsDoneReloading;

    protected Rigidbody m_RigidBody;

    //Events
    public static event Action<BaseTank> OnSpawn;

    /// <summary>
    /// Speed is measured in Kilometres per hour (Let's use some practical units....unlike miles per hour)
    /// </summary>
    public float Speed { get; private set; }

    [SerializeField] protected bool m_DebugShowValues;

    private void OnValidate()
    {
        if (Application.isPlaying)
            return;

        if (!m_RigidBody && TryGetComponent<Rigidbody>(out var rigidbody))
            m_RigidBody = rigidbody;

        if (m_RigidBody)
        {
            //Tanks should be heavy
            if (m_RigidBody.mass < 10000)
            {
                m_RigidBody.mass = Random.Range(10000, 30000);

                Debug.LogWarning("Auto Set Tank Mass : " + m_RigidBody.mass);
            }

            //Set Centre of mass 
            if (m_RigidBody.automaticCenterOfMass)
            {
                m_RigidBody.automaticCenterOfMass = false;
                m_RigidBody.centerOfMass = Vector3.zero;

                Debug.LogWarning("Setting Centre of mass to Zero");
            }

            if (m_RigidBody.drag < 0.25F)
            {
                //Give it some drag
                Debug.LogWarning("Setting Drag to 0.25");
                m_RigidBody.drag = 0.25F;
            }
        }

        if (m_MaxClip <= 0)
            m_MaxClip = 1;
    }

    protected virtual void Awake()
    {
        m_RigidBody = GetComponent<Rigidbody>();

        if (m_RigidBody is null)
            Debug.LogError("There is no rigidbody attached to this tank!");
    }

    protected virtual void Start()
    {
        OnSpawn?.Invoke(this);
    }

    protected void RotateTurret(Vector3 LookAtTarget)
    {
        //Direction from turret to target
        Vector3 Direction = (LookAtTarget - m_TurretTransform.position).normalized;

        Vector3 DeltaDirection = Quaternion.LookRotation(Direction, Vector3.up).eulerAngles;

        DeltaDirection.x = 0;
        DeltaDirection.z = 0;

        Quaternion DeltaRotation = Quaternion.Euler(DeltaDirection);

        m_TurretTransform.rotation = Quaternion.Lerp
            (
                m_TurretTransform.rotation,
                DeltaRotation,
                Time.deltaTime * m_TurretRotationSpeed
            );
    }

    public Transform GetTurretTransform() => m_TurretTransform;

    protected virtual void ProcessFireInput() { }

    protected virtual void MoveWheels() { }

    protected virtual void Update()
    {
        //Reset fire flag.
        IsFiring = false;

        //if we still have ammo
        if (m_TotalAmmo > 0)
            ProcessFireInput();

        MoveWheels();

        if (m_RigidBody != null)
            Speed = m_RigidBody.velocity.magnitude * 3.6F;
    }

    public float GetAverageRPM()
    {
        float rpmComposite = 0;

        foreach (Wheel wheel in m_Wheels)
        {
            rpmComposite += wheel.GetRPM();
        }

        return rpmComposite / m_Wheels.Length;
    }

    protected virtual void Fire(Vector3 forwardDirection)
    {
        IsFiring = true;

        if (m_ShotParticleFX)
        {
            //Play ParticleFX
            m_ShotParticleFX.Play();

            //Play Turret Shot Sound
            if (m_TurretShotAudioSource)
            {
                if (SoundManager.Instance)
                {
                    SoundManager.Instance.PlayInGameSound("TankFX_Shot", m_TurretShotAudioSource, false, true, false, 5.0F);
                    SoundManager.Instance.PlayInGameSound("TankFX_Shot_Thump", m_TurretShotThumpAudioSource, false, true, false, 5.0F);
                }
                else
                    Debug.LogError("There is no Sound Manager in the scene!");
            }
            else
                Debug.LogError("There is no turret shot audio source attached!");
        }

        //Check for an object pool manager in the scene.
        if (ObjectPoolManager.Instance != null)
        {
            //Object pool manager will print out a message if this fails.
            if (ObjectPoolManager.Instance.TryGetPool("Tank_Shell", out Pool pool))
            {
                //Object pool will print out a message if this fails.
                if (pool.TryGetGameObject(out GameObject Projectile))
                {
                    m_GOProjectile = Projectile;

                    //Set position at projectile spawn point.
                    m_GOProjectile.transform.position = m_ProjectileSpawnPoint.position;

                    //Face the spawn point forward direction.
                    m_GOProjectile.transform.forward = m_ProjectileSpawnPoint.forward;

                    if (m_GOProjectile.TryGetComponent(out ShellProjectile shellProjectile))
                    {
                        shellProjectile.SendProjectile(forwardDirection, m_ProjectileThrust, m_ProjectileRange, this);
                    }
                    else
                    {
                        Debug.LogError("There is no shell projectile component on the shell game object!");
                    }
                }
            }
        }
    }

    protected IEnumerator Reload()
    {
        m_IsReloading = true;

        //TO-DO : Play Reload Sound Here

        yield return new WaitForSeconds(m_ReloadTimeInSeconds);

        //If our total ammo is more than a full clip.
        if (m_TotalAmmo > m_MaxClip)
        {
            m_CurrentClip = m_MaxClip; //Reload a full clip
            m_TotalAmmo -= m_CurrentClip; //Take away our current clip from our total ammo
        }
        else
        {
            //if our total ammo is less than a full clip
            //fill our clip with what's left over
            m_CurrentClip = m_TotalAmmo;
            m_TotalAmmo = 0; //we've now run out of ammo.
        }

        m_IsReloading = false;

        m_IsDoneReloading = true;
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (m_DisplayRelativeVelocityMagnitude)
            Debug.Log(collision.relativeVelocity.magnitude);

        if (SoundManager.Instance != null)
        {
            if (collision.relativeVelocity.magnitude >= m_BigCrashRelativeVelocityMagnitude)
            {
                SoundManager.Instance.PlayInGameSound("TankFX_Collision_BigCrash", collision.contacts[0].point, true, 50.0F);
                return;
            }
             
            if (collision.relativeVelocity.magnitude >= m_SmallCrashRelativeVelocityMagnitude && collision.relativeVelocity.magnitude <= m_MedCrashRelativeVelocityMagnitude)
            {
                SoundManager.Instance.PlayInGameSound("TankFX_Collision_MedCrash", collision.contacts[0].point, true, 50.0F);
                return;
            }

            if (collision.relativeVelocity.magnitude <= m_SmallCrashRelativeVelocityMagnitude)
            {
                SoundManager.Instance.PlayInGameSound("TankFX_Collision_SmallCrash", collision.contacts[0].point, true, 50.0F);
                return;
            }
        }
        else
        {
            Debug.LogError("There is no Sound Manager in scene!");
        }
    }

    protected virtual void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        if (m_DebugShowValues)
        {
            string info =
                "Speed : " + Mathf.Floor(Speed)  +
                "\nCurrent Clip : " + m_CurrentClip+
                "\nTotal Ammo : " + m_TotalAmmo +
                "\nIs Reloading ? " + m_IsReloading;

            GUIStyle style = new()
            {
                fontSize = 10
            };

            style.normal.textColor = m_IsReloading ? Color.yellow : Color.white;
            Handles.Label(transform.position + (transform.up * 2) + transform.right * 4, info, style);
        }
#endif
    }
}
