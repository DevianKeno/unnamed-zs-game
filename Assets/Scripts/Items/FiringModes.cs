using System;

namespace UZSG.Items.Weapons
{
    [Flags]
    public enum FiringModes
    {
        All = 1,
        Single = 2,
        FullAuto = 4,
        Burst = 8,
    }

    public enum FiringMode
    {
        Single,
        FullAuto,
        Burst,
    }
}
