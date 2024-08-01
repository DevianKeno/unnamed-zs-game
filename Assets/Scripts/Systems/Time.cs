using UnityEngine;

namespace UZSG.Systems
{
    public class TimeManager : MonoBehaviour
    {
        [SerializeField] float timeScale; 
        public float TimeScale => timeScale;
    }
}