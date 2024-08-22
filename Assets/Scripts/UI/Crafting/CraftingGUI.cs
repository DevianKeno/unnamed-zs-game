using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

using UZSG.Crafting;
using UZSG.Items;
using UZSG.Systems;
using UZSG.Data;
using UZSG.Inventory;
using UZSG.Entities;
using UZSG.Objects;
using UZSG.UI.Objects;

using static UZSG.Crafting.CraftingRoutineStatus;

namespace UZSG.UI.Objects
{
    public class CraftingGUI : ObjectGUI
    {
        protected Workstation workstation;
        public Workstation Workstation => workstation;
        RecipeData _selectedRecipe;
        public RecipeData SelectedRecipe => _selectedRecipe;
        int _amountToCraft = 1;
        public int AmountToCraft
        {
            get
            {
                return _amountToCraft;
            }
            set
            {
                _amountToCraft = value;
                craftAmountInputField.text = $"{_amountToCraft}";
            }
        }

        Dictionary<string, CraftableItemUI> craftableItemUIs = new();
        List<MaterialCountUI> materialSlotUIs = new();
        /// <summary>
        /// Key is the Crafting Routine; Value is the UI element.
        /// </summary>
        Dictionary<CraftingRoutine, CraftingProgressUI> _routineUIs = new();

        [SerializeField] CraftedItemDisplayUI craftedItemDisplay;
        [SerializeField] RectTransform craftablesHolder;
        [SerializeField] RectTransform materialSlotsHolder;
        [SerializeField] Transform queueSlotsHolder;
        [SerializeField] Transform progressContainer;
        [SerializeField] Transform outputSlotsHolder;
        [SerializeField] TMP_InputField craftAmountInputField;
        [SerializeField] Button craftButton;

        void Awake()
        {
            craftButton.onClick.AddListener(RequestCraftItem);
            craftAmountInputField.onEndEdit.AddListener(UpdateAmountToCraft);
        }

        /// <summary>
        /// Called when user clicks on a recipe, also referred here as a "Craftable Item".
        /// </summary>
        void OnClickCraftable(CraftableItemUI craftable)
        {
            _selectedRecipe = craftable.RecipeData;
            craftedItemDisplay.SetDisplayedRecipe(_selectedRecipe);
            DisplayMaterials(_selectedRecipe);
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }


        #region Public methods

        public override void SetPlayer(Player player)
        {
            base.SetPlayer(player);
            
            player.Inventory.Bag.OnSlotItemChanged += OnPlayerBagItemChanged;
        }
        
        public virtual void LinkWorkstation(Workstation workstation)
        {
            this.workstation = workstation;

            CreateOutputSlotUIs(workstation.WorkstationData.OutputSize);
            CreateQueueSlotUIs(workstation.WorkstationData.QueueSize);
        }

        public void IncrementAmountToCraft()
        {
            AmountToCraft++;
        }

        public void DecrementAmountToCraft()
        {
            AmountToCraft--;
        }
        
        public void ResetDisplayed()
        {
            _selectedRecipe = null;
            craftedItemDisplay.ResetDisplayed();
            ClearCraftableItems();
            ClearMaterialSlots();
        }
        
        public override void OnShow()
        {
            ResetDisplayed();

            if (workstation != null)
            {
                AddRecipes(workstation.WorkstationData.IncludedRecipes); /// by raw RecipeData
                /// Situational by workstation
                /// e.g., By Player recipes can also be crafted in Workbench (ofc if the Player knows it)
                /// but not in smelting, etc. as said, situational
                if (workstation.WorkstationData.IncludePlayerRecipes)
                {
                    AddRecipesById(player.SaveData.KnownRecipes); /// by raw Recipe Id. ID!!!!!!
                }
                workstation.OnCraft += OnWorkstationCraft;

                /// Retrieve crafting routines
                foreach (var r in workstation.Crafter.Routines)
                {
                    CreateRoutineProgressUI(r);
                }
            }
        }

