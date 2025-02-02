using System;

namespace UZSG
{
    [Flags]
    public enum SettingsQualityFlags {
        None = 0,
        Off = 1 << 1,
        Very_Low = 1 << 2,
        Low = 1 << 3,
        Medium = 1 << 4,
        High = 1 << 5,
        Very_High = 1 << 6,
        Ultra = 1 << 7,
    }
}