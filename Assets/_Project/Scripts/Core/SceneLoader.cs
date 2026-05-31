using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

namespace AvenueXR.Core
{
    /// <summary>
    /// Gestisce il caricamento iniziale delle scene del gioco in modalità additiva.
    /// Da posizionare in una scena 'Init' vuota.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        [Header("Scenes to Load")]
        [Tooltip("La scena principale con la logica e il giocatore.")]
        public string mainSceneName = "MainScene";
        
        [Tooltip("La scena con l'ambiente e i visual.")]
        public string environmentSceneName = "Env";

        [Header("Settings")]
        [Tooltip("Se vero, la main scene verrà impostata come attiva al termine del caricamento.")]
        public bool setMainAsActive = true;

        private void Start()
        {
            StartCoroutine(LoadGameScenesRoutine());
        }

        private IEnumerator LoadGameScenesRoutine()
        {
            Debug.Log("[SceneLoader] Inizio caricamento additivo scene...");

            // 1. Carichiamo la Main Scene (Additiva)
            AsyncOperation mainLoad = SceneManager.LoadSceneAsync(mainSceneName, LoadSceneMode.Additive);
            while (!mainLoad.isDone)
            {
                yield return null;
            }
            Debug.Log($"[SceneLoader] Scena '{mainSceneName}' caricata.");

            // 2. Carichiamo la Env Scene (Additiva)
            AsyncOperation envLoad = SceneManager.LoadSceneAsync(environmentSceneName, LoadSceneMode.Additive);
            while (!envLoad.isDone)
            {
                yield return null;
            }
            Debug.Log($"[SceneLoader] Scena '{environmentSceneName}' caricata.");

            // 3. Impostiamo la scena attiva (Env solitamente contiene lighting e skybox)
            if (setMainAsActive)
            {
                Scene activeScene = SceneManager.GetSceneByName(environmentSceneName);
                if (activeScene.IsValid())
                {
                    SceneManager.SetActiveScene(activeScene);
                    Debug.Log($"[SceneLoader] Scena attiva impostata su: {environmentSceneName}");
                }
            }

            // 4. Scarichiamo la scena di Init (questa scena)
            Scene initScene = SceneManager.GetActiveScene(); // Al momento è Init se lanciato da lì
            // Per sicurezza cerchiamo per nome se l'active è già cambiata
            if (initScene.name != mainSceneName && initScene.name != environmentSceneName)
            {
                SceneManager.UnloadSceneAsync(initScene);
                Debug.Log($"[SceneLoader] Scena di inizializzazione '{initScene.name}' scaricata.");
            }

            Debug.Log("[SceneLoader] Caricamento completato con successo.");
        }
    }
}
