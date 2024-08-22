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
    public class Workstation : BaseObject, IInteractable, IPlaceable, ICrafter
    {
        public WorkstationData WorkstationData => objectData as WorkstationData;
        public string Action => "Use";
        public string Name => objectData.Name;

        Player player;
        Container inputContainer = new();
        Container fuelSlots = new();
        List<ItemSlot> queueSlots = new();
        Container outputContainer = new();
        public Container OutputContainer => outputContainer;
        [SerializeField] Crafter crafter;
        public Crafter Crafter => crafter;
        CraftingGUI gui;
        public CraftingGUI GUI => gui;

        public event EventHandler<InteractArgs> OnInteract;
        public event Action<CraftingRoutine> OnCraft;

        /// <summary>
        /// Listens to all output slots when their Item is changed.
        /// </summary>
        event Action<ItemSlot.ItemChangedContext> onOutputSlotItemChanged;
        public bool EnableDebugging = false;
        
        protected override void Start()
        {
            base.Start();

            /// TESTING ONLY
            /// Place() should execute when the object is placed on the world :)
            Place(); 
        }

        protected virtual void Place()
        {
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
                    this.gui = (CraftingGUI) gui;
                }

                this.gui.LinkWorkstation(this);
            });
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


        #region Public methods

        public virtual void Interact(IInteractActor actor, InteractArgs args)
        {
            if (actor is not Player player) return;

            this.player = player;
            
            player.InfoHUD.Hide();
            player.Actions.Disable();
            player.Controls.Disable();
            player.FPP.ToggleControls(false);

            InitializeCrafter();
            gui.SetPlayer(player);

            player.UseObjectGUI(gui);
            player.InventoryGUI.OnClose += OnCloseInventory;
            player.InventoryGUI.Show();
            
            Game.UI.ToggleCursor(true);
        }

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
                if (EnableDebugging) Game.Console.Log($"Tried to craft '{options.Recipe.Output.Id}' but had insufficient materials.");
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
                if (EnableDebugging) Game.Console.Log($"Tried to craft '{options.Recipe.Output.Id}' but had insufficient materials.");
                return false;
            }

            _ = inputContainer.TakeItems(totalMaterials);

            fuel_crafter.CraftNewItem(ref options);

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

            return true;
        }
        #endregion


        List<Item> CalculateTotalMaterials(CraftItemOptions options)
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
        void OnRoutineEventCall(CraftingRoutine routine)
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

        void OnRoutineSecond(CraftingRoutine routine, float timeElapsed)
        {
            
        }

        /// Output slots

        void OnOutputSlotItemChanged(object sender, ItemSlot.ItemChangedContext e)
        {
            onOutputSlotItemChanged?.Invoke(e);
        }
        
        #endregion

        void PlayCraftSound()
        {
            if (gui.IsVisible)
            {
                Game.Audio.Play("craft");
            }
        }

        void PlayNoMaterialsSound()
        {
            if (gui.IsVisible)
            {
                Game.Audio.Play("insufficient_materials");
            }
        }

        void OnCloseInventory()
        {
            player.InventoryGUI.OnClose -= OnCloseInventory;
            player.RemoveObjectGUI(gui);
            player.InventoryGUI.Hide();
            
            ClearInputContainers();
            Game.UI.ToggleCursor(false);
            /// encapsulate
            player.InfoHUD.Show();
            player.Actions.Enable();
            player.Controls.Enable();
            player.FPP.ToggleControls(true);
            player = null;
        }
    }
}