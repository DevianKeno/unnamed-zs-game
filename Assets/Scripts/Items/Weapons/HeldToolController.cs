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
using System.Linq;

namespace UZSG.Items.Tools
{
    /// <summary>
    /// Controller for held Tools.
    /// </summary>
    public class HeldToolController : FPPItemController, ICollisionSource
    {
        public static float COMBO_RESET_SECONDS = 1f;

        public Player Player => owner as Player;
        public ToolData ToolData => ItemData as ToolData;
        
        bool _inhibitActions;
        bool _isAttacking;
        bool _onCooldown;
        public int ComboCounter { get; protected set; }

        public string CollisionTag => "Tool";
        ToolItemStateMachine stateMachine;
        public ToolItemStateMachine StateMachine => stateMachine;
        CoroutineHandle _comboResetTimerHandle;

        public bool IsBroken
        {
            get
            {
                if (attributes.TryGet("durability", out var durability))
                {
                    return durability.Value <= 0f;
                }
                else
                {
                    return false; /// makes items that have no durability unbreakable
                }
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
            InitializeInputs();
            InitializeEvents();
        }

        void InitializeAudioController()
        {
            audioSourceController.LoadAudioAssetsData(ToolData.AudioAssetsData);
            audioSourceController.CreateAudioPool(size: 8); 
        }

        void InitializeInputs()
        {
            var inputs = Player.Actions.Inputs;

            inputs["Primary Action"].started += OnPlayerPrimary;
            inputs["Primary Action"].canceled += OnPlayerPrimary;

            inputs["Secondary Action"].started += OnPlayerSecondary;
            inputs["Secondary Action"].canceled += OnPlayerSecondary;
        }

        void InitializeEvents()
        {
            stateMachine.OnTransition += OnStateChanged;
        }

        protected virtual void OnStateChanged(StateMachine<ToolItemStates>.TransitionContext context)
        {
            if (context.To == ToolItemStates.Idle)
            {
                _inhibitActions = false;
                _isAttacking = false;
            }
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
            if (_inhibitActions || _isAttacking) yield break;
            
            Timing.KillCoroutines(_comboResetTimerHandle);
            _inhibitActions = true;
            _isAttacking = true;
            ConsumeStamina();
            PlaySound();
            var atkParams = GetAttackParams();
            CreateAttackRays(ref atkParams);
            stateMachine.ToState(ToolItemStates.Attack);
            ComboCounter++;
            TrackCombo();
            yield return new WaitForSeconds(0.5f); /// SOMETHING ATKSPD THOUGH NOT SO STRAIGHFROWARDS LOTS OF CALCS (short for calculations)
            
            stateMachine.ToState(ToolItemStates.Idle);
        }

        void TrackCombo()
        {
            if (ComboCounter >= 1)
            {
                ComboCounter = 0;
            }
            else /// reset to first attack after a certain period
            {
                _comboResetTimerHandle = Timing.RunCoroutine(_ComboResetTimerCoroutine());
            }
        }
        
        IEnumerator<float> _ComboResetTimerCoroutine()
        {
            yield return Timing.WaitForSeconds(COMBO_RESET_SECONDS);
            ComboCounter = 0;
            yield break;
        }

        void ConsumeStamina()
        {
            owner.Attributes["stamina"].Remove(attributes["stamina_cost"].Value);
        }

        void PlaySound()
        {
            audioSourceController.PlaySound("swing_heavy");
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

            MeleeAttackParametersData parametersData;
            if (ToolData.Attacks.Count == 0) /// no attack combos set, fallback to raycast
            {
                return MeleeAttacks.DefaultRaycast;
            }
            else
            {
                parametersData = ToolData.Attacks[0];
                return MeleeAttacks.FromParametersData(parametersData);
            }
        }      
        
        void CreateAttackRays(ref MeleeAttackParameters atk)
        {
            Timing.RunCoroutine(CreateRaycastRays(atk));
            
            atk.Origin = Player.EyeLevel;
            atk.Up = Player.Up;
            atk.Direction = Player.Forward;

            if (atk.CastType == CastType.Raycast)
            {
                MeleeAttacks.Raycast(owner as IMeleeWeaponActor, ref atk, OnToolAttackHit);
            }
            else if (atk.CastType == CastType.Swingcast)
            {
                MeleeAttacks.Swingcast(owner as IMeleeWeaponActor, ref atk, OnToolAttackHit);
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