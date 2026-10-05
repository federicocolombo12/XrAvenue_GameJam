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

        private void OnGUI()
        {
            if (Cursor.lockState != CursorLockMode.Locked) return;

            bool isHovering = interaction != null && interaction.IsHoveringInteractable;
            float size = isHovering ? hoverDotSize : defaultDotSize;
            Color color = isHovering ? hoverColor : normalColor;

            float x = (Screen.width - size) * 0.5f;
            float y = (Screen.height - size) * 0.5f;

            GUI.color = color;
            GUI.DrawTexture(new Rect(x, y, size, size), _dotTexture);
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
