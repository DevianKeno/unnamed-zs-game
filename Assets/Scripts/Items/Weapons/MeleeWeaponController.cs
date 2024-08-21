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
        Player Player => owner as Player;

        public float attackRange;
        public float attackAngle;
        public float attackDuration;
        public int numberOfRays;
        public LayerMask attackLayer;
        bool _canAttack;
        bool _inhibitActions;
        bool _isAttacking;

        public bool VisualizeAttack;

        public string CollisionTag => "Melee";

        
        #region Melee weapon events

        public event Action<HitboxCollisionInfo> OnMeleeHit;

        #endregion


        MeleeWeaponStateMachine stateMachine;
        public MeleeWeaponStateMachine StateMachine => stateMachine;


        #region Initializing methods

        void Awake()
        {
            stateMachine = GetComponent<MeleeWeaponStateMachine>();
        }

        public override void Initialize()
        {
            InitializeAudioController();
            InitializeEventsFromOwnerInput();
            RetrievePlayerAttributes();
        }

        void InitializeAudioController()
        {
            audioSourceController.LoadAudioAssetsData(WeaponData.AudioAssetsData);
            audioSourceController.CreateAudioPool(size: 8); 
        }

        Attributes.Attribute playerStamina;

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

        void RetrievePlayerAttributes()
        {
            if (Player.Attributes.TryGet("stamina", out var attr))
            {
                playerStamina = attr;
                _canAttack = true;
            }
            else
            {
                Game.Console.LogWarning($"Player {Player.name} does not have a 'stamina' attribute. They will not be able to use '{ItemData.Id}'.");
                _canAttack = false;
            }
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
                if (CanAttack())
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

        [SerializeField] float attackStaminaCost;
        bool CanAttack()
        {
            if (!_canAttack) return false;
            if (playerStamina.Value < attackStaminaCost)
            {
                return false;
            }

            return true;
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
            playerStamina.Remove(attackStaminaCost);
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