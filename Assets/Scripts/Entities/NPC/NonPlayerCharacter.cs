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
    public partial class NonPlayerCharacter : Entity
    {


        #region NPC common data
        public Rigidbody rb;
        protected Player _player;
        protected string defaultPath;
        protected AttributeCollection attributes;
        protected AttributeCollection Attributes => attributes;
        protected string defaultsJson;

        #endregion
        



        #region NPC File Loading

        public override void OnSpawn()
        {
            base.OnSpawn();
            rb = GetComponent<Rigidbody>();
            defaultPath = entityDefaultsPath + $"{entityData.Id}_defaults.json";
            Initialize();
        }

        #endregion




        #region Agent Loading Default Data

        void Initialize() 
        {
            _player = null;
            LoadDefaults(); // read from JSON file the default enemy attributes
        }

        public virtual void LoadDefaults()
        {
            defaultsJson = File.ReadAllText(Application.dataPath + defaultPath);
        }

        #endregion


    }
}