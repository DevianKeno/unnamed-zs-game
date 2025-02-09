namespace UZSG.Attributes
{
    public class AttributeBroker
    {
        public AttributeCollection Attributes { get; set; }
        public StatusEffectCollection StatusEffects;
        public bool HasStatusEffects { get; private set; }
    }
}