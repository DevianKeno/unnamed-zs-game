using UnityEngine;

namespace UZSG.Items
{
    [CreateAssetMenu(fileName = "Weapon", menuName = "URMG/Weapon")]
    public class WeaponData : ItemData
    {
        [Header("Weapon Attributes")]
        public WeaponAttributes Attributes;
        [SerializeField] GameObject _FPPModel;
        public GameObject FPPModel => _FPPModel;

        [Header("Animations")]
        public string Equip;
        public string Dequip;
        public string[] Primary;
        public string Secondary;
    }
}
