using UnityEngine;
using UnityEngine.UI;
using TMPro;

using UZSG.Systems;
using UZSG.UI.Objects;
using System.Collections.Generic;

namespace UZSG.UI.Players
{
    public partial class InventoryUI : Window, IInitializeable
    {
        Container externalContainer;
        Dictionary<ObjectGUI, Button> _appendedFrameButtons = new();

        [Header("Frames")]
        [SerializeField] FrameController frameController;
        /// <summary>
        /// The text displayed at the top right
        /// </summary>
        [SerializeField] TextMeshProUGUI frameText;
        /// <summary>
        /// Reference to the Player's default crafting frame
        /// </summary>
        [SerializeField] Transform craftingFrame;
        [SerializeField] CraftingGUI playerCraftingGUI;
        [SerializeField] Button playerCraftingFrameButton;
        [SerializeField] Transform frameButtonsHolder;
        /// <summary>
        /// Referenced because it's a variant of the button
        /// </summary>
        [SerializeField] GameObject frameButtonPrefab;


        #region FrameController event callbacks

        void OnSwitchFrame(FrameController.SwitchFrameContext context)
        {
            /// Idk about this, selector might be visible on other frames
            /// subject to change
            if (context.Status == FrameController.SwitchStatus.Started)
            {
                selector.Hide();
            }
            else if (context.Status == FrameController.SwitchStatus.Finished)
            {
                if (context.Next == "bag")
                {
                    selector.Show();
                }
            }
            
            PutBackHeldItem();
            DestroyItemOptions();
        }
        
        #endregion


        /// <summary>
        /// Append other frames to the main Frame Controller. 
        /// </summary>
        /// /// <param name="order">The order of the button this gui is. -1 means at the last.</param>
        public void AppendObjectGUI(ObjectGUI gui, int order = -1)
        {
            if (gui.Frame == null)
            {
                Debug.LogWarning($"Failed to append GUI . It does not have a Frame component.");
                return;
            }
            
            if (gui is CraftingGUI)
            {
                HidePlayerCraftingGUI();
            }

            if (gui is StorageGUI sgui)
            {
                externalContainer = sgui.Storage.Container;
                _hasStorageGuiOpen = true;
            }

            var go = Instantiate(frameButtonPrefab, frameButtonsHolder);
            go.transform.SetSiblingIndex(order < 0 ? 99 : order);
            go.name = $"Frame (Button)";

            var tmp = go.GetComponentInChildren<TextMeshProUGUI>();
            tmp.text = $"{gui.name}";

            var btn = go.GetComponent<Button>();
            btn.onClick.AddListener(() => 
            {
                frameController.SwitchToFrame(gui.Frame.Name);
            });
            frameController.AppendFrame(gui.Frame);
            _appendedFrameButtons[gui] = btn;
            
            gui.Show();
        }

        public void RemoveObjectGUI(ObjectGUI gui)
        {
            if (gui is CraftingGUI)
            {
                ShowPlayerCraftingGUI();
            }

            if (gui is StorageGUI)
            {
                _hasStorageGuiOpen = false;
                externalContainer = null;
            }

            gui.Hide();
            if (_appendedFrameButtons.ContainsKey(gui))
            {
                var btn = _appendedFrameButtons[gui];
                Destroy(btn.gameObject);
                _appendedFrameButtons.Remove(gui);
            }
        }
        

        /// <summary>
        /// Replace the Player Crafting GUI with the Workstation GUI.
        /// </summary>
        public void SetGUI(ObjectGUI gui)
        {
            if (gui is CraftingGUI craftingGui)
            {
                playerCraftingGUI.Hide();
                craftingGui.transform.SetParent(craftingFrame, false);
                frameController.SwitchToFrame("crafting", instant: true);
                Show();
            }
            if (gui is StorageGUI storageGui)
            {
                _hasStorageGuiOpen = true;
            }
            
            /// Set the title text to the workstation's name
            // frameText.text = gui.;
        }

        public void ShowPlayerCraftingGUI()
        {
            frameText.text = CraftingTitle;
            playerCraftingGUI.Show();
            playerCraftingFrameButton.gameObject.SetActive(true);
        }

        public void HidePlayerCraftingGUI()
        {
            playerCraftingGUI.Hide();
            playerCraftingFrameButton.gameObject.SetActive(false);
        }
    }
}