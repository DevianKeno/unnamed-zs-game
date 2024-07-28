using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UZSG.Interactions;

namespace UZSG.UI
{
    public class InteractionIndicator : Window
    {
        public struct IndicateOptions
        {
            public string ActionText { get; set; }
            public string ObjectText { get; set; }
        }

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
            
            actionText.text = obj.ActionText;
            objectText.text = obj.Name;
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
            Show();
        }

        public void Indicate(IInteractable obj, IndicateOptions options)
        {
            if (obj == null)
            {
                Hide();
            }
            
            actionText.text = options.ActionText;
            objectText.text = options.ObjectText;
            Show();
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);

        }
    }
}
