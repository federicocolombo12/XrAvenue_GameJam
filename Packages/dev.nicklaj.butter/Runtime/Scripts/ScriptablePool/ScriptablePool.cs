using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;

namespace Dev.Nicklaj.Butter
{
    [CreateAssetMenu(fileName = "ScriptablePool", menuName = "Butter/Object Pool")]
    public class ScriptablePool : ScriptableObject
    {
        public enum PoolType
        {
            Stack,
            LinkedList
        }
        
        public PoolType poolType;
        
        // Collection checks will throw errors if we try to release an item that is already in the pool.
        public bool collectionChecks = true;
        public bool instantiateOnStart = true;
        public int maxPoolSize = 10;

        /// <summary>
        /// Reference to the prefab that should be instantiated.
        /// </summary>
        public GameObject PooledObject;
        
        IObjectPool<GameObject> m_Pool;
        
        public IObjectPool<GameObject> Pool
        {
            get
            {
                if (m_Pool == null)
                {
                    if (poolType == PoolType.Stack)
                        m_Pool = new ObjectPool<GameObject>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, collectionChecks, 10, maxPoolSize);
                    else
                        m_Pool = new LinkedPool<GameObject>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, collectionChecks, maxPoolSize);
                }
                return m_Pool;
            }
        }

        public GameObjectRuntimeSet ActiveObjects;

        /// <summary>
        /// This action is called when a new item is created to be inserted in the pool.
        /// </summary>
        public UnityAction<GameObject> Action_OnPooledItemCreated;
        /// <summary>
        /// This action is called when an item is successfully returned to the pool.
        /// </summary>
        public UnityAction<GameObject> Action_OnReturnedToPool;
        /// <summary>
        /// This action is called when an item is successfully taken from the pool.
        /// </summary>
        public UnityAction<GameObject> Action_OnTakeFromPool;
        /// <summary>
        /// This action is called when an eccess item is about to be destroyed.
        /// </summary>
        public UnityAction<GameObject> Action_OnBeforeDestroyPoolObject;
        /// <summary>
        /// This action is called when an eccess item has succesfully been destroyed.
        /// </summary>
        public UnityAction Action_OnAfterDestroyPoolObject;

        GameObject CreatePooledItem()
        {
            var go = Instantiate(PooledObject);

            // This is used to return ParticleSystems to the pool when they have stopped.
            var returnToPool = go.AddComponent<ReturnToPool>();
            returnToPool.pool = Pool;
            Action_OnPooledItemCreated?.Invoke(go);
            return go;
        }
        
        // Called when an item is returned to the pool using Release
        void OnReturnedToPool(GameObject item)
        {
            ActiveObjects?.Remove(item);
            Action_OnReturnedToPool?.Invoke(item);
            item.SetActive(false);
        }
        
        // Called when an item is returned to the pool using Release
        void OnTakeFromPool(GameObject item)
        {
            ActiveObjects?.Add(item);
            Action_OnTakeFromPool?.Invoke(item);
            item.SetActive(true);
        }
        
        
        // If the pool capacity is reached then any items returned will be destroyed.
        // We can control what the destroy behavior does, here we destroy the GameObject.
        void OnDestroyPoolObject(GameObject item)
        {
            Action_OnBeforeDestroyPoolObject?.Invoke(item);
            Destroy(item);
            Action_OnAfterDestroyPoolObject?.Invoke();
        }
    }
    
    public class ReturnToPool : MonoBehaviour
    {
        public IObjectPool<GameObject> pool;
        /// <summary>
        /// This action is triggered when the item is returned to the pool. Notice that there is no guarantee that the pool won't destroy this item in case it's in eccess.
        /// </summary>
        private UnityAction<GameObject> OnReturnedToPool;

        /// <summary>
        /// This wrapper method allows to return the object to the pool.
        /// </summary>
        public void Release()
        {
            OnReturnedToPool?.Invoke(gameObject);
            pool.Release(this.gameObject);
        }
    }
}