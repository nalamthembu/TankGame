
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
/// <summary>
/// This is a child class of base tank and contains logic for AI controlled tanks.
/// </summary>
/// 
[RequireComponent(typeof(ObstacleAvoidance))]
public class AITank : BaseTank
{
    [Header("----------AI Controls----------")]
    [Tooltip("How often should this tank check for surrounding enemies?")]
    [SerializeField]  float m_PerimeterCheckFrequency = 3;
    [Tooltip("How often should we check if the path is still good?")]
    [SerializeField][Min(1)]  float m_PathCheckFrequency = 1;
    [SerializeField]  float m_CheckRadius = 40.0F;
    [SerializeField] LayerMask m_TankLayerMask;
    [SerializeField]  float m_MaxSpeed = 80;
    [Tooltip("How long should the AI wait before attempting to reverse?")]
    [SerializeField] float m_ReverseWaitTime = 10;
    float m_PerimeterCheckTimer = 0;

    [Header("----------AI Combat-----------")]
    [Tooltip("How often should we fire at our enemy?")]
    //0.25 would be you fire every .25 seconds, keep in mind though, this will be limited
    //by the reload time, so if you say .25 seconds that just means the tank will fire as often as it can.
    [SerializeField] [Min(0.1F)] float m_FireRateInSeconds = 2;

    //This is where the AI chose to roam to.
    Vector3 m_ChosenDestination;
    NavMeshPath m_NavMeshPath;
    float pathCheckTimer = 0;

    //Flags
    bool m_IsCheckingForEnemies;
    bool m_HasDestination;
    bool m_IsFightingEnemies;
    bool m_IsBackingUp;

    float m_FireRateTimer;
    float m_ReverseTimer;
    //Where were we when we started backing up?
    Vector3 m_PositionAtTimeOfReverse;

    //Sqr Distance to save up on performance, I don't need the exact distance...
    float m_ClosestEnemySqrDistance;
    //I don't want to make a copy of a tank just to use as a ref.
    int m_ClosestEnemyIndex; 

    List<BaseTank> m_EnemyTanks;

    ObstacleAvoidance m_ObstacleAvoidance;

    //An artificial Accelerator (gas pedal)
    public float Throttle { get; set; }

    public float SteerDir { get; set; }

    protected override void Awake()
    {
        base.Awake();

        m_NavMeshPath = new();

        m_EnemyTanks = new();

        m_ObstacleAvoidance = GetComponent<ObstacleAvoidance>();

        if (m_ObstacleAvoidance is null)
        {
            Debug.LogError("There has to be an obstacle avoidance component on this tank!");

            enabled = false;
        }
    }

    protected override void Update()
    {
        base.Update();

        //If this tank isn't fighting any enemies...
        if (!m_IsFightingEnemies)
        {
            //Countdown before we check for enemies...
            m_PerimeterCheckTimer += Time.deltaTime;
            if (m_PerimeterCheckTimer >= m_PerimeterCheckFrequency)
            {
                //Check for enemies...
                CheckForEnemies();

                m_PerimeterCheckTimer = 0;
            }

            //Roam around aimlessly until you find an enemy...
            DoRandomRoaming();
        }
        
        //If we have enemies in our list...
        if (m_EnemyTanks.Count > 0)
        {
            EliminateEnemies();
        }
    }

    //Eliminate your enemies!
    private void EliminateEnemies()
    {
        if (m_EnemyTanks.Count <= 0)
        {
            m_IsFightingEnemies = false;
            return;
        }

        //Find the closest enemy...
        for (int i = 0; i < m_EnemyTanks.Count; i++)
        {
            float sqrDistFromEnemy = Mathf.Abs((m_EnemyTanks[i].transform.position - transform.position).sqrMagnitude);

            //If our new sqr dist is less than our previous lowest distance...

            if (sqrDistFromEnemy < m_ClosestEnemySqrDistance)
            {
                //Set that to be the new lowest distance...
                m_ClosestEnemySqrDistance = sqrDistFromEnemy;

                //Keep the index of that enemy so we know who to fight first...
                m_ClosestEnemyIndex = i;
            }
        }

        //Let the world know we're fighting someone...
        m_IsFightingEnemies = true;

        //Go to that enemy...
        m_ChosenDestination = m_EnemyTanks[m_ClosestEnemyIndex].transform.position;

        //Apply pressure on throttle depending on how far you are from the destination...
        ApplyThrottle(Speed >= m_MaxSpeed ? 0.1F : Mathf.Lerp(0, 1, Mathf.Clamp01(GetDistanceFromDestination())));

        //Turn your turret to the enemy...
        RotateTurret(m_ChosenDestination);

        //if we've run out of ammo in our current clip...
        if (m_CurrentClip <= 0)
        {
            //If we're not reloading
            if (!m_IsReloading)
                StartCoroutine(Reload()); //then do so...
        }

        //Don't try to shoot if you are reloading!
        if (m_IsReloading)
            return;

        //count down before firing...
        m_FireRateTimer += Time.deltaTime;

        if (m_FireRateTimer >= m_FireRateInSeconds)
        {
            //Prevent tanks from shooting at the floor...
            float heightOffset = 2.0F;

            Vector3 targetDirection = ((m_ChosenDestination + Vector3.up * heightOffset) - m_TurretTransform.position).normalized;

            Fire(targetDirection * m_ProjectileRange);

            //Reset that timer.
            m_FireRateTimer = 0;

        }
    }

