using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(Canvas))]
public class WorldDialoguePopup : MonoBehaviour
{
    [Header("References")]
    public RectTransform panel;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;

    [Header("Settings")]
    public float animDuration = 0.4f;
    public float typewriterDelay = 0.03f;

    [Header("Persistence")]
    public bool keepPersistent = false;

    private Coroutine _typewriterCoroutine;
    private bool _isOpen = false;
    private bool _isOpening = false;
    private System.Action _onTypewriterComplete;

    private void Awake()
    {
        if (panel == null) panel = GetComponent<RectTransform>();
        
        // Reset iniziale
        if (panel != null) panel.localScale = Vector3.zero;
        if (dialogueText != null) dialogueText.text = "";
        if (speakerNameText != null) speakerNameText.text = "";

        if (!keepPersistent) gameObject.SetActive(false);
    }

    public void Open(System.Action onComplete = null)
    {
        if (_isOpen || _isOpening) 
        {
            onComplete?.Invoke();
            return;
        }
        
        StopTypewriter();
        OpenAnim(() => {
            onComplete?.Invoke();
        });
    }

    public void ShowDialogue(string message, string speakerName = "", System.Action onComplete = null)
    {
        _onTypewriterComplete = onComplete;
        
        if (speakerNameText != null)
            speakerNameText.text = speakerName ?? "";

        // Se è chiuso o si sta chiudendo, lo apriamo
        if (!_isOpen && !_isOpening)
        {
            Open(() => StartTypewriter(message));
        }
        else if (_isOpening)
        {
            // Se si sta già aprendo, il typewriter partirà dalla callback già settata o ne accodiamo una
            // Per semplicità, fermiamo e ripartiamo quando possibile o semplicemente sovrascriviamo
            // In questo caso, StartTypewriter verrà chiamato alla fine di OpenAnim
        }
        else
        {
            // È già aperto, scrivi e basta
            StartTypewriter(message);
        }
    }

    public void Close()
    {
        if (!_isOpen) return;
        StopTypewriter();
        CloseAnim();
    }

    private void OpenAnim(System.Action onComplete = null)
    {
        gameObject.SetActive(true);
        _isOpening = true;
        
        panel.DOKill();
        panel.localScale = Vector3.zero;
        panel.localRotation = Quaternion.Euler(0f, 0f, -5f);

        panel.DOScale(Vector3.one, animDuration).SetEase(Ease.OutBack);
        panel.DOLocalRotate(Vector3.zero, animDuration).SetEase(Ease.OutBack)
             .OnComplete(() => {
                 _isOpen = true;
                 _isOpening = false;
                 onComplete?.Invoke();
             });
    }

    private void CloseAnim()
    {
        _isOpen = false;
        _isOpening = false;
        panel.DOKill();
        
        panel.DOScale(Vector3.zero, animDuration * 0.8f).SetEase(Ease.InBack)
             .OnComplete(() => {
                 if (!keepPersistent) gameObject.SetActive(false);
             });
    }

    private void StartTypewriter(string message)
    {
        StopTypewriter();
        if (gameObject.activeInHierarchy)
        {
            _typewriterCoroutine = StartCoroutine(TypewriterRoutine(message));
        }
        else
        {
            // Fallback se spento
            if (dialogueText != null) dialogueText.text = message;
            _onTypewriterComplete?.Invoke();
        }
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
        if (dialogueText == null) yield break;

        dialogueText.text = message;
        dialogueText.maxVisibleCharacters = 0;

        for (int i = 0; i <= message.Length; i++)
        {
            dialogueText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(typewriterDelay);
        }

        _typewriterCoroutine = null;
        _onTypewriterComplete?.Invoke();
        _onTypewriterComplete = null;
    }
}
