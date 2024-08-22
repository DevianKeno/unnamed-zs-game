using UnityEngine;
using UnityEngine.AI;

using UZSG.Data;
using UZSG.Systems;
using UZSG.Interactions;
using UZSG.Attributes;
using System.Collections.Generic;
using System;
using UZSG.Players;

using static UZSG.Entities.EnemyActionStates;

namespace UZSG.Entities
{
    public partial class Enemy : NonPlayerCharacter, IPlayerDetectable
    {
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


        #region Initializing methods

        public override void OnSpawn()
        {
            base.OnSpawn();

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
            _currentActionState = HandleTransition();
            ExecuteAction(_currentActionState);
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

                Rotation = Quaternion.LookRotation(player.Position);
                actionStateMachine.ToState(EnemyActionStates.Scream, lockForSeconds: 2f);
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


        #region Agent sensors
        
        /// <summary>
        /// determines if enemy can Chase Player or Roam map
        /// </summary>
        public bool HasTargetInSight 
        {
            get
            {
                return _hasTargetInSight;
            }
        }
        /// <summary>
        /// determines if enemy can Attack Player
        /// </summary>
        public bool HasTargetInAttackRange
        {
            get
            {   
                return _hasTargetInAttackRange;
            }
        }
        /// <summary>
        /// determines if the enemy is dead
        /// </summary>
        public bool HasNoHealth
        {
            get
            {
                return IsDead;
            }
        }
        /// <summary>
        /// determines if an event happened that triggered special Attack 1
        /// </summary>
        public bool IsSpecialAttackTriggered
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// Whether if an event happened that triggered special Attack 2
        /// </summary>
        public bool IsSpecialAttack2Triggered
        {
            get
            {
                return false;
            }
        }

        public bool IsInHordeMode
        {
            get
            {   // TODO: palitan mo yung "jericho_method" sa method na nagrereturn ng bool; true if in hordemode (lalakad straight line), false if hindi horde mode zombie
                if (_isInHordeMode)
                {
                    if (HasTargetInSight)
                    {
                        return false;
                    }
                    hordeTransform.SetPositionAndRotation(new Vector3(1, 2, 1), Quaternion.Euler(0, 30, 0));
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// "sense" what's the state of the enemy 
        /// </summary>
        /// <returns></returns>
        public EnemyActionStates HandleTransition()
        {
            if (IsDead)
            {
                return EnemyActionStates.Die;
            }

            // if (IsHordeMode)
            // {
            //     return EnemyActionStates.Horde;
            // }

            if (HasTargetInSight)
            {
                if (HasTargetInAttackRange) 
                {
                    return EnemyActionStates.Attack;
                }
                else /// keep chasing
                {
                    return EnemyActionStates.Chase;
                }
            }
            else
            {
                return EnemyActionStates.Roam;
            }
        }

        #endregion


        #region Agent actuator

        void InitializeActuators() /// uncalled yet
        {
            actionStateMachine[EnemyActionStates.Idle].OnEnter += OnActionIdleEnter;
            actionStateMachine[EnemyActionStates.Chase].OnEnter += OnActionChaseEnter;
        }

        void OnActionIdleEnter(object sender, State<EnemyActionStates>.ChangedContext e)
        {
            if (e.PreviousState == EnemyActionStates.Scream)
            {

            }
        }

        void OnActionChaseEnter(object sender, State<EnemyActionStates>.ChangedContext e)
        {
            /// might run twice because of this and ExecuteAction()
            // Chase();
        }

        /// <summary>
        /// Execute an action depending on what state the entity is on
        /// </summary>
        public void ExecuteAction(EnemyActionStates state) 
        {
            switch (state)
            {
                case EnemyActionStates.Idle:
                    Idle();
                    break;

                case EnemyActionStates.Chase:
                    Chase();
                    break;

                case EnemyActionStates.Roam:
                    Roam();
                    break;

                case EnemyActionStates.Attack2:
                    Attack2();
                    break;

                case EnemyActionStates.Attack:
                    Attack();
                    break;

                case EnemyActionStates.Die:
                    Die();
                    break;

                case EnemyActionStates.SpecialAttack:
                    SpecialAttack();
                    break;

                case EnemyActionStates.SpecialAttack2:
                    SpecialAttack2();
                    break;

                case EnemyActionStates.Horde:
                    Horde();
                    break;
            }
        }

        void Idle()
        {

        }

        void Chase()
        {
            actionStateMachine.ToState(EnemyActionStates.Chase);

            /// set rigid body to dynamic
            rb.isKinematic = false;

            /// allow enemy movement
            navMeshAgent.isStopped = false;
            navMeshAgent.updateRotation = true;

            /// chase player position
            navMeshAgent.SetDestination(targetEntity.transform.position);

            /// Switch move state machine to run state on chase :)
            EnemyMoveStates targetMoveState = EnemyMoveStates.Walk;
            // if (runType == Jog)
            // {
            //     targetMoveState = EnemyMoveStates.Jog;
            // }
            // else if (runType == Run)
            // {
                targetMoveState = EnemyMoveStates.Run;
            // }
            
            moveStateMachine.ToState(targetMoveState);
        }

        void Roam()
        {
            navMeshAgent.isStopped = false;
            /// Check if the enemy has reached its destination and is actively moving
            if (navMeshAgent.remainingDistance >= 0.002f && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                navMeshAgent.isStopped = true;
                navMeshAgent.updateRotation = false;
                moveStateMachine.ToState(EnemyMoveStates.Idle);
                actionStateMachine.ToState(EnemyActionStates.Idle);
            }
            else
            {
                /// Continue moving toward the destination
                _roamTime -= Time.deltaTime;
                if (_roamTime <= 0)
                {
                    /// Get a random position
                    _randomDestination = UnityEngine.Random.insideUnitSphere * _roamRadius;
                    _randomDestination += transform.position;

                    NavMesh.SamplePosition(_randomDestination, out NavMeshHit navHit, _roamRadius, NavMesh.AllAreas);

                    /// Set the agent's destination to the random point
                    navMeshAgent.SetDestination(navHit.position);
                    moveStateMachine.ToState(EnemyMoveStates.Walk);
                    actionStateMachine.ToState(EnemyActionStates.Roam);
                    _roamTime = UnityEngine.Random.Range(1.0f, _roamInterval); // Reset RoamTime for the next movement
                    navMeshAgent.updateRotation = true;
                }
            }
        }

        void Attack2() 
        {
            actionStateMachine.ToState(EnemyActionStates.Attack2);
            Debug.Log("Attack2"); 
        }

        void Attack()
        {
            actionStateMachine.ToState(EnemyActionStates.Attack);

            /// set the rigid body of the enemy to kinematic
            rb.isKinematic = true;

            /// prevent the enemy from moving when in attack range
            navMeshAgent.isStopped = true;
            navMeshAgent.updateRotation = false;
        }

        void SpecialAttack()
        {
            actionStateMachine.ToState(EnemyActionStates.SpecialAttack);
            Debug.Log("SpecialAttack"); 
        }

        public void SpecialAttack2()
        {
            actionStateMachine.ToState(EnemyActionStates.SpecialAttack2);
            Debug.Log("SpecialAttack2"); 
        }

        void Die()
        {
            actionStateMachine.ToState(EnemyActionStates.Die);
            Game.Tick.OnSecond -= OnSecond;
            Game.Entity.Kill(this);
            Debug.Log("Die");
        }

        void Horde()
        {
            actionStateMachine.ToState(EnemyActionStates.Horde);

            /// Set the starting position and rotation of the zombie
            transform.SetPositionAndRotation(hordeTransform.position, hordeTransform.rotation);

            /// Move forward according to speed
            transform.Translate(Vector3.forward * (_moveSpeed * Time.deltaTime));
        }

        #endregion
    }
}