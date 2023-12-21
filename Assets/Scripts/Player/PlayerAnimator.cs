using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace URMG.Player
{
    [RequireComponent(typeof(PlayerCore))]
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] Animator animator;
    }
}