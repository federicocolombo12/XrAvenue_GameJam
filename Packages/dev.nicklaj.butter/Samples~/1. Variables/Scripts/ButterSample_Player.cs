using Dev.Nicklaj.Butter;
using UnityEngine;

class ButterSample_Player : MonoBehaviour {
    public FloatVariable Health;

    private void Awake() => Health.OnValueChanged += OnHealthChanged;

    private void OnHealthChanged(float newValue)
    {
        Debug.Log($"I have been healed! Now at {newValue}");
    }
}
