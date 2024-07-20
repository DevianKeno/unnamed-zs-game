using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UZSG.Interactions;

namespace UZSG.UI
{
    public class InteractionIndicator : Window
    {
        [SerializeField] GameObject indicator;
        [SerializeField] TextMeshProUGUI actionText;
        [SerializeField] TextMeshProUGUI objectText;

        void Awake()
        {
            actionText = transform.Find("Action (TMP)").gameObject.GetComponentInChildren<TextMeshProUGUI>(true);
            objectText = transform.Find("Object (TMP)").gameObject.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        public void Indicate(IInteractable obj)
        {
            if (obj == null)
            {
                Hide();
            }
            
            actionText.text = obj.Action;
            objectText.text = obj.Name;
            Show();
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }
    }
}
