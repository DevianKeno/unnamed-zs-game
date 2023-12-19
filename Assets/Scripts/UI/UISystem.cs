using UnityEngine;
using URMG.InventoryS;

namespace URMG.UI
{
/// <summary>
/// UI Manager for URMG.
/// </summary>
public sealed class UISystem : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    [SerializeField] InteractIndicator _pickupIndicator;
    public InteractIndicator InteractIndicator { get => _pickupIndicator; }

    [SerializeField] InventoryUI _inventoryUI;
    public InventoryUI InventoryUI { get => _inventoryUI; }

    [Header("Prefabs")]
    [SerializeField] GameObject pickupPrefab;
    [SerializeField] GameObject inventoryPrefab;
    
    /// <summary>
    /// Bind given inventory to the UI.
    /// </summary>
    public void BindInventory(InventoryHandler i)
    {
        if (i == null)
        {
            Debug.Log("Failed to bind Inventory to UI as there is no Inventory.");
            return;
        }
    }

    public void Init()
    {
        Debug.Log("Initializing UI...");
        
        _pickupIndicator = Instantiate(pickupPrefab, canvas.transform).GetComponent<InteractIndicator>();
        _pickupIndicator.Hide();

        _inventoryUI = Instantiate(inventoryPrefab, canvas.transform).GetComponent<InventoryUI>();
        _inventoryUI.Hide();
    }
}
}
