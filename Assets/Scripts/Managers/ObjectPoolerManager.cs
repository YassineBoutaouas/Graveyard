using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolerManager : MonoBehaviour
{
    public static ObjectPoolerManager Instance;

    [System.Serializable]
    public class Pool
    {
        public string Tag;
        public GameObject Prefab;
        public Transform PoolParent;
        public int PoolSize;

        public bool HasSleepTime = false;
        public float SleepTime = 100;
    }

    public List<Pool> PoolsList;
    private Dictionary<string, Queue<GameObject>> PoolDictionary = new Dictionary<string, Queue<GameObject>>();

    public static ObjectPoolerManager GetInstance()
    {
        return Instance;
    }

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        int charactersToExclude = "(Clone)".Length;

        foreach (Pool pool in PoolsList)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            pool.Prefab.SetActive(false);

            for (int i = 0; i < pool.PoolSize; i++)
            {
                GameObject obj = Instantiate(pool.Prefab, pool.PoolParent);

                obj.name = obj.name.Substring(0, obj.name.Length - charactersToExclude);

                objectPool.Enqueue(obj);
            }

            PoolDictionary.Add(pool.Tag, objectPool);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        if (!PoolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " does not exist! Please assign tag or check pool properties");
            return null;
        }

        GameObject objectToSpawn = PoolDictionary[tag].Dequeue();

        if (objectToSpawn)
        {
            objectToSpawn.SetActive(true);
            objectToSpawn.transform.position = spawnPosition;
            objectToSpawn.transform.rotation = spawnRotation;
        }

        if (objectToSpawn.TryGetComponent(out IPooledObjects pooledObject))
            pooledObject.OnObjectsSpawn();

        PoolDictionary[tag].Enqueue(objectToSpawn);

        Pool pool = GetPool(tag);

        if (pool.HasSleepTime) StartCoroutine(SleepRoutine(pool, objectToSpawn));

        return objectToSpawn;
    }

    private IEnumerator SleepRoutine(Pool p, GameObject g)
    {
        yield return new WaitForSeconds(p.SleepTime);

        if (g.TryGetComponent(out IPooledObjects pooledObject))
            pooledObject.OnObjectsDeSpawn();

        g.SetActive(false);
    }

    public Pool GetPool(string tag)
    {
        Pool p = null;
        foreach (Pool pool in PoolsList)
        {
            if (pool.Tag == tag)
                p = pool;
        }

        return p;
    }
}

public interface IPooledObjects
{
    void OnObjectsSpawn();

    void OnObjectsDeSpawn();
}