using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UZSG.UI.HUD
{
    public class MissionDisplayUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI titleText;
        [SerializeField] TextMeshProUGUI descriptionText;
        [SerializeField] TextMeshProUGUI distanceText;
        [SerializeField] Image iconImage;

        void Start()
        {
            SetTitleText("hello");
        }


        public void SetTitleText(string message)
        {
            
        }
    }
}