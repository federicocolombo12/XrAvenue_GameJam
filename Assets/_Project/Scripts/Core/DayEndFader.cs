using UnityEngine;
using Dev.Nicklaj.Butter;

namespace AvenueXR.Core
{
    public class DayEndFader : MonoBehaviour
    {
        [Header("Butter Events")]
        public GameEvent onDayEnd;
        public DayDataEvent onDayStart; 

        [Header("Animator Settings")]
        public Animator faderAnimator;
        public string dayEndBool = "DayEnd";

        [Header("Safety Reset")]
        public CanvasGroup faderCanvasGroup;

        private void Awake()
        {
            // Sicurezza: forziamo la trasparenza all'avvio nel caso l'alpha sia rimasta a 1 nell'editor
            if (faderCanvasGroup != null)
            {
                faderCanvasGroup.alpha = 0f;
            }
        }

        void OnEnable()
        {
            // Usiamo una lambda per gestire il parametro richiesto dal sistema di eventi di Butter
            if (onDayEnd != null)
                onDayEnd.RegisterListener(HandleDayEndWrapper);
            
            if (onDayStart != null)
                onDayStart.RegisterListener(HandleDayStart);
        }

        void OnDisable()
        {
            if (onDayEnd != null)
                onDayEnd.DeregisterListener(HandleDayEndWrapper);

            if (onDayStart != null)
                onDayStart.DeregisterListener(HandleDayStart);
        }

        // Wrapper per compatibilità con l'evento di Butter
        private void HandleDayEndWrapper(Unit unit)
        {
            HandleDayEnd();
        }

        private void HandleDayEnd()
        {
            if (faderAnimator != null)
            {
                Debug.Log("[DayEndFader] Attivazione animazione Fine Giornata.");
                faderAnimator.SetBool(dayEndBool, true);
            }
            else
            {
                Debug.LogWarning("[DayEndFader] Animator non assegnato!");
            }
        }

        private void HandleDayStart(DayData data)
        {
            if (faderAnimator != null)
            {
                Debug.Log("[DayEndFader] Reset animazione per Inizio Giornata.");
                faderAnimator.SetBool(dayEndBool, false);
            }
        }
    }
}
