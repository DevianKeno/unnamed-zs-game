using System;
using System.Collections.Generic;

using UZSG.Data;
using UZSG.Entities;
using UZSG.Interactions;
using UZSG.Items;
using UZSG.Items.Tools;

namespace UZSG.Objects
{
    public class OreDeposit : Resource, IInteractable, IDamageable
    {
        public bool AllowInteractions { get; set; } = true;

        public void TakeDamage(DamageInfo damage)
        {
            if (Attributes.TryGet("health", out var health))
            {
                health.Remove(damage.Amount);
            }
        }

        public override void HitBy(HitboxCollisionInfo info)
        {
            base.HitBy(info);

            float damage = 0;

            if (info.Source is HeldToolController tool)
            {
                if (tool.Attributes.TryGet("efficiency", out var efficiency))
                {
                    damage += efficiency.Value;
                }

                if (IsHarvestableBy(tool.ToolData))
                {
                    damage *= 1;
                    if (tool.Owner is Player player)
                    {
                        var yield = new Item(ResourceData.Yield);
                        if (player.Actions.PickUpItem(yield))
                        {
                            
                        }
                        else
                        {
                            Game.Entity.Spawn<ItemEntity>("item_entity", player.Position, onCompleted: (info) =>
                            {
                                info.Entity.Item = yield;
                            });
                        }
                    }

                    Game.Audio.PlayInWorld("mine_pickaxe", Position);
                }
                else /// other tools deals half damage
                {
                    damage *= 0.5f;
                }
            }
            else /// other tools deals little to no damage
            {
                damage *= 0.1f;
            }
            
            if (info.Source is IDamageSource damageSource)
            {
                TakeDamage(new DamageInfo(damageSource, damage));
            }
        }

        public List<InteractAction> GetInteractActionOptions()
        {
            return new();
        }
    }
}