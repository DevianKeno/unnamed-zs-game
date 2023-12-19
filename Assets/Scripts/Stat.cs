
namespace URMG
{
    /// <summary>
    /// Represents
    /// </summary>
    public class Stat
    {
        protected string _name;
        public string Name { get => _name; }

        protected float _baseValue;
        public float Value { get => _baseValue; }
        protected float _bonus;
        public float Bonus { get => _baseValue; }
        protected float _multiplier;
        public float Multiplier { get => _multiplier; }

        public void AddBonus(float amount)
        {
            _bonus += amount;
        }
        
        public void AddMultiplier(float amount)
        {
            _multiplier += amount;
        }
    }
}
