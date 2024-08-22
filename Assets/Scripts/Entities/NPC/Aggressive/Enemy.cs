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
        
    public partial class Enemy : NonPlayerCharacter, IPlayerDetectable
    {
        /**/
        public static void Clear()
        {
            // This simply calls the Clear method in the Console window
            var logEntries = System.Type.GetType("UnityEditor.LogEntries,UnityEditor.dll");
            var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            clearMethod.Invoke(null, null);
        }

        public EnemyData EnemyData => entityData as EnemyData;

        [Header("Agent Information")]
        public LayerMask PlayerLayer; // Layers that the enemy chases
        [SerializeField] bool _isInHordeMode;
        [SerializeField] bool _hasTargetInSight; // checks if the player is in site, attack range or is a target
        [SerializeField] bool _hasTargetInAttackRange;
        [SerializeField] float _roamTime; // Time it takes for the agent to travel a point
        [SerializeField] float _roamRadius; // Radius of which the agent can travel
        [SerializeField] float _roamInterval; // Interval before the model moves again
        [SerializeField] float _distanceFromPlayer;
        [SerializeField] float _moveSpeed;
        [SerializeField] float _siteRadius;
        [SerializeField] float _attackRadius; // Radius of which the enemy detects the player
        [SerializeField] Vector3 _randomDestination; // Destination of agent
        [SerializeField] EnemyActionStates _currentActionState;


        #region Properties

        public float PlayerDetectionRadius
        {
            get
            {
                return attributes["player_detection_radius"].Value;
            }
        }

        #endregion


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

            RetrieveAttributes();
            InitializeAnimator();
            InitializeActuators();
            targetEntity = null;

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
            _roamRadius = Attributes.Get("zombie_roam_radius").Value;
            _roamInterval = Attributes.Get("zombie_roam_interval").Value;
        }

        #endregion


        protected virtual void FixedUpdate()
        {
            
        }

        void OnSecond(SecondInfo s)
        {
            ResetTargetIfNotInRange();
        }


        #region Agent Player Detection

        /// <summary>
        /// Set this Enemy's target to the detected player then show detection animation.
        /// </summary>
        /// <param name="etty"></param>
        public void DetectPlayer(Entity etty)
        {
            if (etty != null && etty is Player player)
            {
                targetEntity = player; 
                _hasTargetInSight = true;

                StartCoroutine(FacePlayerAndScream(player));
            }
        }

        public void PlayerAttackDetect(Entity etty)
        {
            if (etty != null && etty is Player player)
            {
                _hasTargetInAttackRange = true;
            }
        }

        public void ResetTargetIfNotInRange()
        {
            // Check if there is a target, then calculate the distance
            if (_hasTargetInSight)
            {
                _distanceFromPlayer = Vector3.Distance(targetEntity.Position, transform.position); 
        
                if (_siteRadius <= _distanceFromPlayer) // if target no longer in site reset target
                {
                    targetEntity = null;
                    _hasTargetInSight = false;
                    actionStateMachine.ToState(EnemyActionStates.Roam);
                }
                else
                {
                    // Check if no player in attack range, reset to chase
                    if (_hasTargetInAttackRange)
                    {
                        if (_attackRadius <= _distanceFromPlayer)
                        {
                            _hasTargetInAttackRange = false;
                            actionStateMachine.ToState(EnemyActionStates.Chase);
                        }
                    }
                }
            }
        }

        #endregion
    }
}