using UnityEngine;
using UnityEngine.UI;
using TMPro;

using UZSG.Interactions;
using UZSG.Entities;

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

        public void Indicate(IInteractable obj)
        {
            if (obj == null)
            {
                Hide();
                return;
            }
            
            actionText.text = obj.Action;
            objectText.text = obj.Name;
            Show();
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }

        public void SetKey(bool enabled)
        {
            if (enabled)
            {

            }
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
