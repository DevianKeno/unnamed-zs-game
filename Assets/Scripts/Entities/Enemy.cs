using System;

using UnityEngine;
using UnityEngine.AI;

using UZSG.Attributes;
using UZSG.Data;
using UZSG.Interactions;
using UZSG.Systems;
using UZSG.Saves;
using static UZSG.Entities.EnemyActionStates;

namespace UZSG.Entities
{
    public partial class Enemy : NonPlayerCharacter, IPlayerDetectable, IDamageSource
    {
        public EnemyData EnemyData => entityData as EnemyData;

        /// <summary>
        /// Whether if this enemy is spawned from the NaturalEnemySpawnEvent.
        /// </summary>
        internal bool _isNaturallySpawned;
        protected bool _hasAlreadyScreamed;

        public float MoveSpeed
        {
            get => NavMeshAgent.speed;
            set => NavMeshAgent.speed = value;
        }
        public float PlayerVisionRange => _playerVisionRange;

        public event Action<Enemy> OnDeath;

        [field: Header("Enemy Components")]
        public Animator Animator { get; protected set; }
        public EnemyMoveStateMachine MoveStateMachine { get; protected set; }
        public EnemyActionStateMachine ActionStateMachine { get; protected set; }
        public NavMeshAgent NavMeshAgent { get; protected set; }
        [SerializeField] protected Transform hordeTransform;

        [Header("Agent Information")]
        [SerializeField] protected bool _isAttackOnCooldown;
        [SerializeField] protected bool _isAttacking;
        [SerializeField] protected bool _isAlreadyRotating;
        [SerializeField] protected bool _hasTargetInAttackRange;
        [SerializeField] protected bool _hasTargetInSight; /// checks if the player is in site, attack range or is a target
        [SerializeField] protected float _attackCooldown;
        [SerializeField] protected float _roamRadius = 24f; /// Radius of which the agent can travel
        [SerializeField] protected float _roamTimer;
        [SerializeField] protected float _roamInterval = 12f; /// Interval before the model moves again
        [SerializeField] protected float _rotationDamping = 9f;
        [SerializeField] protected float _rotationThreshold; /// note that threshold must be greater than "_siteRadius"
       
        [Header("Cached attribute values")]
        [SerializeField] protected float _attackDamage;
        [SerializeField] protected float _attackRange;
        [SerializeField] protected float _attackSpeed;
        [SerializeField] protected float _playerVisionRange;

        // [SerializeField] Vector3 _randomDestination; /// Destination of agent


        #region Initializing Enemy

        protected override void Awake()
        {
            base.Awake();
            Animator = GetComponentInChildren<Animator>();
            MoveStateMachine = GetComponent<EnemyMoveStateMachine>();
            ActionStateMachine = GetComponent<EnemyActionStateMachine>();
            NavMeshAgent = GetComponent<NavMeshAgent>();
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
        }

        public override void Kill()
        {
            /// Clear the path to stop all movement
            NavMeshAgent.ResetPath();
            IsAlive = false;
            MoveStateMachine.ToState(EnemyMoveStates.Idle);
            ActionStateMachine.ToState(Die);
            // EnableRagdoll();
            Invoke(nameof(Despawn), 5f);
        }

        public override void ReadSaveData(EntitySaveData saveData)
        {
            base.ReadSaveData(saveData);
        }

        public void ReadSaveData(EnemySaveData saveData)
        {

        }

        public new EnemySaveData WriteSaveData()
        {
            var esd = base.WriteSaveData();
            
            var ensd = (EnemySaveData) esd;
            ensd.IsNaturallySpawned = this._isNaturallySpawned ? 1 : 0;

            return ensd;
        }

        #endregion


        public virtual void NotifyDetection(Player player) { }
    }
}