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


        #region Wild Life Movement and Fundamental Data
        public WildlifeActionStatesMachine WildlifeStateMachine => wildlifeStateMachine;
        Player _playerToFlee; // the player that the critter flees from
        bool _inSiteRange;
        float _distanceFromPlayer, _siteRadius;
        public Attributes.Attribute HealthAttri;
        WildlifeActionStates _actionState;
        [SerializeField] NavMeshAgent WildlifeEntity;
        [SerializeField] protected WildlifeActionStatesMachine wildlifeStateMachine;
        [SerializeField] float health;
        [SerializeField] float speed;


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
            ResetPlayerIfNotInRange();
        }

        protected virtual void FixedUpdate()
        {
            _actionState = HandleTransition;
            ExecuteAction(_actionState);
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
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
            WildlifeEntity.speed = attributes.Get("speed").Value;
            _siteRadius = attributes.Get("site_radius").Value;
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

        void ResetPlayerIfNotInRange()
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
            //Debug.Log("Roam");
        }

        void RunAway()
        {
            //Debug.Log("RunAway");
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