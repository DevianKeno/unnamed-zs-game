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
            SetCurrent(gun.CurrentRounds);
            SetFiringMode(gun.CurrentFiringMode);
        }

        public void SetCurrent(int count)
        {
            clipAmmoText.text = $"{count}";
            clipAmmoText.color = ClipTextColor.Evaluate(1 - (count / (ClipSize <= 0 ? 999f : ClipSize)));
        }
        
        public void SetReserve(int count)
        {
            reserveAmmoText.text = $"{count}";
        }

        public void SetFiringMode(FiringMode mode)
        {
            firingModeText.text = $"{mode}";
        }
    }
}