using System;
using System.Collections;
using Dev.Nicklaj.Butter;
using UnityEngine;
using Random = UnityEngine.Random;

public class PoolSample_EnemySpawner : MonoBehaviour
{
    [SerializeField] ScriptablePool Pool;
    public float DelayBetweenSpawns = .5f;
    public float SpawnRadius = 10f;
    
    private Vector3 _spawnPoint;

    private void Awake()
    {
        _spawnPoint = transform.position;
        Pool.Action_OnPooledItemCreated += (_) => Debug.Log("Pooled item created");
        Pool.Action_OnTakeFromPool += (_) => Debug.Log("Pooled item taken from pool");
    } 

    private void Start()
    {
        StartSpawn();
    }

    [ContextMenu("Start Spawn")]
    public void StartSpawn()
    {
        StartCoroutine(SpawnEnemies());
    }

    IEnumerator SpawnEnemies()
    {
        while (Pool?.ActiveObjects?.Items.Count < Pool?.maxPoolSize)
        {
            GameObject go = Pool?.Pool.Get();
            go.transform.position = GetRandomSpawnPoint();
            yield return new WaitForSeconds(DelayBetweenSpawns);
        }
    }

    private Vector3 GetRandomSpawnPoint()
    {
        double a = Random.Range(0f, 1f) * 2 * Math.PI;
        double r = SpawnRadius * Math.Sqrt(Random.Range(0f, 1f));
            
        double x = r * Math.Cos(a);
        double z = r * Math.Sin(a);
        
        return new Vector3((float)x, _spawnPoint.y, (float)z);
    }
}
