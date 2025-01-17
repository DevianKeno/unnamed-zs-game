using UnityEngine;

namespace UZSG.UI
{
    [CreateAssetMenu(fileName = "Static Gradient", menuName = "UZSG/Static/Gradient")]
    public class StaticGradient : ScriptableObject
    {
        public Gradient Gradient;
    }
}