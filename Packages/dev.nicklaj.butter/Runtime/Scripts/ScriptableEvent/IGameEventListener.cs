using UnityEngine;

namespace Dev.Nicklaj.Butter
{
    public interface IGameEventListener<T>
    {
        /// <summary>
        /// Target of the event. Used to show in the editor all the listeners attached to the event.
        /// </summary>
        public GameObject Target { get; set; }
        void OnEventRaised(T data, uint channel = 0);
    }

}