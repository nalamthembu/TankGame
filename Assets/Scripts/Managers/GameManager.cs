using UnityEngine;
using UnityEngine.AI;

public class GameManager : MonoBehaviour
{
    [Header("----------General----------")]
    [SerializeField] GameObject m_PlayerPrefab;
    [SerializeField] bool m_SpawnInRandomLocation = true;
    [SerializeField] Vector2 m_LevelSize = new(250, 250);

    [Header("----------Debugging----------")]
    [SerializeField] bool m_VisualiseLevelBounds;
    [SerializeField] bool m_MakeLevelBoundsSolid;
    [SerializeField] bool m_DontSpawnAnyEnemies;

    private GameObject m_PlayerGameObject;

    private void Awake()
    {
        SpawnPlayerInRandomPosition();
    }

    private void Start()
    {
        if (EnemyWaveGenerator.Instance && !m_DontSpawnAnyEnemies)
        {
            EnemyWaveGenerator.Instance.GenerateWave();
        }
    }

    private void Update()
    {
        if (!m_DontSpawnAnyEnemies)
            SpawnEnemies();
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
                    randomPosition = hit.position;

                    break;
                }

                Debug.Log("Ran out of iterations defaulting position to World Origin");
            }

            m_PlayerGameObject = Instantiate(m_PlayerPrefab, randomPosition, Quaternion.identity);
        }
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
