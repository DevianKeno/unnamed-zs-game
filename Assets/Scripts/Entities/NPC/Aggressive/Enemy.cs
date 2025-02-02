using System;

using UnityEngine;
using UnityEngine.AI;

using UZSG.Data;
using UZSG.Systems;
using UZSG.Interactions;

using static UZSG.Entities.EnemyActionStates;

namespace UZSG.Entities
{
    public partial class Enemy : NonPlayerCharacter, IPlayerDetectable
    {
        public EnemyData EnemyData => entityData as EnemyData;


        #region Properties

        public EnemyActionStates CurrentActionState
        {
            get => actionStateMachine.CurrentState.Key;
        }
        public EnemyMoveStates CurrentMoveState
        {
            get => moveStateMachine.CurrentState.Key;
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

            Game.Tick.OnSecond += OnSecond;
        }

        protected override void OnKill()
        {
            OnDeath?.Invoke(this);
        }

        /// <summary>
        /// Retrieve attribute values and cache
        /// </summary>
        void RetrieveAttributes()
        {
            navMeshAgent.speed = Attributes.Get("move_speed").Value;
            _moveSpeed = Attributes.Get("move_speed").Value;
            _siteRadius = Attributes.Get("zombie_site_radius").Value;
            _attackRadius = Attributes.Get("zombie_attack_radius").Value;
            _attackCooldown = Attributes.Get("zombie_attack_cooldown_time").Value;
            _attackDamage = Attributes.Get("attack_damage").Value;
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


        protected virtual void FixedUpdate()
        {

        }

        void OnSecond(SecondInfo s)
        {
            ResetTargetIfNotInRange();
            KillZombieIfDead();
        }
    }
}