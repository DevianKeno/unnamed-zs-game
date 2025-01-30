using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

using UZSG.Attributes;
using UZSG.Data;
using UZSG.Entities;
using UZSG.Interactions;
using UZSG.Items;
using UZSG.Objects;
using UZSG.Systems;
using UZSG.UI;
using static UZSG.Data.ItemType;

namespace UZSG.Building
{
    public class PlayerBuildingManager : MonoBehaviour
    {
        [SerializeField] Player player;
        [Space]

        [SerializeField] LayerMask BuildableLayers;
        [SerializeField] float HorizontalSurfaceThreshold = 0.75f;

        public bool InBuildMode { get; protected set; } = false;

        float _buildRange = 4.0f;
        bool _isHoldingShift;
        /// <summary>
        /// Current held item.
        /// </summary>
        ItemData _heldItem = null;
        ObjectData _previousObjectData = null;
        bool _showObjectGhost;
        bool _isObjectGhostEntitySpawned;
        ObjectGhost objectGhost;
        /// <summary>
        /// Target position where to place the object ghost.
        /// </summary>
        Vector3 targetPosition;
        Vector3 targetEulerAngle;

        Material validMaterial;
        Material invalidMaterial;

        
        #region Events

        public event Action OnEnteredBuildMode;
        public event Action OnExitedBuildMode;

        #endregion

        
        InputActionMap actionMap;
        Dictionary<string, InputAction> inputs;


        #region Initializing methods

        internal void Initialize(Player player, bool isLocalPlayer)
        {
            this.player = player;
            
            if (isLocalPlayer)
            {
                actionMap = Game.Main.GetActionMap("Building");
                inputs = Game.Main.GetActionsFromMap(actionMap);
                InitializeInputs();
                InitializeAttributes();
                InitializeMaterials();
                InitializeEvents();
            }
        }

        void InitializeInputs()
        {
            inputs["Place"].performed += OnInputPlace;

            inputs["Rotate"].performed += OnInputRotate;

            inputs["Shift"].started += OnInputShift;
            inputs["Shift"].canceled += OnInputShift;

            inputs["Rotate Scroll"].performed += OnInputRotateScroll;
        }

        void InitializeAttributes()
        {
            if (player.Attributes.TryGet("build_range", out var buildRange))
            {
                this._buildRange = buildRange.Value;
                buildRange.OnValueChanged += OnBuildRangeUpdated;
            }
            else
            {
                this._buildRange = 5f;
            }

            void OnBuildRangeUpdated(object sender, AttributeValueChangedContext e)
            {
                this._buildRange = e.New;
            }
        }
        
        void InitializeEvents()
        {
            player.Actions.OnInteract += OnPlayerInteract;
            player.FPP.OnHeldItemChanged += OnHeldItemChanged;
            Game.UI.OnAnyWindowOpened += OnWindowOpened;
            Game.UI.OnAnyWindowClosed += OnWindowClosed;
            Game.World.CurrentWorld.OnPause += OnPause;
            Game.World.CurrentWorld.OnUnpause += OnUnpause;
        }

        void InitializeMaterials()
        {
            validMaterial = Resources.Load<Material>("Materials/Building Valid");
            invalidMaterial = Resources.Load<Material>("Materials/Building Invalid");
        }

        void OnDestroy()
        {
            Game.UI.OnAnyWindowOpened -= OnWindowOpened;
            Game.UI.OnAnyWindowClosed -= OnWindowClosed;
        }

        #endregion

        #region Input callbacks

        void OnInputRotate(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (_isObjectGhostEntitySpawned)
                {
                    targetEulerAngle.y += _isHoldingShift ? 90 : -90;
                    targetEulerAngle.y %= 360f;
                    objectGhost.Rotation = Quaternion.Euler(targetEulerAngle);
                }
            }
        }

        void OnInputShift(InputAction.CallbackContext context)
        {
            if (context.started)
                _isHoldingShift = true;
            else if (context.canceled)
                _isHoldingShift = false;
        }

        void OnInputRotateScroll(InputAction.CallbackContext context)
        {
            if (_isObjectGhostEntitySpawned)
            {
                if (context.ReadValue<float>() > 0)
                {
                    targetEulerAngle.y += 10f;
                }
                else if (context.ReadValue<float>() < 0)
                {
                    targetEulerAngle.y -= 10f;
                }
                targetEulerAngle.y %= 360f;
                objectGhost.Rotation = Quaternion.Euler(targetEulerAngle);
            }
        }

        #endregion
        
        #region Input callbacks

