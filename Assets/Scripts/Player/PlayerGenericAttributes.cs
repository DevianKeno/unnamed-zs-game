using System.Collections.Generic;
using UZSG.Attributes;

namespace UZSG.Player
{
    public class PlayerGenericAttributes : AttributeCollection
    {
        Dictionary<string, Attribute> _attributes;
        public override Dictionary<string, Attribute> Attributes => _attributes;        

        public override Attribute this[string name]
        {
            get => _attributes[name];
        }

        public PlayerGenericAttributes()
        {
            _attributes = new()
            {
                {"MoveSpeed", new(10f)},
                {"JumpHeight", new(10f)},
                {"Armor", new(0f)},
                {"MeleeSpeed", new(1f)},
                {"CritChance", new(0f)},
                {"CritDamage", new(100f)},
            };
        }
    }
}
