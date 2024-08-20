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
    public class Wildlife : NonPlayerCharacter, IDetectable
    { 


        #region Wild Life Fundamental Data
        public WildlifeActionStatesMachine WildlifeStateMachine => wildlifeStateMachine;
        NavMeshHit _navHit;
        Vector3 _randomDestination, _fleeDestination; // used for roaming and the fleeing state of the wildlife
        float _distanceFromPlayer, _siteRadius, _roamInterval, _roamRadius, RoamTime, fleeDistance; // variables that determine wildlife state
        WildlifeActionStates actionState;
        [SerializeField] bool _inSiteRange;
        [SerializeField] NavMeshAgent wildlifeEntity;
        [SerializeField] protected WildlifeActionStatesMachine wildlifeStateMachine;


        #endregion


        #region 

        bool hasCalculatedFleeDestination;

        #endregion


        #region Wild Life Overall Data

        public WildlifeData WildlifeData => entityData as WildlifeData;
        public WildlifeSaveData defaultData;

        #endregion        


        #region Start/Update/Initialize

        protected virtual void Start()
        {
            actionState = HandleTransition;
            ExecuteAction(actionState);
            Game.Tick.OnSecond += Game_Tick_OnSecond;
        }

        private void Game_Tick_OnSecond(SecondInfo info)
        {
            ResetPlayerIfNotInRange();
        }

        protected virtual void FixedUpdate()
        {
            actionState = HandleTransition;
            ExecuteAction(actionState);
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
            defaultPath = entityDefaultsPath + $"{entityData.Id}_defaults.json";
            Initialize();
        }

        void Initialize()
        {
            _player = null; // set player to none
            LoadDefaults(); // read from JSON file the default enemy attributes
            InitializeAttributes(); // set the default attributes of the enemy
        }

        public override void LoadDefaults()
        {
            base.LoadDefaults();
            defaultData = JsonUtility.FromJson<WildlifeSaveData>(defaultsJson);
            Game.Console.Log("Wildlife data: \n" + defaultData);
        }

        void InitializeAttributes()
        {
            attributes = new();
            attributes.ReadSaveData(defaultData.Attributes);

            HealthAttri = attributes.Get("health");
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
                _player = player; // set the current target of the enemy to they player found
                _inSiteRange = true;
            }
        }

        public void PlayerAttackDetect(Player player)
        {
            // remain na walang laman to unless umaatake ung critter
        }   

        void ResetPlayerIfNotInRange()
        {
            if (_inSiteRange)
            {
                _distanceFromPlayer = Vector3.Distance(_player.Position, transform.position);
                if (_siteRadius <= _distanceFromPlayer)
                {
                    _player = null;
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
            if (NavMesh.SamplePosition(_fleeDestination, out _navHit, _roamRadius, NavMesh.AllAreas))
            {
                // Set the agent's destination to the flee destination
                wildlifeEntity.SetDestination(_navHit.position);
                hasCalculatedFleeDestination = true;
            }
        }

        Vector3 CalculateFleeDestination // Next movement with zigzag 
        {
            get
            {
                // Calculate direction vector from the wildlife to the player
                Vector3 _directionToPlayer = _player.transform.position - transform.position;

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