        public override void OnHide()
        {
            player.Inventory.Bag.OnSlotItemChanged -= OnPlayerBagItemChanged;
            this.player = null;

            if (workstation != null)
            {
                workstation.OnCraft -= OnWorkstationCraft;
            }

            foreach (var ui in _routineUIs.Values)
            {
                Destroy(ui.gameObject);
            }
            _routineUIs.Clear();
        }

        #endregion


        protected void CreateOutputSlotUIs(int size)
        {
            for (int i = 0; i < size; i++)
            {
                var slotUI = Game.UI.Create<ItemSlotUI>("Item Slot");
                slotUI.name = $"Output Slot ({i})";
                slotUI.transform.SetParent(outputSlotsHolder);
                slotUI.Index = i;

                slotUI.Link(workstation.OutputContainer[i]); /// link output container to UI
                slotUI.OnMouseDown += OnOutputSlotClick;

                slotUI.Show();
            }
        }
        
        protected void CreateQueueSlotUIs(int size)
        {
            for (int i = 0; i < size; i++)
            {
                var slotUI = Game.UI.Create<ItemSlotUI>("Item Slot");
                slotUI.name = $"Queue Slot"; /// no index, they're shifting :P
                slotUI.transform.SetParent(queueSlotsHolder);

                // slotUI.OnMouseDown += OnQueueSlotClick; /// cancel craft

                slotUI.Show();
            }
        }


        #region Workstation callbacks

        void OnWorkstationCraft(CraftingRoutine routine)
        {
            if (routine.Status == Prepared)
            {
                CreateRoutineProgressUI(routine);
            }
            else if (routine.Status == Started)
            {
                //
            }
            else if (routine.Status == Ongoing)
            {
                RefreshRoutineProgressUIs(routine);
            }
            else if (routine.Status == Paused){
                RefreshRoutineProgressUIs(routine);
            }
            else if (routine.Status == Finished)
            {
                var ui = _routineUIs[routine];
                _routineUIs.Remove(routine);
                Destroy(ui.gameObject);
            }
            else if (routine.Status == Canceled)
            {
                //
            }
        }
        
        void CreateRoutineProgressUI(CraftingRoutine routine)
        {
            var routineUI = Game.UI.Create<CraftingProgressUI>("Craft Progress UI");
            var options = routine.Options;

            routineUI.transform.SetParent(progressContainer, false);
            routineUI.SetCraftingRoutine(routine);
            _routineUIs[routine] = routineUI;
        }

        void Update()
        {

        }

        void RefreshRoutineProgressUIs(CraftingRoutine routine)
        {
            if (_routineUIs.TryGetValue(routine, out var ui))
            {
                ui.Progress = routine.Progress;
                ui.ProgressSingle = routine.ProgressSingle;
            }
        }

        #endregion


        /// <summary>
        /// Submit a request to the Workstation tied to this Crafting GUI to craft the recipe given the options.
        /// </summary>
        protected virtual void RequestCraftItem()
        {
            if (_selectedRecipe == null)
            {
                throw new Exception("selected none recipe");
            }
            
            #region TODO: inhibit negative values in the ui
            #endregion

            if (_selectedRecipe.Output.Count < 0)
            {
                print("invalid recipe count");
                return;
            }

            int maxTimes = Mathf.FloorToInt(_selectedRecipe.Output.Data.StackSize / _selectedRecipe.Yield);
            var count = Math.Clamp(AmountToCraft, 1, maxTimes);
            var options = new CraftItemOptions()
            
            {
                Recipe = _selectedRecipe,
                Count = count,
            };
            if (_selectedRecipe.RequiresFuel)
            {
                workstation.TryFuelCraft(ref options);
                return;
            }
            workstation.TryCraft(ref options);
        }

        void UpdateAmountToCraft(string value)
        {
            int maxTimes = Mathf.FloorToInt(_selectedRecipe.Output.Data.StackSize / _selectedRecipe.Yield);
            AmountToCraft = Math.Clamp(AmountToCraft, 1, maxTimes);
        }

