using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PhysicsSimulation))]
public class MassObjectPlacer : MonoBehaviour
{
    [Header("----------General----------")]
    [SerializeField] ObjectToSpawn[] m_ObjectsToSpawn;
    [SerializeField] Vector3 m_SpawnVolume = Vector3.one;
    readonly List<GameObject> m_SpawnedObjects = new();
    PhysicsSimulation m_PhysicsSimulation;

    [Header("----------Randomisation----------")]
    [SerializeField] bool m_RaycastPosition = false;
    [SerializeField] bool m_RandomiseHeight = false;
    [SerializeField] bool m_RandomiseRotation = false;
    [SerializeField] bool m_RandomiseYRotation = false;
    [SerializeField] bool m_SimulatePhysics = false;

    private void OnValidate()
    {
        if (m_SpawnVolume.magnitude <= 0)
            m_SpawnVolume = Vector3.one * 2;

        if (m_PhysicsSimulation is null)
            m_PhysicsSimulation = GetComponent<PhysicsSimulation>();

        if (m_ObjectsToSpawn.Length <= 0 && m_SpawnedObjects != null &&  m_SpawnedObjects.Count > 0)
            m_SpawnedObjects.Clear();
    }

    [ContextMenu("Clear Items from List")]
    private void ClearItemsFromList()
    {
        m_SpawnedObjects.Clear();
    }

    [ContextMenu("Spawn Items")]
    private void SpawnItems()
    {
        if (m_SpawnedObjects.Count > 0)
            RemoveAllItems();

        if (m_ObjectsToSpawn.Length > 0)
        {

            for (int i = 0; i < m_ObjectsToSpawn.Length; i++)
            {
                for (int j = 0; j < m_ObjectsToSpawn[i].amount; j++)
                {
                    Vector3 pos;

                    Quaternion rot = Quaternion.identity;

                    if (!m_RaycastPosition)
                    {
                        pos = new()
                        {
                            x = transform.position.x + Random.Range(-m_SpawnVolume.x / 2, m_SpawnVolume.x / 2),
                            y = m_RandomiseHeight ? transform.position.y + Random.Range(-m_SpawnVolume.y / 2, m_SpawnVolume.y / 2) : 0,
                            z = transform.position.z + Random.Range(-m_SpawnVolume.z / 2, m_SpawnVolume.z / 2)
                        };

                        rot = m_RandomiseRotation ? Quaternion.Euler(Vector3.one * Random.Range(-360, 360)) : Quaternion.identity;

                        if (m_RandomiseYRotation)
                            rot.y = Quaternion.Euler(Vector3.up * Random.Range(-360, 360)).y;
                    }
                    else
                    {

                        pos = new()
                        {
                            x = transform.position.x + Random.Range(-m_SpawnVolume.x / 2, m_SpawnVolume.x / 2),
                            y = 100,
                            z = transform.position.z + Random.Range(-m_SpawnVolume.z / 2, m_SpawnVolume.z / 2)
                        };

                        if (Physics.Raycast(pos, Vector3.down, out RaycastHit hit))
                        {
                            pos = hit.point;

                            rot = Quaternion.LookRotation(transform.forward, hit.normal);
                        }
                    }

                    GameObject instantiatedObj = Instantiate(m_ObjectsToSpawn[i].prefab, pos, rot, transform);

                    m_SpawnedObjects.Add(instantiatedObj);
                }
            }

            if (m_SimulatePhysics)
            {
                //Simulate Physics...
                m_PhysicsSimulation.RunSimulation();
            }
        }
    }

    [ContextMenu("Remove all items")]
    private void RemoveAllItems()
    {
        if (m_SpawnedObjects.Count <= 0)
            return;

        for (int i = 0; i < m_SpawnedObjects.Count; i++)
        {
            if (m_SpawnedObjects[i] != null)
            {
                DestroyImmediate(m_SpawnedObjects[i]);
            }
        }
        m_SpawnedObjects.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireCube(transform.position, m_SpawnVolume);

        if (m_SpawnedObjects != null && m_SpawnedObjects.Count > 0)
        {
            foreach (GameObject go in m_SpawnedObjects)
            {
                if (go.TryGetComponent<MeshRenderer>(out var renderer))
                {
                    Gizmos.color = Color.magenta;

                    Gizmos.DrawWireCube(go.transform.position, renderer.bounds.size);
                }
            }
        }
    }


    [System.Serializable]
    struct ObjectToSpawn
    {
        public string name;
        public GameObject prefab;
        public int amount;
    }
}