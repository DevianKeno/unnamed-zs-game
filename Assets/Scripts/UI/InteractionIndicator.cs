using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UZSG.Interactions;

namespace UZSG.UI
{
    public class InteractionIndicator : MonoBehaviour
    {
        [SerializeField] GameObject indicator;
        [SerializeField] TextMeshProUGUI actionText;
        [SerializeField] TextMeshProUGUI objectText;

        void Awake()
        {
            actionText = transform.Find("Action (TMP)").gameObject.GetComponentInChildren<TextMeshProUGUI>(true);
            objectText = transform.Find("Object (TMP)").gameObject.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        public void Show(IInteractable obj)
        {
            if (obj == null) return;
            
            actionText.text = obj.Action;
            objectText.text = obj.Name;
            indicator.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }

        public void Hide()
        {
            indicator.SetActive(false);
        }
    }
}
