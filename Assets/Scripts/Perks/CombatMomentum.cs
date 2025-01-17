using UnityEngine;

using UZSG.Systems;
using UZSG.Attributes;
using UZSG.Data.Perks;
using UZSG.Entities;
using System;

namespace UZSG.Perks
{
    public class CombatMomentum : PerkHandler
    {
        Player player;
        int _stacks = 0;
        int _perkLevel = 1;

        void Initialize()
        {
            if (entity is Player p)
            {
                player = p;
            }

            /// sample event
            // player.FPP.OnSuccessfulAttack??? += OnAttackHit;
        }

        void OnAttackHit()
        {
            _stacks += 1;
            Refresh();
        }

        public void Refresh()
        {
            if (entity is not IAttributable actor) return;
            
            if (actor.Attributes.TryGet("attack_speed", out var attackSpeed))
            {
                var oldAtkSpeed = attackSpeed.Value;
                oldAtkSpeed = _stacks * (1 + 0.10f * _perkLevel);
            }
            else
            {
                Game.Console.LogInfo($"Failed to activate '{perkData.Id}' as actor {entity.EntityData.Id} does not have the '{perkData.Id}' Attribute.");
            }
        }
    }
}