        void OnInputPlace(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (_isObjectGhostEntitySpawned && objectGhost.TryPlaceObject(targetPosition, Quaternion.Euler(targetEulerAngle)))
                {
                    var heldItem = new Item(_heldItem, 0);

                    player.Inventory.Hotbar.TakeItem(new(heldItem, 1));
                    if (!player.Inventory.Hotbar.Contains(heldItem))
                    {
                        /// Check for similar items in Bag /// TODO: does not work
                        if (player.Inventory.Bag.Contains(heldItem))
                        {
                            var tookItem = player.Inventory.Bag.TakeItem(new(heldItem, heldItem.Data.StackSize));
                            if (!player.Inventory.Hotbar.TryPutAt(player.FPP.SelectedHotbarIndex, tookItem))
                            {
                                player.Inventory.Bag.TryPutNearest(tookItem);
                            }
                        }
                        else
                        {
                            KillObjectGhost();
                            ExitBuildMode();
                        }
                    }
                }
            }
        }

        #endregion

        #region Event callbacks    

        void OnPlayerInteract(InteractionContext context)
        {
            if (context.Phase == InteractPhase.Started)
            {
                ExitBuildMode();
            }
            else if (context.Phase == InteractPhase.Finished || context.Phase == InteractPhase.Canceled)
            {
                if (InBuildMode)
                {

                }
            }
        }

        void OnHeldItemChanged(ItemData itemData)
        {
            _heldItem = itemData;
            if (itemData == null || !IsValidObject(_heldItem))
            {
                ExitBuildMode();
                KillObjectGhost();
                _showObjectGhost = false;
                _previousObjectData = null;
            }
            else
            {
                EnterBuildMode();
                _showObjectGhost = true;
                TrySpawnObjectGhost();
            }
        }

        void OnWindowClosed(Window window)
        {
            ShowGhosts();
        }

        void OnWindowOpened(Window window)
        {
            HideGhosts();
        }

        void OnUnpause()
        {
            ShowGhosts();
        }

        void OnPause()
        {
            HideGhosts();
        }

        #endregion

        void ShowGhosts()
        {
            if (InBuildMode)
            {
                _showObjectGhost = true;
                if (!_isObjectGhostEntitySpawned)
                    TrySpawnObjectGhost(force: true);
                else
                    objectGhost.Show();
            }
        }

        void HideGhosts()
        {
            if (InBuildMode)
            {
                _showObjectGhost = false;
                if (_isObjectGhostEntitySpawned)
                    objectGhost.Hide();
            }
        }

        void Update()
        {
            if (_showObjectGhost && _isObjectGhostEntitySpawned)
            {
                if (Physics.Raycast(player.EyeLevel, player.Forward, out var hit, _buildRange, BuildableLayers))
                {
                    if (Vector3.Dot(hit.normal, Vector3.up) >= HorizontalSurfaceThreshold)
                    {
                        objectGhost.Show();
                        targetPosition = hit.point;
                        objectGhost.Position = targetPosition;
                        objectGhost.Rotation = Quaternion.Euler(targetEulerAngle);
                    }
                    else
                    {
                        objectGhost.Hide();
                    }
                }
                else
                {
                    objectGhost.Hide();
                }
            }
        }

        bool TrySpawnObjectGhost(bool force = false)
        {
            if (!force && Game.UI.HasActiveWindow) return false;
            
            /// If the previous ghost is different from the new
            if (_previousObjectData == null || _previousObjectData.Id != _heldItem.ObjectData.Id)
            {
                KillObjectGhost();
                Game.Entity.Spawn<ObjectGhost>("object_ghost", targetPosition, onCompleted: (info) =>
                {
                    objectGhost = info.Entity;
                    objectGhost.Position = targetPosition;
                    objectGhost.Rotation = Quaternion.Euler(targetEulerAngle);
                    objectGhost.SetObject(_heldItem.ObjectData);
                    _isObjectGhostEntitySpawned = true;
                });
                _previousObjectData = _heldItem.ObjectData;
                _showObjectGhost = true;
            }
            return true;
        }

        void KillObjectGhost()
        {
            if (_isObjectGhostEntitySpawned)
            {
                objectGhost.Kill();
                _showObjectGhost = false;
                _isObjectGhostEntitySpawned = false;
            }
        }

        public void EnterBuildMode()
        {
            if (InBuildMode) return;

            targetPosition = Vector3.zero;
            targetEulerAngle = Vector3.zero;
            InBuildMode = true;
            OnEnteredBuildMode?.Invoke();
            actionMap.Enable();
        }
        
        public void ExitBuildMode()
        {
            if (!InBuildMode) return;

            actionMap.Disable();
            InBuildMode = false;
            OnExitedBuildMode?.Invoke();
        }

        bool IsValidObject(ItemData itemData)
        {
            return itemData != null
                && itemData.IsObject
                && itemData.ObjectData != null
                && itemData.ObjectData.IsValid();
        }
    }
}