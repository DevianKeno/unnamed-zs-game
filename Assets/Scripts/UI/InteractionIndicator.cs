using UnityEngine;
using UnityEngine.UI;
using TMPro;

using UZSG.Interactions;
using UZSG.Entities;

namespace UZSG.UI
{
    public class InteractionIndicator : UIElement
    {
        public struct Options
        {
            public string ActionText { get; set; }
            public IInteractable Interactable { get; set; }
        }

        public string Button
        {
            get => buttonText.text;
            set => buttonText.text = value;
        }

        [SerializeField] GameObject indicator;
        [SerializeField] GameObject key;
        [SerializeField] TextMeshProUGUI buttonText;
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
            Rebuild();
        }

        public void SetKey(bool enabled)
        {
            key.gameObject.SetActive(enabled);
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
            Rebuild();
        }
    }
}
