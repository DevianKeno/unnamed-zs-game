namespace UZSG.Saves
{
    /// <summary>
    /// Represents objects that can be saved and read with Json data.
    /// </summary>
    public interface ISaveDataReadWrite<T> : ISaveDataReadable<T>, ISaveDataWriteable<T>
    {
        
    }
}