namespace UZSG.Saves
{
    /// <summary>
    /// Represents objects that can be saved and read with Json data.
    /// Pass T as the type of the save data that this reads/writes. 
    /// </summary>
    public interface ISaveDataReadWrite<T> : ISaveDataReadable<T>, ISaveDataWriteable<T>
    {
        
    }
}