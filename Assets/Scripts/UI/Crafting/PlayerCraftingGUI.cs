namespace UZSG.UI.Objects
{
    public class PlayerCraftingGUI : WorkstationGUI
    {
        public override void OnHide()
        {
            if (workstation != null)
            {
                workstation.OnCraft -= OnWorkstationCraft;
            }
            
            foreach (var ui in _routineUIs.Values)
            {
                Destroy(ui.gameObject);
            }
            _routineUIs.Clear();
        }
    }
}