namespace UZSG.Saves
{
    public interface ISaveDataWriteable<T>
    {
        public T WriteSaveJson();
    }
}