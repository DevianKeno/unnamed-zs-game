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

namespace UZSG.Items.Tools
{
    public class HeldToolController : HeldItemController, ICollisionSource
    {
        public Player Player => owner as Player;
        public ToolData ToolData => ItemData as ToolData;
        
        public float attackRange;
        public float attackAngle;
        public float attackDuration;
        public int numberOfRays;
        public LayerMask attackLayer;
        bool _canAttack;
        bool _inhibitActions;
        bool _isAttacking;

        public bool VisualizeAttack;
        public string CollisionTag => "Tool";
        ToolItemStateMachine stateMachine;
        public ToolItemStateMachine StateMachine => stateMachine;

        void Awake()
        {
            stateMachine = GetComponent<ToolItemStateMachine>();
        }

        public override void Initialize()
        {
            InitializeAudioController();
            InitializeEventsFromOwnerInput();
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

        void RetrieveToolAttributes()
        {
            /// runtimeAttributes = ???
            
            /// TEST ONLY
            var durabilityAttr = new GenericAttribute("durability");
            durabilityAttr.Add(100); 
            Attributes.Add(durabilityAttr);
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
            TryHarvestResource();
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
            StartCoroutine(CreateAttackRays());
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
        
        IEnumerator CreateAttackRays()
        {
            float halfAngle = attackAngle / 2;
            float angleStep = attackAngle / (numberOfRays - 1);
            float stepTime = attackDuration / numberOfRays;
            Color rayColor;
            
            HashSet<int> hitEnemies = new();

            for (int i = 0; i < numberOfRays; i++)
            {
                float currentAngle = halfAngle - (angleStep * i);
                var direction = ToolData.SwingDirection switch // wdym its a fucking enum
                {
                    ToolSwingDirection.Upward => Quaternion.Euler(-currentAngle, 0, 0),
                    ToolSwingDirection.Downward => Quaternion.Euler(currentAngle, 0, 0),
                    ToolSwingDirection.Leftward => Quaternion.Euler(0, currentAngle, 0),
                    ToolSwingDirection.Rightward => Quaternion.Euler(0, -currentAngle, 0),
                };

                Vector3 rayOrigin = Player.EyeLevel;

                if (Physics.Raycast(rayOrigin, direction * Player.Forward, out RaycastHit hit, attackRange, attackLayer))
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
                    Debug.DrawRay(rayOrigin, direction * Player.Forward * attackRange, rayColor, 1.0f);
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
                    Source = this,
                    ContactPoint = point,
                });
                return;
            }

            var hit = hitObject.GetComponentInParent<BaseObject>();
            if (hit is Resource resource)
            {
                resource.HitBy(new()
                {
                    Source = this,
                    ContactPoint = point,
                });
                return;
            }
        }

        bool TryHarvestResource()
        {
            return false;
        }

        public override void SetStateFromAction(ActionStates state)
        {
            
        }
    }
}