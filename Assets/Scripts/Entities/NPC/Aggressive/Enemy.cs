using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using UZSG.Data;
using UZSG.Systems;
using UZSG.Interactions;
using UZSG.Attributes;
using System.Collections.Generic;
using System;
using UZSG.Players;

using static UZSG.Entities.EnemyActionStates;
using System.Collections;

namespace UZSG.Entities
{
    public interface IEnemy
    {
        public event Action<IEnemy> OnDeath;    
    }

    public partial class Enemy : NonPlayerCharacter, IPlayerDetectable, IEnemy
    {
        /// <summary>
        /// Clear console messages.
        /// </summary>
        public static void Clear()
        {
            // This simply calls the Clear method in the Console window
            var logEntries = System.Type.GetType("UnityEditor.LogEntries,UnityEditor.dll");
            var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            clearMethod.Invoke(null, null);
        }

        [Header("Enemy Agent Information")]
        public LayerMask PlayerLayer; // Layers that the enemy chases
        [SerializeField] bool _hasAlreadyScreamed;
        [SerializeField] bool attackOnCooldown;
        [SerializeField] bool isAttacking;
        [SerializeField] bool isAlreadyRotating;
        [SerializeField] bool _isInHordeMode;
        [SerializeField] bool _hasTargetInSight; // checks if the player is in site, attack range or is a target
        [SerializeField] bool _hasTargetInAttackRange;
        [SerializeField] float attackCooldown;
        [SerializeField] float attackDamage;
        [SerializeField] float _distanceFromPlayer;
        [SerializeField] float rotationThreshold; // note that threshold must be greater than "_siteRadius"
        [SerializeField] float  distanceThreshold;
        [SerializeField] float _moveSpeed;
        [SerializeField] float _siteRadius;
        [SerializeField] float _attackRadius; // Radius of which the enemy detects the player
        [SerializeField] Vector3 _randomDestination; // Destination of agent
        public EnemyActionStates CurrentActionState
        {
            get => actionStateMachine.CurrentState.Key;
        }
        public EnemyMoveStates CurrentMoveState
        {
            get => moveStateMachine.CurrentState.Key;
        }


        #region Properties

        public float PlayerDetectionRadius
        {
            get
            {
                return _siteRadius;
            }
        }

        public float PlayerAttackableRadius
        {
            get
            {
                return _attackRadius;
            }
        }

        #endregion
        
        
        public event Action<IEnemy> OnDeath;
        public EnemyData EnemyData => entityData as EnemyData;
        public float RotationDamping = 9f;
        public float _roamTime; // Time it takes for the agent to travel a point
        public float _roamRadius = 16f; // Radius of which the agent can travel
        public float _roamInterval = 12f; // Interval before the model moves again

        [Header("Components")]
        [SerializeField] Animator animator;
        public Animator Animator => animator;
        [SerializeField] protected EnemyMoveStateMachine moveStateMachine;
        public EnemyMoveStateMachine MoveStateMachine => moveStateMachine;
        [SerializeField] protected EnemyActionStateMachine actionStateMachine;
        public EnemyActionStateMachine ActionStateMachine => actionStateMachine;
        [SerializeField] NavMeshAgent navMeshAgent;
        public Transform hordeTransform;


        #region Initializing Enemy

        public override void OnSpawn()
        {
            base.OnSpawn();
            Clear();

            // initialize the enemy before spawning
            RetrieveAttributes();
            InitializeAnimator();
            InitializeActuators();
            InitializeAgent();

            // set ragdoll to false when spawning
            RagdollMode(IsRagdollOff);

            Game.Tick.OnSecond += OnSecond;
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
            attackCooldown = Attributes.Get("zombie_attack_cooldown_time").Value;
            attackDamage = Attributes.Get("attack_damage").Value;
        }

        void InitializeAgent()
        {
            navMeshAgent.speed = _moveSpeed;
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