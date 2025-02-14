using UnityEngine;

using UZSG.Entities;
using UZSG.Saves;

namespace UZSG.Objects
{
    public class TreeEntity : Entity
    {
        [SerializeField] Rigidbody rb;
        public Rigidbody Rigidbody => rb;

        public override void ReadSaveData(EntitySaveData saveData)
        {
            base.ReadSaveData(saveData);
            rb.velocity = Utils.ToUnityVector3(saveData.GetEntry<float[]>("velocity"));
        }

        public override EntitySaveData WriteSaveData()
        {
            var sd = base.WriteSaveData();
            sd.AddEntry("velocity", Utils.ToFloatArray(rb.velocity));
            return sd;
        }

        protected override void ReadTransform(TransformSaveData saveData)
        {
            rb.position = Utils.FromFloatArray(saveData.Position);
            rb.rotation = Utils.ToUnityQuat(euler: saveData.Rotation);
        }

        protected override TransformSaveData WriteTransform()
        {
            return new TransformSaveData()
            {
                Position = Utils.ToFloatArray(rb.position),
                Rotation = Utils.ToFloatArray(rb.rotation.eulerAngles),
            };
        }
    }
}