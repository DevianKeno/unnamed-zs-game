using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UZSG.UI
{
    public class LoadingIconAnimated : MonoBehaviour
    {
        void OnEnable()
        {
            StartSpin();
        }
        
        void OnDisable()
        {
            StopSpin();
        }

        public void StartSpin()
        {
            LeanTween.rotate(transform as RectTransform, -360f, 1f)
                .setLoopClamp();
        }

        public void StopSpin()
        {
            LeanTween.cancel(gameObject);
        }
    }
}