using UZSG.Systems;

namespace UZSG.Items.Weapons
{
    public enum MeleeWeaponStates {
        Idle, Walk, Run, Fire, ADS, ADS_Shoot, Reload, Equip, Dequip
    }

    public class MeleeWeaponStateMachine : StateMachine<MeleeWeaponStates>
    {
        
    }
}
