using System;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

using UZSG.Systems;
using UZSG.Data;
using UZSG.Items;
using System.Collections.Generic;
using UZSG.Inventory;
using MEC;
using System.Threading.Tasks;
using UZSG.Entities;

using static UnityEngine.EventSystems.PointerEventData.InputButton;
using Unity.VisualScripting;
using System.Linq;

namespace UZSG.UI.Players
{
    public class CreativeWindow : UIElement, IInventoryWindowAppendable
    {
        Player player;
        [SerializeField] float delaySearchSeconds = 0.25f;

        bool _isInitialized = false;
        int _index = 0;
        int _minItemSlotUICount;
        Container container;

        [Header("UI Elements")]
        [SerializeField] Frame frame;
        public Frame Frame => frame;
        List<ItemSlotUI> _itemSlotUIs;
        Dictionary<ItemType, List<ItemData>> _loadedItemsOfType;

        [Header("UI Elements")]
        [SerializeField] TMP_InputField searchField;
        [SerializeField] Toggle useExactMatchToggle;
        [SerializeField] GameObject containerGameObject;
        [SerializeField] GameObject filterBtns;
        [SerializeField] GameObject scrollView;
        [SerializeField] GameObject loadingElement;

        public async void Initialize(Player player)
        {
            if (_isInitialized) return;

            _isInitialized = true;
            container = new Container(Container.MAX_CONTAINER_SIZE);
            _itemSlotUIs = new();
            _loadedItemsOfType = new();
            _minItemSlotUICount = 50;
            foreach (ItemType type in Enum.GetValues(typeof(ItemType)))
            {
                _loadedItemsOfType[type] = new();
            }
            this.player = player;
            InitializeEvents();
            scrollView.gameObject.SetActive(false);
            loadingElement.gameObject.SetActive(true);

            await LoadResourcesAsync();

            scrollView.gameObject.SetActive(true);
            loadingElement.gameObject.SetActive(false);
        }

        void InitializeEvents()
        {
            searchField.onValueChanged.AddListener(OnSearchFieldInput);
            searchField.onSelect.AddListener((text) =>
            {
                player.Controls.Disable();
            });
            searchField.onDeselect.AddListener((text) =>
            {
                player.Controls.Enable();
            });
            useExactMatchToggle.onValueChanged.AddListener((value) =>
            {
                useExactMatch = value;
            });

            player.InventoryWindow.OnOpened += () =>
            {
                _allowSearch = true;
            };
        }

        async Task LoadResourcesAsync()
        {
            await Task.Yield();
            var assets = Resources.LoadAll<ItemData>("Data/Items");
            if (assets.Length == 0)
            {
                Debug.LogWarning("No items found at the specified path.");
                return;
            }

            _index = 0;
            foreach (ItemData itemData in assets)
            {
                _loadedItemsOfType[itemData.Type].Add(itemData);
                AddItem(itemData, _index);
                _index++;
            }
        }

        bool _allowSearch;
        bool useExactMatch;

        IEnumerator<float> DelayedSearchTimer()
        {
            _allowSearch = false;
            yield return Timing.WaitForSeconds(delaySearchSeconds);
            _allowSearch = true;
        }


        #region Event callbacks

        CoroutineHandle delayedSearchCoroutineHandle;

        void OnSearchFieldInput(string text)
        {
            ClearAllSlots();
            _index = 0;
            if (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text))
            {
                ResetToAllItems();
                return;
            }

            if (!_allowSearch) return;

            // if (!useExactMatch)
            // {
            //     Timing.KillCoroutines(delayedSearchCoroutineHandle);
            //     delayedSearchCoroutineHandle = Timing.RunCoroutine(DelayedSearchTimer());
            // }

