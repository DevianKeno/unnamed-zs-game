using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UZSG.WorldEvents.Raid
{
    public class RaidInstanceHandler : MonoBehaviour
    {
        HordeFormations hordeFormations;
        public HordeFormations HordeFormations 
        {
            get => hordeFormations;
            set => hordeFormations = value;
        }
        
    }
}