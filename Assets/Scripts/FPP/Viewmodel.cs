using System;

using UnityEditor.Animations;
using UnityEngine;

using UZSG.Items;

namespace UZSG.FPP
{
    [Serializable]
    public class Viewmodel
    {
        [field: SerializeField] public AnimatorController ArmsAnimations { get; set; }
        [field: SerializeField] public GameObject Model { get; set; }
        [field: SerializeField] public ItemData ItemData { get; set; }
    }
}