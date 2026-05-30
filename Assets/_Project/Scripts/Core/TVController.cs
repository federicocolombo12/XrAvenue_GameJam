using UnityEngine;
using Dev.Nicklaj.Butter;

namespace AvenueXR.Core
{
    public class TVController : MonoBehaviour
    {
        [Header("Butter Events")]
        public BoolEvent onTVStateChanged;

        [Header("References")]
        public Animator tvAnimator;

        [Header("Animator Parameters")]
        public string tvOnBool = "TvOn";

        private void OnEnable()
        {
            if (onTVStateChanged != null)
                onTVStateChanged.RegisterListener(SetTVState);
        }

        private void OnDisable()
        {
            if (onTVStateChanged != null)
                onTVStateChanged.DeregisterListener(SetTVState);
        }

        public void SetTVState(bool isOn)
        {
            if (tvAnimator != null)
            {
                tvAnimator.SetBool(tvOnBool, isOn);
            }
        }
    }
}
