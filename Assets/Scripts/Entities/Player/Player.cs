using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

using UZSG.Systems;
using UZSG.Inventory;
using UZSG.Attributes;
using UZSG.Players;
using UZSG.Data;
using UZSG.FPP;
using UZSG.UI;
using UZSG.StatusEffects;
using UZSG.Crafting;

namespace UZSG.Entities
{
    public interface IInteractActor
    {
    }

    /// <summary>
    /// Player entity.
    /// </summary>
    public class Player : Entity, IInteractActor
    {
        public bool CanPickUpItems = true;

        [SerializeField] PlayerData playerData;
        public PlayerData PlayerData => playerData;
        [SerializeField] PlayerEntityData playerEntityData;
        public PlayerEntityData PlayerEntityData => playerEntityData;
        [SerializeField] AttributeCollection<VitalAttribute> vitals;
        public AttributeCollection<VitalAttribute> Vitals => vitals;
        [SerializeField] AttributeCollection<GenericAttribute> generic;
        public AttributeCollection<GenericAttribute> Generic => generic;
        [SerializeField] InventoryHandler inventory;
        public InventoryHandler Inventory => inventory;
        [SerializeField] PlayerCrafting craftingAgent;
        public Crafter CraftingAgent => craftingAgent;
        StatusEffectCollection statusEffects;
        public StatusEffectCollection StatusEffects => statusEffects;
        
        public Vector3 Forward => MainCamera.transform.forward;
        public Vector3 EyeLevel => MainCamera.transform.position;

        PlayerInventoryWindow invUI;
        public PlayerInventoryWindow InventoryGUI => invUI;
        PlayerHUD _HUD;
        public PlayerHUD HUD => _HUD;

        #region Events

        public event EventHandler<EventArgs> OnDoneInit;

        #endregion

        [field: Header("Components")]
        /// <summary>
        /// Unity camera tagged "MainCamera"
        /// </summary>
        public Camera MainCamera { get; private set; }
        public PlayerControls Controls { get; private set; }
        public PlayerActions Actions { get; private set; }
        public MovementStateMachine MoveStateMachine { get; private set; }
        public ActionStateMachine ActionStateMachine { get; private set; }
        public FPPController FPP { get; private set; }

        public bool CanJump
        {
            get
            {
                return Vitals.Get("stamina").Value >= Generic.Get("jump_stamina_cost").Value && Controls.IsGrounded;
            }
        }

        public override void OnSpawn()
        {
            Initialize();
        }

        void Awake()
        {
            MainCamera = Camera.main;
            MoveStateMachine = GetComponent<MovementStateMachine>();
            ActionStateMachine = GetComponent<ActionStateMachine>();
            Controls = GetComponent<PlayerControls>();
            Actions = GetComponent<PlayerActions>();
            FPP = GetComponent<FPPController>();
        }

        void Initialize()
        {
            Game.Console.Log("I, player, has been spawned!");

            // Load PlayerData from file
            // Data.LoadFromFile("/path");

            //loads the user-crafting agent
            craftingAgent.InitializePlayer(this);
            craftingAgent.containers.Add(inventory.Bag);
            craftingAgent.containers.Add(inventory.Hotbar);

            InitializeAttributes();
            InitializeStateMachines();
            InitializeInventory();
            InitializeHUD();
            InitializeInputs();
            
            Controls.Initialize();
            Controls.Enable();
            Actions.Initialize();
            Actions.Enable();
            
            FPP.Initialize();
            ParentMainCameraToFPPController();

            Game.Tick.OnTick += Tick;
            OnDoneInit?.Invoke(this, new());
        }

        InputActionMap actionMap;
        Dictionary<string, InputAction> inputs = new();

        void InitializeInputs()
        {
            actionMap = Game.Main.GetActionMap("Player");
            foreach (var action in actionMap.actions)
            {
                inputs[action.name] = action;
                action.Enable();
            }
            
            inputs["Inventory"].performed += OnPerformInventory;        // Tab/E (default)
        }

        void InitializeAttributes()
        {
            /// Get blueprint base
            vitals.ReadAttributesData(playerEntityData.Vitals);
            
            /// Overwrite base with data from file
            // Vitals.LoadData(Data.Vitals);

            generic.ReadAttributesData(playerEntityData.Generic);
            // Generic.LoadData(Data.Generic);

            SetDefaultAttributes();
        }

