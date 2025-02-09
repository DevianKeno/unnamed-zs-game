using UnityEngine;
using UnityEngine.AI;

using UZSG.Attributes;
using UZSG.Interactions;

namespace UZSG.Entities
{
    public class Wildlife : Entity, IPlayerDetectable, IHasHealthBar, IDamageable
    { 
        [SerializeField] protected Entity targetEntity = null;

        [SerializeField] float roamIntervalSeconds = 20f;
        [SerializeField] float roamRadius = 20f;

        bool _inSiteRangeOfPlayer;
        bool _hasCalculatedFleeDestination;
        float _distanceFromPlayer;
        float _playerVisionRange;
        float _roamTimer;
        float _fleeDistance;
        Vector3 _randomDestination;
        Vector3 _fleeDestination;
        // variables that determine wildlife state
        NavMeshHit _navHit;
        // used for roaming and the fleeing state of the wildlife

        public bool IsAlive { get; protected set; }
        public bool HasHitboxes { get; protected set; }
        public float MoveSpeed
        {
            get => NavMeshAgent.speed;
            set => NavMeshAgent.speed = value;
        }
        public float PlayerVisionRange => _playerVisionRange;

        [field: Header("Wildlife Components")]
        public Rigidbody Rigidbody { get; private set; }
        public WildlifeActionStateMachine ActionStateMachine { get; private set; }
        public EntityHitboxController HitboxController { get; private set; }
        public NavMeshAgent NavMeshAgent { get; private set; }
        public EntityHealthBar HealthBar { get; private set; }


        #region Initializing methods
        
        protected virtual void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            HealthBar = GetComponentInChildren<EntityHealthBar>();
            ActionStateMachine = GetComponent<WildlifeActionStateMachine>();
            NavMeshAgent = GetComponent<NavMeshAgent>();
        }

        public override void OnSpawn()
        {
            base.OnSpawn();

            RetrieveAttributes();
            InitializeHitboxes();
            // LoadDefaultSaveData<EntitySaveData>();
            // ReadSaveData(saveData);
            IsAlive = true;

            Game.Tick.OnSecond += OnSecond;
        }

        protected override void OnDespawn()
        {
            Game.Tick.OnSecond -= OnSecond;
        }

        public virtual void Kill()
        {
            NavMeshAgent.ResetPath();
            IsAlive = false;
            ActionStateMachine.ToState(WildlifeActionStates.Die);
            
            Invoke(nameof(Despawn), 5f);
        }

        /// <summary>
        /// Retrieve attribute values and cache
        /// </summary>
        void RetrieveAttributes()
        {
            if (Attributes.TryGet("health", out var health))
            {
                health.OnReachZero += OnHealthReachedZero;
            }
            if (Attributes.TryGet("move_speed", out var moveSpeed))
            {
                this.MoveSpeed = moveSpeed.Value;
                moveSpeed.OnValueChanged += (attr, context) => /// listen to changes in move speed ig
                {
                    this.MoveSpeed = attr.Value;
                };
            }
            if (Attributes.TryGet("player_vision_range", out var pvr))
            {
                this._playerVisionRange = pvr.Value;
            }
        }

        void InitializeHitboxes()
        {
            HitboxController = GetComponent<EntityHitboxController>();
            if (HitboxController != null)
            {
                HasHitboxes = true;
                HitboxController.ReinitializeHitboxes();

                foreach (var hitbox in HitboxController.Hitboxes)
                {
                    hitbox.OnCollision += OnHitboxCollision;
                }
            }
        }

        /// <summary>
        /// Called whenever a Hitbox of this character is hit by anything that is an ICollisionSource.
        /// </summary>
        protected virtual void OnHitboxCollision(object sender, HitboxCollisionInfo info) { }
        public virtual void TakeDamage(DamageInfo info) { }
        
        #endregion


        #region Event callbacks

        protected virtual void FixedUpdate()
        {
            ExecuteAction(HandleTransition());
        }

        void OnSecond(SecondInfo info)
        {
            ResetPlayerIfNotInRange();
        }

