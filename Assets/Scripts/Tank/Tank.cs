using UnityEngine;

/// <summary>
/// This is child class of base tank and contains a tank object that can be controlled by the player.
/// </summary>
/// 
public class Tank : BaseTank
{
    Camera m_Camera;
    protected override void Awake()
    {
        base.Awake();

        m_Camera = Camera.main;

        if (m_Camera is null)
            Debug.LogError("There is no main camera in the scene");
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

    protected override void ProcessFireInput()
    {
        //if we've run out of ammo in our current clip...
        if (m_CurrentClip <= 0)
        {
            //If we're not reloading
            if (!m_IsReloading)
                StartCoroutine(Reload()); //then do so...

            //Don't execute any further code.
            return;
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

                    //Quick thud on the camera.
                    if (ThirdPersonTankCamera.Instance != null)
                        StartCoroutine(ThirdPersonTankCamera.Instance.DoCameraShake(0.25F, 5.0F, 4.0F));

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