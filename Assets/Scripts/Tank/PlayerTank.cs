using UnityEngine;
using System;

/// <summary>
/// This is child class of base tank and contains a tank object that can be controlled by the player.
/// </summary>
/// 
public class PlayerTank : BaseTank
{
    Camera m_Camera;



    public static PlayerTank PlayerTankInstance;

    public static event Action<float, float> OnPlayerShot;

    public static event Action OnPlayerReload;

    public static event Action OnPlayerIsDoneReloading;

    public static event Action<float, float> OnPlayerBigCollision;


    protected override void Awake()
    {
        if (PlayerTankInstance == null)
        {
            PlayerTankInstance = this;
        }
        else
            Destroy(gameObject);

        base.Awake();

        m_Camera = Camera.main;

        if (m_Camera is null)
            Debug.LogError("There is no main camera in the scene");

        //OnSpawn?.Invoke(this);
    }

    private void OnDestroy()
    {
        PlayerTankInstance = null;

        Debug.Log("Destroyed Player Tank Instance!");
    }

    protected override void Update()
    {
        base.Update();

        if (m_Camera != null)
        {
            Ray ray = m_Camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                RotateTurret(hit.point);
            }
        }
    }

    protected override void MoveWheels()
    {
        //Go from the current motor torque to the maxiumum based on how fast the 'engine' responds.
        m_MotorTorque = Mathf.SmoothDamp
            (
                m_MotorTorque,
                m_MaxMotorTorque,
                ref m_MotorTVelocity,
                m_TorqueResponseTime
            );

        if (PlayerInput.Instance != null)
        {
            for (int i = 0; i < m_Wheels.Length; i++)
            {
                if (m_Wheels[i].IsSteeringWheel)
                    m_Wheels[i].SetSteerAngle(PlayerInput.Instance.InputDir.x * m_MaxSteerAngle);

                m_Wheels[i].SetMotorTorque(m_MotorTorque * PlayerInput.Instance.InputDir.y);
            }
        }
        else
            Debug.LogError("There is no player input instance in scene!");
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);

        if (collision.relativeVelocity.magnitude >= m_BigCrashRelativeVelocityMagnitude + 10)
        {
            //Tell those who are listening...
            OnPlayerBigCollision?.Invoke(0.25F, 0.5F);
        }
    }

    protected override void ProcessFireInput()
    {
        //if we've run out of ammo in our current clip...
        if (m_CurrentClip <= 0)
        {
            //If we're not reloading
            if (!m_IsReloading)
            { 
                //then do so...
                StartCoroutine(Reload());

                //let everyone whos listening know...
                OnPlayerReload?.Invoke();
            }

            //Don't execute any further code.
            return;
        }
        else if (m_CurrentClip >0 && m_IsDoneReloading)
        {
            //Let everyone whos listening know we're done reloading.
            m_IsDoneReloading = false;
            OnPlayerIsDoneReloading?.Invoke();
        }

        //Don't try to shoot if you are reloading!
        if (m_IsReloading)
            return;

        //If the player clicks the left mouse button.
        if (Input.GetMouseButtonDown(0))
        {
            //Get Mouse Position
            Ray ray = m_Camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                //If we aren't firing 'danger close'.
                if (hit.distance >= m_MinumumProjectileFireDistance)
                {
                    Vector3 targetDirection = (hit.point - m_ProjectileSpawnPoint.position).normalized;

                    Debug.DrawRay(m_ProjectileSpawnPoint.position, targetDirection, Color.cyan, 5.0F);

                    Fire(targetDirection * m_ProjectileRange); //Shoot!

                    m_CurrentClip--;

                    OnPlayerShot?.Invoke(0.25F, 0.25F);

                    //Add some recoil force to the tank.
                    if (m_RigidBody != null)
                        m_RigidBody.AddForce(m_RecoilForce * m_RecoilForce * -m_TurretTransform.forward, ForceMode.Impulse);
                }    
            }
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        if (m_Camera != null)
        {
            Gizmos.color = Color.red;

            Ray ray = m_Camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Gizmos.DrawWireSphere(hit.point, 2.0f);
            }
        }
    }
}