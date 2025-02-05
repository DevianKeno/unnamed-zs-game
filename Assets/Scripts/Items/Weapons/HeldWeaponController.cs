using UZSG.Data;
using UZSG.Saves;

namespace UZSG.Items.Weapons
{
    public abstract class HeldWeaponController : FPPItemController, IDamageSource
    {
        public WeaponData WeaponData => ItemData as WeaponData;

        public bool IsBroken
        {
            get
            {
                if (attributes.TryGet("durability", out var durability))
                {
                    return durability.Value <= 0f;
                }
                else
                {
                    return false; /// makes items that have no durability unbreakable
                }
            }
        }

        protected void LoadDefaultAttributes()
        {
            attributes = new();
            attributes.AddList(WeaponData.Attributes);
        }
    }
}
