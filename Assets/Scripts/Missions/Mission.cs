using System;

using UnityEngine;

using UZSG.Data;

namespace UZSG.Missions
{
    public class Mission
    {
        MissionData missionData;
        public MissionData Data => missionData;
        
        bool isCompleted = false;
        public event Action OnCompleted;

        public void SetCompleted()
        {
            isCompleted = true;
            OnCompleted?.Invoke();
        }
    }
    
    [Serializable]
    [CreateAssetMenu(fileName = "New Mission Data", menuName = "UZSG/Mission Data")]
    public class MissionData : BaseData
    {
        public string DisplayName;
        [TextArea] public string Description;

    }
}