using UZSG.Data;
using UZSG.Saves;

namespace UZSG.Items.Weapons
{
    public abstract class HeldWeaponController : HeldItemController
    {
        public WeaponData WeaponData => ItemData as WeaponData;

        protected void LoadDefaultAttributes()
        {
            attributes = new();
            attributes.AddList(WeaponData.Attributes);
        }
    }
}
