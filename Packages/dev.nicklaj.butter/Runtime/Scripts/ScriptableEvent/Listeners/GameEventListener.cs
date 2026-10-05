
using System;
using UnityEngine;
using UnityEngine.Events;
using Logger = Dev.Nicklaj.Butter.Helpers.Logger;


namespace Dev.Nicklaj.Butter
{
    /// <summary>
    /// Generic GameEventListener. You can set any type of data that will be sent when the GameEvent is raised. Couples with GameEvent.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GameEventListener<T> : MonoBehaviour, IGameEventListener<T>
    {
        [SerializeField] GameEvent<T> _gameEvent;
        public uint Channel = 0;
        /// <summary>
        /// UnityEvents that will be invoked when the GameEvent is raised. Possibly, set this via the inspector.
        /// </summary>
        [SerializeField] UnityEvent<T> _response;

        public void Awake() => Target = this.gameObject;
        void OnEnable() => _gameEvent?.RegisterListener(this);
        void OnDisable() => _gameEvent?.DeregisterListener(this);
        
        public GameObject Target { get; set; }

        public void OnEventRaised(T data, uint channel)
        {
            if (Channel != channel) return;
            if (_gameEvent.UseDebugLogs)
                Logger.LogInfo($"{this.gameObject.name} received event {_gameEvent.name} with payload {data}");
            _response.Invoke(data);
        }
    }

    /// <summary>
    /// Non-generic GameEventListener. Use when the event doesn't have any data associated with it.
    /// </summary>
    public class GameEventListener : GameEventListener<Unit> { }
}


