using System;

namespace UZSG.Worlds
{
    /// <summary>
    /// Basic time struct as a container for the world time.
    /// </summary>
    [Serializable]
    public struct WorldTime
    {
        public int Day;
        public int Hour;
        public int Minute;
        public int Second;
    }
}