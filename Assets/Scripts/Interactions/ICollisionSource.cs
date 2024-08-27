namespace UZSG.Interactions
{
    /// <summary>
    /// Represents stuff that collisions come from.
    /// </summary>
    public interface ICollisionSource
    {
        /// <summary>
        /// Tag reference for collisions.
        /// </summary>
        public string CollisionTag { get; }
    }
}