using UZSG.Systems;

namespace UZSG.Items.Tools
{
    public enum ToolItemStates {
        Idle, Walk, Run, Attack, Use, Equip, Dequip
    }

    public class ToolItemStateMachine : StateMachine<ToolItemStates>
    {
        
    }
}
