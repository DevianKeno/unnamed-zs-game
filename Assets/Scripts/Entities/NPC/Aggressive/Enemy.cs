using UnityEngine;
using UnityEngine.AI;

using UZSG.Data;
using UZSG.Systems;
using UZSG.Interactions;
using UZSG.Attributes;

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
        [SerializeField] protected EnemyActionStatesMachine enemyStateMachine;
        public EnemyActionStatesMachine EnemyStateMachine => enemyStateMachine;
        [SerializeField] NavMeshAgent navMeshAgent;
        public Transform hordeTransform;


        #region Initializing methods

        public override void OnSpawn()
        {
            base.OnSpawn();

            Initialize();
        }

        void Initialize() 
        {
            RetrieveAttributes();
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

        public void DetectPlayer(Player player)
        {
            if (player != null)
            {
                targetEntity = player; // set the current target of the enemy to they player found
                _hasTargetInSight = true;
            }
        }

        public void PlayerAttackDetect(Player player)
        {
            if (player != null)
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
                    enemyStateMachine.ToState(EnemyActionStates.Roam);
                }
                else
                {
                    // Check if no player in attack range, reset to chase
                    if (_hasTargetInAttackRange)
                    {
                        if (_attackRadius <= _distanceFromPlayer)
                        {
                            _hasTargetInAttackRange = false;
                            enemyStateMachine.ToState(EnemyActionStates.Chase);
                        }
                    }
                }
            }
        }

        #endregion


        #region Agent sensors
        
        public bool HasTargetInSight  // determines if enemy can Chase Player or Roam map
        {
            get
            {
                return _hasTargetInSight;
            }
        }

        public bool HasTargetInAttackRange // determines if enemy can Attack Player
        {
            get
            {   
                return _hasTargetInAttackRange;
            }
        }
        public bool HasNoHealth // determines if the enemy is dead
        {
            get
            {
                return IsDead; // bool stating if the npc is dead
            }
        }

        public bool IsSpecialAttackTriggered // determines if an event happened that triggered special Attack 1
        {
            get
            {
                return false;
            }
        }
        public bool IsSpecialAttack2Triggered // determines if an event happened that triggered special Attack 2
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

        public EnemyActionStates HandleTransition() // "sense" what's the state of the enemy 
        {
            if (IsDead)
            {
                return EnemyActionStates.Die;
            }

            // if enemy is in horde mode
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

        public void Chase()
        {
            enemyStateMachine.ToState(EnemyActionStates.Chase);

            // set rigid body to dynamic
            rb.isKinematic = false;

            // allow enemy movement
            navMeshAgent.isStopped = false;
            navMeshAgent.updateRotation = true;

            // chase player position
            navMeshAgent.SetDestination(targetEntity.transform.position);
        }

        public void Roam()
        {
            navMeshAgent.isStopped = false;
            // Check if the enemy has reached its destination and is actively moving
            if (navMeshAgent.remainingDistance >= 0.002f && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                navMeshAgent.isStopped = true;
                navMeshAgent.updateRotation = false;
            }
            else
            {
                // Continue moving toward the destination
                _roamTime -= Time.deltaTime;
                if (_roamTime <= 0)
                {
                    // Get a random position
                    _randomDestination = UnityEngine.Random.insideUnitSphere * _roamRadius;
                    _randomDestination += transform.position;

                    NavMesh.SamplePosition(_randomDestination, out NavMeshHit navHit, _roamRadius, NavMesh.AllAreas);

                    // Set the agent's destination to the random point
                    navMeshAgent.SetDestination(navHit.position);
                    enemyStateMachine.ToState(EnemyActionStates.Roam);
                    _roamTime = UnityEngine.Random.Range(1.0f, _roamInterval); // Reset RoamTime for the next movement
                    navMeshAgent.updateRotation = true;
                }
            }
        }

        public void Attack2() 
        {
            enemyStateMachine.ToState(EnemyActionStates.Attack2);
            Debug.Log("Attack2"); 
        }
        public void Attack()
        {
            enemyStateMachine.ToState(EnemyActionStates.Attack);

            // set the rigid body of the enemy to kinematic
            rb.isKinematic = true;

            // prevent the enemy from moving when in attack range
            navMeshAgent.isStopped = true;
            navMeshAgent.updateRotation = false;
        }
        public void Die()
        {
            enemyStateMachine.ToState(EnemyActionStates.Die);
            Game.Tick.OnSecond -= OnSecond;
            Game.Entity.Kill(this);
            Debug.Log("Die");
        }
        public void SpecialAttack()
        {
            enemyStateMachine.ToState(EnemyActionStates.SpecialAttack);
            Debug.Log("SpecialAttack"); 
        }
        public void SpecialAttack2()
        {
            enemyStateMachine.ToState(EnemyActionStates.SpecialAttack2);
            Debug.Log("SpecialAttack2"); 
        }

        public void Horde()
        {
            enemyStateMachine.ToState(EnemyActionStates.Horde);

            // Set the starting position and rotation of the zombie
            transform.SetPositionAndRotation(hordeTransform.position, hordeTransform.rotation);

            // Move forward according to speed
            transform.Translate(Vector3.forward * (_moveSpeed * Time.deltaTime));
        }

        public void ExecuteAction(EnemyActionStates action) // execute an action depending on what state the entity is on
        {
            switch (action)
            {
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

        #endregion
    }
}