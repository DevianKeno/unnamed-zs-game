using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using UZSG.Systems;
using UZSG.Interactions;

namespace UZSG.Entities
{
    public class Skinwalker : Enemy
    {

        protected override void Start()
        {
            base.Start();
            OnSpawn();
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
            
            InitializeHitboxEvents();
        }
    }
}