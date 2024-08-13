namespace UZSG.Saves
{
    public interface ISaveDataReadable<T> where T : SaveData
    {
        public void ReadSaveJson(T saveData);
    }
}