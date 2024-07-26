namespace UZSG.Items.Weapons
{
    public interface IReloadable
    {
        public bool TryReload(float durationSeconds);
    }
}
