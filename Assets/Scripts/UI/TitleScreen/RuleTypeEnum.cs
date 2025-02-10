namespace UZSG
{
    public enum RuleTypeEnum {

        NotSet,
        
        #region World
        Map,
        Seed,
        DaytimeLength,
        NighttimeLength,
        MaxPlayers,
        LootRespawns,

        #endregion

        #region Gameplay
        DropItemsOnDeath,

        #endregion
    }
}