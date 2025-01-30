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

        internal void Initialize(Player player, bool isLocalPlayer)
        {
            this.player = player;

            queueSlots = new(WorkstationData.QueueSize);
            outputContainer = new(WorkstationData.OutputSize);
            outputContainer.OnSlotItemChanged += OnOutputSlotItemChanged;

            crafter.Initialize(this);
            crafter.OnRoutineNotify += OnRoutineEventCall;
            crafter.OnRoutineSecond += OnRoutineSecond;

            if (isLocalPlayer)
            {
                this.gui = player.InventoryWindow.PlayerCraftingGUI;
                this.gui.LinkWorkstation(this);
                this.gui.SetPlayer(player);
                this.gui.Show();
            }

            AddInputContainer(player.Inventory.Bag);
        }
    }
}