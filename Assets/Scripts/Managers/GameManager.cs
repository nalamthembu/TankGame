using System;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Random = UnityEngine.Random;


public class GameManager : MonoBehaviour
{
    [Header("----------General----------")]
    [SerializeField] GameObject m_PlayerPrefab;
    [SerializeField] bool m_SpawnInRandomLocation = true;
    [SerializeField] Vector2 m_LevelSize = new(250, 250);

    [Header("----------Player Aid----------")]
    [Tooltip("How often should we spawn a pickup?")]
    [SerializeField] float m_PickupSpawnRate = 15.0F;
    [SerializeField] string[] m_PickupPoolIDs;
    [SerializeField] int m_MaxPickupsInScene = 5;

    [Header("----------Debugging----------")]
    [SerializeField] bool m_VisualiseLevelBounds;
    [SerializeField] bool m_MakeLevelBoundsSolid;
    [SerializeField] bool m_DontSpawnHelp;

    private GameObject m_PlayerGameObject;
    List<PickupBase> m_PickupsInScene = new();
    public static GameManager Instance;

    //Timers
    float m_PickupSpawnTimer = 0;
    float m_TimeSinceStartOfGame = 0;

    //Flags
    bool m_GameHasStarted;
    bool m_GameIsOver;

    //Gameplay Elements
    int m_TotalKillsByPlayer = 0;
    int m_WavesSurvived = 0;

    //Events
    public static event Action OnGameEnded;

    //Game Returns
    public float GameElapsedTime { get { return m_TimeSinceStartOfGame; } }
    public int TotalKillsByPlayer { get { return m_TotalKillsByPlayer; } }
    public int WavesSurvived { get { return m_WavesSurvived; } }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
            Destroy(gameObject);

        SpawnPlayerInRandomPosition();
    }

    private void OnDestroy()
    {
        Instance = null;
        Debug.Log("Destroyed Game Manager instance!");
    }

    private void OnEnable()
    {
        PlayerTankHealth.OnDeath += OnPlayerDeath;
        TankHealth.OnDeath += OnAITankDeath;
        EnemyWaveGenerator.OnSpawnWave += OnEnemyWaveSpawn;
    }

    private void OnDisable()
    {
        PlayerTankHealth.OnDeath -= OnPlayerDeath;
        TankHealth.OnDeath -= OnAITankDeath;
        EnemyWaveGenerator.OnSpawnWave -= OnEnemyWaveSpawn;
    }

    private void OnEnemyWaveSpawn() =>  m_WavesSurvived++;
    
    private void OnAITankDeath(BaseTank attacker)
    {
        print(attacker);

        if (attacker is PlayerTank)
        {
            m_TotalKillsByPlayer++;
        }
    }

    private void OnPlayerDeath()
    {
        //stop the game...
        m_GameIsOver = true;

        //Slow Down...
        Time.timeScale = 0.25F;

        Debug.Log("Game is OVER!");

        OnGameEnded?.Invoke();
    }

    private void Start()
    {
        InitialiseGame();
    }

    private void InitialiseGame()
    {
        m_GameHasStarted = true;
        m_GameIsOver = false;

        m_PickupSpawnTimer = m_PickupSpawnRate;
    }

    private void Update()
    {
        if (m_GameHasStarted && !m_GameIsOver)
            m_TimeSinceStartOfGame += Time.deltaTime;

        if (!m_DontSpawnHelp)
            SpawnHelp();
    }

    private void SpawnHelp()
    {
        if (m_TimeSinceStartOfGame >= 15.0F)
        {
            SpawnPickups();
        }
    }

    private void SpawnPickups()
    {
        if (m_PickupsInScene.Count < m_MaxPickupsInScene)
        {
            m_PickupSpawnTimer += Time.deltaTime;

            if (m_PickupSpawnTimer >= m_PickupSpawnRate)
            {
                //Get a random object pool ID...
                string poolID = m_PickupPoolIDs[Random.Range(0, m_PickupPoolIDs.Length)];

                //Check if that pool is valid...
                if (ObjectPoolManager.Instance != null && ObjectPoolManager.Instance.TryGetPool(poolID, out var pool))
                {
                    //get a pickup from the pool...
                    if (pool.TryGetGameObject(out var spawnedPickup))
                    {
                        spawnedPickup.transform.position = GetRandomPositionInLevelBounds();

                        //get the pickup component, use the base class so that it doesn't matter which pick up we found...
                        if (spawnedPickup.TryGetComponent<PickupBase>(out var pickUpComponent))
                        {
                            //initialise that pickup irrespective of which child class it is...
                            pickUpComponent.Initialise();

                            //add this pick up to our list of pickups in the scene...
                            m_PickupsInScene.Add(pickUpComponent);
                        }
                    }
                }
                else
                    Debug.LogError("There is no object pool manager in this scene or Pool you are trying to get doesn't exist.");

                m_PickupSpawnTimer = 0;
            }
        }
        else
        {
            //if we have too many pickups...
            foreach (PickupBase pickup in m_PickupsInScene)
            {
                //if any of them are disabled then they've been picked up....
                if (!pickup.gameObject.activeSelf)
                {
                    //lets remove them from our list...
                    m_PickupsInScene.Remove(pickup);
                }
            }
        }
    }

    private void SpawnPlayerInRandomPosition()
    {
        if (m_SpawnInRandomLocation)
            m_PlayerGameObject = Instantiate(m_PlayerPrefab, GetRandomPositionInLevelBounds(), Quaternion.identity);

    }

    private Vector3 GetRandomPositionInLevelBounds()
    {
        int maxIteration = 1000;

        Vector3 randomPosition = Vector3.zero;

        for (int j = 0; j < maxIteration; j++)
        {
            Bounds levelBounds = new(Vector3.zero, m_LevelSize);

            randomPosition = new()
            {
                x = Random.Range(-levelBounds.extents.x, levelBounds.extents.x),
                y = 0,
                z = Random.Range(-levelBounds.extents.z, levelBounds.extents.z),
            };

            if (NavMesh.SamplePosition(randomPosition, out var hit, 1000.0F, NavMesh.AllAreas))
            {
                return hit.position;
            }

            Debug.Log("Ran out of iterations defaulting position to World Origin");
        }

        return randomPosition;
    }

    private void OnDrawGizmosSelected()
    {
        if (m_VisualiseLevelBounds)
        {
            Gizmos.color = Color.green;

            if (!m_MakeLevelBoundsSolid)
                Gizmos.DrawWireCube(Vector3.zero, new Vector3(m_LevelSize.x, 1, m_LevelSize.y));
            else
                Gizmos.DrawCube(Vector3.zero, new Vector3(m_LevelSize.x, 1, m_LevelSize.y));
        }
    }
}
