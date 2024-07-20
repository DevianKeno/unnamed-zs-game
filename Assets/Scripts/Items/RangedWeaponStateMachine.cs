using UZSG.Systems;

namespace UZSG.Items.Weapons
{

    public enum RangedWeaponStates {
        Idle, Walk, Run, Fire, Cock, ADS_Down, ADS_Shoot, ADS_Cock, ADS_Up, Reload, Equip, Dequip
    }

    public class RangedWeaponStateMachine : StateMachine<RangedWeaponStates>
    {
    }
}
