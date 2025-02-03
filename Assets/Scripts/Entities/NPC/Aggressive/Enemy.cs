using System;

using UnityEngine;
using UnityEngine.AI;

using UZSG.Data;
using UZSG.Interactions;
using UZSG.Systems;
using UZSG.Saves;
using static UZSG.Entities.EnemyActionStates;

namespace UZSG.Entities
{
    public interface IHasHealthBar
    {
        public EntityHealthBar HealthBar { get; }
    }

    public partial class Enemy : NonPlayerCharacter, IPlayerDetectable, IHasHealthBar
    {
        public EnemyData EnemyData => entityData as EnemyData;


        #region Properties
        
        public float MoveSpeed
        {
            get => navMeshAgent.speed;
            set => navMeshAgent.speed = value;
        }
        public float PlayerDetectionRadius
        {
            get => _siteRadius;
        }
        public float PlayerAttackableRadius
        {
            get => _attackRadius;
        }

        #endregion


        #region Enemy events

        public event Action<Enemy> OnDeath;

        #endregion


        [SerializeField] float rotationDamping = 9f;
        [SerializeField] float _roamTime; /// Time it takes for the agent to travel a point
        [SerializeField] float _roamRadius = 16f; /// Radius of which the agent can travel
        [SerializeField] float _roamInterval = 12f; /// Interval before the model moves again

        [Header("Enemy Components")]
        [SerializeField] Animator animator;
        public Animator Animator => animator;
        [SerializeField] protected EnemyMoveStateMachine moveStateMachine;
        public EnemyMoveStateMachine MoveStateMachine => moveStateMachine;
        [SerializeField] protected EnemyActionStateMachine actionStateMachine;
        public EnemyActionStateMachine ActionStateMachine => actionStateMachine;
        [SerializeField] Transform head;
        public Transform Head => head;
        
        [SerializeField] NavMeshAgent navMeshAgent;
        [SerializeField] Transform hordeTransform;

        [Header("Agent Information")]
        public LayerMask PlayerLayer; /// Layers that the enemy chases
        public bool isInHorde;
        [SerializeField] Transform _hordeTransform;
        [SerializeField] bool _hasAlreadyScreamed;
        [SerializeField] bool _isAttackOnCooldown;
        [SerializeField] bool _isAttacking;
        [SerializeField] bool _isAlreadyRotating;
        [SerializeField] bool _isInHordeMode;
        [SerializeField] bool _hasTargetInSight; /// checks if the player is in site, attack range or is a target
        [SerializeField] bool _hasTargetInAttackRange;
        [SerializeField] float _attackCooldown;
        [SerializeField] float _attackDamage;
        [SerializeField] float _distanceFromPlayer;
        [SerializeField] float _rotationThreshold; /// note that threshold must be greater than "_siteRadius"
        [SerializeField] float _distanceThreshold;
        [SerializeField] float _moveSpeed;
        [SerializeField] float _siteRadius;
        [SerializeField] float _attackRadius; /// Radius of which the enemy detects the player
        [SerializeField] Vector3 _randomDestination; /// Destination of agent


        #region Initializing Enemy

        public override void OnSpawn()
        {
            base.OnSpawn();

            // initialize the enemy before spawning
            RetrieveAttributes();
            InitializeAnimator();
            InitializeActuators();
            InitializeAgent();
            DisableRagdoll();
            
            HealthBar.Initialize();

            Game.Tick.OnSecond += OnSecond;
        }

        protected override void OnKill()
        {
            OnDeath?.Invoke(this);
        }

        public override void ReadSaveData(EntitySaveData saveData)
        {
            base.ReadSaveData(saveData);


        }
        
        /// <summary>
        /// Retrieve attribute values and cache
        /// </summary>
        void RetrieveAttributes()
        {
            if (Attributes.TryGet("move_speed", out var moveSpeed))
            {
                this.MoveSpeed = moveSpeed.Value;
            }
            if (Attributes.TryGet("attack_range", out var atkRange))
            {
                this._siteRadius = atkRange.Value;
            }
            if (Attributes.TryGet("attack_radius", out var atkRadius))
            {
                this._attackRadius = atkRadius.Value;
            }
            if (Attributes.TryGet("attack_damage", out var atkDmg))
            {
                this._attackDamage = atkDmg.Value;
            }
            if (Attributes.TryGet("attack_speed", out var atkSpeed))
            {

            }
        }

        void InitializeAgent()
        {
            navMeshAgent.speed = _moveSpeed;
            actionStateMachine.ToState(Idle);

            // if spawned in an event horde, change state to horde
            if (isInHorde)
            {
                // store the current transform when spawned of the horde zombie
                _hordeTransform = transform;
                actionStateMachine.ToState(Horde);
            }
        }

        #endregion


        void OnSecond(SecondInfo s)
        {
            ResetTargetIfNotInRange();
            KillZombieIfDead();
        }
    }
}