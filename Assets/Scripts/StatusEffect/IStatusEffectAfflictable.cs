namespace UZSG.StatusEffects
{
    /// <summary>
    /// Represents stuffs that can be afflicted with any number of status effects.
    /// </summary>
    public interface IStatusEffectAfflictable
    {
        public StatusEffectCollection StatusEffects { get; }
    }
}