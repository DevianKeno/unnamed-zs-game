using System;

using UnityEngine;

namespace UZSG.Masteries
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Mastery Data", menuName = "UZSG/Mastery Data")]
    public class MasteryData : ScriptableObject
    {
        public string Id;
        public string Name;
        public string Description;
        public bool IsLeveled;
        [HideInInspector] public int MinLevel;
        [HideInInspector] public int MaxLevel;
        public bool IsFinal;
    }
}
