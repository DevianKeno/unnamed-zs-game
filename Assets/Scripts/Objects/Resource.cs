using System;
using UZSG.Attributes;
using UZSG.Data;
using UZSG.Entities;
using UZSG.Interactions;
using UZSG.Items;
using UZSG.Items.Tools;
using UZSG.Systems;

namespace UZSG.Objects
{
    public class Resource : BaseObject
    {
        public ResourceData ResourceData => objectData as ResourceData;
        public AttributeCollection<GenericAttribute> attrCollection = new();
        
        public event EventHandler<CollisionHitInfo> OnHit;

        GenericAttribute healthAttr;

        void Start()
        {
            attrCollection.Add(new GenericAttribute("health"));
            healthAttr = attrCollection["health"];
            healthAttr.Value = 10f;
        }

        public void HitBy(CollisionHitInfo other)
        {
            float damage = 0;

            if (other.By is HeldToolController tool)
            {
                if (tool.ToolData.ToolType == ResourceData.ToolType)
                {
                    damage *= 1;
                    if (tool.Owner is Player player)
                    {
                        var yield = new Item(ResourceData.Yield);
                        if (player.Inventory.Bag.TryPutNearest(yield))
                        {
                            
                        }
                        else
                        {
                            Game.Entity.Spawn<ItemEntity>("item_entity", player.Position, callback: (info) =>
                            {
                                info.Entity.Item = yield;
                            });
                        }
                    }
                }
                else
                {
                    damage *= 0.5f;
                }
            }
            else
            {
                damage *= 0.33f;
            }

            healthAttr.Remove(damage);
        }
    }
}