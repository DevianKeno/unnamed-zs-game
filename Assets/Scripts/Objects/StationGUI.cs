using UnityEngine;
using TMPro;

namespace UZSG.UI
{
    public class WorkstationGUI : Window
    {
        public string Title
        {
            get { return titleText.text; }
            set { titleText.text = value; }
        }
        
        [SerializeField] protected TextMeshProUGUI titleText;
    }
}