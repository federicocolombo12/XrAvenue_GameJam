using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dev.Nicklaj.Butter
{
    public abstract class RuntimeScriptableObject : ScriptableObject
    {
        static readonly List<RuntimeScriptableObject> Instances = new();
        public ResetOn ResetOn = ResetOn.SceneLoaded;

        void OnEnable() => Instances.Add(this);
        void OnDisable() => Instances.Remove(this);
        protected abstract void OnReset();
        
        /// <summary>
        /// Manually reset variable to initial state.
        /// </summary>
        public void Reset() => OnReset();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void ResetAllInstancesOnSceneLoad()
        {
            foreach (RuntimeScriptableObject instance in Instances)
            {
                if (instance.ResetOn != ResetOn.SceneLoaded) continue;
                instance.OnReset();
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void ResetAllInstancesOnApplicationStart()
        {
            foreach (RuntimeScriptableObject instance in Instances)
            {
                if (instance.ResetOn != ResetOn.ApplicationStart) continue;
                instance.OnReset();
            }
        }
    }

}
