using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Logger = Dev.Nicklaj.Butter.Helpers.Logger;

namespace Dev.Nicklaj.Butter
{

    /// <summary>
    /// Base Runtime Set class. Extend this to create your scriptable list that will act
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class RuntimeSetBase<T> : RuntimeScriptableObject, IRuntimeSet
    {
        /// <summary>
        /// Item set.
        /// </summary>
        [NonSerialized] public List<T> Items = new List<T>();

        public bool UseDebugLogs = false;


        public void Add(T item)
        {
            if (Items.Contains(item))
            {
                if(UseDebugLogs) Logger.LogInfo($"Runtime set {this.name} already contains an instance of {item.ToString()}.");
                return;
            }
            
            if(UseDebugLogs) Logger.LogInfo($"Adding {item.ToString()} to {this.name}.");
            Items.Add(item);
        }

        public void Remove(T item)
        {
            if (!Items.Contains(item))
            {
                if(UseDebugLogs) Logger.LogInfo($"Trying to remove an instance of {item.ToString()} from {this.name}, however no instance was found.");
                return;
            }
            
            if(UseDebugLogs) Logger.LogInfo($"Removing {item.ToString()} from {this.name}.");
            Items.Remove(item);
        }
        
        protected override void OnReset()
        {
            if (Items != null && Items.Count > 0)
                Items.Clear();
        }
    }
}