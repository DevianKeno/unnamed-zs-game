using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UZSG.Entities;
using UZSG.Players;
using UZSG.Systems;

namespace UZSG.Items.Weapons
{
    public class MeleeWeaponController : WeaponController, ICollision
    {
        Player Player => owner as Player;
        public WeaponData WeaponData => ItemData as WeaponData;

        public float attackRange;
        public float attackAngle;
        public float attackDuration;
        public int numberOfRays;
        public LayerMask attackLayer;
        bool _inhibitActions;
        bool _isAttacking;

        public bool VisualizeAttack;

        public string CollisionTag => "Melee";

        MeleeWeaponStateMachine stateMachine;
        public MeleeWeaponStateMachine StateMachine => stateMachine;
        [SerializeField] AudioSourceController audioSourceController;

        void Awake()
        {
            stateMachine = GetComponent<MeleeWeaponStateMachine>();
        }

        public override void Initialize()
        {
            InitializeAudioController();
            InitializeEventsFromOwnerInput();
        }
        
        void InitializeAudioController()
        {
            audioSourceController.LoadAudioAssetIds(WeaponData.AudioData);
            audioSourceController.CreateAudioPool(size: 8); 
        }

        void InitializeEventsFromOwnerInput()
        {
            if (owner is not Player player) return;
            actionMap = player.Controls.ActionMap; /// DISBALE ALL ACTIONS ON DISABLE HELD ITEM
            var inputs = player.Controls.Inputs;

            inputs["Primary Action"].started += OnPlayerPrimary;
            inputs["Primary Action"].canceled += OnPlayerPrimary;

            inputs["Secondary Action"].started += OnPlayerSecondary;
            inputs["Secondary Action"].canceled += OnPlayerSecondary;
        }


        #region Player input callbacks

        void OnPlayerPrimary(InputAction.CallbackContext context)
        {
            if (!gameObject.activeSelf) return;
            
            if (context.started)
            {
                StartCoroutine(AttackCoroutine());
            }
            else if (context.canceled)
            {
                
            }
        }

        void OnPlayerSecondary(InputAction.CallbackContext context)
        {
            
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

            StartCoroutine(CreateAttackHitbox());
            PlaySound();
            stateMachine.ToState(MeleeWeaponStates.Attack);
            yield return new WaitForSeconds(0.5f); /// SOMETHING ATKSPD THOUGH NOT SO STRAIGHFROWARDS LOTS OF CALCS (short for calculations)
            
            _inhibitActions = false;
            _isAttacking = false;
        }

        void PlaySound()
        {
            audioSourceController.PlaySound("swing1");
        }

        public override void SetStateFromAction(ActionStates state)
        {
            
        }

        IEnumerator CreateAttackHitbox()
        {
            float halfAngle = attackAngle / 2;
            float angleStep = attackAngle / (numberOfRays - 1);
            float stepTime = attackDuration / numberOfRays;
            Color rayColor;
            
            HashSet<int> hitEnemies = new();

            for (int i = 0; i < numberOfRays; i++)
            {
                float currentAngle = halfAngle - (angleStep * i);
                Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * Player.Forward;
                Vector3 rayOrigin = Player.EyeLevel;

                if (Physics.Raycast(rayOrigin, direction, out RaycastHit hit, attackRange, attackLayer))
                {
                    int hitObjectId = hit.collider.GetInstanceID();

                    if (!hitEnemies.Contains(hitObjectId))
                    {
                        hitEnemies.Add(hitObjectId);
                        OnHit(hit.point, hit.collider);
                        Debug.Log("Hit: " + hit.collider.name);
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
            if (hitObject.TryGetComponent<Hitbox>(out var hitbox))
            {
                // CalculatedDamage = CalculateDamage(hitbox.Part);
                hitbox.HitBy(new()
                {
                    By = this,
                    ContactPoint = point,
                });
            }
        }

    }
}