            text = text.Replace(' ', '_');
            if (useExactMatch)
            {
                if (Game.Items.ItemDataDict.ContainsKey(text.ToLower()))
                {
                    ClearAllSlots();
                    DestroyExcessItemSlotUIs();

                    AddItem(Game.Items.GetData(text.ToLower()), _index);
                    _index++;
                }
            }
            else
            {
                List<ItemData> queriedItems = new();

                foreach (ItemType type in Enum.GetValues(typeof(ItemType)))
                {
                    if (!_loadedItemsOfType.TryGetValue(type, out var itemDataList)) return;

                    foreach (ItemData itemData in itemDataList)
                    {
                        int queryItr = 0;
                        foreach(char a in itemData.name)
                        {
                            
                            if (Char.ToLower(a) != Char.ToLower(text[queryItr]))
                            {
                                queryItr = -1;
                            }
                            if (queryItr == text.Length - 1)
                            {
                                AddItem(itemData, _index);
                                break;
                            }
                            queryItr++;
                        }
                        
                        _index++;
                    }
                }
            }
        }

        void OnSlotMouseUp(object sender, ItemSlotUI.ClickedContext click)
        {
            if (player == null) return;
            if (sender is not ItemSlotUI slotUI) return;
            ItemSlot itemSlot = slotUI.Slot;

            if (player.InventoryWindow.IsHoldingItem)
            {
                if (click.Button == Left)
                {
                    if (itemSlot.Item.Is(player.InventoryWindow.HeldItem))
                    {
                        player.InventoryWindow.HeldItem.Stack(new Item(itemSlot.Item, 1));
                    }
                    else /// discard all held
                    {
                        _ = player.InventoryWindow.TakeHeldItem();
                    }
                }
                else if (click.Button == Right) /// discard one from held
                {
                    _ = player.InventoryWindow.HeldItem.Take(1);
                }
            }
            else
            {
                var item = slotUI.Item;        
                if (click.Button == Middle) /// get a stack
                {
                    player.InventoryWindow.HoldItem(new(item, item.Data.StackSize));
                }
                else /// left click || right click = get one
                {
                    player.InventoryWindow.HoldItem(new(item, 1));
                }
            }
        }

        #endregion


        ItemSlotUI GetOrCreateItemSlotUI(int index)
        {
            if (index < 0) return null;
            if (index >= 0 && index < _itemSlotUIs.Count)
            {
                return _itemSlotUIs[index];
            }

            if (index >= _itemSlotUIs.Count)
            {
                var slotUI = Game.UI.Create<ItemSlotUI>("Item Slot", parent: containerGameObject.transform);
                slotUI.name = $"Item Slot ({index})";
                slotUI.Index = index;
                slotUI.Link(container[index]);
                slotUI.OnMouseUp += OnSlotMouseUp;
                slotUI.Show();
                if (!_itemSlotUIs.Contains(slotUI))
                {
                    _itemSlotUIs.Add(slotUI);
                }
                return slotUI;
            }

            return null;
        }

        void AddItem(ItemData itemData, int index)
        {
            if (itemData.IsUnityNull()) return;
            
            GetOrCreateItemSlotUI(index);
            container.TryPutAt(index, new(itemData, 1));
        }

        async void AddAllItemsOfType(ItemType type)
        {
            if (!_loadedItemsOfType.TryGetValue(type, out var itemDataList)) return;

            await Task.Yield();
            foreach (ItemData itemData in itemDataList)
            {
                AddItem(itemData, _index);
                _index++;
            }
        }

        void ResetToAllItems()
        {
            foreach (ItemType type in Enum.GetValues(typeof(ItemType)))
            {
                AddAllItemsOfType(type);
            }
        }

        void DestroyExcessItemSlotUIs()
        {
            for (int i = _minItemSlotUICount; i < containerGameObject.transform.childCount ; i++)
            {
                _itemSlotUIs[i].Destroy();
            }
            _itemSlotUIs.RemoveRange(_minItemSlotUICount, _itemSlotUIs.Count - _minItemSlotUICount);

        }

        void ClearAllSlots()
        {
            for (int i = 0; i < _index; i++)
            {
                container.ClearAt(i);
            }
            _index = 0;
        }

        public void SetFilter(int value)
        {
            if (!Enum.IsDefined(typeof(ItemType), value)) return;

            AddAllItemsOfType((ItemType) value);
        }
    }
}