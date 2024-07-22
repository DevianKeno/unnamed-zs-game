using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UZSG.Entities
{
    public enum HitboxPart {
        Head, Body, Arms, Legs
    }

    public class EntityHitboxController : MonoBehaviour
    {
        [SerializeField] List<Hitbox> hitboxes;
        public List<Hitbox> Hitboxes => hitboxes;


#if UNITY_EDITOR
        public void ReinitializeeHitboxes()
        {
            hitboxes = new();
            GetHitboxFromChildren(transform);
        }

        void GetHitboxFromChildren(Transform transform)
        {
            foreach (Transform child in transform)
            {
                if (child.TryGetComponent<Hitbox>(out var hitbox))
                {
                    hitbox.GetComponent<Collider>().isTrigger = true;
                    hitboxes.Add(hitbox);
                }
                GetHitboxFromChildren(child);
            }
        }
#endif
    }

}