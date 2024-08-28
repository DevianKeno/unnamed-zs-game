using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UZSG.Entities;
using UZSG.Systems;


namespace UZSG.WorldEvents.Raid
{
    public class HordeFormations : MonoBehaviour
    {
        public GameObject hordeParent;
        public void SpawnFormation(RaidInstance raidInstance)
        {
            switch(raidInstance.raidFormation)
            {
                case RaidFormation.Blob:
                    SpawnAsBlob(raidInstance);
                    break;
                case RaidFormation.Line:
                    SpawnAsLine();
                    break;
                case RaidFormation.Waves:
                    SpawnInWaves();
                    break;
            }
        }

        void SpawnAsBlob(RaidInstance raidInstance)
        {
            for (int i=0; i<raidInstance.mobCount; i++)
            {
                Game.Entity.Spawn<Skinwalker>("skinwalker", callback: (info) => {
                    info.Entity.transform.SetParent(hordeParent.transform);
                });
            }
        }

        void SpawnAsLine()
        {
            
        }
        void SpawnInWaves()
        {
            throw new System.NotImplementedException();
        }
    }
}
