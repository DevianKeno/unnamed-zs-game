using UnityEngine;

using UZSG.Attributes;
using UZSG.Data;
using UZSG.Entities;
using UZSG.Interactions;
using UZSG.Items;
using UZSG.Items.Tools;
using UZSG.Saves;

namespace UZSG.Objects
{
    public class TreeResource : Resource, IInteractable, ISaveDataReadWrite<BaseObjectSaveData>
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
        [SerializeField] GameObject lod0;
        
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
        
        public override void ReadSaveData(BaseObjectSaveData saveData)
        {
            base.ReadSaveData(saveData);
            Attributes.Get(AttributeId.Health).Value = saveData.GetEntry<float>("health");
        }

        public override BaseObjectSaveData WriteSaveData()
        {
            var sd = base.WriteSaveData();
            sd.AddEntry("health", Attributes[AttributeId.Health].Value);
            return sd;
        }

        void TakeDamage(DamageInfo dmg)
        {
            if (Attributes.TryGet(AttributeId.Health, out var health))
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
        
        void PlayChopSound()
        {

        }
        
        public void Cutdown()
        {
            IsChoppable = false;
            IsFelled = true;
            AllowInteractions = false;
            this.rigidbody.isKinematic = true;

            Game.Audio.PlayInWorld("tree_fell", Position);
            Game.Entity.Spawn<TreeEntity>("tree_log", this.Position, this.Rotation, (info) =>
            {
                var treeEntity = info.Entity;
                treeEntity.Rigidbody.mass = this.rigidbody.mass;
                treeEntity.Rigidbody.AddForce(_lastHitAngle * rigidbody.mass, ForceMode.Impulse);
                DestroySelf();
            });
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