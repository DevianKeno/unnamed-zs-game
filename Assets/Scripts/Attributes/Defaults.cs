namespace UZSG.Attributes
{
    public partial class VitalAttribute
    {
        public static VitalAttribute Health => new(
            baseMax: 100f,
            baseChange: 0.15f,
            cycle: CycleType.Regen,
            time: CycleTime.Second
        );
        
        public static VitalAttribute Stamina => new(
            baseMax: 100f,
            baseChange: 2.5f,
            CycleType.Regen,
            CycleTime.Second
        );
        
        public static VitalAttribute Mana => new(
            baseMax: 100f,
            baseChange: 1f,
            CycleType.Regen,
            CycleTime.Second
        );
        
        public static VitalAttribute Hunger => new(
            baseMax: 100f,
            baseChange: 0.03f,
            CycleType.Degen,
            CycleTime.Second
        );

        public static VitalAttribute Hydration => new(
            baseMax: 100f,
            baseChange: 0.03f,
            CycleType.Degen,
            CycleTime.Second
        );
    }
}