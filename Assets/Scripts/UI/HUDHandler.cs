using UnityEngine;
using UZSG.Player;

namespace UZSG.UI
{
    public class HUDHandler : MonoBehaviour
    {
        public PlayerCore Player;

        [Header("Elements")]
        public StaminaBar StaminaBar;

        void Start()
        {
            Player.OnDoneInit += PlayerDoneInitCallback;
        }

        void PlayerDoneInitCallback()
        {
            StaminaBar.SetAttribute(Player.Attributes.Vitals["Stamina"]);
        }
    }
}
