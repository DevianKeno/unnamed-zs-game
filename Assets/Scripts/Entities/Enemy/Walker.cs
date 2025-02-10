// #define DEBUGGING_ENABLED

using System;

using UnityEngine;
using UZSG.Attributes;
using UZSG.Interactions;
using UZSG.Items.Weapons;
using UZSG.Saves;

using static UZSG.Entities.EnemyActionStates;

namespace UZSG.Entities
{
    /// <summary>
    /// Walker class zombie.
    /// </summary>
    public partial class Walker : Enemy
    {
        [SerializeField] float chaseMoveSpeedMultiplier; /// set in Inspector
        [SerializeField] float minIdleTime = 0f;
        [SerializeField] float maxIdleTime = 5f;
        [SerializeField] float scanRadius = 32f;
        [SerializeField] float scanIntervalSeconds = 5f;

        float _scanTimer;
        float _targetMoveSpeed;

        public override void OnSpawnEvent()
        {
            base.OnSpawnEvent();

            LoadDefaultSaveData<EnemySaveData>();
            /// Read attributes()
            attributes = new();
            attributes.ReadSaveData(saveData.Attributes);
            // ReadSaveData(saveData);

            RetrieveAttributes();
            InitializeAnimatorEvents();
            InitializeActuatorEvents();
            HealthBar.Initialize();
            // if spawned in an event horde, change state to horde
            // if (isInHorde)
            // {
            //     // store the current transform when spawned of the horde zombie
            //     _hordeTransform = transform;
            //     ActionStateMachine.ToState(Horde);
            // }
            CastDetectionSphere();
            _scanTimer = scanIntervalSeconds;
            Game.Tick.OnSecond += OnSecond;

            ActionStateMachine.ToState(Roam);
        }

        /// <summary>
        /// Retrieve attribute values and cache
        /// </summary>
        void RetrieveAttributes()
        {
            if (Attributes.TryGet(AttributeId.Health, out var health))
            {
                health.OnReachZero += OnHealthReachedZero;
            }
            if (Attributes.TryGet(AttributeId.MoveSpeed, out var moveSpeed))
            {
                this.MoveSpeed = moveSpeed.Value;
                moveSpeed.OnValueChanged += (attr, context) => /// listen to changes in move speed ig
                {
                    this.MoveSpeed = attr.Value;
                };
            }
            if (Attributes.TryGet(AttributeId.AttackDamage, out var atkDmg))
            {
                this._attackDamage = atkDmg.Value;
            }
            if (Attributes.TryGet(AttributeId.AttackSpeed, out var atkSpeed))
            {
                this._attackSpeed = atkDmg.Value;
            }
            if (Attributes.TryGet(AttributeId.AttackRange, out var atkRange))
            {
                this._attackRange = atkRange.Value;
            }
            if (Attributes.TryGet(AttributeId.PlayerVisionRange, out var pvr))
            {
                this._playerVisionRange = pvr.Value;
            }
        }

        void Update()
        {
            _scanTimer += Time.deltaTime;
            if (_scanTimer <= 0)
            {
                CastDetectionSphere();
                _scanTimer = scanIntervalSeconds;
            }
        }
        
        void OnSecond(SecondInfo info)
        {
            ResetTargetIfNotInRange();
        }

        protected override void OnHitboxCollision(object sender, HitboxCollisionInfo info)
        {
            switch (info.Source)
            {
                case Bullet bullet:
                {
                    var hitbox = sender as Hitbox;

                    TakeDamage(new DamageInfo(source: bullet, amount: 10f));

                    SpawnBlood(info.ContactPoint);
                    // SpawnDamageText(info.ContactPoint);
                    Destroy(bullet.gameObject); /// TODO: change based on penetration?
                    break;
                }
                case MeleeWeaponController meleeWeapon:
                {
                    TakeDamage(new DamageInfo(source: meleeWeapon, amount: 10f));

                    SpawnBlood(info.ContactPoint);
                    // SpawnDamageText(info.ContactPoint);
                    break;
                }
            }
        }

        public override void TakeDamage(DamageInfo info)
        {
            if (Attributes.TryGet("health", out var health))
            {
                health.Remove(info.Amount);
            }
        }

        Collider[] _detectedCollidersResults = new Collider[8];
        public void CastDetectionSphere()
        {
            int dCount = Physics.OverlapSphereNonAlloc(transform.position, scanRadius, _detectedCollidersResults, Game.Entity.DEFAULT_LAYER);
            for (int i = 0; i < dCount; i++)
            {
                var collider = _detectedCollidersResults[i];
                if (!collider.TryGetComponent<Player>(out var player)) continue;

                this.NotifyDetection(player);
            }
        }
        
        void OnHealthReachedZero(object sender, AttributeValueChangedContext context)
        {
            if (context.ValueChangedType == UZSG.Attributes.Attribute.ValueChangeType.Decreased)
            {
                Kill();
            }
        }
        
        /// <summary>
        /// Prints only when the header definition 'DEBUGGING_ENABLED' is defined
        /// </summary>
        /// <param name="message"></param>
        [System.Diagnostics.Conditional("DEBUGGING_ENABLED")]
        new void print(object message)
        {
            Debug.Log(message);
        }
    }
}