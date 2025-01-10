using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using MEC;

using UZSG.Data;
using UZSG.Interactions;
using UZSG.Entities;

namespace UZSG.Attacks
{
    public class MeleeAttacks : MonoBehaviour
    {
        const float DebugRayDuration = 1f;

        public static MeleeAttackParameters DefaultRaycast = new()
        {
            CastType = CastType.Raycast,
            Range = 3f,
            Duration = 0f,
            Delay = 0f,
            Layer = 0,
            RayCount = 1,
            AngleWidth = 0,
            RotationOffset = Vector3.zero,
            Visualize = false,
        };

        public static MeleeAttackParameters FromParametersData(MeleeAttackParametersData data)
        {
            return new()
            {
                CastType = data.SwingType,
                Range = data.Range,
                Duration = data.Duration,
                Delay = data.Delay,
                Layer = data.Layer,
                RayCount = data.RayCount,
                AngleWidth = data.AngleWidth,
                Flip = data.Flip,
                RotationOffset = data.RotationOffset,
                Visualize = data.Visualize,
                IncludeRotationOffset = data.IncludeRotationOffset,
            };
        }

        public delegate void OnAttackHit(HitboxCollisionInfo info);

        public static void Raycast(IMeleeWeaponActor actor, ref MeleeAttackParameters atkParams, OnAttackHit callback = null)
        {
            Timing.RunCoroutine(CreateRaycastRays(actor, atkParams, callback));
        }
        
        public static void Swingcast(IMeleeWeaponActor actor, ref MeleeAttackParameters atkParams, OnAttackHit callback = null)
        {
            Timing.RunCoroutine(CreateSwingcastRays(actor, atkParams, callback));
        }

        static IEnumerator<float> CreateRaycastRays(IMeleeWeaponActor actor, MeleeAttackParameters atk, OnAttackHit callback)
        {
            Color rayColor = Color.yellow;

            yield return Timing.WaitForSeconds(atk.Delay);

            var offset = Quaternion.AngleAxis(atk.RotationOffset.y, actor.Up) *
                Quaternion.AngleAxis(atk.RotationOffset.x, actor.Right) *
                Quaternion.AngleAxis(atk.RotationOffset.z, actor.Forward);
            Vector3 rayDirection = offset * actor.Forward;

            if (Physics.Raycast(actor.EyeLevel, rayDirection, out RaycastHit hit, atk.Range, atk.Layer))
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
                Debug.DrawRay(actor.EyeLevel, rayDirection * atk.Range, rayColor, DebugRayDuration);
            }

            yield break;
        }
        
        static IEnumerator<float> CreateSwingcastRays(IMeleeWeaponActor actor, MeleeAttackParameters atk, OnAttackHit callback)
        {
            float angleStep = atk.AngleWidth / (atk.RayCount - 1);
            float halfAngleWidth = atk.AngleWidth / 2f;
            HashSet<int> hitObjects = new();

            yield return Timing.WaitForSeconds(atk.Delay);
            
            if (atk.Duration <= 0)
            {
                SwingcastInstant(actor, atk, callback, hitObjects, 0, angleStep, halfAngleWidth);
                yield break;
            }
            else
            {
                float elapsedTime = 0f;
                int rayIndex = 0;
                while (rayIndex < atk.RayCount)
                {
                    float timePerRay = atk.Duration / atk.RayCount;
                    float raysPerFrame = Time.deltaTime / timePerRay;
                    int raysToFireThisFrame = Mathf.CeilToInt(raysPerFrame);

                    for (int i = 0; i < raysToFireThisFrame && rayIndex < atk.RayCount; i++, rayIndex++)
                    {
                        CastRay(actor, atk, callback, ref hitObjects, rayIndex, angleStep, halfAngleWidth);
                    }

                    elapsedTime += Time.deltaTime;
                    if (elapsedTime >= atk.Duration) break;
                    yield return Timing.WaitForSeconds(Time.deltaTime);
                }
            }
        }

        static void SwingcastInstant(IMeleeWeaponActor actor, MeleeAttackParameters atk, OnAttackHit callback, HashSet<int> hitObjects, int rayIndex, float angleStep, float halfAngleWidth)
        {
            for (rayIndex = 0; rayIndex < atk.RayCount; rayIndex++)
            {
                CastRay(actor, atk, callback, ref hitObjects, rayIndex, angleStep, halfAngleWidth);
            }
        }

        static void CastRay(IMeleeWeaponActor actor, MeleeAttackParameters atk, OnAttackHit callback, ref HashSet<int> hitObjects, int rayIndex, float angleStep, float halfAngleWidth)
        {
            bool rayHadHit = false;
            float angle = atk.Flip
                ? halfAngleWidth - (rayIndex * angleStep) // else
                : (rayIndex * angleStep) - halfAngleWidth;
            
            Quaternion angleOffset = Quaternion.Euler(actor.Up * angle);
            if (atk.IncludeRotationOffset)
            {
                /// Rotate along local rotation
                var offset = Quaternion.AngleAxis(atk.RotationOffset.x, actor.Right) *
                    Quaternion.AngleAxis(atk.RotationOffset.y, actor.Up) *
                    Quaternion.AngleAxis(atk.RotationOffset.z, actor.Forward);
                angleOffset *= offset;
            }
            Vector3 rayDirection = angleOffset * actor.Forward;

            if (Physics.Raycast(actor.EyeLevel, rayDirection, out RaycastHit hit, atk.Range, atk.Layer))
            {
                int hitTargetId = hit.collider.GetInstanceID();
                var target = hit.collider.GetComponentInParent<ICollisionTarget>();
                if (target != null && !hitObjects.Contains(hitTargetId))
                {
                    hitObjects.Add(hitTargetId);
                    rayHadHit = true;
                    callback?.Invoke(new HitboxCollisionInfo()
                    {
                        CollisionType = HitboxCollisionType.Attack,
                        Target = target,
                        Collider = hit.collider,
                        ContactPoint = hit.point,
                    });
                }
            }

            if (atk.Visualize)
            {
                Color rayColor = rayHadHit ? Color.red : Color.white;
                Debug.DrawRay(actor.EyeLevel, rayDirection * atk.Range, rayColor, DebugRayDuration);
            }
        }
    }
}