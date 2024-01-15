using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectPoolManager : MonoBehaviour
{
    public Pool[] pools;

    public Dictionary<string, Pool> poolDictionary;

    public static ObjectPoolManager Instance;

    private void Awake()
    {
        if (Instance is not null)
            Destroy(gameObject);
        else Instance = this;

        InitialisePools();
    }

    private void OnDestroy()
    {
        Instance = null;
        Debug.Log("Destroyed Object Pool Instance!");
    }

    public Pool GetPool(string poolName)
    {
        if (poolDictionary.TryGetValue(poolName, out Pool value))
        {
            return value;
        }

        Debug.LogError("Couldn't find " + poolName + " pool");

        return null;
    }


    public bool TryGetPool(string poolName, out Pool pool)
    {
        if (poolDictionary.TryGetValue(poolName, out Pool value))
        {
            pool = value;

            return true;
        }

        Debug.LogError("Couldn't find " + poolName + " pool");

        pool = null;

        return false;
    }

    private void InitialisePools()
    {
        poolDictionary = new();

        for (int i = 0; i < pools.Length; i++)
        {
            pools[i].Awake(transform);
            poolDictionary.Add(pools[i].name, pools[i]);
        }
    }

    private IEnumerator DelayedDeactivate(GameObject gameObject, float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }

    public void ReturnGameObject(GameObject gameObject, float delay = 0f) => StartCoroutine(DelayedDeactivate(gameObject, delay));
    
}

[System.Serializable]
public class Pool
{
    public string name;
    [SerializeField] GameObject gameObject;
    [SerializeField] int amount;
    private List<GameObject> pooledObjects;
    [HideInInspector] public Transform objectPoolManager;
    private Transform poolManagerTransform;
    private Transform poolParent;

    public void OnValidate()
    {
        if (amount <= 0)
        {
            amount = 1;
        }
    }

    public void Awake(Transform poolManagerTransform)
    {
        this.poolManagerTransform = poolManagerTransform;

        pooledObjects = new();

        //Start of Pool Parent initialisation.
        poolParent = new GameObject(name + "s").transform;

        poolParent.parent = poolManagerTransform;

        poolParent.transform.localPosition = poolParent.localEulerAngles *= 0;
        //End of pool parent initialisation.

        for (int i = 0; i < amount; i++)
        {
            AddGameObjectToPool();
        }
    }

    private void AddGameObjectToPool()
    {
        GameObject gObj = Object.Instantiate(gameObject, objectPoolManager);

        gObj.SetActive(false);

        //Set gObj Parent to pool parent (this is just to organise the hierachy a little bit)

        gObj.transform.parent = poolParent;

        pooledObjects.Add(gObj);
    }

    public GameObject GetGameObject()
    {
        if (TryGetGameObject(out GameObject pooledObject))
        {
            return pooledObject;
        }

        return null;
    }

    public bool TryGetGameObject(out GameObject GOPoolObject)
    {
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (!pooledObjects[i].activeInHierarchy)
            {
                pooledObjects[i].SetActive(true);

                GOPoolObject = pooledObjects[i];

                return true;
            }
        }

        GOPoolObject = null;

        Debug.LogWarning("Could not get object from " + name + " pool. Adding an additional Object to the pool.");

        AddGameObjectToPool();

        if (TryGetGameObject(out GameObject pooledObj))
        {
            GOPoolObject = pooledObj;
            return true;
        }

        return false;
    }
}