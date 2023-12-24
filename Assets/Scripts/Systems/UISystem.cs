using UnityEngine;
using UZSG.Items;

namespace UZSG.UI
{
    /// <summary>
    /// UI Manager for URMG.
    /// </summary>
    public sealed class UISystem : MonoBehaviour
    {
        [SerializeField] Canvas canvas;
        public Canvas Canvas { get => canvas; }
        [SerializeField] InteractionIndicator _pickupIndicator;
        public InteractionIndicator InteractIndicator { get => _pickupIndicator; }
        [SerializeField] InventoryUI _inventoryUI;
        public InventoryUI InventoryUI { get => _inventoryUI; }
        
        [Header("Prefabs")]
        [SerializeField] GameObject pickupPrefab;
        [SerializeField] GameObject inventoryPrefab;
        public GameObject ItemDisplayPrefab;

        void Start()
        {
            Debug.Log("Initializing UI...");
            
            _inventoryUI.enabled = true;
            _inventoryUI.Hide();

            _pickupIndicator = Instantiate(pickupPrefab, canvas.transform).GetComponent<InteractionIndicator>();
            _pickupIndicator.Hide();
        }

        public ItemDisplayUI CreateItemDisplay(Item item)
        {
            return Instantiate(ItemDisplayPrefab, Canvas.transform).GetComponent<ItemDisplayUI>();
        }
    }
}
