using UnityEngine;
using UnityEngine.AI;

using UZSG.Systems;
using UZSG.Interactions;
using UZSG.Attributes;

namespace UZSG.Entities
{
    public class Wildlife : NonPlayerCharacter, IPlayerDetectable
    { 
        #region Wild Life Fundamental Data

        bool hasCalculatedFleeDestination;
        [SerializeField] bool _inSiteRange;
        float _distanceFromPlayer,
            _siteRadius,
            _roamInterval,
            _roamRadius,
            RoamTime,
            fleeDistance;
        Vector3 _randomDestination,
            _fleeDestination;
        // variables that determine wildlife state
        NavMeshHit _navHit;
        // used for roaming and the fleeing state of the wildlife

        public float PlayerDetectionRadius
        {
            get
            {
                return attributes["player_detection_radius"].Value;
            }
        }

        [Header("Components")]
        [SerializeField] NavMeshAgent navMeshAgent;
        WildlifeActionStates _currentActionState;
        [SerializeField] protected WildlifeActionStatesMachine wildlifeStateMachine;
        public WildlifeActionStatesMachine WildlifeStateMachine => wildlifeStateMachine;

        #endregion


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
            navMeshAgent.speed = attributes.Get("move_speed").Value;

            _siteRadius = attributes.Get("wildlife_site_radius").Value;
            _roamInterval = attributes.Get("wildlife_roam_interval").Value;
            _roamRadius = attributes.Get("wildlife_roam_radius").Value;
        }

        #endregion

        protected virtual void FixedUpdate()
        {
            _currentActionState = HandleTransition();
            ExecuteAction(_currentActionState);
        }

        void OnSecond(SecondInfo info)
        {
            ResetPlayerIfNotInRange();
        }


        #region Sensor

        public void DetectPlayer(Entity etty)
        {
            if (etty != null && etty is Player player)
            {
                targetEntity = player; // set the current target of the enemy to they player found
                _inSiteRange = true;
            }
        }

        public void PlayerAttackDetect(Entity etty)
        {
            // remain na walang laman to unless umaatake ung critter
        }   

        void ResetPlayerIfNotInRange()
        {
            if (_inSiteRange)
            {
                _distanceFromPlayer = Vector3.Distance(targetEntity.Position, transform.position);
                if (_siteRadius <= _distanceFromPlayer)
                {
                    targetEntity = null;
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

        public WildlifeActionStates HandleTransition() // "sense" what's the state of the animal 
        {
            if (IsDead)
            {
                return WildlifeActionStates.Die;
            }

            if (IsInSiteRange)
                return WildlifeActionStates.RunAway;
            else
                return WildlifeActionStates.Roam;
        }

        #endregion


        #region Actuator

        void Die()
        {
            WildlifeStateMachine.ToState(WildlifeActionStates.Die);
            Game.Tick.OnSecond -= OnSecond;
            Game.Entity.Kill(this);
            Debug.Log("Die");
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
                navMeshAgent.SetDestination(navHit.position);
                RoamTime = UnityEngine.Random.Range(1.0f, _roamInterval); // Reset RoamTime for the next movement
                navMeshAgent.updateRotation = true;
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
                navMeshAgent.SetDestination(_navHit.position);
                hasCalculatedFleeDestination = true;
            }
        }

        Vector3 CalculateFleeDestination // Next movement with zigzag 
        {
            get
            {
                // Calculate direction vector from the wildlife to the player
                Vector3 _directionToPlayer = targetEntity.transform.position - transform.position;

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