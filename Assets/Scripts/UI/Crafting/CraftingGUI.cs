using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using static UnityEngine.EventSystems.PointerEventData.InputButton;

using TMPro;

using UZSG.Crafting;
using UZSG.Data;
using UZSG.Entities;
using UZSG.Items;
using UZSG.Inventory;
using UZSG.Objects;
using static UZSG.Crafting.CraftingRoutineStatus;

namespace UZSG.UI
{
    public class CraftingGUI : ObjectGUI
    {
        /// <summary>
        /// The Workstation tied to this Workstation GUI.
        /// </summary> 
        public CraftingStation CraftingStation
        {
            get => (CraftingStation) BaseObject;
            protected set
            {
                BaseObject = value;
            }
        }
        public RecipeData SelectedRecipe { get; protected set; }
        [SerializeField] protected int amountToCraft = 1;
        public int AmountToCraft
        {
            get => amountToCraft;
            protected set
            {
                amountToCraft = value;
                craftAmountInputField.text = $"{amountToCraft}";
            }
        }

        protected Dictionary<string, CraftableItemUI> craftableItemUIs = new();
        protected List<MaterialCountUI> materialSlotUIs = new();
        /// <summary>
        /// Key is the Crafting Routine; Value is the UI element.
        /// </summary>
        protected Dictionary<CraftingRoutine, CraftingProgressUI> routineUIs = new();

        [Header("CraftingGUI Elements")]
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
            base.Awake();
            craftButton.onClick.AddListener(RequestCraftItem);
            craftAmountInputField.onEndEdit.AddListener(UpdateAmountToCraft);
            searchField?.onValueChanged.AddListener(SearchRecipe);
        }


        #region Public methods

        public override void SetPlayer(Player player)
        {
            base.SetPlayer(player);
            
            player.Inventory.Bag.OnSlotItemChanged += OnPlayerBagItemChanged;
        }
        
        /// <summary>
        /// Reads a workstation.
        /// </summary>
        public virtual void ReadWorkstation(CraftingStation workstation)
        {
            CraftingStation = workstation;
            CreateQueueSlotUIs(workstation.WorkstationData.QueueSize);
            CreateOutputSlotUIs(workstation.WorkstationData.OutputSize);

            CraftingStation.OnCraft += OnWorkstationCraft;
        }

        protected virtual void OnDestroy()
        {
            CraftingStation.OnCraft -= OnWorkstationCraft;
        }

        public void IncrementAmountToCraft()
        {
            if (SelectedRecipe == null)
            {
                return;
            }

            AmountToCraft++;
            int maxTimes = Mathf.FloorToInt(SelectedRecipe.Output.Data.StackSize / SelectedRecipe.Yield);
            AmountToCraft = Math.Clamp(AmountToCraft, 1, maxTimes);
            ClearMaterialSlots();
            DisplayMaterials(SelectedRecipe);
        }

        public void DecrementAmountToCraft()
        {
            if (SelectedRecipe == null)
            {
                return;
            }
            if (AmountToCraft - 1 < 1)
            {
                // print("You reached the minimum amount of number to craft");
                return;
            }

            AmountToCraft--;
            ClearMaterialSlots();
            DisplayMaterials(SelectedRecipe);
        }
        
        /// <summary>
        /// Resets the selected recipe and displayed craftable items.
        /// </summary>
        public void ResetDisplayed()
        {
            SelectedRecipe = null;
            craftedItemDisplay.ResetDisplayed();
            ClearCraftableItems();
            ClearMaterialSlots();
        }
        
        protected override void OnShow()
        {
            ResetDisplayed();

            if (CraftingStation == null)
            {
                return;
            }

            AddRecipes(CraftingStation.WorkstationData.IncludedRecipes); /// by raw RecipeData
            /// Situational by workstation
            /// e.g., By Player recipes can also be crafted in Workbench (ofc if the Player knows it)
            /// but not in smelting, etc. as said, situational
            if (CraftingStation.WorkstationData.IncludePlayerRecipes)
            {
                AddRecipesById(Player.SaveData.KnownRecipeIds); /// by raw Recipe Id. ID!!!!!!
            }

            /// Retrieve crafting routines
            foreach (var r in CraftingStation.Crafter.Routines)
            {
                CreateRoutineProgressUI(r);
            }
        }

