using UnityEngine;
using TMPro;
using UZSG.Items.Weapons;

namespace UZSG.UI.HUD
{
    public class AmmoCounterHUD : MonoBehaviour
    {
        public Gradient ClipTextColor;
        public int ClipSize { get; set; }

        [SerializeField] TextMeshProUGUI clipAmmoText;
        [SerializeField] TextMeshProUGUI reserveAmmoText;
        [SerializeField] TextMeshProUGUI firingModeText;
        [SerializeField] TextMeshProUGUI cartridgeText;

        public void DisplayWeaponStats(GunWeaponController gun)
        {
            ClipSize = gun.WeaponData.RangedAttributes.ClipSize;
            SetClip(gun.CurrentRounds);
            SetReserve(gun.Reserve);
            SetFiringMode(gun.CurrentFiringMode);
            SetCartridgeText(gun.WeaponData.RangedAttributes.Ammo.DisplayName);
        }

        public void SetClip(int value)
        {
            clipAmmoText.text = $"{value}";
            clipAmmoText.color = ClipTextColor.Evaluate(1 - (value / (ClipSize <= 0 ? 999f : ClipSize)));
        }
        
        public void SetReserve(int value)
        {
            reserveAmmoText.text = $"{value}";
        }

        public void SetFiringMode(FiringMode mode)
        {
            if (mode == FiringMode.FullAuto)
            {
                firingModeText.text = "Full-auto"; /// with dash :P
                return;
            }
            firingModeText.text = $"{mode}";
        }

        public void SetCartridgeText(string text)
        {
            cartridgeText.text = text;
        }
    }
}