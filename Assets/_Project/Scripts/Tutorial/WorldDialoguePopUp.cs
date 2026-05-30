using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(Canvas))]
public class WorldDialoguePopup : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Il Panel (o RectTransform root) dentro al Canvas")]
    public RectTransform panel;

    [Tooltip("Il componente TextMeshProUGUI per il nome del parlante (appare istantaneamente)")]
    public TextMeshProUGUI speakerNameText;

    [Tooltip("Il componente TextMeshProUGUI per il testo del dialogo")]
    public TextMeshProUGUI dialogueText;

    [Header("Timing")]
    [Tooltip("Durata dell'animazione di apertura/chiusura (secondi)")]
    public float animDuration = 0.5f;

    [Tooltip("Secondi tra una lettera e l'altra nel typewriter")]
    public float typewriterDelay = 0.04f;

    private Coroutine _typewriterCoroutine;
    private bool _isOpen = false;
    private System.Action _onTypewriterComplete;

    private void Awake()
    {
// ... (rest of Awake unchanged)

        if (panel != null)
            panel.localScale = Vector3.zero;

        if (dialogueText != null)
            dialogueText.text = "";

        if (speakerNameText != null)
            speakerNameText.text = "";

        gameObject.SetActive(false);
    }

    public void Open()
    {
        if (_isOpen) return;
        gameObject.SetActive(true);
        _isOpen = true;
        StopTypewriter();
        panel.DOKill();
        OpenAnim();
    }

    public void ShowDialogue(string message, string speakerName = "", System.Action onComplete = null)
    {
        panel.DOKill();
        StopTypewriter();
        _onTypewriterComplete = onComplete;

        if (speakerNameText != null)
            speakerNameText.text = speakerName ?? "";

        // Se il pannello è chiuso o quasi chiuso (magari per un'animazione di chiusura in corso)
        if (!_isOpen || panel.localScale.x < 0.1f)
        {
            gameObject.SetActive(true);
            _isOpen = true;
            OpenAnim(() => StartTypewriter(message));
        }
        else
        {
            dialogueText.text = "";
            StartTypewriter(message);
        }
    }

    public void Clean()
    {
        StopTypewriter();
        panel.DOKill();
        if (dialogueText != null)
            dialogueText.text = "";
    }

    public void Close()
    {
        if (!_isOpen) return;
        StopTypewriter();
        panel.DOKill();
        CloseAnim();
    }

    private void OpenAnim(TweenCallback onComplete = null)
    {
        gameObject.SetActive(true);
        _isOpen = true;

        panel.localScale = Vector3.zero;

        panel.DOScale(Vector3.one, animDuration)
             .SetEase(Ease.OutBack)
             .OnComplete(onComplete);

        panel.localRotation = Quaternion.Euler(0f, 0f, -4f);
        panel.DOLocalRotate(Vector3.zero, animDuration * 0.8f)
             .SetEase(Ease.OutBack);
    }

    private void CloseAnim()
    {

        panel.DOScale(Vector3.zero, animDuration * 0.75f)
             .SetEase(Ease.InBack)
             .OnComplete(() =>
             {
                 _isOpen = false;
                 dialogueText.text = "";
                 if (speakerNameText != null) speakerNameText.text = "";
                 gameObject.SetActive(false);
             });

        panel.DOLocalRotate(new Vector3(0f, 0f, 3f), animDuration * 0.75f)
             .SetEase(Ease.InBack);
    }

    private void StartTypewriter(string message)
    {
        _typewriterCoroutine = StartCoroutine(TypewriterRoutine(message));
    }

    private void StopTypewriter()
    {
        if (_typewriterCoroutine != null)
        {
            StopCoroutine(_typewriterCoroutine);
            _typewriterCoroutine = null;
        }
    }

    private IEnumerator TypewriterRoutine(string message)
    {

        dialogueText.maxVisibleCharacters = 0;
        dialogueText.text = message;

        int total = message.Length;
        for (int i = 0; i <= total; i++)
        {
            dialogueText.maxVisibleCharacters = i;

            panel.DOKill(true);
            panel.localScale = Vector3.one;
            panel.DOScale(new Vector3(1.05f, 0.95f, 1f), typewriterDelay * 0.4f)
                 .SetEase(Ease.OutQuad)
                 .OnComplete(() =>
                     panel.DOScale(Vector3.one, typewriterDelay * 0.6f)
                          .SetEase(Ease.OutElastic));

            yield return new WaitForSeconds(typewriterDelay);
        }

        panel.DOKill(true);
        panel.localScale = Vector3.one;

        _typewriterCoroutine = null;
        
        // Notifichiamo la fine
        _onTypewriterComplete?.Invoke();
        _onTypewriterComplete = null;
    }

#if UNITY_EDITOR
    [ContextMenu("TEST – Open")]
    private void TestOpen() => Open();

    [ContextMenu("TEST – ShowDialogue")]
    private void TestShow() => ShowDialogue("Ciao! Questo è un dialogo in world space, lettera per lettera!", "Mario");

    [ContextMenu("TEST – ShowDialogue (no name)")]
    private void TestShowNoName() => ShowDialogue("Senza nome parlante...");

    [ContextMenu("TEST – Clean")]
    private void TestClean() => Clean();

    [ContextMenu("TEST – Close")]
    private void TestClose() => Close();
#endif
}