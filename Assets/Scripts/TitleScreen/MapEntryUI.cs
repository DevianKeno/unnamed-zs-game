using System;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

using UZSG.Data;
using UZSG.UI;
using UZSG.UI.Colors;

namespace UZSG.TitleScreen
{
    public class MapEntryUI : RuleEntryUI
    {
        public LevelData LevelData;

        public string Label
        {
            get { return labelTmp.text; }
            set { labelTmp.text = value; }
        }
        public string Subheading
        {
            get { return subheadingTmp.text; }
            set { subheadingTmp.text = value; }
        }
        public Texture Image
        {
            get { return image.texture; }
            set { image.texture = value; }
        }

        public event Action<MapEntryUI> OnClicked;

        [Header("Components")]
        [SerializeField] RawImage image;
        [SerializeField] TextMeshProUGUI labelTmp;
        [SerializeField] TextMeshProUGUI subheadingTmp;
        [SerializeField] Button button;
        public Button Button => button;

        protected override void Awake()
        {
            base.Awake();
            button.onClick.AddListener(OnClick);
        }

        public void OnClick()
        {
            OnClicked?.Invoke(this);
        }

        public void SetLevelData(LevelData data)
        {
            if (data == null) return;

            LevelData = data;
            Label = data.Name;
            Subheading = $"{data.DimensionsKilometers.x} x {data.DimensionsKilometers.y}";

            if (data.Image != null)
            {
                image.color = Color.white;
                Image = data.Image;
            } else
            {
                image.color = Colors.Transparent;
                Image = null;
            }
        }
    }
}