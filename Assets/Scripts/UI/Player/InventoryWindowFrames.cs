using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

using UZSG.Systems;
using UZSG.UI.Objects;

namespace UZSG.UI.Players
{
    public partial class InventoryWindow : Window, IInitializeable
    {
        Container externalContainer;
        Dictionary<IInventoryWindowAppendable, Button> _appendedFrameButtons = new();

        [Header("Frames")]
        [SerializeField] FrameController frameController;
        public FrameController FrameController => frameController;
        [SerializeField] PlayerCraftingGUI playerCraftingGUI;
        public PlayerCraftingGUI PlayerCraftingGUI => playerCraftingGUI;

        [Header("Elements")]
        /// <summary>
        /// The text displayed at the top right
        /// </summary>
        [SerializeField] TextMeshProUGUI frameText;
        /// <summary>
        /// Reference to the Player's default crafting frame
        /// </summary>
        [SerializeField] Transform craftingFrame;
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
            /// or when using gamepad
            /// subject to change
            if (context.Status == FrameController.SwitchStatus.Started)
            {
                if (EnableSelector)
                {
                    selector?.Hide();
                }
                if (context.Frame != null)
                {
                    frameText.text = context.Frame.Name;
                }
            }
            else if (context.Status == FrameController.SwitchStatus.Finished)
            {
                if (context.Next == "bag")
                {
                    if (EnableSelector)
                    {
                        selector?.Show();
                    }
                }
            }
            
            PutBackHeldItem();
            DestroyItemOptions();
        }
        
        #endregion

        public void Append(IInventoryWindowAppendable window)
        {
            if (window.Frame == null)
            {
                Debug.LogWarning($"Failed to append Window '{window.gameObject.name}'. It does not have a Frame component.");
                return;
            }

            if (window is CreativeWindow creativeWindow)
            {
                var btn = CreateFrameButton("Creative", 1);
                btn.onClick.AddListener(() => 
                {
                    frameController.SwitchToFrame(creativeWindow.Frame.Name);
                });
                frameController.AppendFrame(creativeWindow.Frame);
                _appendedFrameButtons[creativeWindow] = btn;
            }
            else
            {
                var btn = CreateFrameButton(window.gameObject.name, -1);
                btn.onClick.AddListener(() => 
                {
                    frameController.SwitchToFrame(window.Frame.Name);
                });
                frameController.AppendFrame(window.Frame);
                _appendedFrameButtons[window] = btn;
            }
            
        }

        /// <summary>
        /// Append other frames to the main Frame Controller. 
        /// </summary>
        /// /// <param name="order">The order on which to append the button of this gui. -1 means at the last.</param>
        public void AppendObjectGUI(ObjectGUI gui, int order = -1)
        {
            if (gui.Frame == null)
            {
                Debug.LogWarning($"Failed to append GUI . It does not have a Frame component.");
                return;
            }
            
            if (gui is WorkstationGUI)
            {
                HidePlayerCraftingGUI();
            }

            if (gui is StorageGUI sgui)
            {
                externalContainer = sgui.Storage.Container;
                _hasExternalContainerOpen = true;
            }

            var btn = CreateFrameButton(gui.BaseObject.ObjectData.DisplayName, order);
            btn.onClick.AddListener(() => 
            {
                frameController.SwitchToFrame(gui.Frame.Name);
            });
            frameController.AppendFrame(gui.Frame);
            _appendedFrameButtons[gui] = btn;
            
            /// Set the title text to the workstation's name
            frameText.text = gui.BaseObject.ObjectData.DisplayName;
            gui.Show();
        }

        Button CreateFrameButton(string label, int order)
        {
            var go = Instantiate(frameButtonPrefab, frameButtonsHolder);
            go.transform.SetSiblingIndex(order < 0 ? 99 : order);
            go.name = $"Frame (Button)";

            var tmp = go.GetComponentInChildren<TextMeshProUGUI>();
            tmp.text = label;

            var btn = go.GetComponent<Button>();
            return btn;
        }

        public void RemoveObjectGUI(ObjectGUI gui)
        {
            if (gui is WorkstationGUI)
            {
                ShowPlayerCraftingGUI();
            }

            if (gui is StorageGUI)
            {
                _hasExternalContainerOpen = false;
                externalContainer = null;
            }

            if (_appendedFrameButtons.ContainsKey(gui))
            {
                var btn = _appendedFrameButtons[gui];
                Destroy(btn.gameObject);
                _appendedFrameButtons.Remove(gui);
            }
            frameController.RemoveFrame(gui.Frame);

            gui.Hide();
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