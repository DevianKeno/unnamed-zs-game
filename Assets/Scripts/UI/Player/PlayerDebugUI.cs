using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UZSG.UI
{
    public class PlayerDebugUI : Window
    {
        public TextMeshProUGUI movementStateText;
        public TextMeshProUGUI actionStateText;
        public TextMeshProUGUI physicsText;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                ToggleVisibility();
            }
        }
    }
}