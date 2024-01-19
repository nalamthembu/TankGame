using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

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
    [SerializeField] bool m_DontSpawnAnyEnemies;
    [SerializeField] bool m_DontSpawnHelp;

    private GameObject m_PlayerGameObject;

    List<PickupBase> m_PickupsInScene = new();
    

    //Timers
    float m_PickupSpawnTimer = 0;
    float m_TimeSinceStartOfGame = 0;

    //Flags
    bool m_GameHasStarted;
    bool m_GameIsOver;

    private void Awake()
    {
        SpawnPlayerInRandomPosition();
    }

    private void Start()
    {
        m_GameHasStarted = true;
        m_GameIsOver = false;

        m_PickupSpawnTimer = m_PickupSpawnRate;

        if (EnemyWaveGenerator.Instance && !m_DontSpawnAnyEnemies)
        {
            EnemyWaveGenerator.Instance.GenerateWave();
        }
    }

    private void Update()
    {
        if (m_GameHasStarted && !m_GameIsOver)
            m_TimeSinceStartOfGame += Time.deltaTime;

        if (!m_DontSpawnAnyEnemies)
            SpawnEnemies();

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

    private void SpawnEnemies()
    {
        if (EnemyWaveGenerator.Instance != null &&
    EnemyWaveGenerator.Instance.AreAllEnemiesDefeated())
        {
            EnemyWaveGenerator.Instance.GenerateWave();
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
