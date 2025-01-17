using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

using UZSG.Systems;
using UZSG.Entities;
using UZSG.Interactions;
using UZSG.Crafting;
using UZSG.Items;
using UZSG.Inventory;
using UZSG.Data;
using UZSG.UI.Objects;

using static UZSG.Crafting.CraftingRoutineStatus;

namespace UZSG.Objects
{
    public class Workstation : BaseObject, IInteractable, ICrafter
    {
        public WorkstationData WorkstationData => objectData as WorkstationData;
        
        public string ActionText => "Use";
        public string DisplayName => objectData.DisplayName;
        public bool AllowInteractions { get; set; } = false;

        protected Player player;
        protected Container inputContainer = new();
        protected Container fuelSlots = new();
        protected List<ItemSlot> queueSlots = new();
        protected Container outputContainer = new();
        public Container OutputContainer => outputContainer;
        [SerializeField] protected Crafter crafter;
        public Crafter Crafter => crafter;
        protected WorkstationGUI gui;
        public WorkstationGUI GUI => gui;

        public event Action<CraftingRoutine> OnCraft;

        /// <summary>
        /// Listens to all output slots when their Item is changed.
        /// </summary>
        protected event Action<ItemSlot.ItemChangedContext> onOutputSlotItemChanged;
        public bool EnableDebugging = false;
        
