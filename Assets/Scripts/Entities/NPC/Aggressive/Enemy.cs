using System;
using System.IO;
using UnityEngine;
using UnityEngine.AI;
using UZSG.Attributes;
using UZSG.Data;
using UZSG.Players;
using UZSG.Systems;
using UZSG.Interactions;

namespace UZSG.Entities
{
    public abstract class Enemy : Entity, IDetectable
    {
        #region Agent movement related

        public float AttackRange;  // range from which it follow, attacks Players (to remove)
        public float RoamRadius; // Radius of which the agent can travel
        public float RoamInterval; // Interval before the model moves again
        public float RoamTime; // Time it takes for the agent to travel a point
        Player _target; // Current target of the enemy
        public EnemyActionStatesMachine EnemyStateMachine => enemyStateMachine;
        float _distanceFromPlayer; // the actual distance in game distance from the Player
        bool _inSite, _inAttack; // checks if the player is in site, attack range
        EnemyActionStates actionState;
        PlayerDetection _playerDetected;
        Vector3 _randomDestination; // Destination of agent
        [SerializeField] protected EnemyActionStatesMachine enemyStateMachine;
        [SerializeField] NavMeshAgent _enemyEntity; // the entity's agent movement
        [SerializeField] LayerMask PlayerLayer; // Layers that the enemy chases


        #endregion


        #region Agent data

        public EnemyData EnemyData => entityData as EnemyData;
        public string defaultPath; // Default file path of the specific enemy
        public EnemySaveData defaultData;
        [SerializeField] AttributeCollection<GenericAttribute> generic;


        #endregion

        protected virtual void Start()
        {
            actionState = HandleTransition;
            executeAction(actionState);
        }

        private void PlayerInRange(Player player)
        {
            _target = player;
        }

        public void PlayerDetect(Player player)
        {
            PlayerInRange(player);
        }

        protected virtual void LateUpdate()
        {
            
        }

        void FixedUpdate()
        {
            actionState = HandleTransition;
            executeAction(actionState);
        }

        public override void OnSpawn()
        {
            defaultPath = entityDefaultsPath + $"{entityData.Id}_defaults.json";
            Game.Console.Log("Enemy can chase player!\n");
            Initialize();
        }

        void Initialize() 
        {
            LoadDefaults(); // read from JSON file the default enemy attributes
            InitializeAttributes(); // set the default attributes of the enemy
            Game.Console.Log("Enemy fully initialized!");
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
        }

        #region Agent sensors
        public EnemyActionStates IsInSiteRange  // determines if enemy can Chase Player or Roam map
        {
            get
            {
                if (_target != null)
                {
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

        public EnemyActionStates HandleTransition // "sense" what's the state of the enemy 
        {
            get
            {
                // if enemy has no health, state is dead
                if (IsNoHealth == true)
                {
                    return EnemyActionStates.Die;
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
            enemyStateMachine.ToState(EnemyActionStates.Roam);
            RoamTime -= Time.deltaTime;
            if (RoamTime <= 0)
            {
                // Get a random position
                _randomDestination = UnityEngine.Random.insideUnitSphere * RoamRadius;
                _randomDestination += transform.position;

                NavMesh.SamplePosition(_randomDestination, out NavMeshHit navHit, RoamRadius, NavMesh.AllAreas);

                // Set the agent's destination to the random point
                _enemyEntity.SetDestination(navHit.position);
                RoamTime = UnityEngine.Random.Range(1.0f, RoamInterval);
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

        public void executeAction(EnemyActionStates action) // execute an action depending on what state the entity is on
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
            }
        }

        #endregion


    }
}