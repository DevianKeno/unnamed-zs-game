namespace UZSG.Saves
{
    public interface ISaveDataWriteable<T> where T : SaveData
    {
        public T WriteSaveJson();
    }
}