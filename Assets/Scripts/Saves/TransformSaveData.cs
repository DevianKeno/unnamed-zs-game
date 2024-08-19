using UnityEngine;
using MessagePack;

namespace UZSG.Saves
{
    public class TransformSaveData
    {
        [Key(0)]
        public Vector3 Position;
        [Key(1)]
        public Quaternion Rotation;
        [Key(2)]
        public Vector3 LocalScale;
    }
}