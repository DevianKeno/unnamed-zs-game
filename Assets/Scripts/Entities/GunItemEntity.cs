using UnityEngine.Serialization;

namespace UZSG.Entities
{
    public class GunItemEntity : ItemEntity
    {
        [FormerlySerializedAs("Info")] public GunItemEntityInfo GunInfo;
    }
}