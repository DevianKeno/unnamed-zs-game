using UnityEngine;
using UZSG.Entities;

namespace UZSG.FPP
{
    public sealed class ArmsController : MonoBehaviour
    {
        public Player player;
        [SerializeField] GameObject rig;
        [SerializeField] Animator animator;
        
        void Start()
        {
            animator = rig.GetComponent<Animator>();
        }

        public void LoadAnimationController(RuntimeAnimatorController controller)
        {
            animator.runtimeAnimatorController = controller;
        }

        public void PlayAnimation(string name)
        {
            animator.Play(name);
        }
    }
}