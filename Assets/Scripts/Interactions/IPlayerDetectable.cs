using System;
using UZSG.Entities;

namespace UZSG.Interactions
{
    public interface IDetectable
    {
        public void PlayerSiteDetect(Player player);
        public void PlayerAttackDetect(Player player);
    }
}