using UnityEngine;
using UZSG.Entities;

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

        public Entities.Player Player;

        [Header("Elements")]
        public StaminaBar StaminaBar;

        internal void Initialize()
        {
        }

        public void BindPlayer(Entities.Player player)
        {
            Player = player;
            StaminaBar.SetAttribute(Player.Vitals.GetAttributeFromId("stamina"));
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
