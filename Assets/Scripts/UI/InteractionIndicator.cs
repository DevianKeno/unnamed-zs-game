using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UZSG.Interactions;

namespace UZSG.UI
{
    public class InteractionIndicator : Window
    {
        public struct Options
        {
            public string ActionText { get; set; }
            public IInteractable Interactable { get; set; }
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
                return;
            }
            
            actionText.text = obj.ActionText;
            objectText.text = obj.Name;
            Show();
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }

        public void Indicate(Options options)
        {
            if (options.Interactable == null)
            {
                Hide();
                return;
            }
            
            actionText.text = options.ActionText;
            objectText.text = options.Interactable.Name;
            Show();
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);

        }
    }
}
