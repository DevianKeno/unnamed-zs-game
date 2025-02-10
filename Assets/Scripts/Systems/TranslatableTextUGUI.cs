using UnityEngine;

using TMPro;
using UZSG.Data;

namespace UZSG
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TranslatableTextUGUI : MonoBehaviour
    {
        public string Key = string.Empty;
        public TranslatableKey KeyData;

        [SerializeField] TextMeshProUGUI tmp;

        void Awake()
        {
            tmp = GetComponent<TextMeshProUGUI>();
        }

        void Start()
        {
            Game.Locale.OnLocaleChanged += Refresh;
            Refresh();
        }

        void OnDestroy()
        {
            Game.Locale.OnLocaleChanged -= Refresh;
        }

        [ContextMenu("Refresh")]
        public void Refresh()
        {
            if (KeyData != null)
            {
                tmp.text = KeyData.DefaultText;
            }
            else if (false == string.IsNullOrWhiteSpace(Key))
            {
                tmp.text = Game.Locale.Translatable(Key);
            }
        }
    }
}