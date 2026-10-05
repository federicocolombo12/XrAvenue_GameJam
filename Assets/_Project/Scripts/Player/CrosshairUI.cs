using UnityEngine;

namespace AvenueXR.Player
{
    public class CrosshairUI : MonoBehaviour
    {
        [Header("Settings")]
        public PlayerInteraction interaction;
        public float defaultDotSize = 6f;
        public float hoverDotSize = 10f;
        public Color normalColor = new Color(1f, 1f, 1f, 0.7f);
        public Color hoverColor = new Color(0.2f, 1f, 0.4f, 0.95f);

        private Texture2D _dotTexture;
        private GUIStyle _promptStyle;
        private GUIStyle _promptShadowStyle;

        private void Awake()
        {
            if (interaction == null)
            {
                interaction = GetComponentInParent<PlayerInteraction>();
            }

            // Generiamo una texture 2x2 bianca per il mirino runtime
            _dotTexture = new Texture2D(2, 2);
            Color[] colors = new Color[4] { Color.white, Color.white, Color.white, Color.white };
            _dotTexture.SetPixels(colors);
            _dotTexture.Apply();
        }

        private void InitStyles()
        {
            if (_promptStyle == null)
            {
                _promptStyle = new GUIStyle();
                _promptStyle.alignment = TextAnchor.MiddleCenter;
                _promptStyle.fontSize = 14;
                _promptStyle.fontStyle = FontStyle.Bold;
                _promptStyle.normal.textColor = Color.white;

                _promptShadowStyle = new GUIStyle(_promptStyle);
                _promptShadowStyle.normal.textColor = new Color(0f, 0f, 0f, 0.85f);
            }
        }

        private void OnGUI()
        {
            if (Cursor.lockState != CursorLockMode.Locked) return;

            InitStyles();

            bool isHovering = interaction != null && interaction.IsHoveringInteractable;
            float size = isHovering ? hoverDotSize : defaultDotSize;
            Color color = isHovering ? hoverColor : normalColor;

            float centerX = Screen.width * 0.5f;
            float centerY = Screen.height * 0.5f;

            // 1. Disegna il mirino al centro
            GUI.color = color;
            GUI.DrawTexture(new Rect(centerX - size * 0.5f, centerY - size * 0.5f, size, size), _dotTexture);

            // 2. Disegna la barra di progresso (se il tritatutto è attivo)
            if (interaction != null && interaction.HasActiveProgress)
            {
                float barWidth = 140f;
                float barHeight = 8f;
                float barX = centerX - barWidth * 0.5f;
                float barY = centerY + 22f;

                // Sfondo barra
                GUI.color = new Color(0.1f, 0.1f, 0.1f, 0.7f);
                GUI.DrawTexture(new Rect(barX - 1, barY - 1, barWidth + 2, barHeight + 2), _dotTexture);

                // Riempimento
                float progress = Mathf.Clamp01(interaction.ActiveCrushProgress);
                Color fillColor = Color.Lerp(new Color(1f, 0.6f, 0.1f, 0.95f), new Color(0.2f, 1f, 0.3f, 0.95f), progress);
                GUI.color = fillColor;
                GUI.DrawTexture(new Rect(barX, barY, barWidth * progress, barHeight), _dotTexture);
            }

            // 3. Disegna il prompt contestuale
            if (interaction != null && !string.IsNullOrEmpty(interaction.CurrentActionPrompt))
            {
                string prompt = interaction.CurrentActionPrompt;
                float promptY = interaction.HasActiveProgress ? centerY + 36f : centerY + 20f;
                Rect promptRect = new Rect(centerX - 200f, promptY, 400f, 24f);

                // Drop shadow
                GUI.Label(new Rect(promptRect.x + 1, promptRect.y + 1, promptRect.width, promptRect.height), prompt, _promptShadowStyle);
                // Testo principale
                GUI.color = Color.white;
                GUI.Label(promptRect, prompt, _promptStyle);
            }
        }

        private void OnDestroy()
        {
            if (_dotTexture != null)
            {
                Destroy(_dotTexture);
            }
        }
    }
}
