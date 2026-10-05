using UnityEngine;
using System.Collections;
using Dev.Nicklaj.Butter;

namespace AvenueXR.Core
{
    /// <summary>
    /// Gestore autonomo del Fade to Black per il passaggio tra giornate e per i finali.
    /// Disegna un overlay a schermo intero indipendente da Canvas o Animator fragili.
    /// </summary>
    public class DayEndFader : MonoBehaviour
    {
        [Header("Butter Events")]
        public GameEvent onDayEnd;
        public DayDataEvent onDayStart; 
        public DayDataEvent onFinaleReached;

        [Header("Fade Settings")]
        [Tooltip("Durata in secondi della dissolvenza a nero e da nero.")]
        public float fadeDuration = 0.8f;
        [Tooltip("Se vero, esegue una dissolvenza da nero all'avvio della scena.")]
        public bool fadeInOnStart = true;
        [Tooltip("Se vero, mostra il nome del giorno al centro dello schermo nel buio.")]
        public bool showDayTitle = true;

        [Header("Optional References (Retrocompatibilità)")]
        public Animator faderAnimator;
        public string dayEndBool = "DayEnd";
        public CanvasGroup faderCanvasGroup;

        private float _currentAlpha = 0f;
        private string _overlayText = string.Empty;
        private Texture2D _blackTexture;
        private Coroutine _fadeCoroutine;
        private GUIStyle _titleStyle;

        private void Awake()
        {
            // Crea texture nera pura 2x2
            _blackTexture = new Texture2D(2, 2);
            Color[] pixels = new Color[] { Color.black, Color.black, Color.black, Color.black };
            _blackTexture.SetPixels(pixels);
            _blackTexture.Apply();

            if (fadeInOnStart)
            {
                _currentAlpha = 1f;
            }
            else
            {
                _currentAlpha = 0f;
            }

            if (faderCanvasGroup != null)
            {
                faderCanvasGroup.alpha = 0f;
            }
        }

        private void Start()
        {
            if (fadeInOnStart)
            {
                StartFade(0f, fadeDuration);
            }
        }

        private void OnEnable()
        {
            if (onDayEnd != null)
                onDayEnd.RegisterListener(HandleDayEndWrapper);
            
            if (onDayStart != null)
                onDayStart.RegisterListener(HandleDayStart);

            if (onFinaleReached != null)
                onFinaleReached.RegisterListener(HandleFinaleReached); 
        }

        private void OnDisable()
        {
            if (onDayEnd != null)
                onDayEnd.DeregisterListener(HandleDayEndWrapper);

            if (onDayStart != null)
                onDayStart.DeregisterListener(HandleDayStart);
            
            if (onFinaleReached != null)
                onFinaleReached.DeregisterListener(HandleFinaleReached);
        }

        private void HandleDayEndWrapper(Unit unit)
        {
            HandleDayEnd();
        }

        private void HandleDayEnd()
        {
            Debug.Log("[DayEndFader] Attivazione dissolvenza Fine Giornata (Fade to Black).");
            _overlayText = "FINE TURNO";
            StartFade(1f, fadeDuration);

            // Retrocompatibilità
            if (faderAnimator != null)
            {
                faderAnimator.SetBool(dayEndBool, true);
            }
        }

        private void HandleDayStart(DayData data)
        {
            Debug.Log($"[DayEndFader] Inizio nuova giornata (Fade from Black): {(data != null ? data.dayLabel : "Nuovo Giorno")}");
            if (data != null && !string.IsNullOrEmpty(data.dayLabel))
            {
                _overlayText = data.dayLabel;
            }

            StartFade(0f, fadeDuration);

            // Retrocompatibilità
            if (faderAnimator != null)
            {
                faderAnimator.SetBool(dayEndBool, false);
            }
        }

        private void HandleFinaleReached(DayData data)
        {
            Debug.Log("[DayEndFader] Finale raggiunto. Schermo mantenuto NERO.");
            _overlayText = data != null ? data.endingTitle : string.Empty;
            _currentAlpha = 1f;

            if (faderAnimator != null)
            {
                faderAnimator.SetBool(dayEndBool, true);
            }
        }

        public void StartFade(float targetAlpha, float duration)
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }
            _fadeCoroutine = StartCoroutine(FadeRoutine(targetAlpha, duration));
        }

        private IEnumerator FadeRoutine(float targetAlpha, float duration)
        {
            float startAlpha = _currentAlpha;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                _currentAlpha = Mathf.SmoothStep(startAlpha, targetAlpha, t);

                if (faderCanvasGroup != null)
                {
                    faderCanvasGroup.alpha = _currentAlpha;
                }

                yield return null;
            }

            _currentAlpha = targetAlpha;
            if (faderCanvasGroup != null)
            {
                faderCanvasGroup.alpha = _currentAlpha;
            }

            if (_currentAlpha <= 0.001f)
            {
                _overlayText = string.Empty;
            }

            _fadeCoroutine = null;
        }

        private void InitTitleStyle()
        {
            if (_titleStyle == null)
            {
                _titleStyle = new GUIStyle();
                _titleStyle.alignment = TextAnchor.MiddleCenter;
                _titleStyle.fontSize = 28;
                _titleStyle.fontStyle = FontStyle.Bold;
                _titleStyle.normal.textColor = Color.white;
            }
        }

        private void OnGUI()
        {
            if (_currentAlpha <= 0.001f) return;

            InitTitleStyle();

            // Garantisce che il nero sia renderizzato sopra a qualsiasi elemento a schermo
            GUI.depth = -1000;

            Color oldColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, _currentAlpha);

            // Disegna rettangolo nero a schermo intero
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _blackTexture);

            // Disegna eventuale testo diegetico al centro dello schermo nel buio
            if (showDayTitle && !string.IsNullOrEmpty(_overlayText) && _currentAlpha > 0.4f)
            {
                float textAlpha = Mathf.Clamp01((_currentAlpha - 0.4f) / 0.6f);
                Color textColor = new Color(1f, 1f, 1f, textAlpha);
                
                // Ombra sottile
                GUI.color = new Color(0f, 0f, 0f, textAlpha * 0.8f);
                GUI.Label(new Rect(2, 2, Screen.width, Screen.height), _overlayText, _titleStyle);

                // Testo principale
                GUI.color = textColor;
                GUI.Label(new Rect(0, 0, Screen.width, Screen.height), _overlayText, _titleStyle);
            }

            GUI.color = oldColor;
        }

        private void OnDestroy()
        {
            if (_blackTexture != null)
            {
                Destroy(_blackTexture);
            }
        }
    }
}
