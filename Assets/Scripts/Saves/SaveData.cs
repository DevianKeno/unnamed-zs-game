using System;

namespace UZSG.Saves
{
    public class SaveData
    {
        public string Type => GetType().Name;

        public static bool IsNull(object obj)
        {
            return obj == null;
        }
    }
}