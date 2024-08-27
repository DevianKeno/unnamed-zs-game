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
        public bool IsChoppable { get; set; } = true;
        /// <summary>
        /// If the tree is cut down or not.
        /// </summary>
        public bool IsFelled { get; private set; } = false;
        /// <summary>
        /// The angle of the tree when it gets chopped. 
        /// </summary>
        public float ChopAngle = 8f; /// based on tree's size, hardness, mass?
        /// <summary>
        /// Tree falling animation curve when cut down.
        /// </summary>
        public AnimationCurve fallAnimationCurve;
        public float FallDuration = 5f;
        public float MaxFallingAngle = 85f;

        Quaternion _originalRotation;
        [SerializeField] Transform treeModel;

        /// On load on world
        protected override void Start()
        {
            base.Start();
            
            _originalRotation = transform.rotation;
        }

        public override void HitBy(HitboxCollisionInfo info)
        {
            base.HitBy(info);

            float damage = 0f;
            if (info.Source is HeldToolController tool)
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
                    
                    Game.Audio.PlayInWorld("tree_chop_wood", info.ContactPoint);
                    Game.Particles.Spawn("Tree Bark Break", info.ContactPoint);
                    
                    if (!IsFelled)
                    {
                        AnimateChop((tool.Owner as Player).Right);
                    }
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

            if (IsChoppable && !IsFelled)
            {
                /// Remove Tree health
                Attributes["health"].Remove(damage);
                if (Attributes["health"].Value <= 0)
                {
                    Cutdown();
                }
            }
        }

        public void Cutdown()
        {
            IsChoppable = false;
            IsFelled = true;
            AllowInteractions = false;

            Game.Audio.PlayInWorld("tree_fell", Position);
            
            /// Tree falling animation
            LeanTween.value(0, 1, FallDuration)
            .setOnUpdate((float i) =>
            {
                var t = fallAnimationCurve.Evaluate(i);
                var x = Mathf.Lerp(Rotation.x, MaxFallingAngle, t);
                Rotation = Quaternion.Euler(x, Rotation.y, Rotation.z);
            })
            .setOnComplete(() =>
            {
                DestroySelf();
            });
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
                ResetChopAngle();
            });
        }

        void DestroySelf()
        {
            /// play some wood/bark explosion particles and stuff
        }

        void ResetChopAngle()
        {
            LeanTween.rotate(treeModel.gameObject, Vector3.zero, 0.33f)
            .setEaseOutExpo();
        }
    }
}