using System;
using UnityEngine;

using UZSG.Entities;
using UZSG.Saves;

namespace UZSG.Objects
{
    /// <summary>
    /// The falling tree log after it has been cut.
    /// </summary>
    public class TreeFelledEntity : Entity
    {
        [SerializeField] GameObject leaves;
        [SerializeField] Rigidbody rb;
        public Rigidbody Rigidbody => rb;

        public override void OnSpawnEvent()
        {
            base.OnSpawnEvent();
            Game.Audio.PlayInWorld("leaves_rustle", Position);
            Destroy(leaves, 3f);
        }
        
        public override void ReadSaveData(EntitySaveData saveData)
        {
            base.ReadSaveData(saveData);
            rb.velocity = Utils.ToUnityVector3(saveData.GetJArray<float>("velocity"));
        }

        public override EntitySaveData WriteSaveData()
        {
            var sd = base.WriteSaveData();
            sd.AddArray<float>("velocity", Utils.ToFloatArray(rb.velocity));
            return sd;
        }

        protected override void ReadTransform(TransformSaveData saveData)
        {
            rb.position = Utils.FromFloatArray(saveData.Position);
            rb.rotation = Utils.ToUnityQuat(euler: saveData.Rotation);
            transform.localScale = Utils.FromFloatArray(saveData.LocalScale);
        }

        protected override TransformSaveData WriteTransform()
        {
            return new TransformSaveData()
            {
                Position = Utils.ToFloatArray(rb.position),
                Rotation = Utils.ToFloatArray(rb.rotation.eulerAngles),
                LocalScale = Utils.ToFloatArray(transform.localScale),
            };
        }
    }
}