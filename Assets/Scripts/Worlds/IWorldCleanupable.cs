namespace UZSG.Worlds
{
    /// <summary>
    /// Represents stuff that is cleaned up upon world exit.
    /// </summary>
    public interface IWorldCleanupable
    {
        public void Cleanup();
    }
}