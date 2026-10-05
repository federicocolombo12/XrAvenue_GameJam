using UnityEngine;

namespace Dev.Nicklaj.Butter
{
    public class InitializePoolOnLoad : MonoBehaviour
    {
        public ScriptablePool ObjectPool;
    
        void Awake()
        {
            if (!ObjectPool) return;
            
            for(int i = 0; i < ObjectPool.maxPoolSize; i++)
                ObjectPool.Pool.Get().SetActive(false);
            
            Destroy(this);
        }
    }
}