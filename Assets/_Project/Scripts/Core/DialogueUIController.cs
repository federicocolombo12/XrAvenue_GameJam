using UnityEngine;
using TMPro;
using System.Collections;
using System;

namespace AvenueXR.Core
{
    public class DialogueUIController : MonoBehaviour
    {
        public TextMeshProUGUI textField;
        public CanvasGroup canvasGroup;
        public float fadeSpeed = 5f;

        private Coroutine _typewriterCoroutine;
        private bool _isTyping = false;

        void Awake()
        {
            if (canvasGroup != null) canvasGroup.alpha = 0;
        }

        public void ShowLine(string text, float speed, Action onComplete)
        {
            StopAllCoroutines();
            StartCoroutine(FadeInAndType(text, speed, onComplete));
        }

        public void Hide()
        {
            StopAllCoroutines();
            StartCoroutine(FadeOut());
        }

        private IEnumerator FadeInAndType(string text, float speed, Action onComplete)
        {
            if (canvasGroup != null)
            {
                while (canvasGroup.alpha < 1f)
                {
                    canvasGroup.alpha += Time.deltaTime * fadeSpeed;
                    yield return null;
                }
            }

            _isTyping = true;
            textField.text = "";
            
            foreach (char c in text.ToCharArray())
            {
                textField.text += c;
                yield return new WaitForSeconds(speed);
            }

            _isTyping = false;
            onComplete?.Invoke();
        }

        private IEnumerator FadeOut()
        {
            if (canvasGroup != null)
            {
                while (canvasGroup.alpha > 0f)
                {
                    canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
                    yield return null;
                }
            }
            textField.text = "";
        }
    }
}
