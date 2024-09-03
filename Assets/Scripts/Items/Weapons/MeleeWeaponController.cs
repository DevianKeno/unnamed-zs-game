using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

using UZSG.Systems;
using UZSG.Interactions;
using UZSG.Data;
using UZSG.Entities;
using UZSG.Players;
using UZSG.Attributes;
using UZSG.Attacks;

namespace UZSG.Items.Weapons
{
    public class MeleeWeaponController : HeldWeaponController, ICollisionSource
    {
        public Player Player => owner as Player;
        public string CollisionTag => "Melee";

        bool _canAttack;
        bool _inhibitActions;
        bool _isAttacking;
        bool _onCooldown;
        
        float StaminaCost => attributes["stamina_cost"] != null ? attributes["stamina_cost"].Value : 0f;

        MeleeWeaponStateMachine stateMachine;
        public MeleeWeaponStateMachine StateMachine => stateMachine;

        #region Melee weapon events

        public event Action<HitboxCollisionInfo> OnMeleeHit;

        #endregion


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
                    if (owner.Attributes.TryGet("melee_stamina_cost_multiplier", out var multiplier))
                    {
                        return ownerStamina.Value >= (StaminaCost * multiplier.Value);
                    }
                    else
                    {
                        return ownerStamina.Value >= StaminaCost;
                    }
                }

                return false;
            }
        }

        #region Initializing methods

        void Awake()
        {
            stateMachine = GetComponent<MeleeWeaponStateMachine>();
        }

        public override void Initialize()
        {
            LoadDefaultAttributes();
            InitializeAudioController();
            InitializeEventsFromOwnerInput();
        }

        void InitializeAudioController()
        {
            audioSourceController.LoadAudioAssetsData(WeaponData.AudioAssetsData);
            audioSourceController.CreateAudioPool(size: 8); 
        }

        void InitializeEventsFromOwnerInput()
        {
            if (owner is not Player player) return;
            actionMap = player.Actions.ActionMap; /// DISBALE ALL ACTIONS ON DISABLE HELD ITEM
            var inputs = player.Actions.Inputs;

            inputs["Primary Action"].started += OnPlayerPrimary;
            inputs["Primary Action"].canceled += OnPlayerPrimary;

            inputs["Secondary Action"].started += OnPlayerSecondary;
            inputs["Secondary Action"].canceled += OnPlayerSecondary;
        }


        #endregion


        #region Public methods
        
        public override void SetStateFromAction(ActionStates state)
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
            }
            else if (context.canceled)
            {
                
            }
        }

        void OnPlayerSecondary(InputAction.CallbackContext context)
        {
            if (!gameObject.activeSelf) return;

            if (context.started)
            {
                
            }
            else if (context.canceled)
            {
                
            }
        }

        #endregion 


        void OnEnable()
        {
            ResetStates();
        }

        public void ResetStates()
        {
            _isAttacking = false;
            _inhibitActions = false;
        }

        IEnumerator AttackCoroutine()
        {
            if (_inhibitActions || _isAttacking)
            {
                yield break;
            }
            
            _inhibitActions = true;
            _isAttacking = true;

            ConsumeStamina();
            PlaySound();
            var atkParams = GetAttackParams();
            CreateAttackRays(ref atkParams);
            stateMachine.ToState(MeleeWeaponStates.Attack);
            yield return new WaitForSeconds(0.5f); /// SOMETHING ATKSPD THOUGH NOT SO STRAIGHFROWARDS LOTS OF CALCS (short for calculations)
            
            _inhibitActions = false;
            _isAttacking = false;
        }
        
        MeleeAttackParameters GetAttackParams()
        {
            #region TODO: Implement combos
            #endregion
            var data = WeaponData.MeleeAttacks[0];
            return MeleeAttacks.Parameters(data);
        }   

        void ConsumeStamina()
        {
            owner.Attributes["stamina"].Remove(StaminaCost);
        }

        void PlaySound()
        {
            audioSourceController.PlaySound("swing1");
        }

        void CreateAttackRays(ref MeleeAttackParameters atk)
        {
            atk.Origin = Player.EyeLevel;
            atk.Up = Player.Up;
            atk.Direction = Player.Forward;

            if (atk.SwingType == MeleeSwingType.Raycast)
            {
                MeleeAttacks.Raycast(ref atk, OnMeleeAttackHit);
            }
            else if (atk.SwingType == MeleeSwingType.Swingcast)
            {
                MeleeAttacks.Swingcast(ref atk, OnMeleeAttackHit);
            }
        }

        void OnMeleeAttackHit(HitboxCollisionInfo info)
        {
            info.Source = this;
            info.CollisionType = HitboxCollisionType.Attack;
            info.Target.HitBy(info);
            OnMeleeHit?.Invoke(info);
        }

        Vector3 CalculateForceDirection(Vector3 contactPoint, Vector3 contactNormal, Vector3 swingDirection)
        {
            Vector3 reflectDirection = Vector3.Reflect(swingDirection, contactNormal);
            return reflectDirection;
        }

        internal void SetMeleeCollider(MeleeWeaponCollider mwc)
        {
            if (mwc != null)
            {
                mwc.OnCollide += OnMeleeAttackHit;
            }
        }
    }
}