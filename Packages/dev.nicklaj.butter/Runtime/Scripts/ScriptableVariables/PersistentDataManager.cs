using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Dev.Nicklaj.Butter
{
    public class PersistentDataManager : MonoBehaviour
    {
        public PersistentDataMasterList persistentDataMasterList;
        
        [Header("Events")]
        [Tooltip("Event that will be raised when the game has finished loading.")]
        public GameEvent GameFinishedLoading;
        [Tooltip("Event that will be raised when the game has finished saving.")]
        public GameEvent GameFinishedSaving;

        public UnityAction OnLoadingFinished;
        public UnityAction OnSavingFinished;

        public void SaveData()
        {
            if (persistentDataMasterList == null) return;
            persistentDataMasterList.SaveData();
            GameFinishedSaving?.Raise();
            OnSavingFinished?.Invoke();
        }

        public void LoadData()
        {
            if (persistentDataMasterList == null) return;
            persistentDataMasterList.LoadData();
            GameFinishedLoading?.Raise();
            OnLoadingFinished?.Invoke();
        }
    }
}