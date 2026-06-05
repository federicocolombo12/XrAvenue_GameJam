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
            while (!mainLoad.isDone) yield return null;
            Debug.Log($"[SceneLoader] Scena '{mainSceneName}' caricata.");

            // 2. Carichiamo la Env Scene (Additiva)
            AsyncOperation envLoad = SceneManager.LoadSceneAsync(environmentSceneName, LoadSceneMode.Additive);
            while (!envLoad.isDone) yield return null;
            Debug.Log($"[SceneLoader] Scena '{environmentSceneName}' caricata.");

            // Aspettiamo un frame per sicurezza affinché Unity registri correttamente le scene
            yield return null;

            // 3. Impostiamo la scena attiva (Env solitamente contiene lighting e skybox)
            if (setMainAsActive)
            {
                Scene envScene = SceneManager.GetSceneByName(environmentSceneName);
                if (envScene.IsValid() && envScene.isLoaded)
                {
                    SceneManager.SetActiveScene(envScene);
                    Debug.Log($"[SceneLoader] Scena attiva impostata su: {environmentSceneName}");
                }
            }

            // 4. Scarichiamo la scena di Init (questa scena)
            Scene currentScene = gameObject.scene; 
            if (currentScene.IsValid() && currentScene.name != mainSceneName && currentScene.name != environmentSceneName)
            {
                Debug.Log($"[SceneLoader] Scarico la scena di inizializzazione: {currentScene.name}");
                SceneManager.UnloadSceneAsync(currentScene);
            }

            Debug.Log("[SceneLoader] Caricamento completato con successo.");
        }
    }
}
