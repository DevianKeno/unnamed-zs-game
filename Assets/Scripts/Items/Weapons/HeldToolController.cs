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
using UZSG.Attributes;
using UZSG.Objects;
using UZSG.Attacks;

namespace UZSG.Items.Tools
{
    public class HeldToolController : HeldItemController, ICollisionSource
    {
        public Player Player => owner as Player;
        public ToolData ToolData => ItemData as ToolData;
                
        public float attackRange;
        public float attackAngle;
        public float attackDuration;
        public float attackDelay;
        public int numberOfRays;
        public LayerMask attackLayer;
        public bool VisualizeAttack;

        bool _canAttack;
        bool _inhibitActions;
        bool _isAttacking;

        public string CollisionTag => "Tool";
        ToolItemStateMachine stateMachine;
        public ToolItemStateMachine StateMachine => stateMachine;


        #region Initializing methods

        void Awake()
        {
            stateMachine = GetComponent<ToolItemStateMachine>();
        }

        public override void Initialize()
        {
            InitializeAudioController();
            InitializeEventsFromOwnerInput();
            LoadDefaultAttributes();
            RetrievePlayerAttributes();
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

            attackDuration = attributes["attack_speed"].Value;
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
            
        }

        #endregion

        Attributes.Attribute playerStamina;
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
            playerStamina.Remove(attackStaminaCost);
        }

        void PlaySound()
        {
            audioSourceController.PlaySound("swing1");
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