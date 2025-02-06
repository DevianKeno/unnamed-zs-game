using UnityEngine;


using UZSG.Attributes;
using UZSG.Data.Perks;
using UZSG.Entities;
using System;

namespace UZSG.Perks
{
    public class VitalityStrike : PerkHandler
    {
        Player player;
        int _perkLevel = 1;

        void Initialize()
        {
            if (entity is Player p)
            {
                player = p;
            }

            /// sample event
            // player.FPP.OnBeforeAttack??? += OnBeforeAttack;
        }

        void OnBeforeAttack()
        {
            if (player.Attributes["stamina"].IsFull)
            {

            }
        }
    }
}