using UnityEngine;

namespace URMG.Items
{
    [CreateAssetMenu(fileName = "Weapon", menuName = "URMG/Weapon")]
    public class WeaponData : ItemData
    {
        [Header("Weapon Attributes")]
        public WeaponAttributes Attributes;
        [SerializeField] GameObject FPPModel;
    }
}
