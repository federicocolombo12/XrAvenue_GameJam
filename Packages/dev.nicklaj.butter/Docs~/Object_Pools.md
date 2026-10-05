## Using ObjectPools to spawn and recycle GameObjects
Object Pools are a great way to save memory in your game. If you don't know what these are I recommend to check out this great game-engine agnostic [explaination by Robert Nystrom](https://gameprogrammingpatterns.com/object-pool.html).

So let's say we have a [vampire survivors](https://store.steampowered.com/app/1794680/Vampire_Survivors/) type of game where you have a lot of entities spawning and you want to optimize your game by recycling them when they die. For our testing purposes they will die when they collide with the player.

Butter offers a `Object Pool` scriptable type which will take as fields the prefab of the enemy to create, the max pool size, whether to use a stack or linked list as a pooling algorithm and finally an optional runtime set for the active objects.

![](/Docs~/Assets/Object_Pool.png)

My implementation currently uses the Unity [default Object Pool](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Pool.IObjectPool_1.html) that was introduced in 2021.3 to avoid reinventing the wheel.

I'm going to assume you know how to setup rigidbody collision and skip to the script on the enemy prefab:

```csharp
public class ReturnOnCollision : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        ReturnToPool rtp = this.GetComponent<ReturnToPool>();
        if (rtp != null && other.collider.tag == "Player")
        {
            Debug.Log("I got killed");
            rtp.Release();
        }
    }
}
```

As you can see the most important feat of this code is calling `ReturnToPool.Release()`. When your object gets instanced the pool will automatically add a `ReturnToPool` component, so this is what is getting called here. It's important that the pool is the one to add the component so that it can link itself in the script, otherwise the release might not work.

Now let's quickly setup the enemy spawner that in our case will spawn enemies in a radius.

```csharp
public class EnemySpawner : MonoBehaviour
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
        ...
    }
}
```

In the `SpawnEnemies` method I'm counting the active objects by referencing the runtime set that I've linked to the pool.

> [!tip]
> Remember that runtime sets use a [flyweight pattern](https://gameprogrammingpatterns.com/flyweight.html) so they shouldn't increase your memory usage considerably.

Now when we press play, the game should generate two enemies per second up to ten enemies, and when they collide with the player they will get killed and return to the pool for later usage. Just for debug you can see I've added a couple of logs for when an item gets created and when an item gets fetched using UnityActions. 

The pool actually supports five different unity actions:
```csharp
        public UnityAction<GameObject> Action_OnPooledItemCreated;
        public UnityAction<GameObject> Action_OnReturnedToPool;
        public UnityAction<GameObject> Action_OnTakeFromPool;
        public UnityAction<GameObject> Action_OnBeforeDestroyPoolObject;
        public UnityAction Action_OnAfterDestroyPoolObject;
```

Let's fire our scene and let's see what happens.

![](/Docs~/Assets/Enemy_Pool.png)

Our enemies were correctly created, now let's collide with a couple of them to see whether everything is working smoothly.

| Console | Hierarchy |
| - | - |
| ![](/Docs~/Assets/Pool_Console_1.png) | ![](/Docs~/Assets/Pool_Hierarchy.png) |

It sure looks like everything is working. When enemies get killed they actually deactivate rather than get destroyed.

Now if I retrigger the spawn you can see that the enemies are only getting taken from the pool and activated, not instanced.

![](/Docs~/Assets/Pool_Retrigger.png)

> [!warning]
> If we were to spawn more enemies than what the pool can contain, upon release the eccess items will get destroyed.

This might look like an awful lot of code but you might notice that most of the code we've implemented was actually scene logic, not object pool logic. 
This means that by using this implementation of object pools you can cut down on time spent creating and debugging the pool and spend more time actually working on the game itself.