using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace RL.UI
{
    public class CheckboxCheck : MonoBehaviour
    {
        [SerializeField] Toggle toggle;
        [SerializeField, FormerlySerializedAs("checkImage")] GameObject checkGameObject;

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
            checkGameObject.gameObject.SetActive(isOn);
        }
    }
}
