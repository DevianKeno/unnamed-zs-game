using UnityEngine;

using UZSG.Systems;
using UZSG.Entities;
using UZSG.FPP;
using UZSG.Items;
using UZSG.Items.Weapons;

namespace UZSG.UI.HUD
{
    public class SwitchCrosshair : MonoBehaviour
    {
        public Player Player;
        [Space]

        [SerializeField] GameObject defaultCrosshair;
        [SerializeField] GameObject gunCrosshair;
        
        internal void Initialize(Player player)
        {
            Player = player;
            player.FPP.OnChangeHeldItem += OnChangeHeldItem;
        }

        void OnChangeHeldItem(HeldItemController controller)
        {
            /// Switch crosshairs
            if (Player.FPP.HeldItem is GunWeaponController gunWeapon)
            {
                defaultCrosshair.SetActive(false);
                gunCrosshair.SetActive(true);
            }
            else
            {
                defaultCrosshair.SetActive(true);
                gunCrosshair.SetActive(false);
            }
        }
    }
}