using UnityEngine;
using UnityEngine.InputSystem;
using URMG.Systems;
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

    public void SelectHotbar(int index)
    {            
        if (index < 0 || index > 9) return;

        Debug.Log($"Equipped hotbar slot {index}");
    }


    public void Jump(InputAction.CallbackContext context)
    {            
        //
    }

    /// <summary>
    /// Make this player interact with an Interactable object.
    /// </summary>
    public void Interact(IInteractable obj)
    {
        obj.Interact(this, new InteractArgs());
    }

    public void Equip()
    {

    }

    /// <summary>
    /// Pick up item from ItemEntity and put in the inventory.
    /// There is currently no checking if inventory is full.
    /// </summary>
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
    public void ToggleInventory()
    {
        Game.UI.InventoryUI.ToggleVisibility();
    }
}
}
