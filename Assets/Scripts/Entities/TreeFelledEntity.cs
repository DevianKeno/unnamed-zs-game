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
        bool _enableUpdate = false;
        bool _hadCrashedOnce = false;
        /// <summary>
        /// Audio clip when the tree log is rolling on the ground
        /// </summary>
        AudioSource rollingAudio;
        [SerializeField] Rigidbody leavesRigidbody;
        [SerializeField] Rigidbody rb;
        public Rigidbody Rigidbody => rb;

        public override void OnSpawnEvent()
        {
            base.OnSpawnEvent();
            
            rollingAudio = Game.Audio.Create3D("tree_log_rolling", rb.centerOfMass);
            rollingAudio.transform.SetParent(this.transform, worldPositionStays: false);
        }

        void FixedUpdate()
        {
            if (!_enableUpdate) return;
            
            HandleAudio();
        }

        void HandleAudio()
        {
            /// play rolling sound when the log is moving
            if (rb.velocity.magnitude > 0.2f)
            {
                if (!rollingAudio.isPlaying)
                {
                    rollingAudio.Play();
                }
            }
            else
            {
                rollingAudio.Stop();
            }
        }

        public void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.layer == Game.World.GROUND_LAYER)
            {
                if (!_hadCrashedOnce)
                {
                    _hadCrashedOnce = true;
                    Game.Audio.Play3D("tree_crash", rb.position);
                    Game.Audio.Play3D("leaves_rustle", rb.centerOfMass);
                    leavesRigidbody.isKinematic = false; /// makes leaves fall down
                    Destroy(leavesRigidbody.gameObject, 3f);
                }
                _enableUpdate = true;
            }
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