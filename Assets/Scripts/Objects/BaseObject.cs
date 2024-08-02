using UnityEngine;
using UZSG.Data;

namespace UZSG.Objects
{
    public class BaseObject : MonoBehaviour
    {
        [SerializeField] protected ObjectData objectData;
        public ObjectData Data => objectData;
    }
}