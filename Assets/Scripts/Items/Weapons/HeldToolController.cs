using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

using UZSG.Systems;
using UZSG.Data;
using UZSG.Interactions;
using UZSG.Entities;
using UZSG.Players;
using UZSG.Attacks;
using MEC;

namespace UZSG.Items.Tools
{
    /// <summary>
    /// Controller for held Tools.
    /// </summary>
    public class HeldToolController : HeldItemController, ICollisionSource
    {
        public Player Player => owner as Player;
        public ToolData ToolData => ItemData as ToolData;
        
        bool _inhibitActions;
        bool _isAttacking;
        bool _onCooldown;

        public string CollisionTag => "Tool";
        ToolItemStateMachine stateMachine;
        public ToolItemStateMachine StateMachine => stateMachine;

        public bool IsBroken
        {
            get
            {
                return attributes["durability"].Value <= 0;
            }
        }

        /// <summary>
        /// Whether if the Owner (Player) can use (Attack) this tool.
        /// </summary>
        public bool CanUse
        {
            get
            {
                if (IsBroken) return false;
                if (_onCooldown) return false;

                if (owner.Attributes.TryGet("stamina", out var ownerStamina))
                {
                    if (owner.Attributes.TryGet("tool_stamina_cost_multiplier", out var multiplier))
                    {
                        return ownerStamina.Value >= (attributes["stamina_cost"].Value * multiplier.Value);
                    }
                    else
                    {
                        return ownerStamina.Value >= attributes["stamina_cost"].Value;
                    }
                }

                return false;
            }
        }

        #region Initializing methods

        void Awake()
        {
            stateMachine = GetComponent<ToolItemStateMachine>();
        }

        public override void Initialize()
        {
            LoadDefaultAttributes();
            InitializeAudioController();
            InitializeEventsFromOwnerInput();
        }

        void InitializeAudioController()
        {
            audioSourceController.CreateAudioPool(8); 
        }

        void InitializeEventsFromOwnerInput()
        {
            var inputs = Player.Actions.Inputs;

            inputs["Primary Action"].started += OnPlayerPrimary;
            inputs["Primary Action"].canceled += OnPlayerPrimary;

            inputs["Secondary Action"].started += OnPlayerSecondary;
            inputs["Secondary Action"].canceled += OnPlayerSecondary;
        }

        void LoadDefaultAttributes()
        {
            attributes = new();
            attributes.AddList(ToolData.Attributes);
        }

        void RetrieveToolAttributes()
        {
            
        }

        #endregion


        #region Player input callbacks

        void OnPlayerPrimary(InputAction.CallbackContext context)
        {
            if (!gameObject.activeSelf) return;
            
            if (context.started)
            {
                if (CanUse)
                {
                    StartCoroutine(AttackCoroutine());
                }
                if (IsBroken)
                {
                    /// prompt user broken Item
                }
            }
            else if (context.canceled)
            {
                
            }
        }

        void OnPlayerSecondary(InputAction.CallbackContext context)
        {
            
        }

        #endregion

        IEnumerator AttackCoroutine()
        {
            if (_inhibitActions || _isAttacking)
            {
                yield break;
            }
            
            _inhibitActions = true;
            _isAttacking = true;

            ConsumeStamina();
            // PlaySound();
            var atkParams = GetAttackParams();
            CreateAttackRays(ref atkParams);
            stateMachine.ToState(ToolItemStates.Attack);
            yield return new WaitForSeconds(0.5f); /// SOMETHING ATKSPD THOUGH NOT SO STRAIGHFROWARDS LOTS OF CALCS (short for calculations)
            
            _inhibitActions = false;
            _isAttacking = false;
        }

        void ConsumeStamina()
        {
            owner.Attributes["stamina"].Remove(attributes["stamina_cost"].Value);
        }

        void PlaySound()
        {
            audioSourceController.PlaySound("swing1");
        }

        IEnumerator<float> CooldownAttack()
        {
            _onCooldown = true;

            float cdSeconds = 0f;
            if (attributes.TryGet("attack_speed", out var atkSpd))
            {
                cdSeconds = 1f / atkSpd.Value; 
            }
            yield return Timing.WaitForSeconds(cdSeconds);
            _onCooldown = false;
        }
        
        MeleeAttackParameters GetAttackParams()
        {
            #region TODO: Implement combos
            #endregion
            var data = ToolData.Attacks[0];
            return MeleeAttacks.Parameters(data);
        }      
        
        void CreateAttackRays(ref MeleeAttackParameters atk)
        {
            Timing.RunCoroutine(CreateRaycastRays(atk));
            
            atk.Origin = Player.EyeLevel;
            atk.Up = Player.Up;
            atk.Direction = Player.Forward;

            if (atk.SwingType == MeleeSwingType.Raycast)
            {
                MeleeAttacks.Raycast(ref atk, OnToolAttackHit);
            }
            else if (atk.SwingType == MeleeSwingType.Swingcast)
            {
                MeleeAttacks.Swingcast(ref atk, OnToolAttackHit);
            }
        }

        IEnumerator<float> CreateRaycastRays(MeleeAttackParameters atk)
        {
            Color rayColor = Color.yellow;

            yield return Timing.WaitForSeconds(atk.Delay);

            if (Physics.Raycast(Player.EyeLevel, Player.Forward, out RaycastHit hit, atk.Range, atk.Layer))
            {
                var target = hit.collider.GetComponentInParent<ICollisionTarget>();
                if (target != null)
                {
                    OnToolAttackHit(new()
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
                Debug.DrawRay(atk.Origin, atk.Direction * atk.Range, rayColor, 1.0f);
            }

            yield break;
        }
        
        void OnToolAttackHit(HitboxCollisionInfo info)
        {
            info.Source = this;
            info.Target.HitBy(info);
        }

        public override void SetStateFromAction(ActionStates state)
        {
            
        }
    }
}