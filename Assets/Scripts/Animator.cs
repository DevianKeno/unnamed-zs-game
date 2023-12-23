using UnityEngine;
using UZSG.Items;

namespace UZSG.Systems
{
    public class Animator : MonoBehaviour
    {
        UnityEngine.Animator _animator;
        
        public void Set(WeaponData obj)
        {
            _animator.runtimeAnimatorController = obj.Controller;
        }

        public void Play(string name, float transitionDuration)
        {
            _animator.CrossFade(name, transitionDuration);
        }
    }
}
