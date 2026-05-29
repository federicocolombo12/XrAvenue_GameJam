using UnityEngine;
using TMPro; 
using Dev.Nicklaj.Butter;

namespace AvenueXR.Core
{
    public class BossScreenController : MonoBehaviour
    {
        public TextMeshProUGUI bossTextField;
        public StringEvent onBossSpeech;

        void OnEnable()
        {
            if (onBossSpeech != null) onBossSpeech.RegisterListener(UpdateBossText);
        }

        void OnDisable()
        {
            if (onBossSpeech != null) onBossSpeech.DeregisterListener(UpdateBossText);
        }

        private void UpdateBossText(string text)
        {
            if (bossTextField != null)
            {
                bossTextField.text = $"CAPO: {text}";
            }
            Debug.Log($"Schermo Capo: {text}");
        }
    }
}
