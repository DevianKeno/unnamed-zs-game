using System;

using UnityEngine;
using UnityEngine.UI;

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
            Game.Settings.ShowGlobalInterface();
        }

        void OnExitBtnClick()
        {
            Hide();
            Game.World.ExitCurrentWorld();
        }
    }
}