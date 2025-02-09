using System.Linq;

using UZSG.Items;

namespace UZSG
{
    /// Extension methods for Container class.
    public partial class Container
    {
        /// <summary>
        /// Tries to get an item with count from the nearest slot.
        /// </summary>
        public bool TryGetNearestFuel(int count, out Item item)
        {
            item = Item.None;

            if (_cachedIdSlots.Count > 0)
            {
                var slots = _cachedIdSlots.First();
                var slot = slots.Value.First();
                if (slot.Item.Data.IsFuel)
                {
                    item = TakeFrom(slot.Index, count);
                    return true;
                }
            }

            return false;
        }
    }
}