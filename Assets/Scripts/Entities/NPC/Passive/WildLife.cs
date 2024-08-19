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
    public class Wildlife : Entity, IDetectable
    { 


        #region Wild Life Fundamental Data
        public WildlifeActionStatesMachine WildlifeStateMachine => wildlifeStateMachine;
        public float RoamTime;
        
        public Rigidbody rb;
        public Attributes.Attribute HealthAttri;
        Player _playerToFlee; // the player that the critter flees from
        NavMeshHit navHit;
        WildlifeActionStates _actionState;
        [SerializeField] NavMeshAgent wildlifeEntity;
        
        [SerializeField] bool _inSiteRange;
        [SerializeField] protected WildlifeActionStatesMachine wildlifeStateMachine;
        [SerializeField] float health;
        [SerializeField] float speed;


        #endregion



        #region Wild Life Movement Data

        Vector3 _randomDestination;
        Vector3 _fleeDestination;
        float fleeDistance = 10.0f;
        bool hasCalculatedFleeDestination;
        [SerializeField] float _distanceFromPlayer, _siteRadius, _roamRadius, _roamInterval;

        #endregion



        #region Wild Life Overall Data

        public WildlifeData WildlifeData => entityData as WildlifeData;
        public string defaultPath; // Default file path of the specific enemy
        public WildlifeSaveData defaultData;
        [SerializeField] protected AttributeCollection attributes;
        public AttributeCollection Attributes => attributes;

        #endregion        


        #region Start/Update/Initialize

        protected virtual void Start()
        {
            _actionState = HandleTransition;
            ExecuteAction(_actionState);
            Game.Tick.OnSecond += Game_Tick_OnSecond;
        }

        private void Game_Tick_OnSecond(SecondInfo info)
        {
            ResetIfPlayerNotInRange();
        }

        protected virtual void FixedUpdate()
        {
            _actionState = HandleTransition;
            ExecuteAction(_actionState);
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
            rb = GetComponent<Rigidbody>();
            defaultPath = entityDefaultsPath + $"{entityData.Id}_defaults.json";
            Initialize();
        }

        void Initialize()
        {
            _playerToFlee = null; // set player to none
            LoadDefaults(); // read from JSON file the default enemy attributes
            InitializeAttributes(); // set the default attributes of the enemy
        }

        void LoadDefaults()
        {
            // Read from the JSON file the default data
            var defaultsJson = File.ReadAllText(Application.dataPath + defaultPath);
            defaultData = JsonUtility.FromJson<WildlifeSaveData>(defaultsJson);
            Game.Console.Log("Wildlife data: \n" + defaultData);
        }

        void InitializeAttributes()
        {
            attributes = new();
            attributes.ReadSaveJson(defaultData.Attributes);

            HealthAttri = attributes.Get("health");
            health = HealthAttri.Value;
            wildlifeEntity.speed = attributes.Get("move_speed").Value;
            _siteRadius = attributes.Get("wildlife_site_radius").Value;
            _roamInterval = attributes.Get("wildlife_roam_interval").Value;
            _roamRadius = attributes.Get("wildlife_roam_radius").Value;
        }

        #endregion



        #region Sensor

        public void PlayerSiteDetect(Player player)
        {
            if (player != null)
            {
                _playerToFlee = player; // set the current target of the enemy to they player found
                _inSiteRange = true;
            }
        }

        public void PlayerAttackDetect(Player player)
        {
            // remain na walang laman to unless umaatake ung critter
        }   

        void ResetIfPlayerNotInRange()
        {
            if (_inSiteRange)
            {
                _distanceFromPlayer = Vector3.Distance(_playerToFlee.Position, transform.position);
                if (_siteRadius <= _distanceFromPlayer)
                {
                    _playerToFlee = null;
                    _inSiteRange = false;
                    wildlifeStateMachine.ToState(WildlifeActionStates.Roam);
                }
            }
        }

        bool IsInSiteRange
        {
            get
            {
                if (_inSiteRange)
                {
                    return true;
                }
                return false;
            }
        }

        bool IsDead
        {
            get
            {
                return false;
            }
        }

        public WildlifeActionStates HandleTransition // "sense" what's the state of the animal 
        {
            get
            {
                if (IsDead)
                    return WildlifeActionStates.Die;
                if (IsInSiteRange)
                    return WildlifeActionStates.RunAway;
                else
                    return WildlifeActionStates.Roam;
            }
        }

        #endregion



        #region Actuator

        void Die()
        {
            WildlifeStateMachine.ToState(WildlifeActionStates.Die);
            Game.Tick.OnSecond -= Game_Tick_OnSecond;
            Game.Entity.Kill(this);
            //Debug.Log("Die");
        }

        void Roam()
        {
            wildlifeStateMachine.ToState(WildlifeActionStates.Roam);
            hasCalculatedFleeDestination = false;

            // Continue moving toward the destination
            RoamTime -= Time.deltaTime;
            if (RoamTime <= 0)
            {
                // Get a random position
                _randomDestination = UnityEngine.Random.insideUnitSphere * _roamRadius;
                _randomDestination += transform.position;
                NavMesh.SamplePosition(_randomDestination, out NavMeshHit navHit, _roamRadius, NavMesh.AllAreas);

                // Set the agent's destination to the random point
                wildlifeEntity.SetDestination(navHit.position);
                RoamTime = UnityEngine.Random.Range(1.0f, _roamInterval); // Reset RoamTime for the next movement
                wildlifeEntity.updateRotation = true;
            }
        }

        void RunAway()
        {
            wildlifeStateMachine.ToState(WildlifeActionStates.RunAway);

            // set rigid body to dynamic
            rb.isKinematic = false;

            if (!hasCalculatedFleeDestination)
            {
                _fleeDestination = CalculateFleeDestination;
            }

            // Run away from player with occasional zigzag
            if (NavMesh.SamplePosition(_fleeDestination, out navHit, _roamRadius, NavMesh.AllAreas))
            {
                // Set the agent's destination to the flee destination
                wildlifeEntity.SetDestination(navHit.position);
                hasCalculatedFleeDestination = true;
            }
        }

        Vector3 CalculateFleeDestination // Next movement with zigzag 
        {
            get
            {
                // Calculate direction vector from the wildlife to the player
                Vector3 _directionToPlayer = _playerToFlee.transform.position - transform.position;

                // Invert the direction to get the opposite direction
                Vector3 _oppositeDirection = -_directionToPlayer;

                // Normalize the direction to get a unit vector
                Vector3 _fleeDirection = _oppositeDirection.normalized;

                // Zigzag by alternating between left and right
                Vector3 _zigzagDirection;
                if (UnityEngine.Random.value > 0.5f)
                {
                    _zigzagDirection = Vector3.Cross(_fleeDirection, Vector3.up).normalized;
                }
                else
                {
                    _zigzagDirection = -Vector3.Cross(_fleeDirection, Vector3.up).normalized;
                }

                // New destination
                fleeDistance = 20.0f;
                Vector3 _fleeDestination = transform.position + (_fleeDirection + _zigzagDirection) * fleeDistance;
                return _fleeDestination;
            }
        }

        public void ExecuteAction(WildlifeActionStates action) // execute an action depending on what state the entity is on
        {
            switch (action)
            {
                case WildlifeActionStates.Die:
                    Die();
                    break;
                case WildlifeActionStates.Roam:
                    Roam();
                    break;
                case WildlifeActionStates.RunAway:
                    RunAway();
                    break;
            }
        }

        #endregion


    }
}