using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

using UZSG.Crafting;
using UZSG.Items;

using UZSG.Data;
using UZSG.Inventory;
using UZSG.Entities;
using UZSG.Objects;
using UZSG.UI.Objects;

using static UZSG.Crafting.CraftingRoutineStatus;
using UZSG.TitleScreen;

namespace UZSG.UI.Objects
{
    public class WorkstationGUI : ObjectGUI
    {
        protected Workstation workstation;
        /// <summary>
        /// The Workstation tied to this Workstation GUI.
        /// </summary>The 
        public Workstation Workstation
        {
            get
            {
                return workstation;
            }
            set
            {
                workstation = value;
                baseObject = value;
            }
        }
        protected RecipeData _selectedRecipe;
        public RecipeData SelectedRecipe => _selectedRecipe;
        protected int _amountToCraft = 1;
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

        protected Dictionary<string, CraftableItemUI> craftableItemUIs = new();
        protected List<MaterialCountUI> materialSlotUIs = new();
        /// <summary>
        /// Key is the Crafting Routine; Value is the UI element.
        /// </summary>
        protected Dictionary<CraftingRoutine, CraftingProgressUI> _routineUIs = new();

        [Header("Elements")]
        [SerializeField] protected CraftedItemDisplayUI craftedItemDisplay;
        [SerializeField] protected RectTransform craftablesHolder;
        [SerializeField] protected RectTransform materialSlotsHolder;
        [SerializeField] protected Transform queueSlotsHolder;
        [SerializeField] protected Transform progressContainer;
        [SerializeField] protected Transform outputSlotsHolder;
        [SerializeField] protected TMP_InputField craftAmountInputField;
        [SerializeField] protected TMP_InputField searchField;
        [SerializeField] protected Button craftButton;

        protected override void Awake()
        {
            craftButton.onClick.AddListener(RequestCraftItem);
            craftAmountInputField.onEndEdit.AddListener(UpdateAmountToCraft);
            searchField?.onValueChanged.AddListener(SearchRecipe);
        }

        /// <summary>
        /// Called when user clicks on a recipe, also referred here as a "Craftable Item".
        /// </summary>
        protected void OnClickCraftable(CraftableItemUI craftable)
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
            Workstation = workstation;

            CreateOutputSlotUIs(Workstation.WorkstationData.OutputSize);
            CreateQueueSlotUIs(Workstation.WorkstationData.QueueSize);
        }

        public void IncrementAmountToCraft()
        {
            AmountToCraft++;
            ClearMaterialSlots();
            DisplayMaterials(_selectedRecipe);
        }

        public void DecrementAmountToCraft()
        {
            if (AmountToCraft - 1 < 1)
            {
                print("You reached the minimum amount of number to craft");
                return;
            }
            AmountToCraft--;
            ClearMaterialSlots();
            DisplayMaterials(_selectedRecipe);
        }
        
        public void ResetDisplayed()
        {
            _selectedRecipe = null;
            craftedItemDisplay.ResetDisplayed();
            ClearCraftableItems();
            ClearMaterialSlots();
        }
        
        protected override void OnShow()
        {
            base.OnShow();
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
                workstation.OnCraft -= OnWorkstationCraft;
                workstation.OnCraft += OnWorkstationCraft;

                /// Retrieve crafting routines
                foreach (var r in workstation.Crafter.Routines)
                {
                    CreateRoutineProgressUI(r);
                }
            }
        }

        protected override void OnHide()
        {
            base.OnHide();

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
                // slotUI.Index = i;

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

        protected void OnWorkstationCraft(CraftingRoutine routine)
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


        public void SearchRecipe(String query)
        {
            ClearCraftableItems();

            if (query == "")
            {
                AddRecipes(workstation.WorkstationData.IncludedRecipes);
                return;
            }
            List<RecipeData> queriedRecipes = new();

            foreach (var recipe in workstation.WorkstationData.IncludedRecipes)
            {
                //KMP Algorithm
                int queryItr = 0;
                foreach(char a in recipe.DisplayName)
                {
                    
                    if (Char.ToLower(a) != Char.ToLower(query[queryItr]))
                    {
                        queryItr = -1;
                    }
                    if (queryItr == query.Length - 1)
                    {
                        queriedRecipes.Add(recipe);
                        break;
                    }
                    queryItr++;
                }
            }

            if (queriedRecipes.Count < 1)
            {
                return;
            }

            AddRecipes(queriedRecipes);
            return;
        }

        public void FilterRecipe(String type)
        {
            ClearCraftableItems();
            List<RecipeData> filteredList = new();
            

            Dictionary<string, ItemType> _type = new() {

                ["armor"] = ItemType.Armor,
                ["item"] = ItemType.Item,
                ["equipment"] = ItemType.Equipment,
                ["tool"] = ItemType.Tool,
                ["accessory"] = ItemType.Accessory,
                ["weapon"] = ItemType.Weapon
            };

            if (type == "all")
            {
                AddRecipes(workstation.WorkstationData.IncludedRecipes);
                return;
            }

            foreach (var recipe in workstation.WorkstationData.IncludedRecipes)
            {
                if (recipe.Output.Data.Type != _type[type])
                {
                    continue;
                }

                filteredList.Add(recipe);
            }

            foreach(var recipe in player.SaveData.KnownRecipes)
            {
                var _tempRecipe = Game.Recipes.GetRecipeData(recipe);

                if (_tempRecipe.Output.Data.Type != _type[type])
                {
                    continue;
                }
                filteredList.Add(_tempRecipe);
            }

            AddRecipes(filteredList);
        }

        void CreateRoutineProgressUI(CraftingRoutine routine)
        {
            var routineUI = Game.UI.Create<CraftingProgressUI>("Craft Progress UI", parent: progressContainer);
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
                return;
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
                if (player.InventoryWindow.IsHoldingItem) return;

                if (ctx.ClickType == ItemSlotUI.ClickType.ShiftClick)
                {
                    player.Inventory.Bag.TryPutNearest(slot.TakeAll());
                }
                else
                {
                    player.InventoryWindow.HoldItem(slot.TakeAll());
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
                {
                    CreateCraftableItemUI(data);
                }
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
            if (craftableItemUIs.ContainsKey(recipeData.Id)) /// disregard dupes
            {
                return;
            }

            var craftableUI = Game.UI.Create<CraftableItemUI>("Craftable Item", parent: craftablesHolder);
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
                craftable.Destruct();
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
            matSlot.Needed = material.Count * AmountToCraft;

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