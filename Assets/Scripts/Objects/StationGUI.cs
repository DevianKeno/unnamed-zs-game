using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UZSG.Crafting;
using System;

namespace UZSG.UI
{
    public class WorkstationGUI : Window
    {
        public string Title
        {
            get { return titleText.text; }
            set { titleText.text = value; }
        }

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

        [SerializeField] protected TextMeshProUGUI titleText;
    }
}