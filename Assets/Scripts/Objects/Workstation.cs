using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.InputSystem;

using UZSG.Systems;
using UZSG.Entities;
using UZSG.Interactions;
using UZSG.UI;
using UZSG.Crafting;
using UZSG.Items;
using UZSG.Inventory;
using UZSG.Data;

using static UZSG.Crafting.CraftingRoutineStatus;

namespace UZSG.Objects
{
    public class Workstation : BaseObject, IInteractable, IPlaceable//, ICrafter
    {
        public WorkstationData WorkstationData => objectData as WorkstationData;
        public string ActionText => "Use";
        public string Name => objectData.Name;

        Player player;
        Container inputContainer = new();
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

        InputAction backAction;
        
        protected override void Start()
        {
            base.Start();

            /// TESTING ONLY
            /// Place() should execute when the object is placed on the world :)
            Place(); 
        }

        void Place()
        {
            queueSlots = new(WorkstationData.QueueSize);
            queueSlots = new();
            outputContainer = new(WorkstationData.OutputSize);
            outputContainer.OnSlotItemChanged += OnOutputSlotItemChanged;
            
            crafter.Initialize(this);
            crafter.OnRoutineNotify += OnRoutineEventCall;
            crafter.OnRoutineSecond += OnRoutineSecond;

            LoadGUIAsset(WorkstationData.GUI, onLoadCompleted: (gui) =>
            {
                this.gui = gui;
                this.gui.LinkWorkstation(this);
            });
        }

        void Pickup()
        {

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

        void ReinitializeGUI()
        {
            gui.SetPlayer(player);

            backAction = Game.Main.GetInputAction("Back", "Global");
            backAction.performed += OnInputGlobalBack;
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
            ReinitializeGUI();

            player.UseWorkstation(this);
            player.InventoryGUI.OnClose += OnCloseInventory;
            gui.Show();
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

        void OnRoutineSecond(CraftingRoutine routine, int timeElapsed)
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
                audioController.PlaySound("craft");
            }
        }

        void PlayNoMaterialsSound()
        {
            if (gui.IsVisible)
            {
                audioController.PlaySound("insufficient_materials");
            }
        }

        void OnCloseInventory()
        {
            player.InventoryGUI.OnClose -= OnCloseInventory;
            backAction.performed -= OnInputGlobalBack;
            
            ClearInputContainers();
            player.ResetToPlayerCraftingGUI();
            Game.UI.ToggleCursor(false);
            gui.Hide();
            
            /// encapsulate
            player.InfoHUD.Show();
            player.Actions.Enable();
            player.Controls.Enable();
            player.FPP.ToggleControls(true);
            player = null;
        }


        protected virtual void LoadGUIAsset(AssetReference asset, Action<CraftingGUI> onLoadCompleted = null)
        {
            if (!asset.IsSet())
            {
                Game.Console.LogAndUnityLog($"There's no GUI set for Workstation '{WorkstationData.Id}'. This won't be usable unless otherwise you set its GUI.");
                return;
            }

            Addressables.LoadAssetAsync<GameObject>(asset).Completed += (a) =>
            {
                if (a.Status == AsyncOperationStatus.Succeeded)
                {
                    var go = Instantiate(a.Result);
                    
                    if (go.TryGetComponent<CraftingGUI>(out var gui))
                    {
                        onLoadCompleted?.Invoke(gui);
                        return;
                    }
                }
            };
        }

        void OnInputGlobalBack(InputAction.CallbackContext context)
        {
            OnCloseInventory();
        }
    }
}