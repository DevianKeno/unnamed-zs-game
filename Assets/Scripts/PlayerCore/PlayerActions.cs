using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

using UZSG.Systems;
using UZSG.Items;
using UZSG.Interactions;
using UZSG.Entities;
using UZSG.Inventory;
using UZSG.UI;

namespace UZSG.Players
{
    /// <summary>
    /// Handles the different actions the Player can do.
    /// </summary>
    public class PlayerActions : MonoBehaviour
    {
        public Player Player;

        [Header("Interact Size")]
        public float Radius;
        public float MaxInteractDistance;
        public float HoldThresholdMs = 200f;
        float _holdTimer;
        bool leftClicked;
        bool rightClicked;
        bool heldClick;
        
        /// <summary>
        /// The interactable object the Player is currently looking at.
        /// </summary>
        IInteractable lookingAt;
        RaycastHit hit;
        Ray ray;
        InteractionIndicator interactionIndicator;
        
        InputActionMap actionMap;
        Dictionary<string, InputAction> inputs = new();
        
        void Awake()
        {
            Player = GetComponent<Player>();
        }

        internal void Initialize()
        {
            InitializeInputs();
            interactionIndicator = Game.UI.Create<InteractionIndicator>("interaction_indicator");

            Game.Tick.OnTick += Tick;
        }
  
        void Update()
        {
            if (leftClicked)
            {
                _holdTimer += Time.deltaTime;

                if (_holdTimer > HoldThresholdMs / 1000f)
                {
                    leftClicked = false;
                    Player.smAction.ToState(ActionStates.PrimaryHold);
                }

            }
            else if (rightClicked)
            {
                _holdTimer += Time.deltaTime;

                if (_holdTimer > HoldThresholdMs / 1000f)
                {
                    rightClicked = false;
                    Player.smAction.ToState(ActionStates.SecondaryHold);
                }
            }
        }

        void InitializeInputs()
        {
            actionMap = Game.Main.GetActionMap("Player");
            inputs = Game.Main.GetActionsFromMap(actionMap);
        
            inputs["Primary Action"].started += OnStartPrimary;         // LMB (default)
            inputs["Primary Action"].canceled += OnCancelPrimary;       // LMB (default)

            inputs["Secondary Action"].started += OnStartSecondary;     // RMB (default)
            inputs["Secondary Action"].canceled += OnCancelSecondary;   // RMB (default)

            inputs["Interact"].performed += OnPerformInteract;          // F (default)
            inputs["Hotbar"].performed += OnHotbarSelect;               // Tab/E (default)
        }


        #region Callbacks

        void OnHotbarSelect(InputAction.CallbackContext context)
        {
            if (!int.TryParse(context.control.displayName, out int index)) return;
            Player.Inventory.SelectHotbarSlot(index);
        }

        void HotbarChangeEquippedCallback(object sender, Hotbar.ChangeEquippedArgs e)
        {
            Player.FPP.Equip(e.Index);
        }
        
        void OnPerformInteract(InputAction.CallbackContext context)
        {
            if (lookingAt == null) return;

            lookingAt.Interact(this, new InteractArgs());
        }

        void OnStartPrimary(InputAction.CallbackContext c)
        {
            leftClicked = true;
            _holdTimer = 0f;
        }

        void OnCancelPrimary(InputAction.CallbackContext c)
        {
            if (_holdTimer < HoldThresholdMs / 1000f)
            {
                leftClicked = false;
                Player.smAction.ToState(ActionStates.Primary);
            }
        }

        void OnStartSecondary(InputAction.CallbackContext c)
        {
            rightClicked = true;
            _holdTimer = 0f;
        }

        void OnCancelSecondary(InputAction.CallbackContext c)
        {
            if (_holdTimer < HoldThresholdMs / 1000f)
            {
                rightClicked = false;
                Player.smAction.ToState(ActionStates.Secondary);
            }
        }

        #endregion

        void Tick(object sender, TickEventArgs e)
        {
            CheckLookingAt();
        }

        /// <summary>
        /// Maybe instead of firing every tick, this can just fire everytime the player's ray collides with an IInteractable object
        /// </summary>
        void CheckLookingAt()
        {
            /// Cast a ray from the center of the screen
            ray = Player.MainCamera.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));

            if (Physics.SphereCast(ray, Radius, out RaycastHit hit, MaxInteractDistance, LayerMask.GetMask("Interactable")))
            {
                lookingAt = hit.collider.gameObject.GetComponent<IInteractable>();

                if (lookingAt != null)
                {
                    interactionIndicator.Indicate(lookingAt);
                }
            }
            else
            {
                lookingAt = null;
                interactionIndicator.Hide();
            }
        }        
        
        /// <summary>
        /// Visualizes the interaction size.
        /// </summary>
        void OnDrawGizmos()
        {
            Gizmos.DrawLine(ray.origin, ray.origin + ray.direction * (MaxInteractDistance));
            Gizmos.DrawWireSphere(ray.origin + ray.direction * (MaxInteractDistance + Radius), Radius);
        }

        /// <summary>
        /// Pick up item from ItemEntity and put in the inventory.
        /// TODO: There is currently no checking if inventory is full.
        /// </summary>
        public void PickUpItem(ItemEntity itemEntity)
        {
            if (!Player.CanPickUpItems) return;
            if (Player.Inventory == null) return;
            if (Player.Inventory.IsFull)
            {
                /// Prompt inventory full
                Game.Console.Log($"Can't pick up item. Inventory full");
                Debug.Log($"Can't pick up item. Inventory full");
                return;
            }

            bool gotItem; /// if the player had picked up the item
            Item item = itemEntity.AsItem();

            if (item.Type == ItemType.Weapon)
            {
                gotItem = Player.Inventory.Mainhand.TryPutItem(item);

                if (gotItem)
                {
                    if (WeaponData.TryGetWeaponData(item.Data, out WeaponData weaponData))
                    {
                        Player.FPP.LoadModel(weaponData, HotbarIndex.Mainhand);
                        Player.FPP.Equip(HotbarIndex.Mainhand);
                    }
                }
            }
            else if (item.Type == ItemType.Tool)
            {
                gotItem = Player.Inventory.Hotbar.Offhand.TryPutItem(item);                

                // if (ToolData.TryGetToolData(item.Data, out ToolData toolData))
                // {
                //     _FPP.Load(toolData, 1);
                // }
                // // else try to put in other hotbar slots (3-0) and only if available
            }
            else /// generic item
            {
                gotItem = Player.Inventory.Bag.TryPutNearest(item);
            }

            if (gotItem)
            {
                Game.Entity.Kill(itemEntity);
            }
        }
    }
}
