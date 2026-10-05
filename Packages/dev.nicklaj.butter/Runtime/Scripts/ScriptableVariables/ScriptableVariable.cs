using System;
using Newtonsoft.Json;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Events;
using Logger = Dev.Nicklaj.Butter.Helpers.Logger;

namespace Dev.Nicklaj.Butter
{
    public abstract class ScriptableVariable<T> : RuntimeScriptableObject
    {
        [SerializeField] T _initialValue;
        [SerializeField] T _value;

		public bool UseDebugLogs = false;
        public UnityAction<T> OnValueChanged = delegate { };

        [CreateProperty]
        public T Value
        {
            get => _value;
            set
            {
                if (this._value.Equals(value)) return;
                this._value = value;
				if(UseDebugLogs) 
                    Logger.LogInfo($"Variable {this.name} has changed to {value}.");
                OnValueChanged?.Invoke(value);
            }
        }

        [Obsolete("This should only be used when creating new scriptable variables, avoid calling this directly through code otherwise, it's here just for compatibility reasons.")]
        protected override void OnReset()
        {
            if(UseDebugLogs) 
                Logger.LogInfo($"Variable {this.name} has been reset to {_initialValue}.");
            OnValueChanged?.Invoke(_value = _initialValue);
        }
    }
}

public enum ResetOn
{
    SceneLoaded,
    ApplicationStart,
    None
}