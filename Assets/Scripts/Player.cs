using UnityEngine;

namespace ZS
{
    public class Player : MonoBehaviour, IEntity
    {
        Attributes _attributes;
        public Attributes Attributes { get => _attributes; }
        public string Name;

        public void InflictDamage(float value)
        {
            _attributes.Health.RemoveAmount(value);
        }

        void Start()
        {
            _attributes = new();
        }
    }

    // public interface ISlotContainer
    // {
    //     public int Rows { get; }
    //     public int Cols { get; }
    // }

    // public interface IEquipable
    // {
    //     public void Equip();
    // }

    // public interface IArmor
    // {
    //     public float Armor { get; }
    // }

    // public class Clothing : Item, IEquipable, ISlotContainer
    // {
    //     protected int rows;
    //     public int Rows { get => rows; }
    //     protected int cols;
    //     public int Cols { get => cols; }

    //     public Clothing(string id, string name) : base(id, name)
    //     {
    //         this.id = id;
    //         this.name = name;
    //     }

    //     public void Equip()
    //     {
    //         throw new NotImplementedException();
    //     }
    // }

    // public class Inventory
    // {
    //     public Item Holding;
    //     public bool IsHolding;

    //     public class Hotbar
    //     {
    //         public Slot primary;
    //         public Slot secondary;
    //     }

    //     public class Hands : ISlotContainer
    //     {
    //         protected int rows = (int) Defaults.InventoryHandsDims.x;
    //         public int Rows { get => rows; }
    //         protected int cols = (int) Defaults.InventoryHandsDims.y;
    //         public int Cols { get => cols; }
    //     }

    //     public class Slot
    //     {
    //         public bool isRoot;
    //         protected Item item;
    //         public Item Item { get => item; }
    //         protected int quantity;
    //         public int Quantity { get => quantity; }

    //         public void Swap()
    //         {

    //         }

    //         // Left click
    //         public Item GrabAll()
    //         {
    //             return Item;
    //         }
            
    //         // Right click
    //         public Item GrabOne()
    //         {
    //             return Item;
    //         }
    //     }
    // }
}