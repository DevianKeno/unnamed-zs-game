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

        public void Initialize(GunWeaponController gun)
        {
            ClipSize = gun.WeaponData.RangedAttributes.ClipSize;
            SetClip(gun.CurrentRounds);
            SetFiringMode(gun.CurrentFiringMode);
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
    }
}