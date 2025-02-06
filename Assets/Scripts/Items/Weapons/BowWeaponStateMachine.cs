

namespace UZSG.Items.Weapons
{
    public enum BowWeaponStates {
        Idle, Walk, Run, Draw, Fire, ADS_Down, ADS_Shoot, Reload, Equip, Dequip
    }

    public class BowWeaponStateMachine : StateMachine<BowWeaponStates>
    {

    }
}
