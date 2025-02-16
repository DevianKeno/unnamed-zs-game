using UnityEngine;

namespace UZSG.FPP
{
    /// <summary>
    /// Adds functionalities to the gun in the viewmodel.
    /// </summary>
    public class FPPGunModel : ViewmodelSubcomponent
    {
        [SerializeField] GunMuzzleController muzzleController;
        public GunMuzzleController MuzzleController => muzzleController;
    }
}