        protected override void OnHide()
        {
            if (Player != null)
            {
                Player.Inventory.Bag.OnSlotItemChanged -= OnPlayerBagItemChanged;
                this.Player = null;
            }

            if (CraftingStation != null)
            {
                CraftingStation.OnCraft -= OnWorkstationCraft;
            }

            foreach (var ui in routineUIs.Values)
            {
                Destroy(ui.gameObject);
            }
            routineUIs.Clear();
        }

        #endregion


        protected void CreateOutputSlotUIs(int size)
        {
            for (int i = 0; i < size; i++)
            {
                var slotUI = Game.UI.Create<ItemSlotUI>("Item Slot", parent: outputSlotsHolder);
                slotUI.name = $"Output Slot ({i})";
                slotUI.Link(this.CraftingStation.OutputContainer[i]); /// link output container to UI
                slotUI.OnMouseDown += OnOutputSlotClick;
                slotUI.Show();
            }
        }
        
        protected void CreateQueueSlotUIs(int size)
        {
            for (int i = 0; i < size; i++)
            {
                var slotUI = Game.UI.Create<ItemSlotUI>("Item Slot", parent: queueSlotsHolder);
                slotUI.name = $"Queue Slot"; /// no index, they're shifting :P
                slotUI.OnMouseDown += OnQueueSlotClick; /// TODO: cancel craft
                slotUI.Show();
            }
        }


        #region Workstation callbacks

        protected virtual void OnWorkstationCraft(CraftingRoutine routine)
        {
            switch (routine.Status)
            {
                case Prepared:
                {
                    CreateRoutineProgressUI(routine);
                    break;
                }

                case Started:
                {
                    break;
                }

                // case Ongoing:
                // {
                //     RefreshRoutineProgressUIs(routine);
                //     break;
                // }

                // case Paused:
                // {
                //     RefreshRoutineProgressUIs(routine);
                //     break;
                // }

                case Finished:
                {
                    FinishCraftingRoutine(routine);
                    break;
                }

                case Canceled:
                {
                    break;
                }
            }
        }

        protected virtual void FinishCraftingRoutine(CraftingRoutine routine)
        {
            var ui = routineUIs[routine];
            routineUIs.Remove(routine);
            ui.Destruct();
        }

        /// <summary>
        /// Called when user clicks on a recipe, also referred here as a "Craftable Item".
        /// </summary>
        protected void OnClickCraftable(CraftableItemUI craftable)
        {
            SelectedRecipe = craftable.RecipeData;
            craftedItemDisplay.SetDisplayedRecipe(SelectedRecipe);
            DisplayMaterials(SelectedRecipe);
            Rebuild();
        }

        public void SearchRecipe(string query)
        {
            ClearCraftableItems();

            if (string.IsNullOrWhiteSpace(query))
            {
                AddRecipes(CraftingStation.WorkstationData.IncludedRecipes);
                return;
            }

            List<RecipeData> queriedRecipes = new();
            foreach (var recipe in CraftingStation.WorkstationData.IncludedRecipes)
            {
                /// KMP Algorithm
                int queryItr = 0;
                foreach (char a in recipe.DisplayNameTranslatable)
                {
                    if (char.ToLower(a) != char.ToLower(query[queryItr]))
                    {
                        queryItr--;
                    }
                    if (queryItr == query.Length - 1)
                    {
                        queriedRecipes.Add(recipe);
                        break;
                    }
                    queryItr++;
                }
            }

            if (queriedRecipes.Count > 0)
            {
                AddRecipes(queriedRecipes);
            }

            return;
        }

        public void FilterRecipe(string type)
        {
            ClearCraftableItems();

            List<RecipeData> filteredList = new();
            Dictionary<string, ItemType> itemType = new() {
                ["armor"] = ItemType.Armor,
                ["item"] = ItemType.Item,
                ["equipment"] = ItemType.Equipment,
                ["tool"] = ItemType.Tool,
                ["accessory"] = ItemType.Accessory,
                ["weapon"] = ItemType.Weapon
            };

            if (type == "all")
            {
                AddRecipes(CraftingStation.WorkstationData.IncludedRecipes);
                return;
            }

            foreach (var recipe in CraftingStation.WorkstationData.IncludedRecipes)
            {
                if (recipe.Output.Data.Type != itemType[type])
                {
                    continue;
                }

                filteredList.Add(recipe);
            }

            foreach (var recipe in Player.SaveData.KnownRecipeIds)
            {
                var recipeData = Game.Recipes.GetRecipeData(recipe);
                if (recipeData.Output.Data.Type != itemType[type])
                {
                    continue;
                }
                filteredList.Add(recipeData);
            }

            AddRecipes(filteredList);
        }

