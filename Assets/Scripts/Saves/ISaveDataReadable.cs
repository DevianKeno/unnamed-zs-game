namespace UZSG.Saves
{
    public interface ISaveDataReadable<T>
    {
        public void ReadSaveJson(T saveData);
    }
}