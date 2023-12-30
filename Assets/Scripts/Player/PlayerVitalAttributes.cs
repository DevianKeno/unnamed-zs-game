using System.Collections.Generic;
using UZSG.Attributes;

namespace UZSG.Player
{
    /// <summary>
    /// Represents the Player's vital attributes (Health, Stamina, etc).
    /// </summary>
    public class PlayerVitalAttributes : AttributeCollection
    {
        Dictionary<string, Attribute> _attributes;
        public override Dictionary<string, Attribute> Attributes => _attributes;        

        public override Attribute this[string name]
        {
            get => _attributes[name];
        }

        public PlayerVitalAttributes()
        {
            _attributes = new()
            {
                {"Health", VitalAttribute.Health},
                {"Stamina", VitalAttribute.Stamina},
                // {"Mana", VitalAttributes.Mana},
                {"Hunger", VitalAttribute.Hunger},
                {"Hydration", VitalAttribute.Hydration}
            };
        }

    }
}