    //Pick a random point and roam around there
    private void DoRandomRoaming()
    {
        //If this AI has no where to go...
        if (!m_HasDestination)
        {
            //find a place to go...
            m_ChosenDestination = Vector3.one * Random.Range(-250, 250);

            //Make sure you find a place on the ground...
            m_ChosenDestination.y = 0;

            if (NavMesh.SamplePosition(m_ChosenDestination, out NavMeshHit hit, 10.0F, NavMesh.AllAreas))
            {
                m_ChosenDestination = hit.position;
            }

            //Calculate a path
            CalculatePath();

            m_HasDestination = true;
        }
        else
        {
            CalculatePath();

            //Apply pressure on throttle depending on how far you are from the destination...
            ApplyThrottle(Speed >= m_MaxSpeed ? 0.1F : Mathf.Lerp(0, 1, Mathf.Clamp01(GetDistanceFromDestination())));

            //If we reach the destination...
            if (GetDistanceFromDestination() <= 1)
            {
                m_HasDestination = false;
            }
        }
    }

    private void CalculatePath()
    {
        pathCheckTimer += Time.deltaTime;

        if (pathCheckTimer >= m_PathCheckFrequency)
        {
            NavMesh.CalculatePath(transform.position, m_ChosenDestination, NavMesh.AllAreas, m_NavMeshPath);

            pathCheckTimer = 0;

            print(m_NavMeshPath.corners.Length);
        }
    }

    private float ApplyThrottle(float amountBetweenZeroAndOne) => Throttle = amountBetweenZeroAndOne;
    public void ApplySteer(float steerDir) => SteerDir = Mathf.Clamp(steerDir, -1, 1);

    private float GetDistanceFromDestination() => Vector3.Distance(transform.position, m_ChosenDestination);

    protected override void MoveWheels()
    {
        if (m_Wheels.Length > 0)
        {
            m_MotorTorque = Mathf.SmoothDamp
            (
                m_MotorTorque,
                m_MaxMotorTorque,
                ref m_MotorTVelocity,
                m_TorqueResponseTime
            );

            if (IsSomethingBlockingTheTank() && m_IsBackingUp)
            {
                //Drive backwards
                Throttle = -Throttle;

                //When we are far enough...
                if (Vector3.Distance(transform.position, m_PositionAtTimeOfReverse) >= 10)
                {
                    //Stop backing up.
                    m_IsBackingUp = false;
                }
            }

            m_MotorTorque *= Throttle;

            for (int i = 0; i < m_Wheels.Length; i++)
            {
                    //If the current wheel is a steering wheel and the obstacle avoidance isn't overriding the steering controls...
                    if (m_Wheels[i].IsSteeringWheel && m_ObstacleAvoidance && !m_ObstacleAvoidance.OverrideSteering)
                    {
                        //Steer to the destination IF we have one...
                        if (m_HasDestination)
                        {
                            Vector3 steerVector;

                            steerVector = transform.InverseTransformPoint
                                    (
                                        new
                                            (
                                                m_ChosenDestination.x,
                                                transform.position.y,
                                                m_ChosenDestination.z
                                            )
                                    );

                            float steerDirection = steerVector.x / steerVector.magnitude;

                            m_Wheels[i].SetSteerAngle(steerDirection * m_MaxSteerAngle);
                        }
                    }

                //if not then every other wheel including the steering wheel should put down power.
                m_Wheels[i].SetMotorTorque(m_MotorTorque);
            }
        }
        else Debug.LogError("There are no wheels assigned on this AI!");
    }

    //Is there something preventing movement?
    private bool IsSomethingBlockingTheTank()
    {
        //If the wheels are moving forward and the speed is at [0]
        if (GetAverageRPM() > 0 && Mathf.Floor(Speed) <= 0)
        {
            m_ReverseTimer += Time.deltaTime;

            if (m_ReverseTimer >= m_ReverseWaitTime)
            {
                m_PositionAtTimeOfReverse = transform.position;

                m_ReverseTimer = 0;

                m_IsBackingUp = true;

                return true;
            }
        }

        return false;
    }

    private void CheckForEnemies()
    {
        m_IsCheckingForEnemies = true;

        // Clear the list first if we've already checked for enemies before...
        if (m_EnemyTanks.Count > 0)
            m_EnemyTanks.Clear();

        Collider[] TankColliders = Physics.OverlapSphere(transform.position, m_CheckRadius, m_TankLayerMask);

        if (TankColliders.Length > 0)
        {
            for (int i = 0; i < TankColliders.Length; i++)
            {
                
                if (TankColliders[i].TryGetComponent<BaseTank>(out var tank))
                {
                    //Do not include yourself as an enemy.
                    if (tank.GetInstanceID() == GetInstanceID())
                        continue;

                    m_EnemyTanks.Add(tank);
                }
            }

            m_IsCheckingForEnemies = false;
        }
    }

    private void OnDrawGizmos()
    {
        //Show the developer the path...
        if (m_NavMeshPath != null)
        {
            for (int i = 0; i < m_NavMeshPath.corners.Length - 1; i++)
            {
                Debug.DrawLine(m_NavMeshPath.corners[i], m_NavMeshPath.corners[i + 1], Color.red);
            }
        }
    }


    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();


#if UNITY_EDITOR
        if (m_DebugShowValues)
        {
            string info =
                "Has Destination : " + m_HasDestination +
                "\nIs Looking for Enemies  : " + m_IsCheckingForEnemies +
                "\nIs Fighting Enemies : " + m_IsFightingEnemies;

            GUIStyle style = new()
            {
                fontSize = 7
            };

            style.normal.textColor = m_IsReloading ? Color.yellow : Color.white;
            Handles.Label(transform.position + (transform.up * 8) + transform.right, info, style);
        }
#endif

        if (m_IsCheckingForEnemies)
        { 
            Gizmos.color = Color.cyan;

            Gizmos.DrawWireSphere(transform.position, m_CheckRadius);
        }    
    }
}