using UnityEngine;
using UZSG.Entities;

namespace UZSG.FPP
{
    public sealed class ArmsController: MonoBehaviour
    {
        public Player player;
        public Animator animator;
        FPPModel _model;
        
        public void BindPlayer(Player p)
        {
            player = p;
        }

        public void LoadModel(FPPModel model)
        {
            _model = model;
            
        }
    }
}