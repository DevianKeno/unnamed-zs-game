using UnityEngine;
using UnityEngine.UI;

namespace UZSG.UI
{
    public class ToggleButtonUI : UIElement
    {
        [SerializeField] Toggle toggle;

        protected override void Awake()
        {
            base.Awake();
            toggle.onValueChanged.AddListener(ToggleColor);
        }

        public void ToggleColor(bool value)
        {
            // if (value)
            // {
            //     image.color = new (0.56f, 0.69f, 0.77f, 1f);
            // }
            // else
            // {
            //     image.color = Color.white;
            // }
        }
    }
}