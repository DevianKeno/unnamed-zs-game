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
        bool _inSite, _inAttack; // checks if the player is in site, attack range
        EnemyActionStates _actionState;
        Vector3 _randomDestination; // Destination of agent
        [SerializeField] float RoamTime; // Time it takes for the agent to travel a point
        [SerializeField] float RoamRadius; // Radius of which the agent can travel
        [SerializeField] float RoamInterval; // Interval before the model moves again
        //[SerializeField] float _minWait, _maxWait; // waiting time before roaming again
        //[SerializeField] float _waitTime; // actual waiting time of enemy in second
        //[SerializeField] bool _isWaiting = false; // Track if the AI is currently waiting
        [SerializeField] float _distanceFromPlayer;
        [SerializeField] float _speed;
        [SerializeField] float _radius; // Radius of which the enemy detects the player
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
        [SerializeField] AttributeCollection<GenericAttribute> generic;


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

        public void PlayerDetect(Player player)
        {
            if (player != null)
            {
                _target = player; // set the current target of the enemy to they player found
            }
        }

        public void ResetPlayerIfNotInRange()
        {
            // Check if there is a target, then calculate the distance
            if (_target != null)
            {
                _distanceFromPlayer = Vector3.Distance(_target.Position, transform.position); 
            }

            // Check if player is no longer in range then no more target to chase
            if (_radius <= _distanceFromPlayer)
            {
                _target = null;
            }
        }


        #region Agent Loading Default Data

        void Initialize() 
        {
            _target = null; // set player to none
            LoadDefaults(); // read from JSON file the default enemy attributes
            InitializeAttributes(); // set the default attributes of the enemy
            Debug.Log("Enemy fully initialized!");
        }

        void LoadDefaults()
        {
            var defaultsJson = File.ReadAllText(Application.dataPath + defaultPath);
            defaultData = JsonUtility.FromJson<EnemySaveData>(defaultsJson);
            Game.Console.Log("Enemy data: \n" + defaultData);
        }

        void InitializeAttributes()
        {
            generic = new();
            generic.ReadSaveJSON(defaultData.GenericAttributes);
            _radius = generic.Get("site_radius").Value;
            //_minWait = generic.Get("wait_min_time_roam_zombie").Value;
            //_maxWait = generic.Get("wait_max_time_roam_zombie").Value;
            _speed = generic.Get("move_speed").Value;
            RoamRadius = generic.Get("zombie_roam_radius").Value;
            RoamInterval = generic.Get("zombie_roam_interval").Value;
        }

        #endregion

        #region Agent sensors
        public EnemyActionStates IsInSiteRange  // determines if enemy can Chase Player or Roam map
        {
            get
            {
                if (_target != null)
                {
                    Debug.Log("In chase state dapat.");
                    return EnemyActionStates.Chase;
                }
                return EnemyActionStates.Roam;
            }
        }

        public EnemyActionStates IsInAttackrange // determines if enemy can Attack Player
        {
            get
            {   
                _inAttack = Physics.CheckSphere(transform.position, AttackRange, PlayerLayer);
                if (_inSite)
                {
                    return EnemyActionStates.Attack;
                }
                return EnemyActionStates.Chase;
            }
        }
        public bool IsNoHealth // determines if the enemy is dead
        {
            get
            {
                return false;
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
                if (jericho_method)
                {
                    if (IsInSiteRange == EnemyActionStates.Roam)
                    {
                        //Hordetransform.position = new Vector3(1, 2, 1);             // change mo value sa need mo
                        //Hordetransform.rotation = Quaternion.Euler(0, 30, 0);       // change mo value sa need mo
                        return false; // palitan mo to ng true jericho once na done ka na
                    }
                    return false;
                }
                return false;
            }
        }

        public bool jericho_method // TODO: pwede mo remove ito or palitan ng method mo, pansamantala ko lang yan 
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
                if (IsNoHealth == true)
                {
                    return EnemyActionStates.Die;
                }
                // if enemy is in horde mode
                if (IsHordeMode)
                {
                    return EnemyActionStates.Horde;
                }
                // if Player not in Chase range
                if (IsInSiteRange == EnemyActionStates.Roam)
                {
                    return EnemyActionStates.Roam;
                }
                else
                {
                    if (IsInAttackrange == EnemyActionStates.Chase) // Chase
                    {
                        return EnemyActionStates.Chase;
                    }
                    else // Attack Player
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
            _enemyEntity.SetDestination(_target.transform.position);
        }

        public void Roam()
        {
            // Check if the enemy has reached its destination and is actively moving
            if (_enemyEntity.remainingDistance >= 0.002f && _enemyEntity.remainingDistance <= _enemyEntity.stoppingDistance)
            {
                //Debug.Log("In waiting state");
                _enemyEntity.isStopped = true;
                _enemyEntity.updateRotation = false;
            }
            else
            {
                //Debug.Log("In roam state and distance is: " + _enemyEntity.remainingDistance + "\nstopping distance is: " + _enemyEntity.stoppingDistance);
                // Continue moving toward the destination
                RoamTime -= Time.deltaTime;
                if (RoamTime <= 0)
                {
                    // Get a random position
                    _randomDestination = UnityEngine.Random.insideUnitSphere * RoamRadius;
                    _randomDestination += transform.position;

                    NavMesh.SamplePosition(_randomDestination, out NavMeshHit navHit, RoamRadius, NavMesh.AllAreas);

                    // Set the agent's destination to the random point
                    _enemyEntity.SetDestination(navHit.position);
                    enemyStateMachine.ToState(EnemyActionStates.Roam);
                    RoamTime = UnityEngine.Random.Range(1.0f, RoamInterval); // Reset RoamTime for the next movement
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
            transform.position = Hordetransform.position;
            transform.rotation = Hordetransform.rotation;

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