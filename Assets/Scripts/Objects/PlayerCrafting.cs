using UZSG.Data;
using UZSG.Entities;
using UZSG.Objects;

namespace UZSG.Players
{
    public class PlayerCrafting : Workstation
    {
        protected override void Start()
        {
            /// override, don't call
        }

        internal void Initialize(Player player)
        {
            this.player = player;

            queueSlots = new(WorkstationData.QueueSize);
            outputContainer = new(WorkstationData.OutputSize);
            outputContainer.OnSlotItemChanged += OnOutputSlotItemChanged;

            crafter.Initialize(this);
            crafter.OnRoutineNotify += OnRoutineEventCall;
            crafter.OnRoutineSecond += OnRoutineSecond;

            this.gui = player.InventoryGUI.PlayerCraftingGUI;
            this.gui.LinkWorkstation(this);
            this.gui.SetPlayer(player);
            this.gui.Show();

            AddInputContainer(player.Inventory.Bag);
        }
    }
}