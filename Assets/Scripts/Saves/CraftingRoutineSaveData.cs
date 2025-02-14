using System;

namespace UZSG.Saves
{
    [Serializable]
    public class CraftingRoutineSaveData : SaveData
    {
        public string RecipeId;
        public int Count;
        public int TotalYield;
        public int CurrentYield;
        public int RemainingYield;
        public float SecondsElapsed;
        public float SecondsElapsedSingle;
        public int Status;
    }
}