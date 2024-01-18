using UnityEngine;
using UnityEngine.AI;

public class EnemyWaveGenerator : MonoBehaviour
{
    [Header("----------General----------")]

    [SerializeField] GameObject[] m_EnemiesToSpawn;

    [SerializeField] int m_InitialEnemyCount = 2;

    [SerializeField] int m_MaxEnemies = 10;

    [SerializeField] int m_EnemyAdditionOnEachWave = 1;

    [SerializeField] Vector2 m_LevelSize = new(250, 250);

    private int m_CurrentEnemyCount = 0;

    private int m_WaveCount;

    [Header("----------Debugging----------")]
    [SerializeField] bool m_VisualiseLevelBounds;
    [SerializeField] bool m_MakeLevelBoundsSolid;

    private void Awake()
    {
        m_CurrentEnemyCount = m_InitialEnemyCount;
    }

    [ContextMenu("Generate Wave")]
    public void GenerateWave()
    {
        if (m_WaveCount > 0)
            m_CurrentEnemyCount += m_EnemyAdditionOnEachWave;

        m_WaveCount++;

        int limit = m_CurrentEnemyCount > 10 ? 10 : m_CurrentEnemyCount;

        for (int i = 0; i < limit; i++)
        {
            GameObject GOEnemy = m_EnemiesToSpawn[Random.Range(0, m_EnemiesToSpawn.Length)];

            int maxIteration = 25;

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

            Instantiate(GOEnemy, randomPosition, Quaternion.identity, transform);
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
