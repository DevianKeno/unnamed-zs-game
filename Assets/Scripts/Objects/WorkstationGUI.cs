using System;
using System.Collections.Generic;

using UnityEngine;
using TMPro;

using UZSG.Objects;
using UZSG.Crafting;

namespace UZSG.UI
{
    public class WorkstationGUI : Window
    {
        public InventoryCrafting Crafter;

        void Awake()
        {
            // Crafter.OnStartCraft += OnStartCraft;
        }

        void OnStartCraft(CraftingRoutine routine)
        {
            CreateCraftingRoutineGUI(routine);
        }

        void CreateCraftingRoutineGUI(CraftingRoutine routine)
        {
            // instantiate
        }
    }
}