using UnityEngine;
using UnityEngine.InputSystem;
using URMG.Core;
using URMG.Items;
using URMG.Interactions;

namespace URMG.Player
{
/// <summary>
/// Represents the different actions the Player can do.
/// </summary>
[RequireComponent(typeof(PlayerCore))]
public class PlayerActions : MonoBehaviour
{
    PlayerCore player;
    public PlayerCore Player { get => player; }

    void Awake()
    {
        player = GetComponent<PlayerCore>();
    }
    
    public void Jump(InputAction.CallbackContext context)
    {            
        //
    }

    /// <summary>
    /// Make this player interact with the given Interactable.
    /// </summary>
    public void Interact(IInteractable obj)
    {
        obj.Interact(this, new InteractArgs());
    }

    public void PickUpItem(ItemEntity itemEntity)
    {
        if (!player.CanPickUpItems) return;
        if (player.Inventory == null) return;
        
        if (player.Inventory.TryPutNearest(itemEntity.AsItem()))
        {
            Destroy(itemEntity.gameObject);
        }
    }
    
    /// <summary>
    /// I want the cam to lock and cursor to appear only when the key is released :P
    /// </summary>
    public void CheckInventory()
    {
        Game.UI.InventoryUI.ToggleVisibility();
    }
}
}
