using UnityEngine;
using UZSG.Player;

namespace UZSG.UI
{
    public class HUDHandler : MonoBehaviour
    {
        public PlayerEntity Player;

        [Header("Elements")]
        public StaminaBar StaminaBar;

        void Start()
        {
            Player.OnDoneInit += PlayerDoneInitCallback;
        }

        void PlayerDoneInitCallback(object sender, System.EventArgs e)
        {
            StaminaBar.SetAttribute(Player.Attributes["Stamina"]);
        }
    }
}
