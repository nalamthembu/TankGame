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
    [SerializeField] GameState m_GameState = GameState.NOT_RUNNING;
    [SerializeField] float m_TimeBeforeShowingFinalScore;

    [Header("----------Player Aid----------")]
    [Tooltip("How often should we spawn a pickup?")]
    [SerializeField] float m_PickupSpawnRate = 15.0F;
    [SerializeField] string[] m_PickupPoolIDs;
    [SerializeField] int m_MaxPickupsInScene = 5;

    [Header("----------Debugging----------")]
    [SerializeField] bool m_VisualiseLevelBounds;
    [SerializeField] bool m_MakeLevelBoundsSolid;
    [SerializeField] bool m_DontSpawnHelp;

    [Header("----------Score System----------")]
    [SerializeField] int m_KillScore = 100;
    [SerializeField] int m_PickupHealthScore = 10;
    [SerializeField] int m_PickupArmorScore = 15;
    [SerializeField] int m_SurviveWaveScore = 12;
    [SerializeField] int m_SurvivedMoreThan5WavesScore = 13;

    private GameObject m_PlayerGameObject;
    List<PickupBase> m_PickupsInScene = new();
    List<Body> m_FrozenObjects = new();
    public static GameManager Instance;

    //Flags
    bool m_AllObjectsAreFrozen;
    bool m_GameIsPaused;
    bool m_ShownFinalScore;

    //Timers
    float m_PickupSpawnTimer = 0;
    float m_TimeSinceStartOfGame = 0;
    float m_EndOfGameTimer = 0;

    //Gameplay Elements
    int m_TotalKillsByPlayer = 0;
    int m_WavesSurvived = 0;
    int m_Score;

    //Events
    public static event Action OnGamePaused;
    public static event Action OnGameResume;
    public static event Action OnGameEnded;
    public static event Action<int> OnScoreChange;
    public static event Action<int, int, float> OnShowEndOfGameScreen;

    //Game Returns
    public float GameElapsedTime { get { return m_TimeSinceStartOfGame; } }
    public int TotalKillsByPlayer { get { return m_TotalKillsByPlayer; } }
    public int WavesSurvived { get { return m_WavesSurvived; } }
    public bool GameIsPaused { get { return m_GameIsPaused; } }

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


    private void OnEnable()
    {
        PickupBase.OnPickUp += OnPickUpRetrieved;
        PlayerTankHealth.OnDeath += OnPlayerDeath;
        TankHealth.OnDeath += OnAITankDeath;
        EnemyWaveGenerator.OnSpawnWave += OnEnemyWaveSpawn;
        PauseMenuManager.OnPauseMenuClose += OnPauseMenuClose;
    }

    private void OnDisable()
    {
        PlayerTankHealth.OnDeath -= OnPlayerDeath;
        TankHealth.OnDeath -= OnAITankDeath;
        EnemyWaveGenerator.OnSpawnWave -= OnEnemyWaveSpawn;
        PauseMenuManager.OnPauseMenuClose -= OnPauseMenuClose;
    }

    private void OnPauseMenuClose() => SetGameState(GameState.RUNNING);

    private void OnPickUpRetrieved(PickupBase pickUp)
    {
        switch (pickUp)
        {
            case HealthPickup:
                AddToScore(m_PickupHealthScore);
                break;

            case ShieldPickup:
                AddToScore(m_PickupArmorScore);
                break;
        }
    }

    private void OnEnemyWaveSpawn()
    {
        m_WavesSurvived++;

        if (m_WavesSurvived > 1)
        {
            AddToScore(m_SurviveWaveScore);

            if (m_WavesSurvived > 5)
                AddToScore(m_SurvivedMoreThan5WavesScore);
        }
    }

    private void AddToScore(int amount)
    {
        m_Score += amount;

        //Let all the listeners know what the new score is.
        OnScoreChange?.Invoke(m_Score);
    }
    
    private void OnAITankDeath(BaseTank attacker)
    {
        print(attacker);

        if (attacker is PlayerTank)
        {
            m_TotalKillsByPlayer++;

            AddToScore(m_KillScore);
        }
    }

    private void OnPlayerDeath()
    {
        //stop the game...
        m_GameState = GameState.OVER;

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
        m_PickupSpawnTimer = m_PickupSpawnRate;

        //TO-DO : Add timer before game starts...3,2,1, GO!

        SetGameState(GameState.RUNNING);
    }

    private void Update()
    {
        switch (m_GameState)
        {
            case GameState.RUNNING:

                m_GameIsPaused = false;

                Cursor.visible = false;

                if (m_AllObjectsAreFrozen)
                    UnFreezeAllDynamicObjects();

                //Check if the player pressed pause...
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    m_GameIsPaused = true;
                    SetGameState(GameState.PAUSED);
                }

                m_TimeSinceStartOfGame += Time.deltaTime;

                if (!m_DontSpawnHelp)
                    SpawnHelp();

                //TO-DO : If we eliminate the last tank in the wave, do a cool slow-mo view of the destruction.

                break;

            case GameState.PAUSED:

                Cursor.visible = true;

                ProcessPausedGame();

                break;

            case GameState.OVER:

                //TO-DO : Compile results and display them after a few seconds...

                if (!m_ShownFinalScore)
                {
                    m_EndOfGameTimer += Time.deltaTime;
                    
                    if (m_EndOfGameTimer >= m_TimeBeforeShowingFinalScore)
                    {
                        m_ShownFinalScore = true;

                        //Show us...

                        int scoreBeforeAddingTime = m_Score;

                        //Add the elapsed time bonus...

                        m_Score += (int) m_TimeSinceStartOfGame / 100;

                        OnShowEndOfGameScreen?.Invoke(scoreBeforeAddingTime, m_Score, m_TimeSinceStartOfGame); 

                        m_EndOfGameTimer = 0;
                    }
                }

                //Show the cursor...
                Cursor.visible = true;

                //TO-DO : Save results...

                return;
        }
    }

    private void ProcessPausedGame()
    {
        m_GameIsPaused = true;

        //Check if the player pressed RESUME...
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetGameState(GameState.RUNNING);

            m_GameIsPaused = false;

            return;
        }

        if (!m_AllObjectsAreFrozen)
        FreezeAllDynamicObjects();
    }

    private void UnFreezeAllDynamicObjects()
    {
        if (m_AllObjectsAreFrozen)
        {
            //Freeze all particles
            ParticleSystem[] particleSystems = FindObjectsOfType<ParticleSystem>();

            foreach (ParticleSystem particle in particleSystems)
                particle.Play(true);
            
            //UN-freeze all tanks
            foreach (Body body in m_FrozenObjects)
            {
                body.UnFreeze();
            }

            m_AllObjectsAreFrozen = false;
        }
    }

    private void FreezeAllDynamicObjects()
    {
        if (m_AllObjectsAreFrozen)
        {
            Debug.LogWarning("All objects are already frozen!");
            return;
        }
        
        //Freeze all particles
        ParticleSystem[] particleSystems = FindObjectsOfType<ParticleSystem>();

        foreach(ParticleSystem particle in particleSystems)
            particle.Pause(true);
        

        Rigidbody[] rigidBodiesInScene = FindObjectsOfType<Rigidbody>();

        m_FrozenObjects.Clear();

        //freeze all tanks
        foreach (Rigidbody body in rigidBodiesInScene)
        {
            if (body.isKinematic)
                continue;

            m_FrozenObjects.Add(new(body, body.velocity));
        }

        m_AllObjectsAreFrozen = true;
    }

    private void SetGameState(GameState newState)
    { 
        m_GameState = newState;

        switch (m_GameState)
        {
            case GameState.PAUSED:
                OnGamePaused?.Invoke();
                break;

            case GameState.RUNNING:
                OnGameResume?.Invoke();
                break;
        }
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
        if (m_PlayerPrefab is null)
        {
            Debug.LogError("There is no player prefab assigned!");
            return;
        }

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

public struct Body
{
    private Vector3 m_PreviousVelocity;
    private Rigidbody m_RigidBody;

    public Body(Rigidbody rigidbody, Vector3 previousVelocity)
    {
        m_PreviousVelocity = previousVelocity;
        m_RigidBody = rigidbody;
        Freeze();
    }

    public void Freeze()
    {
        m_RigidBody.isKinematic = true;
    }

    public void UnFreeze()
    {
        m_RigidBody.isKinematic = false;
        m_RigidBody.velocity = m_PreviousVelocity;
    }
}