using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UZSG.UI.HUD
{
    public class MissionDisplayUI : MonoBehaviour
    {
        public string Heading
        {
            get
            {
                return headingTmp.text;
            }
            set
            {
                headingTmp.text = value;
            }
        }
        public string Subheading
        {
            get
            {
                return subheadingTmp.text;
            }
            set
            {
                subheadingTmp.text = value;
            }
        }
        public string Body
        {
            get
            {
                return bodyTmp.text;
            }
            set
            {
                bodyTmp.text = value;
            }
        }
        [SerializeField] TextMeshProUGUI headingTmp;
        [SerializeField] TextMeshProUGUI subheadingTmp;
        [SerializeField] TextMeshProUGUI bodyTmp;
        [SerializeField] TextMeshProUGUI distanceText;
        [SerializeField] Image iconImage;

        void Start()
        {
            Heading = "hello";
        }
    }
}