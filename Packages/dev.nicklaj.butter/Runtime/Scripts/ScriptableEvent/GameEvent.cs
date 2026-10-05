using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEditor;
using UnityEngine.Events;
using Logger = Dev.Nicklaj.Butter.Helpers.Logger;

namespace Dev.Nicklaj.Butter
{
    /// <summary>
    /// Generic GameEvent that will send a data of type T when raised. Couples with GameEventListener.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GameEvent<T> : ScriptableObject
    {
        protected readonly List<IGameEventListener<T>> listeners = new();
        public bool UseDebugLogs = false;
        public List<IGameEventListener<T>> Listeners => listeners;

        private UnityAction<T> _callbacks;

        public void Raise(T data, uint channel = 0)
        {
            if(UseDebugLogs)
                Logger.LogInfo($"Raising Event {name} with payload {data}");
            
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i].OnEventRaised(data, channel);
            }
            
            _callbacks?.Invoke(data);
        }

        public void RegisterListener(UnityAction<T> callback) => _callbacks += callback;
        public void DeregisterListener(UnityAction<T> callback) => _callbacks -= callback;
        public void RegisterListener(IGameEventListener<T> listener) => listeners.Add(listener);
        public void DeregisterListener(IGameEventListener<T> listener) => listeners.Remove(listener);
    }

    /// <summary>
    /// Non-generic version of GameEvent. You can use this version when the event you want to raise doesn't need to have any data associated with it.
    /// </summary>
    [CreateAssetMenu(menuName = "Butter/Events/Game Event")]
    public class GameEvent : GameEvent<Unit>, ISerializedRaise
    {
        public bool HasArgument => false;

        public void OnRaiseButtonSubmit(string arg0 = "", uint channel = 0)
        {
            Raise(channel);
        }

        public void Raise(uint channel = 0) => Raise(Unit.Default, channel);
    }

    public struct Unit
    {
        public static Unit Default => default;
    }
}

