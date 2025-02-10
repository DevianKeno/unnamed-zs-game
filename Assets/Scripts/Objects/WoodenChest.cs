using UZSG.Data;
using UZSG.Entities;
using UZSG.UI;

namespace UZSG.Objects
{
    public class WoodenChest : StorageObject
    {
        protected override void Open(Player player)
        {
            if (!StorageData.GUIAsset.IsSet())
            {
                Game.Console.LogWarn($"StorageObject '{StorageData.Id}' has no GUI asset set.");
                return;
            }

            this.Player = player;
            Game.UI.CreateFromAddressableAsync<StorageGUI>(StorageData.GUIAsset, callback: (element) =>
            {
                GUI = element;
                GUI.ReadStorage(this);
                player.UseObjectGUI(GUI);
                
                animator.CrossFade("open", 0.5f);
                player.InfoHUD.Hide();
                player.Actions.Disable();
                player.Controls.Disable();
                player.FPP.ToggleControls(false);

                player.InventoryWindow.OnClosed += OnCloseInventory;
                player.InventoryWindow.Show();

                Game.UI.SetCursorVisible(true);
            });
        }

        protected override void OnCloseInventory()
        {
            Player.InventoryWindow.OnClosed -= OnCloseInventory;
            Player.RemoveObjectGUI(GUI);
            Player.InventoryWindow.Hide();

            animator.CrossFade("close", 0.5f);
            Game.UI.SetCursorVisible(false);
            /// encapsulate
            Player.InfoHUD.Show();
            Player.Actions.Enable();
            Player.Controls.Enable();
            Player.FPP.ToggleControls(true);
            Player = null;
        }
    }
}