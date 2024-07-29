using System;

namespace UZSG.Items.Weapons
{
    [Flags]
    public enum FiringModes
    {
        None = 0,
        Single = 1 << 1,
        Burst = 1 << 2,
        FullAuto = 1 << 3,
    }

    public enum FiringMode
    {
        Single,
        Burst,
        FullAuto,
    }
}
