using System;

namespace UZSG.Items.Weapons
{
    [Flags]
    public enum FiringModes
    {
        All = 1,
        Single = 2,
        Automatic = 4,
        Burst = 8,
    }
}
