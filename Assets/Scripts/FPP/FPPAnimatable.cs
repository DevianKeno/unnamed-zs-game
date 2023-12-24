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
        Animator _animator;

        void Awake()
        {            
            _animator = GetComponent<Animator>();
        }

        void Start()
        {
            Game.Tick.OnTick += Tick;
        }

        void Tick(object sender, TickEventArgs e)
        {
            
        }

        public void Load(IFPPVisible obj)
        {
            _animator.runtimeAnimatorController = obj.Controller;
        }

        public void Play(string name, float normalizedTransitionDuration)
        {
            _animator.CrossFade(name, normalizedTransitionDuration);
        }
    }  
}

