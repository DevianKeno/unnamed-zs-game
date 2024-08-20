namespace UZSG.Saves
{
    public interface ISaveDataReadable<T>
    {
        public void ReadSaveData(T saveData);
    }
}