        public override void Place()
        {
            if (IsPlaced) return;

            base.Place();
            Initialize();

            queueSlots = new(WorkstationData.QueueSize);
            outputContainer = new(WorkstationData.OutputSize);
            outputContainer.OnSlotItemChanged += OnOutputSlotItemChanged;
            fuelSlots = new(WorkstationData.FuelSlotsSize);

            crafter.Initialize(this);
            crafter.OnRoutineNotify += OnRoutineEventCall;
            crafter.OnRoutineSecond += OnRoutineSecond;

            LoadGUIAsset(WorkstationData.GUI, onLoadCompleted: (gui) =>
            {
                if (WorkstationData.RequiresFuel)
                {
                    this.gui = (FuelCraftingGUI) gui;
                }
                else
                {
                    this.gui = (WorkstationGUI) gui;
                }

                this.gui.LinkWorkstation(this);
            });

            AllowInteractions = true;
            IsPlaced = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        void InitializeCrafter()
        {
            if (crafter == null)
            {
                Debug.LogError($"Please assign a Crafter to prefab Workstation '{WorkstationData.Id}'.");
                throw new Exception();
            }

            AddInputContainer(player.Inventory.Bag);
            // crafter.AddInputContainer(player.Inventory.Hotbar); /// salt
        }


        void Use(Player player)
        {
            this.player = player;
            
            player.InfoHUD.Hide();
            player.Actions.Disable();
            player.Controls.Disable();
            player.FPP.ToggleControls(false);

            InitializeCrafter();
            gui.SetPlayer(player);

            player.UseObjectGUI(gui);
            player.InventoryWindow.OnClosed += OnCloseInventory;
            player.InventoryWindow.Show();
            
            Game.UI.SetCursorVisible(true);
        }


        #region Public methods

        #region IInteractable

        public virtual List<InteractAction> GetInteractActions()
        {
            var actions = new List<InteractAction>();

            var use = new InteractAction()
            {
                Interactable = this,
                ActionText = "Use",
                InteractableText = this.DisplayName,
                InputAction = Game.Input.InteractPrimary,
            };
            actions.Add(use);

            if (this.ObjectData.CanBePickedUp)
            {
                actions.Add(new()
                {
                    Interactable = this,
                    ActionText = "Pick Up",
                    InteractableText = this.DisplayName,
                    InputAction = Game.Input.InteractSecondary,
                    IsHold = true,
                    HoldDurationSeconds = this.ObjectData.PickupTimeSeconds,
                });
            }

            return actions;
        }

        public virtual void Interact(InteractionContext context)
        {
            if (context.Actor is not Player player) return;

            if (context.Action == "Use")
            {
                this.Use(player);
            }
            else if (context.Action == "Pick Up")
            {
                this.Pickup(player);
            }
        }

        #endregion

        public void AddInputContainer(Container other)
        {
            this.inputContainer.Extend(other);
        }

        public void SetOutputContainer(Container other)
        {
            this.outputContainer = other;
        }

        public void ClearInputContainers()
        {
            inputContainer = new();
        }

        public bool TryCraft(ref CraftItemOptions options)
        {
            if (crafter.Routines.Count >= WorkstationData.QueueSize)
            {
                return false;
            }

            var totalMaterials = CalculateTotalMaterials(options);
            if (!player.Inventory.Bag.ContainsAll(totalMaterials))
            {
                PlayNoMaterialsSound();
                if (EnableDebugging) Game.Console.LogInfo($"Tried to craft '{options.Recipe.Output.Id}' but had insufficient materials.");
                return false;
            }

            _ = inputContainer.TakeItems(totalMaterials);
            crafter.CraftNewItem(ref options);

            return true;
        }

        public bool TryFuelCraft(ref CraftItemOptions options)
        {
            
            if (!options.Recipe.RequiresFuel)
            {
                print("Your recipe does not require fuel to be crafted");
                return false;
            }

            var fuel_crafter = (FuelBasedCrafting) crafter;

            if (!fuel_crafter.IsFuelRemainingAvailable() && !fuel_crafter.IsFuelAvailable())
            {
                print("You cannot use this without any kind of fuel");
                return false;
            }

            if (fuel_crafter.Routines.Count >= WorkstationData.QueueSize)
            {
                return false;
            }

            var totalMaterials = CalculateTotalMaterials(options);
            if (!player.Inventory.Bag.ContainsAll(totalMaterials))
            {
                PlayNoMaterialsSound();
                if (EnableDebugging) Game.Console.LogInfo($"Tried to craft '{options.Recipe.Output.Id}' but had insufficient materials.");
                return false;
            }

            _ = inputContainer.TakeItems(totalMaterials);

            if (!fuel_crafter.IsFuelRemainingAvailable())
            {
                if(fuel_crafter.TryConsumeFuel())
                {
                    fuel_crafter.StartBurn();
                }
                else 
                {
                    return false;
                }      
            }

            fuel_crafter.CraftNewItem(ref options);

            return true;
        }
        #endregion


        public List<Item> CalculateTotalMaterials(CraftItemOptions options)
        {
            var list = new List<Item>();
            var mats = options.Recipe.Materials;

            for (int i = 0; i < mats.Count; i++)
            {
                var mat = new Item(mats[i]) * options.Count;
                list.Add(mat);
            }

            return list;
        }


        #region Event callbacks

        /// Crafting routines
        protected void OnRoutineEventCall(CraftingRoutine routine)
        {
            if (routine.Status == Prepared)
            {
                OnCraft?.Invoke(routine);
            }
            else if (routine.Status == Started)
            {
                OnCraft?.Invoke(routine);
            }
            else if (routine.Status == Ongoing)
            {
                OnCraft?.Invoke(routine);
            }
            else if (routine.Status == Paused)
            {
                OnCraft?.Invoke(routine);
            }
            else if (routine.Status == CraftSingle)
            {
                var outputItem = new Item(routine.Recipe.Output);
                
                if (outputContainer.TryPutNearest(outputItem))
                {
                    PlayCraftSound();
                    OnCraft?.Invoke(routine);
                    return;
                }

                /// output slot is full wtf?? what do lmao
                onOutputSlotItemChanged += PutItemWhenOutputSlotIsEmpty;
                void PutItemWhenOutputSlotIsEmpty(ItemSlot.ItemChangedContext slotInfo)
                {
                    /// look for empty space
                    if (!slotInfo.NewItem.CompareTo(Item.None)) return;
                    
                    onOutputSlotItemChanged -= PutItemWhenOutputSlotIsEmpty;
                    slotInfo.ItemSlot.Put(outputItem);
                    PlayCraftSound();
                    OnCraft?.Invoke(routine);
                };
            }
            else if (routine.Status == Finished)
            {
                OnCraft?.Invoke(routine);
            }
            else if (routine.Status == Canceled)
            {
                OnCraft?.Invoke(routine);
            }
        }

        protected void OnRoutineSecond(CraftingRoutine routine, float timeElapsed)
        {
            
        }

        /// Output slots

        protected void OnOutputSlotItemChanged(object sender, ItemSlot.ItemChangedContext e)
        {
            onOutputSlotItemChanged?.Invoke(e);
        }
        
        #endregion

        protected void PlayCraftSound()
        {
            if (gui.IsVisible)
            {
                Game.Audio.Play("craft");
            }
        }

        protected void PlayNoMaterialsSound()
        {
            if (gui.IsVisible)
            {
                Game.Audio.Play("insufficient_materials");
            }
        }

        protected void OnCloseInventory()
        {
            player.InventoryWindow.OnClosed -= OnCloseInventory;
            player.RemoveObjectGUI(gui);
            player.InventoryWindow.Hide();
            
            ClearInputContainers();
            Game.UI.SetCursorVisible(false);
            /// encapsulate
            player.InfoHUD.Show();
            player.Actions.Enable();
            player.Controls.Enable();
            player.FPP.ToggleControls(true);
            player = null;
        }
    }
}