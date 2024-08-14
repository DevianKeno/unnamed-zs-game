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
    public class WildLife : Entity
    { 


        #region Wild Life Movement Data
        [SerializeField] NavMeshAgent wildLifeEntity;
        [SerializeField] protected WildLifeActionStatesMachine wildLifeStateMachine;


        #endregion


        #region Wild Life Data

        public WildLifeData WildlifeData => entityData as WildLifeData;
        public string defaultPath; // Default file path of the specific enemy
        public WildLifeSaveData defaultData;
        [SerializeField] protected AttributeCollection attributes;
        public AttributeCollection Attributes => attributes;

        #endregion        


        #region Start/Update/Initialize

        protected virtual void Start()
        {

        }

        protected virtual void LateUpdate()
        {
            
        }

        protected virtual void FixedUpdate()
        {
            
        }

        public override void OnSpawn()
        {
            Initialize();
        }

        void Initialize()
        {
            InitializeAttribute();
        }

        void InitializeAttribute()
        {
            
        }

        #endregion



        #region Sensor



        #endregion



        #region Actuator



        #endregion


    }
}