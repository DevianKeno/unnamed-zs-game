using UnityEngine;
using UnityEngine.UI;

namespace RL.UI
{
    public class CheckboxCheck : MonoBehaviour
    {
        [SerializeField] Toggle toggle;
        [SerializeField] Image checkImage;

        void OnValidate()
        {
            if (gameObject.activeInHierarchy)
            {
                ToggleCheckImage(toggle.isOn);
            }
        }

        void Awake()
        {
            toggle.onValueChanged.AddListener(ToggleCheckImage);
        }

        public void ToggleCheckImage(bool isOn)
        {
            checkImage.gameObject.SetActive(isOn);
        }
    }
}
