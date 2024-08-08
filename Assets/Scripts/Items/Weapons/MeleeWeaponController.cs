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

namespace UZSG.Items.Weapons
{
    public class MeleeWeaponController : HeldWeaponController, ICollisionSource
    {
        Player Player => owner as Player;
        public WeaponData WeaponData => ItemData as WeaponData;

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

        public event Action<CollisionHitInfo> OnMeleeHit;

        #endregion


        MeleeWeaponStateMachine stateMachine;
        public MeleeWeaponStateMachine StateMachine => stateMachine;
        [SerializeField] AttributeCollection<GenericAttribute> runtimeAttributes;

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
            if (Player.Vitals.TryGet("stamina", out var attr))
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
            StartCoroutine(CreateAttackRays());
            stateMachine.ToState(MeleeWeaponStates.Attack);
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

        IEnumerator CreateAttackRays()
        {
            float halfAngle = attackAngle / 2;
            float angleStep = attackAngle / (numberOfRays - 1);
            float stepTime = attackDuration / numberOfRays;
            Color rayColor;
            
            HashSet<int> targets = new();

            for (int i = 0; i < numberOfRays; i++)
            {
                float currentAngle = halfAngle - (angleStep * i);
                Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * Player.Forward;
                Vector3 rayOrigin = Player.EyeLevel;

                if (Physics.Raycast(rayOrigin, direction, out RaycastHit hit, attackRange, attackLayer))
                {
                    int hitObjectId = hit.collider.GetInstanceID();

                    if (!targets.Contains(hitObjectId))
                    {
                        targets.Add(hitObjectId);
                        OnHit(hit.point, hit.collider);
                    }
                    rayColor = Color.red;
                }
                else
                {
                    rayColor = Color.white;
                }
                
                if (VisualizeAttack)
                {
                    Debug.DrawRay(rayOrigin, direction * attackRange, rayColor, 1.0f);
                }

                yield return new WaitForSeconds(stepTime);
            }
        }
        
        void OnHit(Vector3 point, Collider hitObject)
        {
            var info = new CollisionHitInfo()
            {
                Type = CollisionType.Melee,
                Source = this,
                ContactPoint = point,
            };

            var target = hitObject.GetComponentInParent<ICollisionTarget>();
            if (target != null)
            {
                info.Target = target;
                target.HitBy(info);
                OnMeleeHit?.Invoke(info);
            }
        }

        public override void SetStateFromAction(ActionStates state)
        {
            
        }
    }
}