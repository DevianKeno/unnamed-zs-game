using UnityEngine;

namespace UZSG.Player
{
    [RequireComponent(typeof(PlayerCore))]
    public class PlayerAnimator : Systems.Animator
    {
        [SerializeField] PlayerCore _player;
        
        void Awake()
        {
            _player = GetComponent<PlayerCore>();
        }
    }
}