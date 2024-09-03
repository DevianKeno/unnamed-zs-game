using UnityEngine;

using UZSG.Systems;
using UZSG.Entities;
using UZSG.FPP;
using UZSG.Items;
using UZSG.Items.Weapons;

namespace UZSG.UI.HUD
{
    public class CrosshairHandler : MonoBehaviour
    {
        public Player Player;
        [Space]

        public bool ShowDot = true;

        [SerializeField] GameObject dotCrosshair;
        [SerializeField] DynamicCrosshair dynamicCrosshair;
        
        internal void Initialize(Player player)
        {
            Player = player;
            dynamicCrosshair.Initialize(player);
            player.FPP.OnChangeHeldItem += OnChangeHeldItem;
        }

        void OnValidate()
        {
            if (gameObject.activeInHierarchy)
            {
                dotCrosshair.SetActive(ShowDot);
            }
        }

        void OnChangeHeldItem(HeldItemController controller)
        {
            /// Switch crosshairs
            if (Player.FPP.HeldItem is GunWeaponController gunWeapon)
            {
                dotCrosshair.gameObject.SetActive(false);
                dynamicCrosshair.Show();
            }
            else
            {
                dynamicCrosshair.Hide();
                dotCrosshair.gameObject.SetActive(true);
            }
        }

        public void Show()
        {
            dotCrosshair.gameObject.SetActive(true);
            dynamicCrosshair.Show();
        }

        public void Hide()
        {
            dotCrosshair.gameObject.SetActive(false);
            dynamicCrosshair.Hide();
        }
    }
}