using System.Collections.Generic;

using UnityEngine;

using MEC;

using UZSG.Data;
using UZSG.Entities;
using UZSG.Interactions;
using UZSG.Items;
using UZSG.Items.Tools;

namespace UZSG.Objects
{
    public class Tree : Resource, IInteractable
    {
        public bool AllowInteractions { get; set; } = true;
        public bool IsChoppable { get; set; } = true;
        /// <summary>
        /// If the tree is cut down or not.
        /// </summary>
        public bool IsFelled { get; private set; } = false;

        /// <summary>
        /// Whether the tree is generated via resource chunks.
        /// </summary>
        bool isNaturallyGenerated;
        /// <summary>
        /// The angle of the tree when it gets chopped. 
        /// </summary>
        [SerializeField] float chopAngle = 8f; /// based on tree's size, hardness, mass?
        [SerializeField] float despawnSeconds = 15f;

        Quaternion _originalRotation;
        Vector3 _lastHitAngle;

        [Space, Header("Components")]
        [SerializeField] GameObject leaves0; /// lod0
        [SerializeField] new Rigidbody rigidbody;
        [SerializeField] LODGroup lodGroup;
        
        protected override void OnPlaceEvent()
        {
            base.OnPlaceEvent();
            _originalRotation = transform.rotation;
        }

        public override void HitBy(HitboxCollisionInfo info)
        {
            float damage = 0f;

            if (info.Source is HeldToolController tool)
            {
                if (tool.Attributes.TryGet("efficiency", out var efficiency))
                {
                    damage += efficiency.Value;
                }

                if (this.IsHarvestableBy(tool.ToolData))
                {
                    damage *= 1;

                    if (ResourceData.HarvestType == HarvestType.PerAction)
                    {
                        if (tool.Owner is Player player)
                        {
                            GiveYield(player);
                        }
                    }
                    
                    Game.Audio.PlayInWorld("tree_chop_wood", info.ContactPoint);
                    Game.Particles.Create("tree_bark_break", info.ContactPoint);
                    
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
                if (info.Source is IDamageSource damageSource)
                {
                    TakeDamage(new DamageInfo(damageSource, damage));
                }
            }
        }
        
        void TakeDamage(DamageInfo dmg)
        {
            if (Attributes.TryGet("health", out var health))
            {
                MarkDirty();
                health.Remove(dmg.Amount);
                
                if (health.Value <= 0)
                {
                    Cutdown();
                }
            }
        }

        void GiveYield(Player player)
        {
            var yield = new Item(ResourceData.Yield);

            if (player.Actions.PickUpItem(yield))
            {
                
            }
            else
            {
                Game.Entity.SpawnItem(yield, player.Position);
            }
        }

        public void Cutdown()
        {
            Timing.RunCoroutine(_CutdownAnimationCoroutine());
        }

        IEnumerator<float> _CutdownAnimationCoroutine()
        {
            IsChoppable = false;
            IsFelled = true;
            AllowInteractions = false;

            // lODGroup.enabled = false;
            leaves0.transform.SetParent(rigidbody.transform); /// 
            gameObject.isStatic = false;
            rigidbody.isKinematic = false;
            rigidbody.AddForce(_lastHitAngle * rigidbody.mass, ForceMode.Impulse);
            Game.Audio.PlayInWorld("tree_fell", Position);

            yield return Timing.WaitForSeconds(despawnSeconds); /// despawn everything after
            DestroySelf();
        }

        void AnimateChop(Vector3 swingDirection)
        {
            LeanTween.cancel(gameObject);

            _lastHitAngle = swingDirection;
            var targetRotation = Vector3.zero;
            targetRotation += swingDirection * chopAngle;
            targetRotation = Quaternion.Inverse(_originalRotation) * targetRotation;
            
            LeanTween.rotate(gameObject, targetRotation, 0f)
            .setOnComplete(() =>
            {
                ResetChopAngle();
            }); 
        }

        void DestroySelf()
        {
            /// play some wood/bark explosion particles and stuff
            Destruct();
        }

        void ResetChopAngle()
        {
            LeanTween.rotate(gameObject, Vector3.zero, 0.33f)
            .setEaseOutExpo();
        }
    }
}