using UnityEngine;

namespace UZSG.StatusEffects
{
    [CreateAssetMenu(fileName = "Status Effect", menuName = "UZSG/Status Effect")]
    public class StatusEffectData : ScriptableObject
    {
        public string Id;
        public string Name;
        public string Description;
    }
}