using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UZSG.Systems;

namespace UZSG.UI
{
    public class PauseMenuWindow : Window
    {

        [SerializeField] Button backToGameBtn;
        [SerializeField] Button inviteBtn;
        [SerializeField] Button optionsBtn;
        [SerializeField] Button exitBtn;

        void Start()
        {
            backToGameBtn.onClick.AddListener(Hide);
            exitBtn.onClick.AddListener(ExitWorld);
        }
        
        void ExitWorld()
        {
            Game.World.ExitCurrentWorld();
        }
    }
}