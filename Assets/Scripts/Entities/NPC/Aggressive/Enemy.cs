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
    public abstract class Enemy : Entity, IDetectable
    {
        #region Agent movement related

        public float AttackRange;  // range from which it follow, attacks Players (to remove)
        public EnemyActionStatesMachine EnemyStateMachine => enemyStateMachine;
        Player _target; // Current target of the enemy
        EnemyActionStates _actionState;
        Vector3 _randomDestination; // Destination of agent
        [SerializeField] bool _hasTargetInSite, _hasTargetInAttack; // checks if the player is in site, attack range or is a target
        [SerializeField] float RoamTime; // Time it takes for the agent to travel a point
        [SerializeField] float _roamRadius; // Radius of which the agent can travel
        [SerializeField] float _roamInterval; // Interval before the model moves again
        //[SerializeField] float _minWait, _maxWait; // waiting time before roaming again
        //[SerializeField] float _waitTime; // actual waiting time of enemy in second
        //[SerializeField] bool _isWaiting = false; // Track if the AI is currently waiting
        [SerializeField] float _distanceFromPlayer;
        [SerializeField] float _speed;
        [SerializeField] float _siteRadius, _attackRadius; // Radius of which the enemy detects the player
        [SerializeField] protected EnemyActionStatesMachine enemyStateMachine;
        [SerializeField] NavMeshAgent _enemyEntity; // the entity's agent movement
        [SerializeField] LayerMask PlayerLayer; // Layers that the enemy chases

        
        #endregion


        #region Horde Setting
        
        public Transform Hordetransform;

        #endregion


        #region Agent data

        public EnemyData EnemyData => entityData as EnemyData;
        public string defaultPath; // Default file path of the specific enemy
        public EnemySaveData defaultData;
        [SerializeField] protected AttributeCollection attributes;
        public AttributeCollection Attributes => attributes;


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
            defaultPath = entityDefaultsPath + $"{entityData.Id}_defaults.json";
            Initialize();
        }

        #endregion


        #region Agent Player Detection

        public void PlayerSiteDetect(Player player)
        {
            if (player != null)
            {
                _target = player; // set the current target of the enemy to they player found
                _hasTargetInSite = true;
            }
        }

        public void PlayerAttackDetect(Player player)
        {
            if (player != null)
            {
                _hasTargetInAttack = true;
            }
        }

        public void ResetPlayerIfNotInRange()
        {
            // Check if there is a target, then calculate the distance
            if (_hasTargetInSite)
            {
                _distanceFromPlayer = Vector3.Distance(_target.Position, transform.position); 
        
                if (_siteRadius <= _distanceFromPlayer) // if target no longer in site reset target
                {
                    _target = null;
                    _hasTargetInSite = false;
                    enemyStateMachine.ToState(EnemyActionStates.Roam);
                }
                else
                {
                    // Check if no player in attack range, reset
                    if (_hasTargetInAttack)
                    {
                        if (_attackRadius <= _distanceFromPlayer)
                        {
                            _hasTargetInAttack = false;
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
            _target = null; // set player to none
            LoadDefaults(); // read from JSON file the default enemy attributes
            InitializeAttributes(); // set the default attributes of the enemy
        }

        void LoadDefaults()
        {
            var defaultsJson = File.ReadAllText(Application.dataPath + defaultPath);
            defaultData = JsonUtility.FromJson<EnemySaveData>(defaultsJson);
            Game.Console.Log("Enemy data: \n" + defaultData);
        }

        void InitializeAttributes()
        {
            attributes = new();
            attributes.ReadSaveJson(defaultData.Attributes);
            
            _siteRadius = Attributes.Get("zombie_site_radius").Value;
            _attackRadius = Attributes.Get("zombie_attack_radius").Value;
            _speed = Attributes.Get("move_speed").Value;
            _roamRadius = Attributes.Get("zombie_roam_radius").Value;
            _roamInterval = Attributes.Get("zombie_roam_interval").Value;
        }

        #endregion

        #region Agent sensors
        public bool IsInSiteRange  // determines if enemy can Chase Player or Roam map
        {
            get
            {
                if (_hasTargetInSite)
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
                if (_hasTargetInAttack)
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
                return false;
            }
        }

        public void TakeDamage(float damage)
        {
            var health = attributes.Get("health");
            health.Remove(damage);
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
                if (JerichoMethod)
                {
                    if (IsInSiteRange)
                    {
                        Hordetransform.SetPositionAndRotation(new Vector3(1, 2, 1), Quaternion.Euler(0, 30, 0));
                        return true;
                    }
                    return false;
                }
                return false;
            }
        }

        public bool JerichoMethod // TODO: pwede mo remove ito or palitan ng method mo, pansamantala ko lang yan 
        {
            get
            {
                return true;
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
            _enemyEntity.isStopped = false;
            enemyStateMachine.ToState(EnemyActionStates.Chase);
            _enemyEntity.SetDestination(_target.transform.position);
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
            _enemyEntity.isStopped = true;
            _enemyEntity.updateRotation = false;
            Debug.Log("Attack"); 
        }
        public void Die()
        {
            enemyStateMachine.ToState(EnemyActionStates.Die);
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