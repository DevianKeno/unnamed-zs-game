using System;
using System.IO;
using UnityEngine;
using UnityEngine.AI;
using UZSG.Attributes;
using UZSG.Data;
using UZSG.Players;
using UZSG.Systems;
using UZSG.Interactions;
using System.Collections;

namespace UZSG.Entities
{
    public partial class Enemy : NonPlayerCharacter, IDetectable
    {
        #region Agent movement and Fundamental data

        protected float AttackRange;  // range from which it follow, attacks Players (to remove)
        public EnemyActionStatesMachine EnemyStateMachine => enemyStateMachine;
        public bool _isInHorde;
        EnemyActionStates _actionState;
        Vector3 _randomDestination; // Destination of agent
        float RoamTime; // Time it takes for the agent to travel a point
        float _roamRadius; // Radius of which the agent can travel
        float _roamInterval; // Interval before the model moves again
        float _distanceFromPlayer;
        float _speed;
        float _siteRadius, _attackRadius; // Radius of which the enemy detects the player
        [SerializeField] bool hasTargetInSite, hasTargetInAttack, isDead; // checks if the player is in site, attack range or is a target
        [SerializeField] protected EnemyActionStatesMachine enemyStateMachine;
        [SerializeField] NavMeshAgent _enemyEntity; // the entity's agent

        
        #endregion


        #region Horde Setting
        
        protected Transform Hordetransform;

        #endregion


        #region Agent data

        public EnemyData EnemyData => entityData as EnemyData;
        public EnemySaveData defaultData;


        #endregion


        #region Enemy Start/Update
        protected virtual void Start()
        {
            _actionState = HandleTransition;
            ExecuteAction(_actionState);
            Game.Tick.OnSecond += Game_Tick_OnSecond;
        }

        private void Game_Tick_OnSecond(SecondInfo info)
        {
            ResetPlayerIfNotInRange();
        }

        protected virtual void LateUpdate()
        {
            
        }

        void FixedUpdate()
        {
            _actionState = HandleTransition;
            ExecuteAction(_actionState);
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
            _enemyEntity = GetComponent<NavMeshAgent>();
            Initialize();
        }

        #endregion


        #region Agent Player Detection

        public void PlayerSiteDetect(Player player)
        {
            if (player != null)
            {
                _player = player; // set the current target of the enemy to they player found
                hasTargetInSite = true;
            }
        }

        public void PlayerAttackDetect(Player player)
        {
            if (player != null)
            {
                hasTargetInAttack = true;
            }
        }

        public void ResetPlayerIfNotInRange()
        {
            // Check if there is a target, then calculate the distance
            if (hasTargetInSite)
            {
                _distanceFromPlayer = Vector3.Distance(_player.Position, transform.position); 
        
                if (_siteRadius <= _distanceFromPlayer) // if target no longer in site reset target
                {
                    _player = null;
                    hasTargetInSite = false;
                    enemyStateMachine.ToState(EnemyActionStates.Roam);
                }
                else
                {
                    // Check if no player in attack range, reset to chase
                    if (hasTargetInAttack)
                    {
                        if (_attackRadius <= _distanceFromPlayer)
                        {
                            hasTargetInAttack = false;
                            enemyStateMachine.ToState(EnemyActionStates.Chase);
                        }
                    }
                }
            }
        }

        #endregion


        #region Agent Loading Default Data

        void Initialize() 
        {
            _player = null; // set player to none
            LoadDefaults(); // read from JSON file the default enemy attributes
            InitializeAttributes(); // set the default attributes of the enemy
        }

        public override void LoadDefaults()
        {
            base.LoadDefaults();
            defaultData = JsonUtility.FromJson<EnemySaveData>(defaultsJson);
            Game.Console.Log("Enemy data: \n" + defaultData);
        }

        void InitializeAttributes()
        {
            attributes = new();
            attributes.ReadSaveData(defaultData.Attributes);
            _siteRadius = Attributes.Get("zombie_site_radius").Value;
            _attackRadius = Attributes.Get("zombie_attack_radius").Value;
            _speed = Attributes.Get("move_speed").Value;
            _roamRadius = Attributes.Get("zombie_roam_radius").Value;
            _roamInterval = Attributes.Get("zombie_roam_interval").Value;
            _enemyEntity.speed = Attributes.Get("move_speed").Value;
            HealthAttri = attributes.Get("health");
        }