        void OnOutputSlotClick(object sender, ItemSlotUI.ClickedContext ctx)
        {
            var slot = ((ItemSlotUI) sender).Slot;

            if (ctx.Pointer.button == PointerEventData.InputButton.Left)
            {
                if (slot.IsEmpty) return;
                if (player.InventoryGUI.IsHoldingItem) return;

                if (ctx.ClickType == ItemSlotUI.ClickType.ShiftClick)
                {
                    player.Inventory.Bag.TryPutNearest(slot.TakeAll());
                }
                else
                {
                    player.InventoryGUI.HoldItem(slot.TakeAll());
                }
            }
        }

        void OnPlayerBagItemChanged(object sender, ItemSlot.ItemChangedContext context)
        {
            //{
                /// Update craftable list status
                foreach (CraftableItemUI craftable in craftableItemUIs.Values)
                {
                    UpdateCraftableStatusForPlayer(craftable);
                }

                /// Update displayed material count
                foreach (var matSlot in materialSlotUIs)
                {
                    if (player.Inventory.Bag.IdItemCount.TryGetValue(matSlot.Item.Id, out int count))
                    {
                        matSlot.Present = count;
                    }
                    else
                    {
                        matSlot.Present = 0;
                    }
                }
            //}
        }


        #region Craftables Item UIs

        /// <summary>
        /// Add recipes to be displayed by Recipe Id.
        /// </summary>
        public void AddRecipesById(List<string> ids)
        {
            foreach (var id in ids)
            {
                if (Game.Recipes.TryGetRecipeData(id, out var data))
                CreateCraftableItemUI(data);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }

        /// <summary>
        /// Add recipes to be displayed by Recipe Data.
        /// </summary>
        public void AddRecipes(List<RecipeData> recipes)
        {
            foreach (var recipe in recipes)
            {
                CreateCraftableItemUI(recipe);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }
        
        public void ReplaceRecipes(List<RecipeData> recipes)
        {
            ClearCraftableItems();
            AddRecipes(recipes);
        }

        public void CreateCraftableItemUI(RecipeData recipeData)
        {
            if (craftableItemUIs.ContainsKey(recipeData.Output.Id)) /// disregard dupes
            {
                return;
            }

            var craftableUI = Game.UI.Create<CraftableItemUI>("Craftable Item", craftablesHolder);
            craftableUI.SetRecipe(recipeData);
            craftableUI.OnClick += OnClickCraftable;

            UpdateCraftableStatusForPlayer(craftableUI);

            craftableItemUIs[recipeData.Output.Id] = craftableUI;
        }

        void UpdateCraftableStatusForPlayer(CraftableItemUI craftable)
        {
            if (player.Inventory.Bag.ContainsAll(craftable.RecipeData.Materials))
            {
                craftable.Status = CraftableItemStatus.CanCraft;
            }
            else
            {
                craftable.Status = CraftableItemStatus.CannotCraft;
            }
        }


        void ClearCraftableItems()
        {
            foreach (var craftable in craftableItemUIs.Values)
            {
                craftable.Destroy();
            }
            craftableItemUIs.Clear();
        }

        #endregion

        
        #region Materials list

        public void DisplayMaterials(RecipeData data)
        {
            ClearMaterialSlots();

            foreach (var material in data.Materials)
            {
                CreateMaterialSlot(material);
            }
        }

        void CreateMaterialSlot(Item material)
        {
            var matSlot = Game.UI.Create<MaterialCountUI>("Material Count", materialSlotsHolder);
            matSlot.SetDisplayedItem(material);
            matSlot.Needed = material.Count;

            /// Retrieve cached count for Item of Id
            if (player.Inventory.Bag.IdItemCount.TryGetValue(material.Id, out var count))
            {
                /// Can also change colors here if you want
                matSlot.Present = count;
            }

            materialSlotUIs.Add(matSlot);
        }        

        void ClearMaterialSlots()
        {
            foreach (var slot in materialSlotUIs)
            {
                Destroy(slot.gameObject);
            }
            materialSlotUIs.Clear();
        }

        #endregion
    }
}