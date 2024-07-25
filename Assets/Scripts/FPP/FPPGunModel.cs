using UnityEngine;
using UZSG.Systems;

namespace UZSG.FPP
{
    /// <summary>
    /// Adds functionalities to the gun in the viewmodel.
    /// </summary>
    public class FPPGunModel : MonoBehaviour
    {
        [SerializeField] GunMuzzleController muzzleController;
        public GunMuzzleController MuzzleController => muzzleController;
    }
}