        protected virtual void CreateRoutineProgressUI(CraftingRoutine routine)
        {
            var routineUI = Game.UI.Create<CraftingProgressUI>("Craft Progress UI", parent: progressContainer);
            routineUI.SetCraftingRoutine(routine);
            routineUIs[routine] = routineUI;
        }
        
        #endregion


        /// <summary>
        /// Submit a request to the Workstation tied to this Crafting GUI to craft the recipe given the options.
        /// </summary>
        protected virtual void RequestCraftItem()
        {
            if (SelectedRecipe == null)
            {
                return;
            }
            if (SelectedRecipe.Output.Count < 0)
            {
                return;
            }

            int maxTimes = Mathf.FloorToInt(SelectedRecipe.Output.Data.StackSize / SelectedRecipe.Yield);
            var count = Math.Clamp(AmountToCraft, 1, maxTimes);
            var options = new CraftItemOptions()
            {
                Recipe = SelectedRecipe,
                Count = count,
            };

            if (SelectedRecipe.RequiresFuel)
            {
                if (CraftingStation is FueledWorkstation fueledWorkstation)
                {
                    fueledWorkstation.TryFueledCraft(ref options);
                }
                else
                {
                    Game.Console.LogWarn($"[WorkstationGUI/RequestCraftItem()]: Workstation mismatch! Recipe '{options.Recipe.Id}' requires fuel to craft but was crafted on non-fueled workstation '{CraftingStation.WorkstationData.Id}'");
                    return;
                }
            }
            else
            {
                CraftingStation.TryCraft(ref options);
            }
        }

        protected virtual void UpdateAmountToCraft(string value)
        {
            int maxTimes = Mathf.FloorToInt(SelectedRecipe.Output.Data.StackSize / SelectedRecipe.Yield);
            AmountToCraft = Math.Clamp(AmountToCraft, 1, maxTimes);
        }

        protected virtual void OnQueueSlotClick(object sender, ItemSlotUI.ClickedContext ctx)
        {
            var slot = ((ItemSlotUI) sender).Slot;

            if (ctx.Pointer.button == Left)
            {
                if (slot.IsEmpty) return;

                CraftingStation.CancelCraft(slot.Index);
            }
        }

        protected virtual void OnOutputSlotClick(object sender, ItemSlotUI.ClickedContext ctx)
        {
            var slot = ((ItemSlotUI) sender).Slot;

            if (ctx.Pointer.button == Left)
            {
                if (slot.IsEmpty) return;
                if (Player.InventoryWindow.IsHoldingItem) return;

                if (ctx.ClickType == ItemSlotUI.ClickType.ShiftClick)
                {
                    Player.Inventory.Bag.TryPutNearest(slot.TakeAll());
                }
                else
                {
                    Player.InventoryWindow.HoldItem(slot.TakeAll());
                }
            }
        }

        protected virtual void OnPlayerBagItemChanged(object sender, ItemSlot.ItemChangedContext context)
        {
            /// Update craftable list status
            foreach (CraftableItemUI craftable in craftableItemUIs.Values)
            {
                UpdateCraftableStatusForPlayer(craftable);
            }

            /// Update displayed material count
            foreach (var matSlot in materialSlotUIs)
            {
                if (Player.Inventory.Bag.IdItemCount.TryGetValue(matSlot.Item.Id, out int count))
                {
                    matSlot.PresentItemsCount = count;
                }
                else
                {
                    matSlot.PresentItemsCount = 0;
                }
            }
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
            Rebuild();
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
            if (Player.Inventory.Bag.ContainsAll(craftable.RecipeData.Materials))
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
            matSlot.NeededItemsCount = material.Count * AmountToCraft;
            /// Retrieve cached count for Item of Id
            /// Can also change colors here if you want
            matSlot.PresentItemsCount = Player.Inventory.Bag.CountItem(material.Id);
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