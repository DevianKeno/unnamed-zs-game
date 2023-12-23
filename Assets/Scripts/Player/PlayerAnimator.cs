using UnityEngine;

namespace UZSG.Player
{
    [RequireComponent(typeof(PlayerCore), typeof(Animator))]
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] PlayerCore _player;
        [SerializeField] Animator _animator;

        void Awake()
        {
            _player = GetComponent<PlayerCore>();
            _animator = GetComponent<Animator>();
        }
    }
}