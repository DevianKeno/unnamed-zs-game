using UnityEngine;
using UnityEngine.UI;
using UZSG.Player;

namespace UZSG.UI
{
    public interface IToggleable
    {
        public bool IsVisible { get; }
    }

    public class HUDHandler : MonoBehaviour, IToggleable
    {
        bool _isVisible;
        public bool IsVisible => _isVisible;

        public PlayerEntity Player;

        [Header("Elements")]
        public StaminaBar StaminaBar;

        internal void Initialize()
        {
        }

        void Start()
        {
            Player.OnDoneInit += PlayerDoneInitCallback;
        }

        void PlayerDoneInitCallback(object sender, System.EventArgs e)
        {
            StaminaBar.SetAttribute(Player.Attributes["Stamina"]);
        }

        public void SetPlayer(PlayerEntity player)
        {
            Player = player;
        }        

        public void ToggleVisibility()
        {
            ToggleVisibility(!_isVisible);
        }
        
        public void ToggleVisibility(bool isVisible)
        {
            _isVisible = isVisible;
            gameObject.SetActive(isVisible);
        }
    }
}
