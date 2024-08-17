using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

using UZSG.Systems;
using UZSG.Entities;
using UZSG.Inventory;
using UZSG.Items;
using UZSG.Items.Weapons;

namespace UZSG.UI.HUD
{
    public class PlayerHUDInfo : Window
    {
        public Player Player;
        [Space]

        public LeanTweenType VignetteEase;
        public float VignetteFadeDuration;

        Dictionary<int, ItemSlotUI> _equipmentSlotUIs = new();
        
        [Header("Elements")]
        public GameObject equipment;
        public AmmoCounterHUD AmmoCounter;
        public TextMeshProUGUI equippedWeaponTMP;
        public WeaponDetailsUI weaponDetails;
        public DynamicCrosshair crosshair;
        public SwitchCrosshair allCrosshair;
        public Compass compass;
        public Image vignette;
        public GameObject pickupsIndicatorContainer;

        internal void Initialize(Player player)
        {
            if (player == null)
            {
                Game.Console.LogAndUnityLog($"Invalid player.");
                return;
            }

            Player = player;
            InitializeEquipmentSlots();
            crosshair.Initialize(player);
            allCrosshair.Initialize(player);
            compass.Initialize(player);
            InitializeEvents();
        }

        void InitializeEquipmentSlots()
        {
            /// Equipment slots are already set
            int index = 1; /// Only 1 (mainhand) and 2 (offhand), as 0 (arms) is not displayed :)
            foreach (Transform child in equipment.transform)
            {
                if (child.TryGetComponent<ItemSlotUI>(out var slotUI)) /// there might be other GameObjects
                {
                    _equipmentSlotUIs[index] = slotUI;
                    index++;
                }
            }
        }

        void InitializeEvents()
        {            
            Player.Inventory.Equipment.OnSlotItemChanged += OnEquipmentSlotChanged;
            Player.FPP.OnChangeHeldItem += OnChangeHeldItem;
            
            Player.Actions.OnPickupItem += (item) =>
            {
                var indicator = Game.UI.Create<PickupsIndicator>("Pickups Indicator", show: false);
                indicator.transform.SetParent(pickupsIndicatorContainer.transform); 
                indicator.SetDisplayedItem(item);
                indicator.PlayAnimation();
            };
        }

        public void FadeVignette(float alpha)
        {
            LeanTween.cancel(vignette.rectTransform);
            LeanTween.alpha(vignette.rectTransform, alpha, VignetteFadeDuration)
            .setEase(VignetteEase);
        }
        

        void OnEquipmentSlotChanged(object sender, ItemSlot.ItemChangedContext e)
        {
            _equipmentSlotUIs[e.ItemSlot.Index].SetDisplayedItem(e.ItemSlot.Item);
        }

        void OnChangeHeldItem(HeldItemController heldItem)
        {
            if (heldItem == null) return;

            if (heldItem is GunWeaponController gun)
            {
                weaponDetails.SetGunVariant();
                AmmoCounter.SetClip(gun.CurrentRounds);
                AmmoCounter.SetReserve(999);
                AmmoCounter.SetFiringMode(gun.CurrentFiringMode);
            }
            else
            {
                weaponDetails.SetMeleeVariant();
            }

            equippedWeaponTMP.text = heldItem.ItemData.Name;
        }
        
    }
}