        void SetDefaultAttributes()
        {
#region Temporary /// These should be read from save data

            vitals["health"].ReadSaveData(new()
            {
                Value = 100f,
                BaseMaximum = 100f,
                ChangeType = VitalAttributeChangeType.Regen,
                TimeCycle = VitalAttributeTimeCycle.Tick,
                BaseChange = 0.1f / TickSystem.NormalTPS,
            });
            vitals["stamina"].ReadSaveData(new()
            {
                Value = 100f,
                BaseMaximum = 100f,
                ChangeType = VitalAttributeChangeType.Regen,
                TimeCycle = VitalAttributeTimeCycle.Tick,
                BaseChange = 5f / TickSystem.NormalTPS,
                EnableDelayedChange = true,
                DelayedChangeDuration = 2f,
            });
            vitals["hunger"].ReadSaveData(new()
            {
                Value = 100f,
                BaseMaximum = 100f,
                ChangeType = VitalAttributeChangeType.Degen,
                TimeCycle = VitalAttributeTimeCycle.Tick,
                BaseChange = 0.1f / TickSystem.NormalTPS,
            });
            vitals["hydration"].ReadSaveData(new()
            {
                Value = 100f,
                BaseMaximum = 100f,
                ChangeType = VitalAttributeChangeType.Degen,
                TimeCycle = VitalAttributeTimeCycle.Tick,
                BaseChange = 0.1f / TickSystem.NormalTPS,
            });

            /// Should save and read all though
            generic["move_speed"].ReadSaveData(new()
            {
                Value = 300f,
            });
            generic["run_speed"].ReadSaveData(new()
            {
                Value = 400f,
            });
            generic["crouch_speed"].ReadSaveData(new()
            {
                Value = 200f,
            });
            generic["run_stamina_cost"].ReadSaveData(new()
            {
                Value = 5 / TickSystem.NormalTPS,
            });
            generic["jump_stamina_cost"].ReadSaveData(new()
            {
                Value = 20f,
            });
            generic["jump_hunger_cost"].ReadSaveData(new()
            {
                Value = 0.33f,
            });
            generic["jump_hydration_cost"].ReadSaveData(new()
            {
                Value = 0.33f,
            });
            generic["bag_slots_count"].ReadSaveData(new()
            {
                Value = 16,
            });
            generic["hotbar_slots_count"].ReadSaveData(new()
            {
                Value = 2,
            });

#endregion
        }

        void InitializeInventory()
        {
            // Inventory.LoadData(Data.Inventory);
            inventory.Bag.SlotsCount = Mathf.FloorToInt(Generic.Get("bag_slots_count").Value);
            inventory.Hotbar.SlotsCount = Mathf.FloorToInt(Generic.Get("hotbar_slots_count").Value);
            inventory.Initialize();

            invUI = Game.UI.Create<PlayerInventoryWindow>("Player Inventory", show: false);
            invUI.BindPlayer(this);
            invUI.Initialize();
            invUI.OnOpen += () =>
            {
                Actions.ActionMap.Disable();
                inputs["Look"].Disable();
            };
            invUI.OnClose += () =>
            {
                Actions.ActionMap.Enable();
                inputs["Look"].Enable();
            };
        }

        void InitializeHUD()
        {
            _HUD = Game.UI.Create<PlayerHUD>("Player HUD");
            _HUD.BindPlayer(this);
            _HUD.Initialize();
        }
        
        void InitializeStateMachines()
        {
            MoveStateMachine.InitialState = MoveStateMachine.States[MoveStates.Idle];

            MoveStateMachine.States[MoveStates.Idle].OnEnter += OnIdleEnter;
            MoveStateMachine.States[MoveStates.Run].OnEnter += OnRunEnter;
            MoveStateMachine.States[MoveStates.Jump].OnEnter += OnJumpEnter;
            MoveStateMachine.States[MoveStates.Crouch].OnEnter += OnCrouchEnter;      
        }

        void ParentMainCameraToFPPController()
        {
            Camera.main.transform.SetParent(FPP.CameraController.Camera.transform, false);
            Camera.main.transform.localPosition = Vector3.zero;
        }

        void Tick(TickInfo t)
        {
            /// Consume stamina while running
            if (Controls.IsRunning && Controls.IsMoving)
            {
                /// Cache attributes for better performance
                var runStaminaCost = Generic.Get("run_stamina_cost").Value;
                Vitals.Get("stamina").Remove(runStaminaCost);
            }
        }

        void OnIdleEnter(object sender, State<MoveStates>.ChangedContext e)
        {
        }

        void OnRunEnter(object sender, State<MoveStates>.ChangedContext e)
        {
        }

        void OnCrouchEnter(object sender, State<MoveStates>.ChangedContext e)
        {
        }

        void OnJumpEnter(object sender, State<MoveStates>.ChangedContext e)
        {
            /// Consume Stamina on jump
            if (Vitals.TryGet("stamina", out var stamina))
            {
                float jumpStaminaCost = Generic.Get("jump_stamina_cost").Value;
                stamina.Remove(jumpStaminaCost);
            }
            /// Consume Hunger on jump
            if (Vitals.TryGet("hunger", out var hunger))
            {
                float jumpHungerCost = Generic.Get("jump_hunger_cost").Value;
                hunger.Remove(jumpHungerCost);
            }
            /// Consume Hydration on jump
            if (Vitals.TryGet("hydration", out var hydration))
            {
                float jumpHydrationCost = Generic.Get("jump_hydration_cost").Value;
                hydration.Remove(jumpHydrationCost);
            }
        }
        
        /// <summary>
        /// I want the cam to lock and cursor to appear only when the key is released :P
        /// </summary>    
        void OnPerformInventory(InputAction.CallbackContext context)
        {
            ToggleInventory();
        }

        public void ToggleInventory()
        {
            invUI.ToggleVisibility();
            if (invUI.IsVisible)
            {
                HUD.Hide();
                invUI.OnClose += () =>
                {
                    HUD.Show();
                };
            }
        }
    }
}