        #endregion

        #region Agent sensors
        public bool IsInSiteRange  // determines if enemy can Chase Player or Roam map
        {
            get
            {
                if (hasTargetInSite)
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsInAttackrange // determines if enemy can Attack Player
        {
            get
            {   
                if (hasTargetInAttack)
                {
                    return true;
                }
                return false;
            }
        }
        public bool IsNoHealth // determines if the enemy is dead
        {
            get
            {
                return IsDeadNPC; // bool stating if the npc is dead
            }
        }

        public bool IsSpecialAttackTriggered // determines if an event happened that triggered special Attack 1
        {
            get
            {
                return false;
            }
        }
        public bool IsSpecialAttackTriggered2 // determines if an event happened that triggered special Attack 2
        {
            get
            {
                return false;
            }
        }

        public bool IsHordeMode
        {
            get
            {   // TODO: palitan mo yung "jericho_method" sa method na nagrereturn ng bool; true if in hordemode (lalakad straight line), false if hindi horde mode zombie
                if (_isInHorde)
                {
                    if (IsInSiteRange)
                    {
                        return false;
                    }
                    Hordetransform.SetPositionAndRotation(new Vector3(1, 2, 1), Quaternion.Euler(0, 30, 0));
                    return true;
                }
                return false;
            }
        }

        public EnemyActionStates HandleTransition // "sense" what's the state of the enemy 
        {
            get
            {
                // if enemy has no health, state is dead
                if (IsNoHealth)
                {
                    return EnemyActionStates.Die;
                }
                // if enemy is in horde mode
                // if (IsHordeMode)
                // {
                //     return EnemyActionStates.Horde;
                // }
                // if Player not in Chase range
                if (!IsInSiteRange)
                {
                    return EnemyActionStates.Roam;
                }
                else
                {
                    if (!IsInAttackrange) // If no player found in attack range keep chasing
                    {
                        return EnemyActionStates.Chase;
                    }
                    else
                    {
                        return EnemyActionStates.Attack;
                    }
                }
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
            _enemyEntity.isStopped = false;
            _enemyEntity.updateRotation = true;

            // chase player position
            _enemyEntity.SetDestination(_player.transform.position);
        }

        public void Roam()
        {
            _enemyEntity.isStopped = false;
            // Check if the enemy has reached its destination and is actively moving
            if (_enemyEntity.remainingDistance >= 0.002f && _enemyEntity.remainingDistance <= _enemyEntity.stoppingDistance)
            {
                _enemyEntity.isStopped = true;
                _enemyEntity.updateRotation = false;
            }
            else
            {
                // Continue moving toward the destination
                RoamTime -= Time.deltaTime;
                if (RoamTime <= 0)
                {
                    // Get a random position
                    _randomDestination = UnityEngine.Random.insideUnitSphere * _roamRadius;
                    _randomDestination += transform.position;

                    NavMesh.SamplePosition(_randomDestination, out NavMeshHit navHit, _roamRadius, NavMesh.AllAreas);

                    // Set the agent's destination to the random point
                    _enemyEntity.SetDestination(navHit.position);
                    enemyStateMachine.ToState(EnemyActionStates.Roam);
                    RoamTime = UnityEngine.Random.Range(1.0f, _roamInterval); // Reset RoamTime for the next movement
                    _enemyEntity.updateRotation = true;
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
            _enemyEntity.isStopped = true;
            _enemyEntity.updateRotation = false;
        }
        public void Die()
        {
            enemyStateMachine.ToState(EnemyActionStates.Die);
            Game.Tick.OnSecond -= Game_Tick_OnSecond;
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
            transform.SetPositionAndRotation(Hordetransform.position, Hordetransform.rotation);

            // Move forward according to speed
            transform.Translate(Vector3.forward * (_speed * Time.deltaTime));
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