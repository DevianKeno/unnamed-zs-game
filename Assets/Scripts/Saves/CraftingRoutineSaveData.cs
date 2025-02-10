using System;

namespace UZSG.Saves
{
    [Serializable]
    public class CraftingRoutineSaveData : SaveData
    {
        public string RecipeId;
        public int Count;
        public float StartTime;
        public float EndTime;
        public float Progress;
    }
}