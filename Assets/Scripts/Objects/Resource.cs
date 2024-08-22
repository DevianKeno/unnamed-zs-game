using System;
using System.Collections.Generic;

using MEC;
using UZSG.Attributes;
using UZSG.Data;
using UZSG.Interactions;
using UZSG.Saves;
using UZSG.Systems;
using UZSG.UI;

namespace UZSG.Objects
{
    public class Resource : BaseObject, ILookable
    {
        public ResourceData ResourceData => objectData as ResourceData;
        public LookableType LookableType => LookableType.Resource; 
        public bool ShowUI = true;

        bool _hasVisibleUI;
        float _uiLifeDuration = 3f;
        float _uiLifeTimer;
        ResourceHealthRingUI healthUI;

        protected SaveData saveData;
        
        /// On load on world
        protected override void Start()
        {
            base.Start();
            
            LoadDefaultAttributes();
        }
        
        protected virtual void LoadDefaultAttributes()
        {
            attributes = new();
            attributes.AddList(objectData.Attributes);
        }
    }
}