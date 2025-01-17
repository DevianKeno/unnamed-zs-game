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
        [Header("Elements")]
        [SerializeField] Button backToGameBtn;
        [SerializeField] Button inviteBtn;
        [SerializeField] Button optionsBtn;
        [SerializeField] Button exitBtn;

        protected override void Awake()
        {
            backToGameBtn.onClick.AddListener(OnBackBtnClick);
            inviteBtn.onClick.AddListener(OnInviteBtnClick);
            optionsBtn.onClick.AddListener(OnOptionsBtnClick);
            exitBtn.onClick.AddListener(OnExitBtnClick);
        }

        void OnBackBtnClick()
        {
            Hide();
            Game.World.UnpauseCurrentWorld();
        }

        void OnInviteBtnClick()
        {
            throw new NotImplementedException();
        }

        void OnOptionsBtnClick()
        {
            throw new NotImplementedException();
        }


        void OnExitBtnClick()
        {
            Hide();
            Game.World.ExitCurrentWorld();
        }
    }
}