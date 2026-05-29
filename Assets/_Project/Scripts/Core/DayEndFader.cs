using UnityEngine;
using UnityEngine.UI;
using Dev.Nicklaj.Butter;
using PrimeTween;

namespace AvenueXR.Core
{
    public class DayEndFader : MonoBehaviour
    {
        [Header("Butter Events")]
        public GameEvent onDayEnd;

        [Header("UI References")]
        public CanvasGroup faderCanvasGroup;
        
        [Header("Settings")]
        public float blinkDuration = 0.5f;
        public int blinkCount = 2;

        void OnEnable()
        {
            if (onDayEnd != null) onDayEnd.RegisterListener(_ => PlayBlinkEffect());
        }

        void OnDisable()
        {
            if (onDayEnd != null) onDayEnd.DeregisterListener(_ => PlayBlinkEffect());
        }

        [ContextMenu("Test Blink")]
        public void PlayBlinkEffect()
        {
            if (faderCanvasGroup == null)
            {
                Debug.LogWarning("[DayEndFader] CanvasGroup non assegnato!");
                return;
            }

            Debug.Log("[DayEndFader] Inizio animazione blink fine giornata.");

            // Sequenza di blink: Fade In (nero) -> Fade Out
            // Usiamo SetLoops(count) sulla sequenza
            Sequence.Create(cycles: blinkCount)
                .Chain(Tween.Alpha(faderCanvasGroup, 1f, blinkDuration / 2f, Ease.InQuad))
                .Chain(Tween.Alpha(faderCanvasGroup, 0f, blinkDuration / 2f, Ease.OutQuad))
                .OnComplete(() => Debug.Log("[DayEndFader] Animazione completata."));
        }
    }
}
