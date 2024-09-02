using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using MEC;

using UZSG.Systems;
using UZSG.Inventory;
using UZSG.Interactions;
using UZSG.Attributes;
using UZSG.Players;
using UZSG.FPP;
using UZSG.UI.HUD;
using UZSG.UI.Players;
using UZSG.Crafting;
using UZSG.StatusEffects;
using UZSG.Saves;
using UZSG.UI.Objects;
using UZSG.UI;

using static UZSG.Players.MoveStates;   

namespace UZSG.Entities
{
    /// <summary>
    /// Player entity.
    /// </summary>
    public partial class Player : IPlayerBeingDamage
    {
        [Header("Player Information")]
        [SerializeField] float currentHealth;
        public void DamagePlayer(float damageDealt)
        {
            currentHealth -= damageDealt;
        }
    }
}