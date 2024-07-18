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
        public Animator Animator;
        public bool HasCameraAnims;
        public Transform Camera;

        void Awake()
        {            
            Animator = GetComponent<Animator>();
        }
        
        /// <summary>
        /// Load animation data of the object.
        /// </summary>
        public void Load(IFPPVisible obj)
        {
            // Animator.runtimeAnimatorController = obj.ModelController;
        }

        public void Play(string name)
        {
            Animator.Play(name);
        }

        public void PauseUntil(float time)
        {
            Animator.speed = 0f;
            StartCoroutine(ResumeAnim(time));
        }

        IEnumerator ResumeAnim(float duration)
        {
            yield return new WaitForSeconds(duration);
            Animator.speed = 1f;
        }
    }
}

