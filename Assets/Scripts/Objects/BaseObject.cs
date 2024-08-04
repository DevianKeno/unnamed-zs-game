using UnityEngine;
using UZSG.Data;

namespace UZSG.Objects
{
    public abstract class BaseObject : MonoBehaviour
    {
        [SerializeField] protected ObjectData objectData;
        public ObjectData ObjectData => objectData;
    }
}