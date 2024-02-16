using System.Collections;
using UnityEngine;
using UZSG.Systems;

namespace UZSG.FPP
{
    /// <summary>
    /// Represents objects that are animated in first-person perspective.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class FPPModel : MonoBehaviour
    {
        /// <summary>
        /// This camera is the Camera that is animated in the animations.
        /// </summary>
        public Transform CameraAnims;
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
            _animator.runtimeAnimatorController = obj?.ModelController;
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

