using System;
using System.IO;
using UnityEngine;
using UnityEngine.AI;
using UZSG.Attributes;
using UZSG.Data;
using UZSG.Players;
using UZSG.Systems;
using UZSG.Interactions;
using System.Collections;

namespace UZSG.Entities
{
    public partial class NonPlayerCharacter : Entity
    {
        [SerializeField] Animator _animator;
        [SerializeField] GameObject _rigidBody;
        [SerializeField] BoxCollider _characterCollider;
        Collider[] _ragdollColliders;
        Rigidbody[] _limbsRigidBodies;

        #region NPC Ragdoll On/Off

        void GetRagdollComponents()
        {
            _ragdollColliders = _rigidBody.GetComponentsInChildren<BoxCollider>();
            _limbsRigidBodies = _rigidBody.GetComponentsInChildren<Rigidbody>();

            foreach (var collider in _ragdollColliders)
            {
                Debug.Log("collider" + collider);
            }

            foreach (var limbs in _limbsRigidBodies)
            {
                Debug.Log("collider" + limbs);
            }
        }

        protected void RagdollOn()
        {
            foreach(Collider col in _ragdollColliders)
            {
                col.enabled = false;
            }

            foreach(Rigidbody rigid in _limbsRigidBodies)
            {
                rigid.isKinematic = true;
            }

            _animator.enabled = false;
            _characterCollider.enabled = false;
            GetComponent<Rigidbody>().isKinematic = true;
        }

        protected void RagdollOff()
        {
            _animator.enabled = true;
            _characterCollider.enabled = true;
            GetComponent<Rigidbody>().isKinematic = false;
        }

        #endregion


    }
}