using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Random = UnityEngine.Random;
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
    [Tooltip("How often should we check if the path is still good?")]
    [SerializeField][Min(1)]  float m_PathCheckFrequency = 5.0F;
    [Tooltip("How long should we wait before resetting the AI when it gets stuck?")]
    [SerializeField][Min(2)] float m_WaitTimeBeforeReset = 5.0F;
    [Tooltip("How far are we allowed to reverse?")]
    [SerializeField] float m_MaxReverseDistance = 5.0F;

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
    [Tooltip("How far should we look for enemies?")]
    [SerializeField] float m_CheckRadius = 40.0F;
    [Tooltip("How often should this tank check for surrounding enemies?")]
    [SerializeField] float m_PerimeterCheckFrequency = 3;
    [Tooltip("How far is the enemy allowed to be before we stop and look for another?")]
    [SerializeField] float m_MaxDistanceFromEnemy = 50.0F;

    [Header("----------Debugging----------")]
    [Tooltip("Do you want the tank to stop moving forward?")]
    [SerializeField] bool m_KillThrottle = false;

    //This is where the AI chose to roam to.
    Vector3 m_TargetDestination;
    Vector3 m_DestinationAlongPath;
    Vector3 m_PositionAtStartOfReverse;
    private int m_IndexAlongPath = 0;
    private NavMeshPath m_NavMeshPath;
    private readonly List<Vector3> m_WayPoints = new();

    //Flags
    bool m_IsCheckingForEnemies = false;
    bool m_HasDestination = false;
    bool m_HasStartedAlongThePath = false;
    bool m_IsFightingEnemies = false;
    bool m_IsReversing = false;


    //Timers
    float m_FireRateTimer;
    float m_ResetTimer;
    float m_ReverseTimer = 0;
    float m_PathCheckTimer = 0;

    //Combat
    //Sqr Distance to save up on performance, I don't need the exact distance...
    float m_ClosestEnemySqrDistance;
    //I don't want to make a copy of a tank just to use as a ref.
    int m_ClosestEnemyIndex; 
    List<BaseTank> m_EnemyTanks;
    BaseTank m_ClosestEnemyTank;

    //Navigation
    ObstacleAvoidance m_ObstacleAvoidance;

    //An artificial Accelerator (gas pedal)
    public float Throttle { get; private set; }

    public float Brake { get; private set; }

    public float SteerDir { get; private set; }

    public bool IsReversing { get { return m_IsReversing; } }

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
        if (m_IsFrozen)
            return;

        base.Update();

        if (m_IsCheckingForEnemies)
        {
            CheckForEnemies();
        }

        if (m_IsFightingEnemies)
        {
            EliminateEnemies();
        }
        else
        {
            DoRandomRoaming();
        }
    }

    private void CheckForEnemies()
    {
        m_PerimeterCheckTimer += Time.deltaTime;

        if (m_PerimeterCheckTimer >= m_PerimeterCheckFrequency)
        {
            if (m_EnemyTanks.Count > 0)
                m_EnemyTanks.Clear();

            m_IsCheckingForEnemies = true;

            Collider[] tankColliders = Physics.OverlapSphere(transform.position, m_CheckRadius, m_TankLayerMask);

            foreach (Collider collider in tankColliders)
            {
                if (collider.TryGetComponent<BaseTank>(out var tankComponent))
                {
                    //DO NOT include yourself as an enemy...
                    if (tankComponent.GetInstanceID() == this.GetInstanceID())
                    {
                        continue;
                    }
                    else
                    {
                        m_EnemyTanks.Add(tankComponent);
                    }
                }
            }

            //if we have found some enemies...lets fight!
            if (m_EnemyTanks.Count > 0)
            {
                m_IsFightingEnemies = true;
                m_HasDestination = false;
                m_IsCheckingForEnemies = false;
                m_ClosestEnemyTank = null;
            }

            m_PerimeterCheckTimer  = 0;
        }
    }

    //Eliminate your enemies!
    private void EliminateEnemies()
    {
        if (m_ClosestEnemyTank == null && !m_HasDestination)
        {
            m_HasStartedAlongThePath = false;
            m_ClosestEnemyIndex = FindClosestEnemyIndex();
            m_ClosestEnemyTank = m_EnemyTanks[m_ClosestEnemyIndex];
            CheckIfTargetDestinationIsValid(m_ClosestEnemyTank.transform.position);
        }
        else
        {
            RecalculatePath(m_ClosestEnemyTank.transform.position);
            TravelAlongPath();

            //If we are too far from the enemy...
            float distanceFromClosestTank = Vector3.Distance(transform.position, m_ClosestEnemyTank.transform.position);
            if (distanceFromClosestTank >= m_MaxDistanceFromEnemy)
            {
                //Leave them alone, find a new enemy...
                m_HasStartedAlongThePath = false;
                m_IsFightingEnemies = false;
                m_IsCheckingForEnemies = true;
                m_HasDestination = false;
                m_ClosestEnemyTank = null;

                return;
            }

            RotateTurret(m_ClosestEnemyTank.transform.position);

            m_FireRateTimer += Time.deltaTime;

            if (m_FireRateTimer >= m_FireRateInSeconds)
            {
                Vector3 dirToTarget = (m_ClosestEnemyTank.transform.position + Vector3.up / 2 - transform.position).normalized;

                Debug.DrawLine(m_TurretTransform.position, dirToTarget * distanceFromClosestTank, Color.red);

                Fire(dirToTarget);

                m_FireRateTimer = 0;
            }
        }
    }

    private void RecalculatePath(Vector3 targetLocation)
    {
        if (m_WayPoints.Count > 0)
        {
            m_PathCheckTimer += Time.deltaTime;

            if (m_PathCheckTimer >= m_PathCheckFrequency)
            {
                CheckIfTargetDestinationIsValid(targetLocation);

                m_PathCheckTimer = 0;
            }
        }
    }

    private int FindClosestEnemyIndex()
    {
        for (int i = 0; i < m_EnemyTanks.Count; i++)
        {
            //Exclude yourself from enemy list...
            if (m_EnemyTanks[i] == this)
            {
                continue;
            }

            float currentClosestSqrDist = (m_EnemyTanks[i].transform.position - transform.position).sqrMagnitude;

            currentClosestSqrDist = Mathf.Abs(currentClosestSqrDist);

            if (currentClosestSqrDist < m_ClosestEnemySqrDistance)
            {

                m_ClosestEnemySqrDistance = currentClosestSqrDist;

                return i;
            }
        }

        return 0;
    }

    //Pick a random point and roam around there
    private void DoRandomRoaming()
    {
        if (!m_HasDestination)
        {
            m_HasStartedAlongThePath = false;

            //Get a random position in the level...
            m_TargetDestination = new()
            {
                x = Random.Range(-250, 250),
                y = 0,
                z = Random.Range(-250, 250)
            };

            CheckIfTargetDestinationIsValid(m_TargetDestination);
        }
        else
        {
            TravelAlongPath();

            m_IsCheckingForEnemies = true;
        }
    }

    private void CheckIfTargetDestinationIsValid(Vector3 targetDestination)
    {

        //check with the nav mesh...
        if (NavMesh.SamplePosition(targetDestination, out NavMeshHit hit, 10.0F, NavMesh.AllAreas))
        {
            m_TargetDestination = hit.position;
            //Calculate a path...
            CalculatePath();
            m_HasDestination = true;
            m_HasStartedAlongThePath = true;
        }
    }

    private void TravelAlongPath()
    {
        m_HasStartedAlongThePath = true;

        float distanceFromTarget = Vector3.Distance(transform.position, m_TargetDestination);

        int minDistance = 10;

        float throttle = Mathf.Lerp(0, 1, distanceFromTarget / minDistance);

        ApplyThrottle(throttle);

        if (GetDistanceFromNextWaypointOnPath() <= minDistance)
        {
            m_IndexAlongPath++;

            if (m_IndexAlongPath > m_WayPoints.Count - 1 || m_WayPoints.Count <= 0)
            {
                m_HasDestination = false;

                m_IndexAlongPath = 0;
            }
            else
            {
                m_DestinationAlongPath = m_WayPoints[m_IndexAlongPath];
            }
        }

        if (distanceFromTarget <= minDistance)
        {
            //FIND A NEW PLACE TO GO...
            m_HasDestination = false;
        }
    }

    public void CalculatePath()
    {
        //if it is then calculate a path...
        if (NavMesh.CalculatePath(transform.position, m_TargetDestination, NavMesh.AllAreas, m_NavMeshPath))
        {
            m_WayPoints.Clear();

            foreach (Vector3 pos in m_NavMeshPath.corners)
            {
                m_WayPoints.Add(pos);
            }
        }
    }

    private void ApplyThrottle(float amountBetweenZeroAndOne) => Throttle = amountBetweenZeroAndOne;

    public void ApplyBrake(float amountBetweenZeroAndOne) => Brake = amountBetweenZeroAndOne;

    public void ApplySteer(float steerDir) => SteerDir = Mathf.Clamp(steerDir, -1, 1);
    

    private float GetDistanceFromNextWaypointOnPath()
    {
        if (m_IndexAlongPath < m_WayPoints.Count)
        {
            return Vector3.Distance(transform.position, m_WayPoints[m_IndexAlongPath]);
        }

        return 0;
    }

    void CheckIfTankIsStuck()
    {
        //If the AI gets stuck for whatever reason
        if (Mathf.Round(Speed) <= 0 && Mathf.Abs(Throttle) > 0 && m_IsReversing == false)
        {
            m_ResetTimer += Time.deltaTime;

            if (m_ResetTimer >= m_WaitTimeBeforeReset)
            {
                Vector3 randomPosition = transform.position + Random.insideUnitSphere * Random.Range(10, 20);

                randomPosition.y = 0;

                if (TryTeleportTank(randomPosition))
                    m_ResetTimer = 0;
            }
        }
        else
        {
            m_ResetTimer = 0;
        }
    }

    //Will return true of position is a valid position on the navmesh....
    bool TryTeleportTank(Vector3 position)
    {
        if (NavMesh.SamplePosition(position, out var hit, 1000.0F, NavMesh.AllAreas))
        {
            //Place the tank in a random position around itself and look toward the next check point.

            transform.SetPositionAndRotation(hit.position, Quaternion.LookRotation(m_DestinationAlongPath, transform.up));

            return true;
        }

        return false;
    }

    void CheckIfTankIsStoppedButCanReverse()
    {
        if (Mathf.Round(Speed) <= 0 && Mathf.Abs(Throttle) > 0 && m_IsReversing == false)
        {
            m_ReverseTimer += Time.deltaTime;

            if (m_ReverseTimer >= m_ReverseWaitTime)
            {
                m_PositionAtStartOfReverse = transform.position;

                m_ReverseTimer = 0;

                m_IsReversing = true;
            }
        }
        else
            m_ReverseTimer = 0;
    }

    float GetDistanceSinceStartOfReverse() => Vector3.Distance(transform.position, m_PositionAtStartOfReverse);

    protected override void MoveWheels()
    {
        CheckIfTankIsStuck();

        CheckIfTankIsStoppedButCanReverse();

        if (m_Wheels.Length > 0)
        {
            m_MotorTorque = Mathf.SmoothDamp
            (
                m_MotorTorque,
                m_MaxMotorTorque,
                ref m_MotorTVelocity,
                m_TorqueResponseTime
            );

            if (m_KillThrottle || Speed > m_MaxSpeed * 1.25F)
            {
                m_MotorTorque *= 0;
            }

            for (int i = 0; i < m_Wheels.Length; i++)
            {

                m_Wheels[i].SetMotorTorque(m_IsReversing ? m_MotorTorque * -1 : m_MotorTorque * Throttle);

                if (m_Wheels[i].IsSteeringWheel)
                {

                    //Steer
                    Vector3 steerVector;

                    float steerDirection;

                    steerVector = transform.InverseTransformPoint
                            (
                                new
                                    (
                                        m_DestinationAlongPath.x,
                                        transform.position.y,
                                        m_DestinationAlongPath.z
                                    )
                            );

                    steerDirection = steerVector.x / steerVector.magnitude;

                    //If we're reversing, steering the other direction....
                    if (m_IsReversing)
                    {
                        if (!m_ObstacleAvoidance.OverrideSteering)
                            ApplySteer(steerDirection * -1);

                        if (m_Wheels[i].IsSteeringWheel)
                            m_Wheels[i].SetSteerAngle(SteerDir * m_MaxSteerAngle);

                        if (GetDistanceSinceStartOfReverse() >= m_MaxReverseDistance)
                        {
                            m_IsReversing = false;
                        }
                    }
                    else
                    {
                        if (!m_ObstacleAvoidance.OverrideSteering)
                        {
                            ApplySteer(steerDirection);
                            m_Wheels[i].SetSteerAngle(SteerDir * m_MaxSteerAngle);
                        }
                    }
                }
            }
        }
        else Debug.LogError("There are no wheels assigned on this AI!");
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

            if (m_HasDestination && !m_HasStartedAlongThePath)
            {
                Debug.DrawRay(m_TargetDestination, Vector3.up, Color.magenta, 5.0F);
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
                "\nHas Started along the path " + m_HasStartedAlongThePath +
                "\nIs Looking for Enemies  : " + m_IsCheckingForEnemies +
                "\nIs Fighting Enemies : " + m_IsFightingEnemies;

            GUIStyle style = new()
            {
                fontSize = 10
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