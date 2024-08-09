using UZSG.Systems;

namespace UZSG.Items.Weapons
{
    public enum GunWeaponStates {
        Idle, Walk, Run, Fire, Cock, ADS_Down, ADS_Shoot, ADS_Cock, ADS_Up, Reload, TacticalReload, SelectFire, Equip, Dequip
    }

    public class GunWeaponStateMachine : StateMachine<GunWeaponStates>
    {

    }
}