using System;

using UnityEngine;

using UZSG.Attributes;
using UZSG.Data;
using UZSG.Entities;
using UZSG.Interactions;
using UZSG.Items;
using UZSG.Items.Tools;
using UZSG.Systems;

namespace UZSG.Objects
{
    public class Tree : Resource
    {
        public float ChopAngle = 8f; /// based on tree's size, hardness, mass?
        
        Quaternion _originalRotation;
        [SerializeField] Transform treeModel;

        /// On load on world
        protected override void Start()
        {
            base.Start();
            
            _originalRotation = transform.rotation;
        }

        public override void HitBy(HitboxCollisionInfo other)
        {
            float damage = 0f;

            if (other.Source is HeldToolController tool)
            {
                if (tool.Attributes.TryGet("efficiency", out var efficiency))
                {
                    damage += efficiency.Value;
                }

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
                    
                    Game.Audio.Play("chop", transform.position);
                    AnimateChop((tool.Owner as Player).Right);
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

            Attributes["health"].Remove(damage);
            if (Attributes["health"].Value <= 0)
            {
                Cutdown();
            }
        }

        public void Cutdown()
        {

        }

        void AnimateChop(Vector3 swingDirection)
        {
            LeanTween.cancel(treeModel.gameObject);

            var targetRotation = Vector3.zero;
            targetRotation += swingDirection * ChopAngle;
            targetRotation = Quaternion.Inverse(_originalRotation) * targetRotation;

            LeanTween.rotate(treeModel.gameObject, targetRotation, 0f)
            .setOnComplete(() =>
            {
                ResetTransforms();
            });
        }

        void ResetTransforms()
        {
            LeanTween.rotate(treeModel.gameObject, Vector3.zero, 0.33f)
            .setEaseOutExpo();
        }
    }
}