        void OnHealthReachedZero(object sender, AttributeValueChangedContext context)
        {
            if (context.ValueChangedType == UZSG.Attributes.Attribute.ValueChangeType.Decreased)
            {
                Kill();
            }
        }
        
        #endregion

        
        #region Sensor

        public void NotifyDetection(Player player)
        {
            if (player == null) return;
            
            targetEntity = player;
            _inSiteRangeOfPlayer = true;
        }

        void ResetPlayerIfNotInRange()
        {
            if (_inSiteRangeOfPlayer)
            {
                _distanceFromPlayer = Vector3.Distance(targetEntity.Position, transform.position);
                if (_playerVisionRange <= _distanceFromPlayer)
                {
                    targetEntity = null;
                    _inSiteRangeOfPlayer = false;
                    ActionStateMachine.ToState(WildlifeActionStates.Roam);
                }
            }
        }

        public WildlifeActionStates HandleTransition() // "sense" what's the state of the animal 
        {
            if (!IsAlive)
            {
                return WildlifeActionStates.Die;
            }

            if (_inSiteRangeOfPlayer)
            {
                return WildlifeActionStates.Flee;
            }
            else
            {
                return WildlifeActionStates.Roam;
            }
        }

        #endregion


        #region Actuator

        public void ExecuteAction(WildlifeActionStates action) // execute an action depending on what state the entity is on
        {
            switch (action)
            {
                case WildlifeActionStates.Roam:
                    ActionRoam();
                    break;
                case WildlifeActionStates.Flee:
                    ActionFlee();
                    break;
            }
        }

        void ActionRoam()
        {
            ActionStateMachine.ToState(WildlifeActionStates.Roam);
            _hasCalculatedFleeDestination = false;

            // Continue moving toward the destination
            _roamTimer -= Time.deltaTime;
            if (_roamTimer <= 0)
            {
                // Get a random position
                _randomDestination = UnityEngine.Random.insideUnitSphere * roamRadius;
                _randomDestination += transform.position;
                NavMesh.SamplePosition(_randomDestination, out NavMeshHit navHit, roamRadius, NavMesh.AllAreas);

                // Set the agent's destination to the random point
                NavMeshAgent.SetDestination(navHit.position);
                _roamTimer = UnityEngine.Random.Range(1.0f, roamIntervalSeconds); // Reset RoamTime for the next movement
                NavMeshAgent.updateRotation = true;
            }
        }

        void ActionFlee()
        {
            ActionStateMachine.ToState(WildlifeActionStates.Flee);

            // set rigid body to dynamic
            Rigidbody.isKinematic = false;

            if (!_hasCalculatedFleeDestination)
            {
                _fleeDestination = CalculateNextFleeDestination();
            }

            // Run away from player with occasional zigzag
            if (NavMesh.SamplePosition(_fleeDestination, out _navHit, roamRadius, NavMesh.AllAreas))
            {
                // Set the agent's destination to the flee destination
                NavMeshAgent.SetDestination(_navHit.position);
                _hasCalculatedFleeDestination = true;
            }
        }

        #endregion


        Vector3 CalculateNextFleeDestination() // Next movement with zigzag 
        {
            /// Calculate direction vector from the wildlife to the player
            Vector3 directionToPlayer = targetEntity.transform.position - transform.position;
            /// Normalize the direction to get a unit vector
            Vector3 _fleeDirection = -directionToPlayer.normalized;
            /// Zigzag by alternating between left and right
            Vector3 _zigzagDirection;
            if (UnityEngine.Random.value > 0.5f)
            {
                _zigzagDirection = Vector3.Cross(_fleeDirection, Vector3.up).normalized;
            }
            else
            {
                _zigzagDirection = -Vector3.Cross(_fleeDirection, Vector3.up).normalized;
            }

            /// New destination
            _fleeDistance = 20.0f;
            Vector3 _fleeDestination = transform.position + (_fleeDirection + _zigzagDirection) * _fleeDistance;
            return _fleeDestination;
        }
    }
}