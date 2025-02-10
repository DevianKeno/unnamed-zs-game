namespace UZSG.UI.Objects
{
    public class PlayerCraftingGUI : CraftingGUI
    {
        protected override void OnHide()
        {
            if (CraftingStation != null)
            {
                CraftingStation.OnCraft -= OnWorkstationCraft;
            }
            
            foreach (var ui in routineUIs.Values)
            {
                Destroy(ui.gameObject);
            }
            routineUIs.Clear();
        }
    }
}