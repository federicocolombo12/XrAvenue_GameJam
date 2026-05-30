using UnityEngine;
using Dev.Nicklaj.Butter;

namespace AvenueXR.Core
{
    public class DayEndFader : MonoBehaviour
    {
        [Header("Butter Events")]
        public GameEvent onDayEnd;
        public DayDataEvent onDayStart; // Per resettare il fader a inizio giorno

        [Header("Animator Settings")]
        public Animator faderAnimator;
        public string dayEndBool = "DayEnd";

        void OnEnable()
        {
            if (onDayEnd != null)
                onDayEnd.RegisterListener(HandleDayEnd);

            if (onDayStart != null)
                onDayStart.RegisterListener(HandleDayStart);
        }

        void OnDisable()
        {
            if (onDayEnd != null)
                onDayEnd.DeregisterListener(HandleDayEnd);

            if (onDayStart != null)
                onDayStart.DeregisterListener(HandleDayStart);
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

