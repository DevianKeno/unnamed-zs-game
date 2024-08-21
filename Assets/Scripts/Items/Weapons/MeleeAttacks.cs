using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using MEC;

using UZSG.Data;
using UZSG.Interactions;

namespace UZSG.Attacks
{
    public class MeleeAttacks : MonoBehaviour
    {
        const float DebugRayDuration = 1f;

        public static MeleeAttackParameters Parameters(MeleeAttackParametersData data)
        {
            return new()
            {
                SwingType = data.SwingType,
                Range = data.Range,
                Duration = data.Duration,
                Delay = data.Delay,
                Layer = data.Layer,
                RayCount = data.RayCount,
                AngleWidth = data.AngleWidth,
                RotationOffset = data.RotationOffset,
                Visualize = data.Visualize,
            };
        }

        public delegate void OnAttackHit(HitboxCollisionInfo info);

        public static void Raycast(ref MeleeAttackParameters atkParams, OnAttackHit callback = null)
        {
            Timing.RunCoroutine(CreateRaycastRays(atkParams, callback));
        }
        
        public static void Swingcast(ref MeleeAttackParameters atkParams, OnAttackHit callback = null)
        {
            Timing.RunCoroutine(CreateSwingcastRays(atkParams, callback));
        }

        static IEnumerator<float> CreateRaycastRays(MeleeAttackParameters atk, OnAttackHit callback)
        {
            Color rayColor = Color.yellow;

            yield return Timing.WaitForSeconds(atk.Delay);

            if (Physics.Raycast(atk.Origin, atk.Direction, out RaycastHit hit, atk.Range, atk.Layer))
            {
                var target = hit.collider.GetComponentInParent<ICollisionTarget>();
                if (target != null)
                {
                    callback?.Invoke(new()
                    {
                        Target = target,
                        Collider = hit.collider,
                        ContactPoint = hit.point,
                    });
                    rayColor = Color.red;
                }
            }

            if (atk.Visualize)
            {
                Debug.DrawRay(atk.Origin, atk.Direction * atk.Range, rayColor, DebugRayDuration);
            }

            yield break;
        }
        
        static IEnumerator<float> CreateSwingcastRays(MeleeAttackParameters atk, OnAttackHit callback)
        {
            float angleStep = atk.AngleWidth / (atk.RayCount - 1);
            float halfAngleWidth = atk.AngleWidth / 2f;
            HashSet<int> hitObjects = new();

            yield return Timing.WaitForSeconds(atk.Delay);

            for (int i = 0; i < atk.RayCount; i++)
            {
                Color rayColor = Color.white;
                
                float currentAngle = (i * angleStep) - halfAngleWidth;
                Quaternion angleOffset = Quaternion.Euler(atk.Up * currentAngle);
                Vector3 rayDirection = angleOffset * atk.Direction;
                
                if (Physics.Raycast(atk.Origin, rayDirection, out RaycastHit hit, atk.Range, atk.Layer))
                {
                    int hitTargetId = hit.collider.GetInstanceID();
                    var target = hit.collider.GetComponentInParent<ICollisionTarget>();
                    if (target != null && !hitObjects.Contains(hitTargetId))
                    {
                        /// Record hit target via its InstanceId to avoid being hit again on the same attack
                        hitObjects.Add(hitTargetId);
                        callback?.Invoke(new()
                        {
                            Target = target,
                            Collider = hit.collider,
                            ContactPoint = hit.point,
                        });

                        rayColor = Color.red;
                    }
                }

                if (atk.Visualize)
                {
                    Debug.DrawRay(atk.Origin, rayDirection * atk.Range, rayColor, DebugRayDuration);
                }

                if (atk.Duration > 0)
                {
                    yield return Timing.WaitForSeconds(atk.Duration / atk.RayCount);
                }
            }
        }
    }
}