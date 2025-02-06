using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UZSG;
using UZSG.Entities;


public class PlayerReference : MonoBehaviour
{
    public Player PlayerEntity;

    private void Start()
    {
        Game.Entity.OnEntitySpawned += OnSpawn;
    }

    private void OnSpawn(EntityManager.EntityInfo info)
    {
        if(info.Entity is Player player)
        {
            PlayerEntity = player;
        }
    }
}
