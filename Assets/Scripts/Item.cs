using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

namespace ZS
{
    public class Item : IPickupable, IDroppable
    {
        protected string id;
        protected string name;
        protected string description;
        protected Image sprite;
        protected Vector2 size;
        protected bool isStackable;
        protected int maxStackCount;
        public int orientation;
        public string Id { get => id; }
        public string Name { get => name; }
        public string Description { get => description; }
        
        public Item(string id, string name)
        {
            this.id = id;
            this.name = name;
        }

        public void Drop()
        {
            throw new System.NotImplementedException();
        }

        public void Pickup()
        {
            throw new System.NotImplementedException();
        }
    }

    public abstract class Food : Consumable
    {

    }
}