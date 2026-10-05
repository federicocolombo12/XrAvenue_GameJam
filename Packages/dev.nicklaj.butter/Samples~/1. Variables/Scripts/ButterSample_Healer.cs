using System.Collections;
using System.Collections.Generic;
using Dev.Nicklaj.Butter;
using UnityEngine;

class ButterSample_Healer : MonoBehaviour {
    public FloatVariable PlayerHealth;

    void Awake() => StartCoroutine(HealPlayerCoroutine());

    private IEnumerator HealPlayerCoroutine()
    {
        while (this.isActiveAndEnabled)
        {
            HealPlayer(5f);
            yield return new WaitForSeconds(1f);
        }
    }

    public void HealPlayer(float amount){
        PlayerHealth.Value += amount;
    }
}
