using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UZSG.WorldEvents.Raid
{
    public class HordeFormations : MonoBehaviour
    {
        public void SpawnFormation(RaidInstance raidInstance)
        {
            switch(raidInstance.raidFormation)
            {
                case RaidFormation.Blob:
                    SpawnAsBlob();
                    break;
                case RaidFormation.Line:
                    SpawnAsLine();
                    break;
                case RaidFormation.Waves:
                    SpawnInWaves();
                    break;
            }
        }

        void SpawnAsBlob()
        {
            
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
