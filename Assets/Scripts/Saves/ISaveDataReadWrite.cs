namespace UZSG.Saves
{
    public interface ISaveDataReadWrite<T> : ISaveDataReadable<T>, ISaveDataWriteable<T>
    {
    }
}