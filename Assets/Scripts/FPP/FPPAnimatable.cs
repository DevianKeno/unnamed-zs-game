using System.Collections;
using UnityEngine;
using UZSG.Systems;

namespace UZSG.FPP
{
    /// <summary>
    /// Represents objects that are animated in first-person perspective.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class FPPAnimatable : MonoBehaviour
    {
        public float HitstopDuration = 0.25f;
        Animator _animator;

        void Awake()
        {            
            _animator = GetComponent<Animator>();
        }
        
        /// <summary>
        /// Load animation data of the object.
        /// </summary>
        public void Load(IFPPVisible obj)
        {
            _animator.runtimeAnimatorController = obj?.Controller;
        }

        public void Play(string name)
        {
            _animator.Play(name);
        }

        public void PauseUntil(float time)
        {
            _animator.speed = 0f;
            StartCoroutine(ResumeAnim(time));
        }

        IEnumerator ResumeAnim(float duration)
        {
            yield return new WaitForSeconds(duration);
            _animator.speed = 1f;
        }
    }
}

