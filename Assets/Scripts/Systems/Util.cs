using UZSG.Systems;

namespace UZSG
{
    public static class Utils
    {
        public static bool IsSameVersion(string version)
        {
            return string.Compare(version, Game.Main.GetVersionString()) != 0;
        }
    }
}