using System;
using UnityEngine;
using UZSG.Data;
using UZSG.Systems;

namespace UZSG.Entities
{
    /// <summary>
    /// Represent dynamic objects that appear in the World.
    /// </summary>
    public abstract class Enemy : Entity
    {
        [SerializeField] protected EnemyData enemyData;
        public EnemyData EnemyData => enemyData;
    }
}